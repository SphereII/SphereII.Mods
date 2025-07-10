# Food Spoilage V2 Configuration

This documentation details the configuration properties for the Food Spoilage V2 system, as defined in the `blocks.xml`
file. These settings control how food items spoil in the game, including spoilage rates in different inventory locations
and the resulting spoiled item.

Note on Item Expiration Beyond Food
While typically used for food, this spoilage system can be creatively applied to any item in the game to implement a
slow expiration or degradation mechanic. By setting the Spoilable property to true and configuring the other spoilage
properties on non-food items, you can define their decay rate and what they turn into upon "spoiling," offering
versatile gameplay possibilities.

## XML Configuration Example

The following XML snippet shows the default configuration for the Food Spoilage system found within the
`ConfigFeatureBlock` in `blocks.xml`:

```xml

<property class="FoodSpoilage">
    <property name="Logging" value="false"/>
    <property name="FoodSpoilage" value="false"/>
    <property name="UseAlternateItemValue" value="false"/>
    <property name="Toolbelt" value="6"/>
    <property name="Backpack" value="5"/>
    <property name="Container" value="4"/>
    <property name="MinimumSpoilage" value="1"/>
    <property name="TickPerLoss" value="10"/>
    <property name="SpoiledItem" value="foodRottingFlesh"/>
    <property name="FullStackSpoil" value="false"/>
</property>
```

## Configuration Properties

Here is a detailed explanation of each property within the `FoodSpoilage` class:

* **`Logging`**
    * **Value**: `false`
    * **Description**: A boolean flag to enable or disable verbose logging for the Food Spoilage system. When set to
      `true`, more detailed messages related to food spoilage will appear in the game logs.
* **`FoodSpoilage`**
    * **Value**: `false`
    * **Description**: This is the main switch to globally enable or disable the entire food spoilage feature. If set to
      `false`, food items will not spoil, regardless of other settings.
* **`UseAlternateItemValue`**
    * **Value**: `false`
    * **Description**: If set to `true`, when an item spoils, it will use an alternative item value. This can be used to
      define different durability or properties for the spoiled item compared to its fresh state.
* **`Toolbelt`**
    * **Value**: `6`
    * **Description**: This integer value represents a penalty applied to the spoilage rate for items stored in the
      player's toolbelt. A higher value indicates a faster spoilage rate in the toolbelt.
* **`Backpack`**
    * **Value**: `5`
    * **Description**: Similar to `Toolbelt`, this integer value applies a penalty to the spoilage rate for items stored
      in the player's backpack. A higher value means faster spoilage in the backpack.
* **`Container`**
    * **Value**: `4`
    * **Description**: This integer value applies a penalty to the spoilage rate for items stored in various
      containers (e.g., chests, refrigerators, storage boxes). A higher value indicates faster spoilage within
      containers.
* **`MinimumSpoilage`**
    * **Value**: `1`
    * **Description**: Defines the absolute minimum spoilage that occurs per tick. This ensures that items will always
      have a slight chance to spoil, even under ideal storage conditions.
* **`TickPerLoss`**
    * **Value**: `10`
    * **Description**: A global setting that determines the frequency of spoilage checks. This integer value specifies
      how many game ticks must pass before the spoilage amount increases by one point.
* **`SpoiledItem`**
    * **Value**: `foodRottingFlesh`
    * **Description**: This string defines the item ID or name that a food item will transform into once it has fully
      spoiled. For example, fresh meat could turn into `foodRottingFlesh`.
* **`FullStackSpoil`**
    * **Value**: `false`
    * **Description**: A boolean flag. If set to `true`, when any single item within a stack spoils, the entire stack of
      that item will spoil simultaneously. If `false`, items spoil individually within the stack.

To enable food spoilage for individual items, you need to define specific properties within the `<item>` entry in your
`items.xml` file.

Here's an example taken from the provided `items.xml`, which shows the properties needed to turn on food spoilage for a
specific item:

```xml

<item name="foodSpoilageTest">
    <property name="Extends" value="foodShamSandwich"/>
    <property name="DisplayType" value="melee"/>
    <property name="Tags" value="perkMasterChef"/>

    <property name="Spoilable" value="true"/>
    <property name="SpoiledItem" value="foodRottingFlesh"/>
    <property name="TickPerLoss" value="500"/>

    <property name="ShowQuality" value="false"/>
    <property name="SpoilageMax" value="1000"/>
    <property name="SpoilagePerTick" value="1"/>
</item>
```

Based on this example, the following XML properties are used to configure spoilage for an individual item:

* **`<property name="Spoilable" value="true" />`**

    * Sets whether the item is capable of spoiling. You must set this to `true` to enable spoilage for the item.

* **`<property name="SpoiledItem" value="foodRottingFlesh" />`**

    * Defines the item that this food item will transform into once it has fully spoiled.
    * SpoiledItem set to "None" will produce nothing when spoiled.

* **`<property name="TickPerLoss" value="500" />`**

    * Determines how many game ticks must pass before the item's spoilage level increases by one.

* **`<property name="SpoilageMax" value="1000" />`**

    * Specifies the maximum spoilage value an item can accumulate before it spoils completely and transforms into the
      `SpoiledItem`.

* **`<property name="SpoilagePerTick" value="1" />`**

    * Indicates how much spoilage points the item gains per `TickPerLoss` interval.