
# Buff and Action Requirements

Requirements are conditions that must be met for certain actions to occur, such as a buff being applied, a triggered effect activating, or a game event firing. This mod introduces several custom requirements that allow for more dynamic and context-aware behaviors in the game.

### Summary of Requirements

* **Entity & Environment State:**
  * `TargetCVarCompare`: Checks a CVar on the target entity.
  * `RequirementSameFactionSDX`: Checks if the entity is in the same faction as another.
  * `RequirementOnSpecificBiomeSDX`: Checks if the entity is in a specific biome.
  * `RequirementLookingAt`: Checks what the entity is looking at.
  * `RequirementIsUnderground`: Checks if the entity is underground.
  * `RequirementIsOutdoor`: Checks if the entity is outdoors.
  * `RequirementIsNearFire`: Checks if the entity is near a fire source.
  * `RequirementIsBloodMoon`: Checks if it is a Blood Moon.
  * `FactionRelationshipValue`: Checks the relationship value with a faction.
* **Time-Based:**
  * `RequirementEveryXHourSDX`: Triggers every X in-game hours.
  * `RequirementEveryXDaySDX`: Triggers every X in-game days.
  * `RequirementEveryDawn`: Triggers at dawn.
* **Player & Progression:**
  * `RequirementQuestObjective`: Checks for active quest objectives.
  * `RequirementIsProgressionLocked`: Checks if a perk, skill, or attribute is locked.
  * `CanPurchasePerk` : Checks if a perk, skill, or attribute is able to be purchased, which means all requirements are available.
`
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

### 1\. `TargetCVarCompare`

Checks if a CVar on the target entity meets a comparison condition.

```xml
<requirement name="TargetCVarCompare, SCore" target="self" cvar="myTargetCVar" value="10" operator="GTE"/>
```

**Explanation**: Requires `myTargetCVar` on the target to be Greater Than or Equal to 10. `operator` can be `LT`, `LTE`, `GT`, `GTE`, `EQ`, `NEQ`.

### 2\. `RequirementSameFactionSDX`

Checks if the entity is in the same faction as a specified other entity.

```xml
<requirement name="SameFactionSDX, SCore" target="self" other_entity="player"/>
```

**Explanation**: Requires the entity to be in the same faction as the player.

### 3\. `RequirementOnSpecificBiomeSDX`

Checks if the entity is in a specific biome.

```xml
<requirement name="OnSpecificBiomeSDX, SCore" target="self" biome="pine_forest"/>
```

**Explanation**: Requires the entity to be in the "pine\_forest" biome.

### 4\. `RequirementLookingAt`

Checks if the entity is looking at another entity or block.

```xml
<requirement name="LookingAt, SCore" target="self" look_at_entity="player"/>
```

**Explanation**: Requires the entity to be looking at the player.

### 5\. `RequirementIsUnderground`

Checks if the entity is underground.

```xml
<requirement name="IsUnderground, SCore" target="self"/>
```

**Explanation**: Requires the entity to be currently underground.

### 6\. `RequirementIsOutdoor`

Checks if the entity is outdoors.

```xml
<requirement name="IsOutdoor, SCore" target="self"/>
```

**Explanation**: Requires the entity to be currently outdoors (not under a roof).

### 7\. `RequirementIsNearFire`

Checks if the entity is near fire.

```xml
<requirement name="IsNearFire, SCore" target="self" range="5"/>
```

**Explanation**: Requires the entity to be within 5 blocks of an active fire.

### 8\. `RequirementIsBloodMoon`

Checks if it is currently a Blood Moon night.

```xml
<requirement name="IsBloodMoon, SCore"/>
```

**Explanation**: Requires the current game day to be a Blood Moon day.

### 9\. `RequirementHoldingItemDurability`

Checks the durability of the item the entity is holding.

```xml
<requirement name="HoldingItemDurability, SCore" target="self" min_durability="0.5" max_durability="0.9"/>
```

**Explanation**: Requires the held item's durability to be between 50% and 90%.

### 10\. `RequirementEveryXHourSDX`

Triggers every X in-game hours.

```xml
<requirement name="EveryXHourSDX, SCore" hours="6"/>
```

**Explanation**: Activates every 6 in-game hours.

### 11\. `RequirementEveryXDaySDX`

Triggers every X in-game days.

```xml
<requirement name="EveryXDaySDX, SCore" days="7"/>
```

**Explanation**: Activates every 7 in-game days.

### 12\. `RequirementEveryDawn`

Triggers every dawn.

```xml
<requirement name="EveryDawn, SCore"/>
```

**Explanation**: Activates at the start of every in-game dawn.

### 13\. `FactionRelationshipValue`

Checks the relationship value with a specific faction.

```xml
<requirement name="FactionRelationshipValue, SCore" target="self" faction="bandits" value="500" operator="GTE"/>
```

**Explanation**: Requires the entity's relationship with the "bandits" faction to be Greater Than or Equal to 500. `operator` can be `LT`, `LTE`, `GT`, `GTE`, `EQ`, `NEQ`.

### 14\. `RequirementQuestObjective`

Checks all in-progress quests for specific in-progress objectives.

```xml
<requirement name="RequirementQuestObjective, SCore" objective="TreasureChest" id="cntBuriedLootStashChest"/>
```

**Explanation**: This requirement will only return `true` if the player has an active `TreasureChest` quest for the block `cntBuriedLootStashChest` and is currently within the treasure's radius. It can also check for other objective types by `id` and `objective` type.

### 15\. `RequirementIsTargetBlock`

Checks if the block being interacted with possesses specific tags.

```xml
<requirement name="RequirementIsTargetBlock, SCore" tags="wood,stone" has_all_tags="false"/>
```

**Explanation**: Requires the target block to have either the "wood" or "stone" tag. Set `has_all_tags` to `true` to require all listed tags.

### 16\. `RequirementBlockHasHarvestTags`

Checks if a block's harvestable items possess any of a set of tags.

```xml
<requirement name="RequirementBlockHasHarvestTags, SCore" tags="salvageHarvest"/>
```

**Explanation**: Requires the target block to drop at least one item during harvest that has the "salvageHarvest" tag.

### 17\. `CanPurchasePerk`

Checks whether a specified progression (such as an attribute, perk, or skill) is currently locked for the player.

```xml
<requirement name="!CanPurchasePerk, SCore" progression_name="attPerception"/>
```

**Explanation**: This example returns `true` if the "attPerception" attribute is *unlocked*. The `!` prefix is shorthand for `invert="true"`.

### 18\. `ItemHasProperty`

Checks if the item that triggered the action has a specific property.

```xml
<requirement name="ItemHasProperty, SCore" property="UnlockedBy" prop_value="craftingHarvestingTools" />

<requirement name="ItemHasProperty, SCore" property="UnlockedBy" />
```

**Explanation**:

* `name`: `ItemHasProperty, SCore`
* `property`: The name of the property to check for.
* `prop_value`: (Optional) The specific value the property must have.

### 19\. `ItemHasQuality`

Checks the quality level of the item that triggered the action.

```xml
<requirement name="ItemHasQuality, SCore" operation="Equals" value="4"/>
```

**Explanation**:

* `name`: `ItemHasQuality, SCore`
* `operation`: The comparison operator (e.g., `Equals`, `GTE`).
* `value`: The quality level to compare against.

### 20\. `ItemPercentDamaged`

Checks the percentage of damage or usage of the item that triggered the action.

```xml
<requirement name="ItemPercentDamaged, SCore" operation="GTE" value="0.5"/>
```

**Explanation**:

* `name`: `ItemPercentDamaged, SCore`
* `operation`: The comparison operator (e.g., `GTE`).
* `value`: The damage percentage as a decimal (0.0 to 1.0).

### 21\. `BlockHasDestroyName`

Checks the name of a block that is being destroyed. Supports wildcards.

```xml
<requirement name="BlockHasDestroyName, SCore" block_name="plantedGraceCorn1" />

<requirement name="BlockHasDestroyName, SCore" block_name="planted*,blockTree*" />
```

**Explanation**:

* `name`: `BlockHasDestroyName, SCore`
* `block_name`: A comma-delimited list of block names to check for. `*` can be used as a wildcard.

### 22\. `BlockHasName`

Checks the name of the active block being interacted with. Supports wildcards.

```xml
<requirement name="BlockHasName, SCore" block_name="DewCollector*,Workbench*,ChemistryStation*,CementMixer*" />
```

**Explanation**:

* `name`: `BlockHasName, SCore`
* `block_name`: A comma-delimited list of block names to check for. `*` can be used as a wildcard.

### 23\. `RequirementRecipeCraftArea`

Checks if a recipe is crafted at one of the specified workstations.

```xml
<requirement name="RequirementRecipeCraftArea, SCore" craft_area="forge,workbench"/>
```

**Explanation**: Requires the recipe to be crafted in either the "forge" or the "workbench". The `craft_area` parameter accepts a comma-separated list of valid workstation names.

### 24\. `RequirementRecipeHasLongCraftTime`

Checks if a recipe's base crafting time meets a certain condition.

```xml
<requirement name="RequirementRecipeHasLongCraftTime, SCore" operation="GTE" value="60"/>
```

**Explanation**: Requires the recipe's crafting time to be Greater Than or Equal To 60 seconds. The `operation` can be `LT`, `LTE`, `GT`, `GTE`, or `E`.

### 25\. `RequirementRecipeHasTags`

Checks if a recipe has at least one of the specified tags.

```xml
<requirement name="RequirementRecipeHasTags, SCore" tags="perkGreaseMonkey,tool"/>
```

**Explanation**: Requires the recipe to have either the "perkGreaseMonkey" or the "tool" tag. The `tags` parameter accepts a comma-separated list.

### 26\. `RecipeHasIngredients`

Checks if the ingredients of a recipe match a given list of item names. Supports wildcards.

```xml
<requirement name="RecipeHasIngredients, SCore" ingredients="planted*" />

<requirement name="RecipeHasIngredients, SCore" ingredients="plantedCotton1,plantedCoffee*" />
```

**Explanation**:

* `name`: `RecipeHasIngredients, SCore`
* `ingredients`: A comma-delimited list of item names to check for in the recipe's ingredients. `*` can be used as a wildcard.