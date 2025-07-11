The "Food Spoilage and Preserved Goods" modlet, created by khzmusik, significantly enhances the game's realism by
introducing a comprehensive food spoilage system. This system is deeply integrated with and leverages the custom food
spoilage features provided by the `0-SCore` modlet.

Here's a detailed breakdown of how this modlet implements and utilizes SCore's food spoilage system, acting as a guide
for modders looking to customize or expand upon these mechanics:

-----

## Food Spoilage System: A Modder's Guide (khzmusik's "Food Spoilage and Preserved Goods" Modlet)

Source: https://gitlab.com/karlgiesing/7d2d-2.0-mods/-/tree/11-update-food-spoilage-to-version-2-0/khzmusik_Food_Spoilage?ref_type=heads

This modlet introduces a realistic food spoilage system into the game, making most consumables degrade over time and
transform into spoiled versions. It also adds new preserved foods that are immune to spoilage and introduces craftable
powered refrigerators for better food storage. The core of this functionality relies on the food spoilage code within
the `0-SCore` modlet.

### 1\. Core Concepts of SCore's Food Spoilage

The spoilage system functions on a "loss calculation" basis. When a player accesses a container holding consumable
items, the game calculates how much time has passed since the last calculation and applies spoilage accordingly.

* **Loss Calculation**: This is the process where the game determines how many items in a stack have spoiled. It's
  triggered when a container with consumables is opened.
* **Ticks Per Loss (`TickPerLoss`)**: A configurable number of in-game ticks that must elapse before a spoilage
  calculation can occur. This can be set globally or per item.
* **Spoilage Per Tick (`SpoilagePerTick`)**: The base amount of spoilage accumulated per loss calculation. This is also
  configurable globally or per item.
* **Maximum Spoilage (`SpoilageMax`)**: The total spoilage amount an item can accumulate before one unit of that item
  spoils. Once this threshold is met, one item is removed from the stack, and a "spoiled item" is created.
* **Preserve Bonus (`PreserveBonus`)**: A property on containers that reduces the spoilage applied to items stored
  within them. A higher value means better preservation.
* **Spoiled Item (`SpoiledItem`)**: The specific item that a consumable transforms into once it spoils (e.g.,
  `foodRottingFlesh`, `resourceFertilizer`, `drinkJarRiverWater`).

### 2\. Global Food Spoilage Configuration

The overarching settings for the spoilage system are found in the `ConfigFeatureBlock` within `Config/blocks.xml`,
specifically under the `FoodSpoilage` property class.

* **Enabling the System**:
    * Set `FoodSpoilage` to `true` to activate the system.
    * **Example (`blocks.xml`)**:
      ```xml
      <set xpath="//block[@name='ConfigFeatureBlock']/property[@class='FoodSpoilage']/property[@name='FoodSpoilage']/@value">true</set>
      ```
* **Default Spoilage Rates**:
    * `TickPerLoss`: Global default for how many game ticks pass for a spoilage loss calculation. This modlet sets it to
      `200`.
    * **Example (`blocks.xml`)**:
      ```xml
      <set xpath="//block[@name='ConfigFeatureBlock']/property[@class='FoodSpoilage']/property[@name='TickPerLoss']/@value">200</set>
      ```
* **Logging (Optional)**:
    * `Logging`: Can be uncommented and set to `true` to enable verbose logging of spoilage calculations for debugging
      purposes.
* **Location-Based Spoilage (Configurable in SCore, not explicitly in provided `blocks.xml` for this modlet)**:
    * SCore's code also allows for different spoilage amounts to be added per loss calculation based on where the item
      stack is located: player's `Toolbelt`, `Backpack`, or a `Container`.
* **Experimental Feature (`UseAlternateItemValue`)**:
    * This feature aimed to prevent split item stacks from sharing spoilage. It's noted as experimental and commented
      out in your `blocks.xml` due to potential issues.

### 3\. Item-Specific Spoilage Properties

The core of this modlet's implementation lies in `Config/items.xml`, where spoilage properties are added to individual
consumable items.

* **Enabling Spoilage on an Item**:
    * **`Spoilable`**: Set to `true` on an item's `<property>` to make it spoil.
* **Spoilage Rates and Transformation**:
    * **`SpoiledItem`**: Specifies what the item turns into (e.g., `foodRottingFlesh`, `resourceFertilizer`,
      `drinkJarRiverWater`).
    * **`TickPerLoss`**: Item-specific ticks for a spoilage calculation.
    * **`SpoilageMax`**: The total spoilage points an item can accrue before spoiling (default `1000`).
    * **`SpoilagePerTick`**: Spoilage points added per loss calculation (default `1`).
* **Visual Representation**:
    * **`ShowQuality`**: Whether a quality bar is displayed for the item (often set to `false` for spoilable items in
      this modlet).
    * **`QualityTierColor`**: The color of the quality bar, by tier number (0-7).
* **Examples of Item Spoilage Configuration (`items.xml`)**:
    * **General Foods**: Most basic foods and prepared dishes are given `Spoilable="true"`,
      `SpoiledItem="foodRottingFlesh"`, `TickPerLoss="168"` (1 week), and `SpoilagePerTick="3"`.
      ```xml
      <append xpath="//item[starts-with(@name, 'food') and not(contains(@name, 'foodCan')) and not(contains(@name, 'Schematic')) and not(contains(@name, 'Magazine'))]">
          <property name="Spoilable" value="true" />
          <property name="ShowQuality" value="false" />
          <property name="QualityTierColor" value="1" />
          <property name="SpoiledItem" value="foodRottingFlesh" />
          <property name="TickPerLoss" value="168" />
          <property name="SpoilageMax" value="1000" />
          <property name="SpoilagePerTick" value="3" />
      </append>
      ```
    * **Crops**: Crops spoil into `resourceFertilizer`, a revived item in this modlet.
      ```xml
      <append xpath="//item[starts-with(@name, 'resourceCrop')]">
          <property name="Spoilable" value="true" />
          <property name="SpoiledItem" value="resourceFertilizer" />
          <property name="TickPerLoss" value="168" />
          <property name="SpoilageMax" value="1000" />
          <property name="SpoilagePerTick" value="3" />
      </append>
      ```
    * **Meats and Stews**: Raw and cooked meats have a lower `TickPerLoss` (`24` or `48` hours) and
      `SpoilagePerTick="1"`, making them spoil faster and be more affected by container bonuses. Stews are also
      configured similarly.
      ```xml
      <set xpath="//item[@name='foodRawMeat']/property[@name='TickPerLoss']/@value">24</set>
      <set xpath="//item[@name='foodRawMeat']/property[@name='SpoilagePerTick']/@value">1</set>
      <set xpath="//item[@name='foodBoiledMeat']/property[@name='TickPerLoss']/@value">48</set>
      <set xpath="//item[@name='foodBoiledMeat']/property[@name='SpoilagePerTick']/@value">1</set>
      ```
    * **Drinks**: Most drinks spoil into `drinkJarRiverWater` within a week.
      ```xml
      <append xpath="//item[@name='drinkJarYuccaJuice']">
          <property name="Spoilable" value="true" />
          <property name="SpoiledItem" value="drinkJarRiverWater" />
          <property name="TickPerLoss" value="168" />
          <property name="SpoilageMax" value="1000" />
          <property name="SpoilagePerTick" value="3" />
      </append>
      ```
    * **Items Explicitly Not Spoiling**: Canned foods, honey, already spoiled items (`foodRottingFlesh`), and certain
      raw crafting resources (coffee beans, cotton, hops) are set not to spoil by removing their `Spoilable` property.
      ```xml
      <remove xpath="//item[@name='foodCornMeal']/property[@name='Spoilable']" />
      <remove xpath="//item[@name='foodRottingFlesh']/property[@name='Spoilable']" />
      ```

### 4\. Container Preservation

Containers can have a `PreserveBonus` in `Config/blocks.xml` which reduces the spoilage applied to items stored inside
them.

* **Existing Containers**: Coolers, fridges, and freezers are given `PreserveBonus` values (e.g., `1` for coolers, `1.5`
  for retro fridges, `1.75` for stainless steel fridges and freezers).
    * **Example (`blocks.xml`)**:
      ```xml
      <append xpath="//block[starts-with(@name, 'cntCooler') and not(contains(@name, 'Helper')) and not(contains(@name, 'Open'))]">
          <property name="PreserveBonus" value="1" />
      </append>
      ```
* **New Powered Refrigerators**: The modlet introduces craftable powered versions of refrigerators and mini-coolers.
  These blocks have a high `PreserveBonus` of `4`, making them ideal for long-term food storage.
    * **Example (`blocks.xml`)**:
      ```xml
      <block name="cntRetroFridgePowered">
          <property name="Extends" value="cntRetroFridgeVer1Closed_Player" />
          <property name="PreserveBonus" value="4" />
          <property name="CreativeMode" value="Player" />
          <property name="UnlockedBy" value="craftingElectrician" />
          </block>
      ```
    * **Crafting (`recipes.xml`)**: Recipes for these powered refrigerators require an `appliancesVariantHelper`,
      `smallEngine`, and `carBattery`.
    * **Progression (`progression.xml`)**: These recipes are unlocked with the same perks and levels as vanilla battery
      banks.
    * **Localization (`Localization.txt`)**: Provides names like "Powered Retro Refrigerator" and "Powered Refrigerator,
      Stainless Steel".

### 5\. New Preserved Foods and Recipes

The modlet adds several new food items designed not to spoil, and updates existing recipes to incorporate these
preserved ingredients.

* **IPA (India Pale Ale)** (`drinkJarIPA`): A new beer that does not spoil and requires more hops to craft.
    * **Crafting (`recipes.xml`)**: Crafted at a `chemistryStation`.
    * **Progression (`progression.xml`)**: Unlocked with the vanilla beer recipe perk.
* **Canned Vegetables**: New non-spoiling canned versions of corn, mushrooms, potato, and pumpkin (`foodCanCorn`,
  `foodCanMushrooms`, `foodCanPotato`, `foodCanPumpkin`).
    * **Crafting (`recipes.xml`)**: Crafted at a `campfire` with a `toolCookingPot`.
    * **Progression (`progression.xml`)**: Unlocked with the same perk and level as blueberry pie.
* **Preserved Blueberries (`foodPreservedBlueberries`)**: A non-spoiling version of blueberries.
    * **Crafting (`recipes.xml`)**: Crafted at a `campfire` with a `toolCookingPot`.
    * **Progression (`progression.xml`)**: Unlocked with the same perk and level as blueberry pie.
* **Smoked/Cured Meat (`foodPreservedMeat`)**: A non-spoiling meat option.
    * **Crafting (`recipes.xml`)**: Crafted at a `campfire` with a `toolCookingPot`, using `foodRawMeat`,
      `resourceCoal`, and `resourcePotassiumNitratePowder`.
    * **Progression (`progression.xml`)**: Unlocked with the same perk and level as grilled meat.
* **"Shamway Fruit Pie" (`foodPreservedPie`)**: A loot-only, non-spoiling pie.
* **Updated Recipes**: Many existing recipes for stews, pies, and drinks have been updated in `Config/recipes.xml` to
  optionally use the new canned/preserved substitutes.
* **`resourceFertilizer`**: This item, which was previously removed in vanilla A20, is reintroduced as the spoiled
  product for crops. A new material `MresourceFertilizer` is defined for it.

### 6\. Loot and Trader Adjustments

The modlet modifies loot tables (`Config/loot.xml`) and trader inventories (`Config/traders.xml`) to integrate the new
spoilage system.

* **Loot Changes**: Spoilable foods are largely removed from loot groups and replaced with their canned/preserved
  equivalents. IPA replaces much of the regular beer in loot.
* **Trader Inventories**: Traders now sell the new canned/preserved foods and IPA.

### 7\. Loading Screen Tip

A new loading screen tip (`loadingTipFoodSpoilage`) is added in `Config/loadingscreen.xml` to inform players about the
food spoilage mechanics. Its full text is provided in `Config/Localization.txt`, explaining how food spoils and how
containers help.

### Important Technical Details and Warnings (from `README.md`)

* This modlet requires the **1.0 version of the `0-SCore` modlet or higher**.
* The `0-SCore` modlet uses C\# code and is **NOT compatible with EAC (Easy Anti-Cheat)**.
* This modlet must be installed on **both clients and servers**.
* **Crucially, you MUST start a new game after installing this modlet.** Loading an existing game world after
  installation will corrupt the save.