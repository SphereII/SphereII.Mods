# Quest Rescue

Quest objectives for rescuing a captive NPC: `RescueNPCSDX` spawns the captive inside the
quest's POI and completes when it's freed; `EscortNPCSDX` completes when the freed captive
reaches the quest giver. The quest fails if the captive dies along the way.

To have the rescued survivor turn the POI into an outpost with a trader once the quest is turned
in, see [POI Outposts](../POIOutposts/ReadMe.md).

For complete, working examples (a custom captive that joins, a marker-block spawn in a POI variant,
and a stock NPC-pack captive), see [Tutorial.md](Tutorial.md). A commented-out sample quest,
`score_tier1_rescue`, is in SCore's `Config/quests.xml`; the dialog requirement and action are
documented in `Config/dialogs.xml`, and the escort AI packages are in `Config/utilityai.xml`.

Test quests in `Z-SphereIIDebugging` (offered by the test trader; they need 0-XNPCCore for the
captive, `npcRescueCaptiveTest`):
- `test_rescue_captive`: no `spawn_block`, so the captive spawns at a sleeper spawn point in
  `cabin_01`. Its escort uses `on_complete="join"`, so the captive stays hired.
- `test_rescue_captive_marker`: the rally point rebuilds `cabin_01` as `cabin_01_captive` (Quest
  POI Variants), and the captive, `npcRescueBakerTest` (0-XNPCCore's SDCS test baker), spawns at
  one of its three `captiveSpawnMarker` blocks.
  `cabin_01_captive` is `cabin_01` with `chairWood01` renamed to `captiveSpawnMarker` in its
  `.blocks.nim`, so the markers sit where the dining chairs were.
- `test_rescue_npcpack`: the captive is a stock 0-XNPCCore civilian (`npcBakerEmptyHand` or
  `npcNurseEmptyHand`, both whiteriver and friendly to players) from the modlet's
  `npcRescueFriendlyTestGroup`. The modlet adds the free response to the package's `GenericNPC`
  dialog and `NPCModRescueEscort` to those two classes' AI packages. Its escort uses
  `on_complete="leave"`, so once you lead the civilian 10 blocks out of the cabin it walks off and
  is removed 10 seconds later.

## Usage

Put the objective in the same phase as `ClearSleepers`, after the `RallyPoint`:

```xml
<objective type="RescueNPCSDX, SCore">
    <property name="phase" value="3"/>
    <property name="entity_group" value="npcCaptiveGroup"/>
    <property name="spawn_block" value="captiveSpawnMarker"/>
    <property name="captive_buff" value="buffCaptive"/>
    <property name="nav_object" value="quest"/>
</objective>
```

| Property | Required | Meaning |
|---|---|---|
| `entity_group` | yes | Entity group (or a single entity class) the captive is picked from. The group's `none` entries are skipped, so a biome spawn group padded with `none` still always spawns a captive. |
| `spawn_block` | no | Block name(s), comma separated. The captive spawns at a random one inside the POI: on top of it if it's solid, inside it if you can walk through it. |
| `captive_buff` | no | Buff added to the captive when it spawns, e.g. to make it invulnerable or play a tied-up animation until it's freed. |
| `nav_object` | no | Compass marker that follows the captive. |
| `leave_delay` | no | Only for a quest without `EscortNPCSDX`: seconds between the freed captive being dismissed at the end of the quest and being removed. Default 10. |

If no `spawn_block` is found, the captive spawns at a random sleeper spawn point in the POI, or
at the POI's center if it has none.

## Freeing the captive

Add a response to the captive's dialog:

```xml
<response id="free" text_key="dialog_free_captive" nextstatementid="done">
    <requirement type="IsCaptiveSDX, SCore" requirementtype="Hide"/>
    <action type="FreeCaptiveSDX, SCore"/>
</response>
```

- `IsCaptiveSDX` is true for a captive that hasn't been freed. `value="not"` negates it, e.g. to
  hide the NPC's normal hire response while it's a captive.
- `FreeCaptiveSDX` makes the captive join the player who freed it (leader and owner, Follow order)
  at no cost, removes the `captive_buff`, and completes the objective for everyone on the quest.
- Any player can free the captive, not only the quester.
- Freeing makes the captive follow the player, but it only stays hired if the quest has an
  `EscortNPCSDX` with `on_complete="join"` and the quest is completed. Otherwise it leaves: at
  the end of the escort, or, in a quest without an escort, when the quest ends (completed, failed
  or abandoned). `RescueNPCSDX`'s `leave_delay` (default 10) sets how long until it's removed then.

## Escorting the captive

Put `EscortNPCSDX` in the turn-in phase, next to `ReturnToNPC` and `InteractWithNPC`:

```xml
<objective type="EscortNPCSDX, SCore">
    <property name="phase" value="4"/>
    <property name="distance" value="10"/>
    <property name="nav_object" value="quest"/>
</objective>
```

| Property | Required | Meaning |
|---|---|---|
| `escort_to` | no | Where the escort ends. `quest_giver`: within `distance` blocks of the quest giver. `poi`: `distance` blocks outside the POI's bounds. Default: `poi` with `on_complete="leave"` (the default), `quest_giver` with `join`. |
| `distance` | no | How close (in blocks) the captive must get to the quest giver, or how far outside the POI. Default 10. |
| `nav_object` | no | Compass marker that follows the captive. |
| `on_complete` | no | `leave` (default): when the escort completes, the captive is dismissed, wanders off, and is removed after `leave_delay` seconds. The escort ends as soon as the captive is out of the POI, unless `escort_to` says otherwise. `join`: the captive stays hired by the player who freed it. |
| `leave_delay` | no | With `leave`, seconds before the captive is removed. Default 10. If a player hires it again in that time, it isn't removed. |

- It uses the captive from the quest's `RescueNPCSDX`. Without one, it logs an error and
  completes, so the quest can't get stuck.
- The captive can't really be lost on the way: hired NPCs teleport to their leader when they fall
  25 blocks or more behind.
- While the objective runs, the captive has the cvar `RescueEscort` = 1.
- With `escort_to="quest_giver"`, a quest with no quest giver (e.g. one given with the `givequest`
  console command) completes the escort once the captive is `distance` blocks outside the POI.
- By default (`leave`) the captive is only escorted out of the POI. Once it's `distance` blocks
  outside, it's dismissed and goes away; you don't have to take it to the trader or talk to it.
- A captive only stays hired when the quest sets `on_complete="join"` and is completed. If the
  quest fails or is abandoned during the escort, the captive leaves either way.

### Keeping the captive moving in a fight

The normal hired follow actions stop while an enemy is within 20-30 blocks, so a captive would
stand still in a fight. Add the escort AI package to the captive's entity class:

```xml
<property name="AIPackages" value="NPCModCore, NPCModNPCHired, NPCModRescueEscort"/>
```

- `NPCModRescueEscort` uses the V3 task names (0-XNPCCore NPCs); `NPCModRescueEscortV4` uses the
  V4 ones.
- Its one action follows the leader while `RescueEscort` is 1, at weight 10, above combat and flee
  actions. When the escort ends, the captive goes back to its normal behavior.

## Behavior

- The captive is spawned by the server once the rally point has rebuilt the POI. If the quest has
  no rally point, it spawns after about 30 seconds anyway and logs a warning.
- Party members share one captive. Each member's objective gets the same entity.
- Until freed, the captive has the `Stay` order, guards its spawn point, and does not despawn.
  It has the cvar `RescueCaptive` = 1.
- "Freed" means the captive has a leader (`Leader` or `Owner` cvar above 0), so hiring it any
  other way also completes the objective.
- If the quest fails or is abandoned before the captive is freed, the captive is removed, unless a
  party member is still on the quest.
- The captive's entity ID is saved in the quest's data, so it survives a restart.

## Captive entity setup

- Its faction must not be hostile to the player, or the player can't talk to it. It should also
  be ignored by the POI's sleepers (`captive_buff` can cover this).
- After it's freed, the captive follows using the V4 hired AI (`NPCModNPCHiredV4`). Its follow
  actions don't run while an enemy is within 20-30 blocks, so an unarmed captive stands still
  in a fight. A captive-specific AI package that keeps following is planned with the escort
  objective.
