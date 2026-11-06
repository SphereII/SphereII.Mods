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
- The game's POI list is not changed. The map name, POI tier and tags still describe the original, and
  from a distance the POI shows the original's low-detail model until its chunks load.
- Nothing is saved. Every reset of a POI that has a variant works over the tallest column among the
  original and its variants:
  - all sleeper and trigger volumes in that column are removed first, so a restart can't leave bandit
    volumes inside a rebuilt original (or the other way round);
  - blocks above the prefab being built are cleared to air, so a taller variant's upper floors don't
    survive a shorter rebuild. Anything players built in that space above the POI is removed too.
