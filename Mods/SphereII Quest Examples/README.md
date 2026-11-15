# SphereII Quest Examples

The working test quests behind SCore's quest tutorials. Install it next to **0-SCore** and
**0-XNPCCore**, read the tutorial, and copy what you need into your own modlet.

| Quest | Shows | Tutorial |
|---|---|---|
| `test_poi_variant_bandit` | The rally point rebuilds `cabin_01` as `cabin_01_bandit`. | `0-SCore/Features/QuestPOIVariants/Tutorial.md` |
| `test_rescue_captive` | A captive with its own entity class and dialog, who joins the player. | `0-SCore/Features/QuestRescue/Tutorial.md` (Example 1) |
| `test_rescue_captive_marker` | The captive spawns at a marker block in `cabin_01_captive`. | `0-SCore/Features/QuestRescue/Tutorial.md` (Example 2) |
| `test_rescue_npcpack` | The captive is a stock 0-XNPCCore civilian. | `0-SCore/Features/QuestRescue/Tutorial.md` (Example 3) |
| `test_rescue_timed` | `test_rescue_captive` with a 5-minute `time_limit` on the escort back to the trader. | `0-SCore/Features/QuestRescue/Tutorial.md` (Example 4) |
| `test_rescue_outpost` | Turning the rescue in rebuilds the cabin as `cabin_01_outpost` and puts a trader (`npcOutpostTraderTest`) in it. | `0-SCore/Features/POIOutposts/ReadMe.md` |

## Getting the quests

The vanilla test trader (`traderTest`) offers them in job slots 7 to 12. You can also give them
from the console, e.g. `givequest test_rescue_captive`.

- Turn on **Enemy Spawning**. With it off, quests with `ClearSleepers` are refused at the rally
  point.
- Every quest goes to the nearest `cabin_01`. On a small world with only one, also turn on
  `ReusePOILocations` (`AdvancedQuests` in SCore's `Config/blocks.xml`) so a quest can send you back
  to a cabin you've already done. It's a global SCore setting, so this modlet doesn't change it for you.

## Files

| File | Contents |
|---|---|
| `Config/quests.xml` | The six quests, added to the vanilla `test_quests` list. |
| `Config/dialogs.xml` | The captive dialog (`RescueCaptiveTest`), the free response added to 0-XNPCCore's `GenericNPC`, the outpost trader's dialog (`OutpostTraderTest`), and the test trader's job entries. |
| `Config/npc.xml` | `npc_info` for the captive and the outpost trader. Each one names its dialog. |
| `Config/entityclasses.xml` | `npcRescueCaptiveTest`, `npcRescueBakerTest`, `npcOutpostTraderTest`, and the escort AI package added to the stock baker and nurse. |
| `Config/entitygroups.xml` | `npcRescueFriendlyTestGroup`, Example 3's captives. |
| `Config/buffs.xml` | `buffRescueCaptiveTest`, which protects the captive until it's freed. |
| `Config/blocks.xml` | `captiveSpawnMarker` and `outpostSettlerMarker`, chairs that mark where the captive and the trader spawn. |
| `Config/traders.xml` | Trader stock `6500` for the outpost trader. |
| `Config/Localization.csv` | HUD names for the three variants, shown once a quest swaps the cabin, e.g. "Willy's Cabin (Bandit Camp)". |
| `Prefabs/POIs/cabin_01_bandit.*` | The bandit variant of `cabin_01`. |
| `Prefabs/POIs/cabin_01_captive.*` | The captive variant, with `captiveSpawnMarker` chairs. |
| `Prefabs/POIs/cabin_01_outpost.*` | The outpost variant, with `outpostSettlerMarker` chairs. |

The variants keep `cabin_01`'s `QuestTags` (`clear`) and `DifficultyTier`, and have `Tags` emptied so
random world generation doesn't place them.

## Load order

Modlets load in alphabetical order of folder name. The patches that depend on 0-XNPCCore check that
its classes exist first, e.g. `append xpath="/entity_classes[entity_class[@name='npcBakerPlayerCtrlTest']]"`,
so they're skipped when 0-XNPCCore isn't installed. If you copy them into a modlet whose folder name
sorts before `0-XNPCCore`, they're skipped too, silently. Name your modlet so it sorts after it.
