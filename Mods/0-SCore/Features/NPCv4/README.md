# NPCv4 — EntityAliveSDXV4

## Overview

NPCv4 is a ground-up rewrite of the original `EntityAliveSDX` NPC class. Rather than extending
`EntityAlive` (as V2 did), `EntityAliveSDXV4` extends `EntityTrader`. This gives NPCs full access
to the vanilla trader system — inventory, quests, dialog, and the trader tile entity — without
hacking around them.

The class uses a **component-based architecture**: behaviour that was previously crammed into a
single monolithic class is now split into focused, self-contained components. Each component owns
one concern, carries its own state, and is updated via a shared per-tick data cache that prevents
redundant world lookups.

**Usage in entityclasses.xml:**
```xml
<property name="Class" value="EntityAliveSDXV4, SCore" />
```

---

## Architecture

### Partial class files

`EntityAliveSDXV4` is split into focused partial class files to keep each area of behaviour easy
to find and modify:

| File | Contents |
|---|---|
| `EntityAliveSDXV4.cs` | Core class: components, public fields, frame cache, `OnUpdateLive`, patrol, pathing setup |
| `EntityAliveSDXV4.Combat.cs` | `SetRevengeTarget`, `SetAttackTarget`, `DamageEntity`, `ExecuteAction` |
| `EntityAliveSDXV4.Interaction.cs` | Dialog, trade, quest-giver, hire/fire, inventory interactions |
| `EntityAliveSDXV4.Lifecycle.cs` | `PostInit`, `SetDead`, `OnEntityUnload`, `HandleNavObject`, death events |
| `EntityAliveSDXV4.Movement.cs` | `MoveEntityHeaded`, `TeleportToPlayer`, `CheckStuck`, speed overrides |
| `EntityAliveSDXV4.Persistence.cs` | `Read`, `Write`, `WriteSyncData`, `ReadSyncData`, `SendSyncData` |
| `EntityAliveSDXV4.Weapons.cs` | Weapon setup, swapping, `UpdateWeapon`, starting items |

---

## Components

Components implement `INPCComponent`, which exposes four lifecycle hooks:

```
Initialize(entity)        — called once during PostInit
OnUpdateLive(ref cache)   — called every game tick
OnDead()                  — called when the entity dies
OnUnload()                — called when the entity is removed from the world
```

At the top of every `OnUpdateLive` tick, the entity fills an `NPCFrameCache` struct **once**
and passes it by reference to each component. This means expensive world lookups
(`GetLeaderOrOwner`, `GetAttackOrRevengeTarget`, distance calculations) happen exactly once per
tick regardless of how many components or methods need that data.

---

### NPCFrameCache

Not a component — a shared data snapshot filled at the start of every tick.

**What it stores:**
- The entity's current leader/owner and whether they are a player
- Pre-calculated distance to the leader
- The current attack or revenge target
- A pre-computed search bounds for nearby entity queries
- A shared, reusable entity buffer list for `GetEntitiesInBounds` calls (never reallocated)

**Why it matters:** In V2, each subsystem independently called `GetLeaderOrOwner` and
`GetAttackOrRevengeTarget`. Every call is a dictionary lookup. With 20+ NPCs each running
several subsystems per tick, that adds up. The frame cache collapses all of those into two
calls per tick per NPC.

---

### NPCLeaderComponent

**What it does:** Manages everything related to following a hired leader.

- Tracks who owns the NPC and keeps the `hired_<entityId>` cvar on the leader so the party
  system stays in sync
- Disables collision passthrough when an allied player is within 2 blocks (so you don't get
  physically wedged against your own NPC)
- When the leader gets into a vehicle, hides the NPC and snaps them to the vehicle so they
  don't fall behind
- Teleports the NPC to the leader if they fall more than **60 units** behind
- Registers the NPC on the player's companion list and sets the compass marker color
- Periodically forces the leader cache to expire so ownership changes are picked up

**How often it runs:**

| Work | Frequency |
|---|---|
| Core follow/teleport logic | Every tick |
| Allied-player collision scan | Every **0.5 seconds** |
| Leader cache invalidation | Every **30 ticks** |

The collision scan in V2 ran every frame for every NPC. The throttle here reduces allied-player
proximity queries by roughly 30× at 60 fps.

---

### NPCPatrolComponent

**What it does:** Stores the NPC's patrol waypoints and guard position.

- `PatrolCoordinates` — the list of world-space positions the NPC walks between
- `GuardPosition` / `GuardLookPosition` — where the NPC stands and faces when ordered to guard
- Duplicate waypoint detection at block granularity: positions within the same block are
  treated as the same waypoint and are silently ignored

**How often it runs:** It does **not** run on a timer. The component is purely a data holder.
UAI patrol tasks call `AddPatrolPoint` when they need to add a waypoint; the component itself
has no per-tick logic.

**V2 improvement:** V2 used `List.Contains` for duplicate detection — O(n) per insert. NPCv4
uses a `HashSet<Vector3>` keyed on the block-centre of each position, making duplicate
detection O(1) regardless of how many waypoints the NPC has.

---

### NPCCombatComponent

**What it does:** Acts as a gatekeeper for target assignment and keeps combat state clean.

Per-tick work:
- If the NPC is alerted but its cached target is already dead, clears the alert flag
  immediately rather than waiting for the next UAI evaluation

Target-assignment validation (called from `SetRevengeTarget` / `SetAttackTarget`):
- Blocks friendly fire: the NPC cannot target its own leader or any allied NPC
- Prevents clearing a live attack target (some AI tasks reset their target on stun; this
  preserves the target so the NPC re-engages immediately)
- Ignores dead targets

**How often it runs:** Every tick, but the per-tick work is a single null + dead check — it is
essentially free. The heavier validation only runs when a target change is actually requested.

---

### NPCEffectsComponent

**What it does:** Applies block-radius environmental effects to the NPC (campfire warmth,
environmental buffs from nearby blocks, etc.) by calling `EntityUtilities.UpdateBlockRadiusEffects`.

**How often it runs:** Once every **2 seconds**. In V2 this scan ran every frame for every
NPC. Block lookups in a radius are not trivial — throttling to 2-second intervals is
imperceptible in gameplay but dramatically reduces block-query load in areas with many NPCs.

Remote entities (not authoritative on this machine) are skipped entirely.

---

## EntityAliveSDXV4 improvements over EntityAliveSDX

### Movement

**`MoveEntityHeaded` restored:** `EntityTrader` overrides this method and intentionally does
not call the base `EntityAlive` implementation, which leaves NPCs standing in a T-pose with
no physics response. V4 replicates the full `EntityAlive` movement pipeline — root-motion
blending, jump handling, swimming, ragdoll, and animation speed state — so NPCs move correctly
despite extending `EntityTrader`.

**Teleport safety:** `TeleportToPlayer` uses breadcrumb positions from the leader's recent
path, validates the destination height after a 1-second coroutine delay, and retries if the
NPC lands underground. Teleport is suppressed if the NPC is on a Stay or Guard order, or
within 20 units of the leader.

**Fall state:** V4 tracks fall time with a small random jitter on the fall threshold so that
NPCs with identical configs don't all enter fall animations on the same frame.

**Stuck resolution:** Block checks are throttled to every 5 ticks (~83 ms). The push applied
when stuck persists in `motion` between skipped frames, so responsiveness is maintained
without checking every tick.

### Combat

**Friendly fire prevention:** `SetRevengeTarget` and `SetAttackTarget` route through
`NPCCombatComponent.ShouldAllow*`, which gates out the leader, allied NPCs, and dead targets
before any base call is made.

**Damage guards:** `DamageEntity` and `ProcessDamageResponseLocal` check Invulnerable buff and
cvar, as well as the `IsOnMission` flag (NPCs inside vehicles take no damage).

**Trader ID toggling:** During `DamageEntity` the NPC's trader ID is temporarily cleared so
vanilla `EntityTrader.DamageEntity` code doesn't treat the NPC as invulnerable, then restored
immediately after.

### Persistence

**Patrol coordinate serialization:** V2 built the save string by concatenating positions in a
loop — O(n²) string allocations. V4 uses `string.Join(";", PatrolCoordinates)` — O(n).

**Read / PostInit ordering:** `Read` is called before `PostInit` in some code paths, so
components don't exist yet when data is deserialized. V4 deserializes patrol and guard data
into temporary legacy fields (`_patrolCoordinatesLegacy`, etc.) and flushes them into the
patrol component during `PostInit` once components are initialized.

### Entity name caching

`EntityName` in V2 called `Localization.Get` and built the display string on every access.
V4 caches the result in `_cachedDisplayName` and only rebuilds it when the name key or title
key actually changes. This matters because `EntityName` is accessed frequently in UAI logging,
debug output, and nav objects.

### Save/load persistence

`IsSavedToFile` returns `true` if the NPC has a `Leader` or `Persist` cvar. Biome- and
Dynamic-spawned NPCs without those cvars are not saved, matching vanilla zombie behavior and
preventing save file bloat from ambient NPCs.

### Trader data sanity

`PostInit` calls `SanitizeTraderData`: if the trader inventory is null or has more than 200
items (a sign of corruption), it resets the trader tile entity and logs a warning. This
prevents corrupted NPC trader data from crashing on load.

---

## UAI integration

### AIPackage diagnostics on spawn

When a V4 NPC spawns (`PostInit`), `LogMissingAIPackages` runs immediately and logs:

- Which packages were found in `UAIBase.AIPackages`
- Which packages were declared in `entityclasses.xml` but not registered (missing)
- A warning if the AIPackages list is empty entirely

This makes load-order conflicts immediately visible in the log rather than silently failing
every UAI tick.

**Common cause of missing packages:** `utilityai.xml` is loaded per-mod in alphabetical order.
If a mod with a lower sort order (e.g. `0-NPCMod`) replaces the file entirely, any V4 package
definitions added by `0-SCore` will be wiped. Fix: move V4 `utilityai.xml` content to a mod
with a `Z-` prefix so it loads last and is not overwritten.

### UAI Harmony patches (Harmony/UtilityAI/UAIBase.cs)

SCore patches three methods on `UAIBase`:

**`chooseAction` (Prefix — full replacement)**
- Populates entity targets list with the NPC itself (required for `IsSelf` considerations),
  revenge target, and all entities within sight distance — sorted by distance
- Populates waypoint targets (event-driven; sorted only when more than one entry exists)
- Logs a one-time warning if the entity has no AIPackages configured
- Uses `TryGetValue` for package lookup (single hash lookup vs. the vanilla triple lookup)
- Logs a per-package warning (once per unique package name) if a package name is not registered
- Sticks with the current action if no package scores higher than the current one, or if the
  winning action and target are unchanged
- Properly stops and resets the current task before switching to a new action

**`updateAction` (Prefix — full replacement)**
- Guards against a null `CurrentTask` before calling any task methods
- Initializes, starts, updates, and resets tasks in the correct order
- Advances `TaskIndex` when the current task completes, and resets to 0 when all tasks finish

**`Update` (Prefix — early exit)**
- Returns false (skips the UAI update) if the entity is sleeping, preventing sleeping NPCs
  from evaluating AI every tick.

---

## UAITaskFarmingV4

`UAITaskFarmingV4` (`Features/NPCv4/UtilityAI/UAITaskFarming.cs`) is the V4-specific farming
task. It is structurally identical to `UAITaskFarming` (V2/V3) but uses `PathingUtils.FindPath`
instead of `SCoreUtils.FindPath` to match V4's pathing layer.

### Plot claim system

Without a reservation mechanism, two farmer NPCs running `FindTargetFarmPlot` in the same tick
could select the same plot and both path to it. The claim system prevents this.

```
static HashSet<Vector3i> _claimedPlots   — shared across all UAITaskFarmingV4 instances
Vector3i _claimedPosition                — the position held by THIS instance
```

- **`Start()`** — after finding a plot, immediately adds its position to `_claimedPlots` and
  stores it in `_claimedPosition`
- **`FindTargetFarmPlot()`** — calls `IsClaimed()` after each `FarmPlotManager` query; nulls
  the result and continues to the next priority if the plot is already claimed
- **`Stop()`** — removes `_claimedPosition` from `_claimedPlots` and resets it to
  `Vector3i.zero`; called for both normal task completion and interruption, so the claim is
  always released

The set is `static` because UAI tasks are per-entity instances, not singletons. Marking it
static lets every farmer NPC share the same registry without any manager singleton. This is
safe because UAI runs on Unity's main thread.

### Harvest bug fixes

Three bugs in `HandleHarvestingAndCleanup` caused harvested items to silently disappear:

| Bug | Symptom | Fix |
|---|---|---|
| `FastMax(0, minCount)` produced a 0-count `ItemStack` | `AddItem` rejects empty stacks silently | Clamp to `FastMax(1, minCount)` |
| `item.prob` never checked | Items with `prob < 1` always added regardless of luck | Roll `random.RandomFloat > item.prob` and skip on fail |
| `ItemClass.GetItem` not validated | Unknown item names produced `ItemValue.None` stacks, crashing `AddItem` | Check for `null` / `ItemValue.None` and skip with a warning |

`AdvLogging` statements (gated behind `AdvancedTroubleshootingFeatures`) were added at three
points to aid field diagnosis:

1. After `FarmPlotData.Manage()` returns — logs item count
2. Per-item — logs each attempt to add to inventory
3. Per-item — logs when an item is dropped on the ground instead

---

## XML reference

### Entity class properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Class` | string | — | Must be `EntityAliveSDXV4, SCore` |
| `Hirable` | bool | `true` | Whether players can hire this NPC |
| `IsQuestGiver` | bool | `true` | Whether the NPC offers quests in dialog |
| `Names` | comma list | — | Localization keys to pick a random first name from |
| `Titles` | comma list | — | Localization keys to pick a random title from |
| `IsAlwaysAwake` | bool | `false` | Skip sleeper trigger; NPC is always active |
| `SleeperInstantAwake` | bool | `false` | Alias for `IsAlwaysAwake` |
| `CanCollideWithLeader` | bool | `true` | Whether collision with the leader is enabled |
| `EyeHeight` | float | auto | Override eye height for line-of-sight checks |
| `dialogWindow` | string | `"dialog"` | XUi window name opened when talking to the NPC |
| `BagItems` | comma list | — | Items pre-loaded into the NPC's bag on spawn (`name` or `name=count`) |
| `AIPackages` | comma list | — | UAI package names this NPC evaluates each tick |

### Boundary box (nested class)

```xml
<property name="Class" value="EntityAliveSDXV4, SCore">
    <property name="Boundary">
        <property name="BoundaryBox" value="0.8,1.8,0.8" />
        <property name="Center"      value="0,0.9,0" />
    </property>
</property>
```

### Pathing cube integration

V4 reads `PathingCube` / `PathingCube2` sign blocks near the NPC's spawn point at init time.
Supported sign text keys:

| Key | Values | Effect |
|---|---|---|
| `task` | `stay`, `wander`, `guard`, `follow` | Applies the corresponding order buff |
| `buff` | comma list of buff names | Applies each buff to the NPC |
| `pc` | float | Sets the `PathingCode` cvar on the NPC |

```
task=guard;buff=buffMyCustomBuff;pc=1
```
