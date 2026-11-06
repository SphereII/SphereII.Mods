# Quest POI Variants: Step-by-Step Example

This walks through the test setup in `Z-SphereIIDebugging`: a quest that sends the player to `cabin_01`
and, when the rally point is activated, rebuilds it as `cabin_01_bandit`, a reinforced cabin with
bandits and different loot. Follow the same steps for your own POIs and quests.

See [ReadMe.md](ReadMe.md) for how the feature behaves.

## Files involved

| File | Purpose |
|---|---|
| `Prefabs/POIs/cabin_01_bandit.*` | The variant prefab. |
| `Config/quests.xml` | The quest, plus adding it to a quest list. |
| `Config/dialogs.xml` | Only needed for the vanilla **test trader**. See [Step 3](#step-3-make-the-quest-show-up-at-a-trader). |

## Step 1: Make the variant prefab

1. Copy every file of the original POI from `Data/Prefabs/POIs` into your modlet's `Prefabs/POIs`
   folder, renaming `cabin_01` to `cabin_01_bandit`:

   ```
   cabin_01.blocks.nim  ->  cabin_01_bandit.blocks.nim
   cabin_01.ins         ->  cabin_01_bandit.ins
   cabin_01.jpg         ->  cabin_01_bandit.jpg
   cabin_01.mesh        ->  cabin_01_bandit.mesh
   cabin_01.tts         ->  cabin_01_bandit.tts
   cabin_01.xml         ->  cabin_01_bandit.xml
   ```

   The name must be the original name plus the suffix the quest uses (here `_bandit`).

2. Open `cabin_01_bandit` in the prefab editor and make your changes: walls, loot containers, and
   sleeper volumes with bandit NPC spawn groups.

Rules for the variant:
- **Keep the footprint.** The x and z size must match the original. Height can change; `cabin_01` is
  42×16×42 and `cabin_01_bandit` is 42×21×42.
- **Keep `clear` in `QuestTags`.** The rally point only resets POIs whose quest tags match the quest.
  The copied `.xml` already has `clear, infested`.
- The variant is built at the original's base height. Its own `YOffset` is ignored.

A variant in a modlet's `Prefabs/POIs` folder may be picked by random world generation (RWG) for new
worlds. RWG reads the prefab's `Tags` property (e.g. `<property name="Tags" value="wilderness" />`)
and skips a prefab when either:
- `Tags` is empty, or the property is removed; or
- `Tags` includes `testonly`, `devonly`, `navonly` or `biomeonly`. Only RWG uses these tags.

If the quest picks POIs by tag (see [Using it in a real quest](#using-it-in-a-real-quest)), the variant
needs to keep that tag, so its `Tags` can't be empty. In that case, swap the RWG tags for `testonly`:
`<property name="Tags" value="banditvariant,testonly" />`.

## Step 2: Add the quest

`Config/quests.xml` in the test modlet:

```xml
<configs>
    <insertBefore xpath="/quests/quest_list[@id='test_quests']">
        <quest id="test_poi_variant_bandit">
            <property name="name_key" value="test quest"/>
            <property name="subtitle_key" value="Test POI Variant"/>
            <property name="description_key" value="Go to the nearest cabin_01. The rally point rebuilds it as cabin_01_bandit."/>
            <property name="icon" value="ui_game_symbol_quest"/>
            <property name="repeatable" value="true"/>
            <property name="category_key" value="quest"/>
            <property name="difficulty_tier" value="1"/>
            <property name="offer_key" value="Test the bandit POI variant."/>
            <property name="statement_key" value="Clear the bandit cabin."/>
            <property name="response_key" value="Test - POI Variant (Bandit)"/>
            <property name="login_rally_reset" value="true"/>
            <property name="completiontype" value="TurnIn"/>
            <property name="completion_key" value="Bandit cabin cleared."/>

            <!-- SCore Quest POI Variants: rebuild the POI from <name>_bandit on rally point reset. -->
            <property name="poi_variant_suffix" value="_bandit"/>

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

            <objective type="InteractWithNPC">
                <property name="phase" value="4"/>
                <property name="nav_object" value="return_to_trader"/>
            </objective>

            <reward type="Exp" value="100"/>
        </quest>
    </insertBefore>

    <append xpath="/quests/quest_list[@id='test_quests']">
        <quest id="test_poi_variant_bandit"/>
    </append>
</configs>
```

What matters in it:

- **`poi_variant_suffix`**: turns the feature on for this quest. `_bandit` means the POI is rebuilt
  from `<POI name>_bandit`. If no such prefab exists for the POI the quest lands on, it resets normally.
- **`GotoPOISDX, SCore` with `PrefabName`**: sends the player to the **closest** `cabin_01`. That's
  handy for testing a single POI. For a real quest, see [Using it in a real quest](#using-it-in-a-real-quest).
- **`biome_filter_type` = `AnyBiome`**: POI goto objectives default to "same biome as the quest
  giver". Set it to `AnyBiome` when you want a specific POI wherever it is.
- **`RallyPoint`**: activating the rally point resets the POI. This is when the swap happens.
- **`ClearSleepers`**: adds the `clear` quest tag, which is what lets the rally point reset this POI.
  The POI's `QuestTags` must contain `clear`.
- **Don't add the `bandit` quest tag** (for example through `extra_tags`). The game has its own
  `bandit` quest tag, and when a quest carries it, sleeper spawns drop to 20%.

## Step 3: Make the quest show up at a trader

### Regular traders: add it to their quest list

Traders' job lists show whatever the trader rolled from its `quest_list` for each tier, so adding the
quest to the list is all that's needed. No dialog changes:

```xml
<append xpath="/quests/quest_list[@id='trader_rekt_quests']">
    <quest id="my_bandit_quest"/>
</append>
```

The quest's `difficulty_tier` must be at or below the player's tier with that trader. Each tier shows
at most 7 quests, picked at random, so a new quest won't appear every time.

### The vanilla test trader: also patch its dialog

The test trader (`traderTest`, which uses `quest_list="test_quests"`) doesn't read its quest list for
the job menu. Its dialog names each test quest by id. Adding the quest to `test_quests` isn't enough;
it also needs a `quest_entry` in the dialog. `Config/dialogs.xml`:

```xml
<insertBefore xpath="/dialogs/dialog[@id='traderTest']/statement[@id='currentjobs']/response_entry[@id='nevermind']">
    <quest_entry id="test_poi_variant_bandit" type="add" listindex="7" />
</insertBefore>
```
`listindex` continues after the seven vanilla entries (0–6). A quest named this way only appears if it
can find its POI. If it can't, the entry is left out of the list.

## Step 4: Test it

1. Load a world that has a `cabin_01` (or use Playtest on `cabin_01`), and talk to the trader.
2. Pick **Test - POI Variant (Bandit)**. A marker appears on the nearest `cabin_01`.
3. Go to the cabin and activate the rally point. The log shows:

   ```
   [SCore] QuestPOIVariants: cabin_01.1 at (-21, 61, -21): rebuilding from cabin_01_bandit (size 42, 21, 42)
   ResetPOIS cabin_01.1. Pos: -21, 61, -21, Rot: 0
   ```

4. To check that it switches back, run a normal clear quest (e.g. **Test - Clear Sleepers**) on the
   same cabin. It rebuilds `cabin_01`, and if the variant was taller, the log shows
   `cleared N blocks above height 16`.

## Troubleshooting

| What you see | Cause |
|---|---|
| No `QuestPOIVariants` line after activating the rally point, and no warning | The game didn't find `<name><suffix>`. Check the prefab name and that the modlet's `Prefabs` folder loaded. |
| `The footprint (x and z) must match` warning | The variant's x or z size differs from the original. |
| `has YOffset ... The variant is built at the original's height` warning | Only a notice; the variant is still used. |
| No `ResetPOIS` line at all | The rally point wasn't activated, or the POI's `QuestTags` don't include the quest's tags (e.g. `clear`). |
| Quest missing from the test trader's list | The dialog `quest_entry` is missing (see [Step 3](#the-vanilla-test-trader-also-patch-its-dialog)), or the quest couldn't find its POI. |
| `Unable to find POI for quest clear` | `RandomTaggedPOIGotoSDX` found no POI that passes every filter. See [Using it in a real quest](#using-it-in-a-real-quest). |
| Very few sleepers in the variant | The quest has the vanilla `bandit` quest tag, which cuts spawns to 20%. |

## Using it in a real quest

For a quest that should pick any POI at its tier that has a variant, rather than one named POI, use
SCore's `RandomTaggedPOIGotoSDX` with `require_poi_variant`:

```xml
<quest id="quest_tier5_bandit_clear">
    <property name="difficulty_tier" value="5"/>
    <property name="poi_variant_suffix" value="_bandit"/>
    ...
    <objective type="RandomTaggedPOIGotoSDX, SCore">
        <property name="phase" value="1"/>
        <property name="require_poi_variant" value="true"/>
        <property name="biome_filter_type" value="AnyBiome"/>
        <property name="nav_object" value="quest"/>
    </objective>
    <!-- RallyPoint, ClearSleepers, etc. as in the example above -->
</quest>
```

`require_poi_variant` keeps only POIs that have a `<POI name><poi_variant_suffix>` prefab with the
same footprint. With `_bandit` at tier 5, if your modlet ships `<mesa_poi>_bandit` and
`<area7_poi>_bandit`, the quest picks randomly among the Mesa and Area 7 POIs in the world. To add
another POI, drop its `_bandit` prefab into the modlet. No `PrefabName`, no tags, and no changes to the
original POIs.

Keep `poi_variant_suffix`, `RallyPoint` and `ClearSleepers` as in the example, and add the quest to
the traders' quest lists.

Things to know:
- A POI is only picked if **all** of these are true. If none qualifies, the log shows
  `Unable to find POI for quest clear`.
  - It has a usable variant (with `require_poi_variant`).
  - Its `DifficultyTier` matches the quest's `difficulty_tier`. Give the variant the same
    `DifficultyTier` as the original.
  - It has the quest's quest tag (e.g. `clear`). After a swap this is read from the variant, so keep
    the variant's `QuestTags` the same as the original's.
  - It's in the same biome as the trader, unless `biome_filter_type` is `AnyBiome`. The default is
    the same biome.
  - It's within `distance` of the trader, if `distance` is set. Leave it out when only a few POIs
    have variants. From a trader, the search only covers the trader's own POI lists for that tier.
  - It isn't locked by another quest and wasn't already offered in the same job list.
  - The player hasn't completed it for this trader in the last 7 days. This is a vanilla rule, saved
    with the game, so a restart or a new day doesn't clear it. The log shows
    `Prefab bunker_00.3 has already been used`. With only a few POIs that have variants, the quest
    runs out quickly. Add `<property name="reuse_completed_pois" value="true"/>` to the objective
    to let completed POIs be picked again. Locked POIs and POIs already in the same job list are
    still skipped.
- A world may simply have no POI with a variant at that tier near the trader. Check the world's POI
  list if a quest never shows up.
- To see why each POI was rejected, turn on the debug menu (`dm` in the console) and open the job
  list again. Each rejected POI logs a line such as
  `Quest clear: Prefab cabin_01.3 has no _bandit variant`.
- `include_tags` and `exclude_tags` still work alongside `require_poi_variant` if you want to narrow
  the choice further. Tags are read from the variant after a swap, so put any tag you filter on in
  both the original's and the variant's `.xml`. Changing an original's tags means copying the whole
  POI into your modlet under the same name, since prefab `.xml` files can't be XPath-patched (the
  modlet's copy wins over `Data/Prefabs`).
