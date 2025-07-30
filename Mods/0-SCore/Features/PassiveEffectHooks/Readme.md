
-----

### FireEvent Tracker Debugging Command

This powerful debugging tool allows you to monitor all `FireEvents` being executed by a specific entity in real-time. It is designed for troubleshooting and to help modders identify which events are firing and when.

#### How to Use

The tracker is controlled via console commands. Follow these steps to enable it:

1.  **Find the Entity ID**: First, you need the unique ID of the entity you wish to monitor. You can get this by using the `le` (list entities) command in the console.

2.  **Enable the Tracker**: Once you have the ID, run the following `setcvar` command, replacing `<entityId>` with your target's ID:

    ```
    setcvar $fireeventtracker <entityId>
    ```

3.  **Monitor the Log**: After the command is executed, a Harmony patch is activated that logs every `FireEvent` from the specified entity. You can view this output in the game's log file.

#### How to Disable

To turn the tracker off, run the following command in the console:

```
setcvar $fireeventtracker 0
```

#### Important Notes

* **Persistence**: This setting will persist even after you restart the game. You must manually disable it once you are finished with your troubleshooting.

-----

## New Custom Triggers for Learn by Doing (SCore)

To support more immersive "Learn by Doing" progression systems, new custom triggers have been added. These hooks allow effects to be triggered by a wider variety of specific player actions.

Below is a list of the available triggers, their functions, and examples of their use.

-----

#### **onSelfLockpickSuccess**

Fires when the player successfully picks a lock. This trigger is also compatible with the "Locks" modlet.

* **Example:**
  ```xml
  <triggered_effect trigger="onSelfLockpickSuccess" action="LogMessage" message="Lock Pick successful"/>
  ```

-----

#### **onSelfItemBought**

Fires when the player purchases an item from a trader or vending machine. This trigger exposes two cvars: **`_item_value`** holds the value of the current transaction, and **`_last_item_value`** holds the value of the previous transaction.

* **Example:**
  ```xml
  <triggered_effect trigger="onSelfItemBought" action="LogMessage" message="Bought Something"/>
  ```

-----

#### **onSelfItemSold**

Fires when the player sells an item to a trader or vending machine. This trigger exposes two cvars: **`_item_value`** holds the value of the current transaction, and **`_last_item_value`** holds the value of the previous transaction.

* **Example:**
  ```xml
  <triggered_effect trigger="onSelfItemSold" action="LogMessage" message="Sold Something"/>
  ```

-----

#### **onSelfQuestComplete**

Fires when the player turns in a quest and it is marked as complete.

* **Example:**
  ```xml
  <triggered_effect trigger="onSelfQuestComplete" action="LogMessage" message="Quest Complete"/>
  ```

-----

#### **onSelfItemCrafted**

Fires each time an item finishes its crafting process in the queue. For items crafted in a workstation, this event will fire for each completed item when the player next opens that workstation's interface.

* **Example:**
  ```xml
  <triggered_effect trigger="onSelfItemCrafted" action="LogMessage" message="Item Was Crafted"/>
  ```

-----

#### **onSelfCraftedRecipe**

This trigger is specifically for use with workstations. It fires while the workstation's interface is open and is primarily used to pass the station's `CraftArea` name to requirements like `RequirementRecipeCraftArea`.

* **Example:**
  ```xml
  <triggered_effect trigger="onSelfCraftedRecipe" action="LogMessage" message="Recipe Was Crafted"/>
  ```

-----

#### **onSelfItemRepaired**

Fires when the player successfully repairs an item. This trigger also provides metadata about the item's state before the repair: **`DamageAmount`** (the total damage repaired) and **`PercentDamaged`** (the percentage of use the item had).

* **Example:**
  ```xml
  <triggered_effect trigger="onSelfItemRepaired" action="LogMessage" message="Item Was Repaired"/>
  ```

-----

#### **onSelfScrapItem**

Fires when the player scraps an item.

* **Example:**
  ```xml
  <triggered_effect trigger="onSelfScrapItem" action="LogMessage" message="LBD DEBUG: Scrapped a tool">
     <requirement name="ItemHasTags" tags="miningTool,toolAxe,toolShovel"/>
  </triggered_effect>
  ```

-----

## Custom Requirements for Buffs and Actions

The following requirements can be used with the custom triggers, as well as with standard game events, to create conditional logic for your mod.

### Summary of Requirements

* **Item-Based:**
    * `RequirementHoldingItemDurability`: Checks the durability of the held item.
    * `ItemHasProperty`: Checks if an item has a specific property and optional value.
    * `ItemHasQuality`: Checks the quality of an item.
    * `ItemPercentDamaged`: Checks the damage percentage of an item.
* **Block-Based:**
    * `RequirementIsTargetBlock`: Checks if the target block has specific tags.
    * `RequirementBlockHasHarvestTags`: Checks if a block's harvest drops have specific tags.
    * `BlockHasDestroyName`: Checks the name of a block being destroyed.
    * `BlockHasName`: Checks the name of the block being interacted with.
* **Recipe-Based:**
    * `RequirementRecipeCraftArea`: Checks if a recipe is crafted at a specific workstation.
    * `RequirementRecipeHasLongCraftTime`: Checks if a recipe's craft time meets a condition.
    * `RequirementRecipeHasTags`: Checks if a recipe has specific tags.
    * `RecipeHasIngredients`: Checks if a recipe contains specific ingredients.

-----

### `RequirementHoldingItemDurability`

Checks the durability of the item the entity is holding.

```xml
<requirement name="HoldingItemDurability, SCore" target="self" min_durability="0.5" max_durability="0.9"/>
```

**Explanation**: Requires the held item's durability to be between 50% and 90%.

### `RequirementIsTargetBlock`

Checks if the block being interacted with possesses specific tags.

```xml
<requirement name="RequirementIsTargetBlock, SCore" tags="wood,stone" has_all_tags="false"/>
```

**Explanation**: Requires the target block to have either the "wood" or "stone" tag. Set `has_all_tags` to `true` to require all listed tags.

### `RequirementBlockHasHarvestTags`

Checks if a block's harvestable items possess any of a set of tags.

```xml
<requirement name="RequirementBlockHasHarvestTags, SCore" tags="salvageHarvest"/>
```

**Explanation**: Requires the target block to drop at least one item during harvest that has the "salvageHarvest" tag.

### `ItemHasProperty`

Checks if the item that triggered the action has a specific property.

```xml
<requirement name="ItemHasProperty, SCore" property="UnlockedBy" prop_value="craftingHarvestingTools" />

<requirement name="ItemHasProperty, SCore" property="UnlockedBy" />
```

**Explanation**:

* `name`: `ItemHasProperty, SCore`
* `property`: The name of the property to check for.
* `prop_value`: (Optional) The specific value the property must have.

### `ItemHasQuality`

Checks the quality level of the item that triggered the action.

```xml
<requirement name="ItemHasQuality, SCore" operation="Equals" value="4"/>
```

**Explanation**:

* `name`: `ItemHasQuality, SCore`
* `operation`: The comparison operator (e.g., `Equals`, `GTE`).
* `value`: The quality level to compare against.

### `ItemPercentDamaged`

Checks the percentage of damage or usage of the item that triggered the action.

```xml
<requirement name="ItemPercentDamaged, SCore" operation="GTE" value="0.5"/>
```

**Explanation**:

* `name`: `ItemPercentDamaged, SCore`
* `operation`: The comparison operator (e.g., `GTE`).
* `value`: The damage percentage as a decimal (0.0 to 1.0).

### `BlockHasDestroyName`

Checks the name of a block that is being destroyed. Supports wildcards.

```xml
<requirement name="BlockHasDestroyName, SCore" block_name="plantedGraceCorn1" />

<requirement name="BlockHasDestroyName, SCore" block_name="planted*,blockTree*" />
```

**Explanation**:

* `name`: `BlockHasDestroyName, SCore`
* `block_name`: A comma-delimited list of block names to check for. `*` can be used as a wildcard.

### `BlockHasName`

Checks the name of the active block being interacted with. Supports wildcards.

```xml
<requirement name="BlockHasName, SCore" block_name="DewCollector*,Workbench*,ChemistryStation*,CementMixer*" />
```

**Explanation**:

* `name`: `BlockHasName, SCore`
* `block_name`: A comma-delimited list of block names to check for. `*` can be used as a wildcard.

### `RequirementRecipeCraftArea`

Checks if a recipe is crafted at one of the specified workstations.

```xml
<requirement name="RequirementRecipeCraftArea, SCore" craft_area="forge,workbench"/>
```

**Explanation**: Requires the recipe to be crafted in either the "forge" or the "workbench". The `craft_area` parameter accepts a comma-separated list of valid workstation names.

### `RequirementRecipeHasLongCraftTime`

Checks if a recipe's base crafting time meets a certain condition.

```xml
<requirement name="RequirementRecipeHasLongCraftTime, SCore" operation="GTE" value="60"/>
```

**Explanation**: Requires the recipe's crafting time to be Greater Than or Equal To 60 seconds. The `operation` can be `LT`, `LTE`, `GT`, `GTE`, or `E`.

### `RequirementRecipeHasTags`

Checks if a recipe has at least one of the specified tags.

```xml
<requirement name="RequirementRecipeHasTags, SCore" tags="perkGreaseMonkey,tool"/>
```

**Explanation**: Requires the recipe to have either the "perkGreaseMonkey" or the "tool" tag. The `tags` parameter accepts a comma-separated list.

### `RecipeHasIngredients`

Checks if the ingredients of a recipe match a given list of item names. Supports wildcards.

```xml
<requirement name="RecipeHasIngredients, SCore" ingredients="planted*" />

<requirement name="RecipeHasIngredients, SCore" ingredients="plantedCotton1,plantedCoffee*" />
```

**Explanation**:

* `name`: `RecipeHasIngredients, SCore`
* `ingredients`: A comma-delimited list of item names to check for in the recipe's ingredients. `*` can be used as a wildcard.