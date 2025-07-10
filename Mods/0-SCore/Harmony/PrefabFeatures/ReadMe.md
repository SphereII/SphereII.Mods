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