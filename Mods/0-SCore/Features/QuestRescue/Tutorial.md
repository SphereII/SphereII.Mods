# Quest Rescue: Step-by-Step Examples

These are the three working test quests in `Z-SphereIIDebugging`. Each one sends the player to the
nearest `cabin_01`, spawns a captive inside when the rally point is activated, and has the player
free the captive and bring it back to the trader.

| Quest | What it shows |
|---|---|
| [`test_rescue_captive`](#example-1-a-custom-captive) | A captive with its own entity class and dialog. The captive joins the player afterwards. |
| [`test_rescue_captive_marker`](#example-2-spawn-the-captive-at-a-marker-block) | The captive spawns at a marker block in a POI variant, and leaves once it's out of the POI. |
| [`test_rescue_npcpack`](#example-3-a-stock-npc-from-an-npc-pack) | The captive is a stock NPC from an NPC pack, and leaves once it's out of the POI. |

By default a freed captive goes away once it reaches safety. Only Example 1 keeps its captive, by
opting in with `on_complete="join"`.

See [ReadMe.md](ReadMe.md) for every property and how the feature behaves. The captives come from
**0-XNPCCore**, so all three examples need it installed.

## Files involved

| File | Purpose |
|---|---|
| `Config/entityclasses.xml` | The captive's entity class, and the escort AI package on the captives. |
| `Config/npc.xml` | Gives the custom captive its own dialog. |
| `Config/dialogs.xml` | The "free" response, and the quests in the test trader's job list. |
| `Config/buffs.xml` | A buff that protects the captive until it's freed. |
| `Config/entitygroups.xml` | Example 3's group of stock NPCs. |
| `Config/blocks.xml` | Example 2's marker block. |
| `Config/quests.xml` | The quests, plus adding them to a quest list. |
| `Prefabs/POIs/cabin_01_captive.*` | Example 2's POI variant with the marker blocks. |

## Example 1: a custom captive

`test_rescue_captive`: the captive is `npcRescueCaptiveTest`, a 0-XNPCCore SDCS survivor with its own
dialog. After it's freed it follows the player and stays hired.

### Step 1: The captive's entity class

`Config/entityclasses.xml`:

```xml
<!-- The predicate skips this when 0-XNPCCore isn't loaded. -->
<append xpath="/entity_classes[entity_class[@name='npcSdcsSurvivorMaleT1']]">
    <entity_class name="npcRescueCaptiveTest" extends="npcSdcsSurvivorMaleT1">
        <property name="NPCID" value="npcRescueCaptiveTest"/>
        <property name="UserSpawnType" value="Menu"/>
        <!-- npcSdcsBase's packages plus NPCModRescueEscort. -->
        <property name="AIPackages" value="SdcsNpcCore, SdcsNpcRangedAdvanced, SdcsNpcHired, SdcsNpcMedic, SdcsNpcLoot, SdcsNpcLeave, SdcsCover, NPCModRescueEscort"/>
    </entity_class>
</append>
```

- **The captive must be friendly to players.** An NPC whose faction is hostile to the player has no
  "Greet" prompt, so it can't be freed. The SDCS survivors are `whiteriver`.
- **`NPCModRescueEscort`** keeps the captive following during the escort. The normal hired follow
  actions stop whenever an enemy is near, so without it the captive stands still in a fight. Use
  `NPCModRescueEscortV4` for an NPC whose other packages use the V4 tasks.
- **`AIPackages` replaces the parent's list**, it doesn't add to it. Copy the parent's current list
  (here `npcSdcsBase` in 0-XNPCCore 3.4.03) and add the escort package. If the NPC pack changes its
  list in an update, copy it again.
- **The predicate** (`[entity_class[@name='...']]`) only adds the class when its parent exists, so
  the modlet still loads without 0-XNPCCore. Without it, a missing parent is an error at load.

`Config/npc.xml` gives the captive its own dialog (`dialog_id`):

```xml
<configs>
    <append xpath="/npc[npc_info[@id='npcSdcsTest']]">
        <npc_info id="npcRescueCaptiveTest" name="Captive" faction="whiteriver" stance="Like" dialog_id="RescueCaptiveTest" trader_id="64"/>
    </append>
</configs>
```

### Step 2: The dialog

`Config/dialogs.xml`: the SDCS NPC dialog plus a "free" response. `IsCaptiveSDX` hides the response
unless the NPC is a captive that hasn't been freed.

```xml
<append xpath="/dialogs[dialog[@id='SdcsNPC']]">
    <dialog id="RescueCaptiveTest" startstatementid="start" extends="SdcsNPC">
        <statement id="start" text="Please, get me out of here!">
            <response_entry id="rescueFree"/>
            <response_entry id="Hire"/>
            <response_entry id="Command Menu"/>
            <response_entry id="Weapon Menu"/>
            <response_entry id="Admin Menu"/>
            <response_entry id="Pickup"/>
            <response_entry id="ShowMe"/>
            <response_entry id="done"/>
        </statement>
        <statement id="rescueFreed" text="Thank you! I'll stay close.">
            <response_entry id="done"/>
        </statement>
        <response id="rescueFree" text="You're free. Follow me." nextstatementid="rescueFreed">
            <requirement type="IsCaptiveSDX, SCore" requirementtype="Hide"/>
            <action type="FreeCaptiveSDX, SCore"/>
        </response>
    </dialog>
</append>
```

`FreeCaptiveSDX` hires the captive at no cost (leader, owner, Follow order), removes its
`captive_buff`, and completes the rescue objective for everyone on the quest.

### Step 3: The captive buff

`Config/buffs.xml`: the captive can't be hurt until it's freed, so a stray shot or a zombie can't fail
the quest. Optional; leave out `captive_buff` to make protecting the captive part of the challenge.

```xml
<append xpath="/buffs">
    <buff name="buffRescueCaptiveTest" hidden="true">
        <stack_type value="ignore"/>
        <effect_group>
            <passive_effect name="GeneralDamageResist" operation="base_add" value="1"/>
        </effect_group>
    </buff>
</append>
```

### Step 4: The quest

`Config/quests.xml`:

```xml
<insertBefore xpath="/quests/quest_list[@id='test_quests']">
    <quest id="test_rescue_captive">
        <property name="name_key" value="test quest"/>
        <property name="subtitle_key" value="Test Rescue"/>
        <property name="description_key" value="Go to the nearest cabin_01, clear it, and free the captive inside."/>
        <property name="icon" value="ui_game_symbol_quest"/>
        <property name="repeatable" value="true"/>
        <property name="category_key" value="quest"/>
        <property name="difficulty_tier" value="1"/>
        <property name="offer_key" value="Test the rescue objective."/>
        <property name="statement_key" value="Someone is held in the cabin. Get them out."/>
        <property name="response_key" value="Test - Rescue Captive"/>
        <property name="login_rally_reset" value="true"/>
        <property name="completiontype" value="TurnIn"/>
        <property name="completion_key" value="Captive rescued."/>

        <objective type="GotoPOISDX, SCore">
            <property name="phase" value="1"/>
            <property name="PrefabName" value="cabin_01"/>
            <property name="biome_filter_type" value="AnyBiome"/>
            <property name="nav_object" value="quest"/>
        </objective>

        <objective type="RallyPoint">
            <property name="phase" value="2"/>
            <property name="nav_object" value="rally"/>
            <property name="rally_marker_type" value="questRallyMarkerClear"/>
        </objective>

        <objective type="ClearSleepers">
            <property name="phase" value="3"/>
            <property name="nav_object" value="sleeper_volume"/>
        </objective>

        <!-- Spawn the captive; complete when it's freed. -->
        <objective type="RescueNPCSDX, SCore">
            <property name="phase" value="3"/>
            <property name="entity_group" value="npcRescueCaptiveTest"/>
            <property name="captive_buff" value="buffRescueCaptiveTest"/>
            <property name="nav_object" value="quest"/>
        </objective>

        <objective type="POIStayWithin">
            <property name="phase" value="3"/>
            <property name="radius" value="25"/>
        </objective>

        <action type="UnlockPOI">
            <property name="phase" value="4"/>
        </action>

        <objective type="ReturnToNPC">
            <property name="phase" value="4"/>
            <property name="nav_object" value="return_to_trader"/>
        </objective>

        <!-- The captive must reach the trader alive, then stays hired. -->
        <objective type="EscortNPCSDX, SCore">
            <property name="phase" value="4"/>
            <property name="distance" value="10"/>
            <property name="nav_object" value="quest"/>
            <property name="on_complete" value="join"/>
        </objective>

        <objective type="InteractWithNPC">
            <property name="phase" value="4"/>
            <property name="nav_object" value="return_to_trader"/>
        </objective>

        <reward type="Exp" value="100"/>
    </quest>
</insertBefore>

<append xpath="/quests/quest_list[@id='test_quests']">
    <quest id="test_rescue_captive"/>
</append>
```

What matters in it:

- **`RescueNPCSDX` goes in the same phase as `ClearSleepers`, after the `RallyPoint`.** The captive is
  spawned once the rally point has rebuilt the POI, and the phase only ends when the POI is cleared
  *and* the captive is freed, in either order.
- **`entity_group`** takes an entity group or a single entity class. Here it's the class itself.
- **No `spawn_block`**, so the captive spawns at a random sleeper spawn point in the cabin.
- **`EscortNPCSDX` goes in the turn-in phase.** With `on_complete="join"` it completes when the
  captive is within `distance` blocks of the trader, and the captive stays hired. Without `join`,
  the captive would leave once it's out of the POI (Examples 2 and 3). If the quest fails or is
  abandoned during the escort, the captive leaves even with `join`.
- **`ClearSleepers`** adds the `clear` quest tag, which the POI's `QuestTags` must contain for the rally
  point to reset it.
- If the captive dies before the quest is done, the quest fails.

### Step 5: Make the quest show up at a trader

For regular traders, add the quest to their quest list:

```xml
<append xpath="/quests/quest_list[@id='trader_rekt_quests']">
    <quest id="my_rescue_quest"/>
</append>
```

The vanilla test trader (`traderTest`) names each test quest in its dialog, so it also needs a
`quest_entry` there. `listindex` continues after the existing entries:

```xml
<insertBefore xpath="/dialogs/dialog[@id='traderTest']/statement[@id='currentjobs']/response_entry[@id='nevermind']">
    <quest_entry id="test_rescue_captive" type="add" listindex="8" />
</insertBefore>
```

See the [Quest POI Variants tutorial](../QuestPOIVariants/Tutorial.md#step-3-make-the-quest-show-up-at-a-trader)
for more on trader job lists.

## Example 2: Spawn the captive at a marker block

`test_rescue_captive_marker`: the rally point rebuilds `cabin_01` as `cabin_01_captive` (a
[Quest POI Variant](../QuestPOIVariants/ReadMe.md)), and the captive spawns at one of the variant's
marker blocks instead of a sleeper spawn point. The captive is a different NPC: 0-XNPCCore's SDCS
test baker.

### The captive

`Config/entityclasses.xml`. `npcBakerPlayerCtrlTest` is an SDCS character with the baker model.
Pointing its `NPCID` at Example 1's `npc_info` gives it the same dialog, with the free response, so
no new dialog is needed:

```xml
<append xpath="/entity_classes[entity_class[@name='npcBakerPlayerCtrlTest']]">
    <entity_class name="npcRescueBakerTest" extends="npcBakerPlayerCtrlTest">
        <property name="NPCID" value="npcRescueCaptiveTest"/>
        <property name="AIPackages" value="SdcsNpcCore, SdcsNpcRangedAdvanced, SdcsNpcHired, SdcsNpcMedic, SdcsNpcLoot, SdcsNpcLeave, SdcsCover, NPCModRescueEscort"/>
    </entity_class>
</append>
```

### The marker block

`Config/blocks.xml`. Any block can be a marker; this one looks like a wooden chair but doesn't block
movement, so the captive spawns sitting in the chair rather than standing on top of it:

```xml
<append xpath="/blocks">
    <block name="captiveSpawnMarker">
        <property name="Extends" value="chairWood01"/>
        <property name="Collide" value="melee,bullet,arrow,rocket"/>
    </block>
</append>
```

The captive spawns *on top of* a marker that blocks movement (a bed, a crate) and *inside* one that
doesn't. With several markers in the POI, one is picked at random.

### The variant prefab

Normally you'd copy the POI into your modlet as `<name>_captive` (see the
[POI variants tutorial](../QuestPOIVariants/Tutorial.md#step-1-make-the-variant-prefab)) and place
`captiveSpawnMarker` blocks in the prefab editor.

The test variant was made without the editor: `cabin_01_captive` is a copy of `cabin_01` with
`chairWood01` renamed to `captiveSpawnMarker` in its `.blocks.nim`, the table of block names the
prefab's block data refers to. Every wooden chair in the copy (three) became a marker; the block data
(`.tts`) wasn't touched. Its `.xml` has `Tags` emptied so random world generation doesn't place it.

### The quest

The same as Example 1, with the baker as `entity_group`, two additions, and without
`on_complete="join"`, so the captive leaves once it's 10 blocks out of the cabin:

```xml
<quest id="test_rescue_captive_marker">
    ...
    <property name="completion_key" value="Captive rescued."/>

    <!-- Quest POI Variants: rebuild the POI from cabin_01_captive on rally point reset. -->
    <property name="poi_variant_suffix" value="_captive"/>
    ...
    <objective type="RescueNPCSDX, SCore">
        <property name="phase" value="3"/>
        <property name="entity_group" value="npcRescueBakerTest"/>
        <property name="spawn_block" value="captiveSpawnMarker"/>
        <property name="captive_buff" value="buffRescueCaptiveTest"/>
        <property name="nav_object" value="quest"/>
    </objective>
    ...
</quest>
```

`spawn_block` takes a comma-separated list. If the POI has none of those blocks, the captive falls back
to a sleeper spawn point and the log shows a warning.

## Example 3: A stock NPC from an NPC pack

`test_rescue_npcpack`: the captive is one of 0-XNPCCore's unarmed civilians, `npcBakerEmptyHand` or
`npcNurseEmptyHand` (both `whiteriver`, friendly to players), with no custom entity class. It isn't
meant to stay: once you've led it out of the cabin, it walks off and is removed.

### The entity group

`Config/entitygroups.xml`:

```xml
<configs>
    <append xpath="/entitygroups[entitygroup[@name='npcFriendlyCoreGroupGS01']]">
        <entitygroup name="npcRescueFriendlyTestGroup">
            <e n="npcBakerEmptyHand" p="1"/>
            <e n="npcNurseEmptyHand" p="1"/>
        </entitygroup>
    </append>
</configs>
```

- An NPC pack's own spawn groups work too. Their `none` entries are skipped, so the captive always
  spawns, but they often mix in animals or armed NPCs. A group of your own controls who the captive is.
- A group entry that names a missing class is an error at load. The predicate only adds the group
  when 0-XNPCCore is loaded.

### The dialog

The civilians use 0-XNPCCore's `GenericNPC` dialog, so the free response is added to it.
`IsCaptiveSDX` keeps it hidden on every NPC that isn't an unfreed captive:

```xml
<insertBefore xpath="/dialogs/dialog[@id='GenericNPC']/statement[@id='start']/response_entry[@id='Hire']">
    <response_entry id="rescueFree"/>
</insertBefore>
<append xpath="/dialogs/dialog[@id='GenericNPC']">
    <statement id="rescueFreed" text="Thank you! I'll stay close.">
        <response_entry id="done"/>
    </statement>
    <response id="rescueFree" text="You're free. Follow me." nextstatementid="rescueFreed">
        <requirement type="IsCaptiveSDX, SCore" requirementtype="Hide"/>
        <action type="FreeCaptiveSDX, SCore"/>
    </response>
</append>
```

To find an NPC's dialog: its entity class's `NPCID` names an `npc_info` in `npc.xml`, and that
`npc_info`'s `dialog_id` is the dialog.

### The escort AI package

`Config/entityclasses.xml`: the civilians' packages (from `npcAdvancedEmptyHandTemplate`) plus
`NPCModRescueEscort`. The escort action only runs while an escort is in progress, so these NPCs behave
normally the rest of the time.

```xml
<append xpath="/entity_classes/entity_class[@name='npcBakerEmptyHand']">
    <property name="AIPackages" value="NPCModCore, NPCModNPCMeleeAdvanced, NPCModNPCRangedAdvanced, NPCModNPCHired, NPCModRescueEscort"/>
</append>
<append xpath="/entity_classes/entity_class[@name='npcNurseEmptyHand']">
    <property name="AIPackages" value="NPCModCore, NPCModNPCMeleeAdvanced, NPCModNPCRangedAdvanced, NPCModNPCHired, NPCModRescueEscort"/>
</append>
```

### The quest

The same as Example 1, with the group as `entity_group`, and the escort sending the captive away:

```xml
<quest id="test_rescue_npcpack">
    ...
    <objective type="RescueNPCSDX, SCore">
        <property name="phase" value="3"/>
        <property name="entity_group" value="npcRescueFriendlyTestGroup"/>
        <property name="captive_buff" value="buffRescueCaptiveTest"/>
        <property name="nav_object" value="quest"/>
    </objective>
    ...
    <objective type="EscortNPCSDX, SCore">
        <property name="phase" value="4"/>
        <property name="distance" value="10"/>
        <property name="nav_object" value="quest"/>
        <!-- Not a permanent hire: once it's 10 blocks out of the cabin, it's dismissed and
             removed 10s later. leave is the default; it's spelled out here for clarity. -->
        <property name="on_complete" value="leave"/>
        <property name="leave_delay" value="10"/>
    </objective>
    ...
</quest>
```

With `on_complete="leave"`, the escort ends as soon as the captive is `distance` blocks outside the
POI (`escort_to` defaults to `poi`). The captive is dismissed (it stops following and wanders), and
removed `leave_delay` seconds later. If a player hires it again in that time, it stays. To make a
leaving captive walk back to the trader first, add `<property name="escort_to" value="quest_giver"/>`.

A captive that joins (Example 1) is different: it's escorted to the trader (`escort_to` defaults to
`quest_giver`), and stays hired after that.

## Test it

1. **Turn on Enemy Spawning** for the world. With it off, the game refuses any quest with
   `ClearSleepers` at the rally point: "You cannot accept this quest at this time."
2. Use a world with a `cabin_01` (any biome), and take the quest from the test trader (job slots 8, 9
   and 10), or give it with the console:

   ```
   givequest test_rescue_captive
   givequest test_rescue_captive_marker
   givequest test_rescue_npcpack
   ```

3. Go to the cabin and activate the rally point. The log shows where the captive spawned:

   ```
   [SCore] QuestRescue: cabin_01.1: spawned captive Clark (1234) at (12.50, 62.00, -8.50) (sleeper spawn point)
   ```

   Example 2 also shows the variant swap first, and spawns at `(marker captiveSpawnMarker)`.
4. Clear the cabin, talk to the captive, and pick **You're free. Follow me.** The log shows
   `captive ... freed by player ...`, and the captive follows you.
5. Finish clearing the cabin. The log shows `escort started` when the turn-in phase begins.
   - Example 1: walk back to the trader. The log shows `escort ended` when the captive is within
     10 blocks of the trader, and the captive stays hired.
   - Examples 2 and 3: walk 10 blocks out of the cabin. The log shows `escort ended`, then
     `is leaving; removing it in 10s`, and about 10 seconds later `removing ... it left after the escort`.

A quest given with `givequest` has no trader. Its turn-in completes when you talk to any NPC (the
captive counts), and an escort to the quest giver completes once the captive is `distance` blocks
outside the POI instead.

## Troubleshooting

| What you see | Cause |
|---|---|
| "You cannot accept this quest at this time" at the rally point | Enemy Spawning is off for the world. |
| No `spawned captive` line after the rally point | The captive spawns once the POI rebuild is done; give it a few seconds. Without a rally point it waits about 30 seconds, then logs `was never locked by a rally point`. |
| `is not an entity group or entity class` | `entity_group` is misspelled, or the NPC pack isn't loaded. |
| `has no spawnable entity classes` | Every entry in the group is `none` or names a missing class. |
| `has no 'captiveSpawnMarker' block; using a sleeper spawn point` | The POI (or its variant) has no marker. Check `poi_variant_suffix` and that the variant loaded. |
| No "Greet" prompt on the captive | Its faction is hostile to the player, or it's fighting something (it has a target). |
| "Greet" shows a tooltip about enemies instead of the dialog | An enemy is near the captive. Clear the area first. |
| No free response in the dialog | The response isn't in the dialog the NPC actually uses (check its `NPCID` → `npc_info` → `dialog_id`), or the NPC isn't an unfreed captive. |
| The captive wanders off before it's freed | Its AI packages have no action for the `Stay` order. |
| The captive stops following in a fight | `NPCModRescueEscort` (or the V4 one) is missing from its `AIPackages`. |
| `EscortNPCSDX ... no captive` | The quest has no `RescueNPCSDX` before the escort. The escort completes so the quest isn't stuck. |
