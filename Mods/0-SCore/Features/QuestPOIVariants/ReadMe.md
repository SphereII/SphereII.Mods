# Quest POI Variants

When a quest's rally point resets its POI, rebuild the POI from a variant prefab instead of the
original. Example: send the player to `cabin_01`, and when the rally point is activated, the cabin is
rebuilt as `cabin_01_bandit`, a reinforced version with bandit sleepers and different loot.

For a full walkthrough of the working test quest (prefab, quests.xml, trader and dialog changes, and
how to verify it), see [Tutorial.md](Tutorial.md).

## Setup

1. Build the variant prefab. Name it `<original name><suffix>`, e.g. `cabin_01_bandit`.
   - Its **footprint (x and z) must match** the original. If not, the variant is skipped and a warning
     is logged.
   - It **may be taller or shorter** than the original. It is built at the original's base height; the
     variant's own `YOffset` is not used (a warning is logged if they differ).
   - Give its sleeper volumes the spawn groups you want (e.g. bandit NPC groups).
   - If it has sign canvases, ship the `<variant>_signs.xml` the prefab editor saves next to it
     (e.g. `cabin_01_bandit_signs.xml`). The signs are stored there, not in the prefab. Without it the
     canvases show blank and the server logs a warning naming the prefab.
   - Give it a Localization entry keyed by the prefab name (e.g. `cabin_01_bandit,Bandit Cabin`).
     Once the POI is swapped, the HUD shows the variant's name, and without an entry it shows the raw
     prefab name.
2. Add the property to the quest:

```xml
<quest id="quest_bandit_cabin">
    <property name="poi_variant_suffix" value="_bandit"/>
    ...
    <objective type="RandomTaggedPOIGotoSDX, SCore">
        <property name="require_poi_variant" value="true"/>
        <property name="biome_filter_type" value="AnyBiome"/>
        ...
    </objective>
    <objective type="RallyPoint"> ... </objective>
    <objective type="ClearSleepers"> ... </objective>
</quest>
```

`require_poi_variant` makes the quest pick only POIs, at the quest's tier, that have a variant for its
`poi_variant_suffix`. Adding a variant prefab is all it takes to add a POI to the quest. The variant
must keep the original's `QuestTags` (e.g. `clear`) and `DifficultyTier`. Without
`require_poi_variant`, a quest that lands on a POI without a variant resets it normally.

## Behavior

- The swap is never reverted on its own. The POI keeps the variant until another quest resets it. A
  quest without `poi_variant_suffix` rebuilds the original prefab.
- Blocks are rebuilt on the server, so every player sees the same building.
- Once swapped, the POI's name, tier and tags come from the variant, for every player: the HUD
  location name and "entering" banner show the variant's name, updating as soon as the rally point
  is activated, even for players already standing in the POI.
- Each swap is saved with the world (`SCore_POIVariants.xml` in the save folder) and restored on
  startup, and the server sends swaps to clients as they happen and in full when a player joins.
  Clients need the variant prefab installed (as they do for any modlet with prefabs); a client
  without it keeps the original's name and logs a warning.
- From a distance the POI shows the variant's low-detail model (`<variant>.mesh`) if it has one,
  and the original's otherwise.
- Every reset of a POI that has a variant works over the tallest column among the original and its
  variants, so a world whose blocks don't match the saved swap (e.g. from an older save) still
  rebuilds cleanly:
  - all sleeper and trigger volumes in that column are removed first, so a restart can't leave bandit
    volumes inside a rebuilt original (or the other way round);
  - blocks above the prefab being built are cleared to air, so a taller variant's upper floors don't
    survive a shorter rebuild. Anything players built in that space above the POI is removed too.
