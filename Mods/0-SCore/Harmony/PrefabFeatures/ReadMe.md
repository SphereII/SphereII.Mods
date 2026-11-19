The `RemoveTraderProtection.cs` patch, part of the Advanced Prefab Features, allows modders to disable the standard
protection around trader areas. This enables more dynamic interactions with traders and their environments, including
potentially allowing building within their zones.

## Functionality

This feature is implemented through a Harmony patch that modifies how trader protection areas are enforced. When
enabled, it removes the default invulnerability and building restrictions typically associated with trader POIs. This
allows players to damage traders, and potentially build or destroy blocks within the trader's designated area, depending
on other related settings.

* **Patch Target**: (Implicit, related to trader area protection checks, e.g., `TraderArea.Is...`)
* **Purpose**: To disable trader protection in their prefabs.

## Configuration

The "Remove Trader Protection" feature is controlled by properties within the `AdvancedPrefabFeatures` class in your
`blocks.xml` file:

```xml

<property class="AdvancedPrefabFeatures">
    <property name="DisableTraderProtection" value="false"/>
    <property name="AllowBuildingInTraderArea" value="false"/>
</property>
```

**Explanation**:

* **`DisableTraderProtection`**: `false` - Setting this property's `value` to `true` globally disables the protection
  for traders within their prefabs, making them vulnerable to damage and other interactions.
* **`AllowBuildingInTraderArea`**: `false` - If `true`, this allows players to place and remove blocks within the
  trader's protected area. **This property only takes effect if `DisableTraderProtection` is also set to `true`**.
# No Region Reset (`NoRegionReset.cs`)

Protects individual POIs from the timed region reset, without making them a trader area.

Add this to the POI's own prefab XML, the file next to its `.tts` (not the world's `prefabs.xml`):

```xml
<property name="SCoreNoRegionReset" value="True" />
```

* Every chunk the POI occupies gets the same protection the game gives trader compounds, so the reset skips it.
  The admin `regionreset` command respects it too, except in its "reset everything" mode.
* No gameplay side effects: blocks stay breakable, building is allowed, no trader waypoint, no closing hours.
* Works for POIs that fit in a single chunk, and in every reset grouping mode. In `GroupedPOIs` mode the whole chunk
  group is protected, so a POI that overlaps another won't be half reset.
* Each prefab reads only its own XML. Setting it on `cabin_bandit.xml` protects every placed `cabin_bandit`, and nothing
  else. `cabin.xml` is unaffected, and numbered variants (`cabin_01`, `cabin_02`, ...) each need it.
* POIs swapped to a variant (Quest POI Variants, POI Outposts) use the variant's XML once swapped. To protect outposts,
  set it in the outpost variant (e.g. `cabin_01_outpost.xml`).
* Anything players change in a protected POI is permanent, since it never resets.
* The server log prints `[SCore] SCoreNoRegionReset: protecting N POI(s) from region reset.` whenever the count changes.
