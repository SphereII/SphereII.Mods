The `ItemActionRepairLimiter` patch modifies the standard item repair action to control the **total number of times an
item can undergo repair**. This introduces a mechanic where an item's repairability is finite, adding a layer of
resource management and item degradation over its lifespan.

### XML Example

You can define the `RepairLimit` property on an item in `items.xml`. This property determines the maximum number of
times that a specific item can be repaired throughout its existence.

```xml

<item name="myRepairableTool">
    <property name="Extends" value="toolRepairKit"/>
    <property name="RepairLimit" value="10"/>
</item>
```

**Explanation**: When `myRepairableTool` is repaired, the `ItemActionRepairLimiter` patch would track the number of
repairs. Once the item has been repaired 10 times, it would no longer be repairable, even if repair kits are available.

---

The `GetExecuteActionTarget` patch adjusts how the game processes melee weapon attacks, specifically focusing on
determining if an attack has missed its intended target.

* **Patch Target**: `ItemActionMelee.GetExecuteActionTarget`
* **Purpose**: This patch checks if a melee action, particularly when targeting zombies, results in a miss. If a miss is
  detected, it triggers specific MinEvents: `onSelfPrimaryActionEnd` and `onSelfPrimaryActionMissEntity`.

---

The `DurabilityAffectsDamage` feature modifies the damage output of melee weapons, causing it to decrease as the
weapon's durability wears down. This introduces a realistic degradation mechanic and encourages players to maintain
their tools and weapons.

## Functionality

When enabled, this feature directly influences the damage calculation for melee weapons. As a weapon's durability drops
from its maximum, its damage output will be proportionally reduced. This means a heavily damaged weapon will perform
less effectively than a fully repaired one.

## Configuration

This feature is controlled by a setting within the `AdvancedItemFeatures` class in your `blocks.xml` file:

```xml

<property class="AdvancedItemFeatures">
    <property name="DurabilityAffectsDamage" value="false"/>
</property>
```

**Explanation**:

* **`DurabilityAffectsDamage`**: Setting this property's `value` to `true` will activate the feature, causing melee
  weapon damage to be affected by their current durability. If set to `false`, weapon damage remains constant regardless
  of durability (unless other mods alter this).

---

The `ScrapItems` property, part of the Advanced Item Repair feature, allows modders to define custom scrapping recipes
for individual items. This provides granular control over what resources are yielded when an item is scrapped, moving
beyond the game's default scrapping mechanics.

## Functionality

The `ScrapItems` property, part of the Advanced Item Repair feature, allows modders to define custom scrapping recipes
for individual items. This provides granular control over what resources are yielded when an item is scrapped, moving
beyond the game's default scrapping mechanics.

When the `AdvancedItemRepair` feature is enabled (in `blocks.xml` under `AdvancedItemFeatures`), the `ScrapItems`
property can be added to an item's definition. If `ScrapItems` is *not* defined for an item, but `AdvancedItemRepair` is
`true`, a default "reduced raw ingredients recipe" might be created unless `DisableScrapFallback` is set to `true`. By
explicitly defining `ScrapItems`, you gain full control over the output.

## XML Examples for `ScrapItems`

Here are two ways to define the `ScrapItems` property for an item:

### 1\. Simple Value Format

This format uses a single `value` attribute to define the output items and their quantities, separated by commas.

```xml

<property name="ScrapItems" value="resourceWood,0,resourceLeather,2"/>
```

**Explanation**:

* **`value="resourceWood,0,resourceLeather,2"`**: This string defines the items and their counts that are yielded when
  the item is scrapped. The format `itemName,count` is repeated.
    * `resourceWood,0`: Yields 0 units of `resourceWood`.
    * `resourceLeather,2`: Yields 2 units of `resourceLeather`.

### 2\. Class Property Format

This format uses a nested `<property Class="ScrapItems">` structure, allowing for more detailed definitions or
potentially more organized lists of scrap outputs.

```xml
<property Class="ScrapItems">
    <property name="resourceFeather" value="1"/>
</property>
```

**Explanation**:

* **`<property Class="ScrapItems">`**: This indicates a collection of properties related to scrapping.
* **`<property name="resourceFeather" value="1"/>`**: Defines a single output item. The `name` attribute is the item ID,
  and the `value` attribute is the quantity yielded.

## Interaction with `AdvancedItemFeatures`

This property works in conjunction with the following settings in `blocks.xml`:

```xml

<property class="AdvancedItemFeatures">
    <property name="AdvancedItemRepair" value="false"/>
    <property name="DisableScrapFallback" value="false"/>
</property>
```

* Set `AdvancedItemRepair` to `true` to enable the custom `ScrapItems` property.
* Set `DisableScrapFallback` to `true` if you only want items with explicitly defined `ScrapItems` (or `RepairItems`) to
  be scrapable, preventing the game from generating a default reduced-ingredient scrap recipe.

---

The `RepairItems` property, part of the Advanced Item Repair feature, allows modders to define custom repair recipes for
individual items. This provides granular control over what resources are required to repair an item, moving beyond the
game's default repair mechanics.

## Functionality

When the `AdvancedItemRepair` feature is enabled (in `blocks.xml` under `AdvancedItemFeatures`), the `RepairItems`
property can be added to an item's definition. If `RepairItems` is *not* defined for an item, but `AdvancedItemRepair`
is `true`, a default "reduced raw ingredients recipe" might be created for repair unless `DisableScrapFallback` is set
to `true`. By explicitly defining `RepairItems`, you gain full control over the required repair components.

## XML Example

While a direct example of `RepairItems` within an item's XML definition is not provided in the uploaded files,
conceptually, it would be added as a property to an `<item>` entry in `items.xml`. The value would typically be a
comma-separated list of item-count pairs that the item requires for repair.

```xml

<item name="myCustomRepairableArmor">
    <property name="Extends" value="armorIronBoots"/>
    <property name="CustomItemProperty" value="someValue"/>

    <property name="RepairItems" value="resourceWood,10,resourceForgedIron,10"/>

    <property Class="RepairItems">
        <property name="resourceFeather" value="2"/>
    </property>
</item>
```

**Explanation**:

* **`<property name="RepairItems" value="resourceWood,10,resourceForgedIron,10"/>`**: This format uses a direct `value`
  attribute. The `value` contains item-count pairs separated by commas. For instance, `resourceWood,10` means 10 units
  of `resourceWood` are needed, and `resourceForgedIron,10` means 10 units of `resourceForgedIron` are needed.
* **`<property Class="RepairItems"> <property name="resourceFeather" value="2"/> </property>`**: This is an alternative,
  more structured format using a nested property class. Here, 2 units of `resourceFeather` would be required for repair.

## Interaction with `AdvancedItemFeatures`

This property works in conjunction with the following setting in `blocks.xml`:

```xml

<property class="AdvancedItemFeatures">
    <property name="AdvancedItemRepair" value="false"/>
    <property name="DisableScrapFallback" value="false"/>
</property>
```

* Set `AdvancedItemRepair` to `true` to enable the custom `RepairItems` property.
* Set `DisableScrapFallback` to `true` if you only want items with explicitly defined `RepairItems` (or `ScrapItems`) to
  be repairable/scrappable, preventing the game from generating a default reduced-ingredient repair/scrap recipe.