Buff Requirements are conditions that must be met for a buff to be applied, to persist on an entity, or for a buff's
triggered effects to activate. This mod introduces several custom buff requirements that allow for more dynamic and
context-aware buff behaviors.

Here's a summary of the custom Buff Requirements available:

* **`TargetCVarCompare.cs`**: This requirement checks if a specific CVar (Console Variable) on the target entity
  compares to a given value using a defined operator (e.g., greater than, less than, equals).
* **`RequirementSameFactionSDX.cs`**: This requirement checks if the entity being evaluated is in the same faction as
  another specified entity (e.g., the player or another NPC).
* **`RequirementOnSpecificBiomeSDX.cs`**: This requirement checks if the entity is currently located within a specified
  biome.
* **`RequirementLookingAt.cs`**: This requirement checks if the entity is currently looking at another specific entity
  or a type of entity.
* **`RequirementIsUnderground.cs`**: This requirement checks if the entity is currently located underground.
* **`RequirementIsOutdoor.cs`**: This requirement checks if the entity is currently located outdoors (not under a roof
  or inside a structure).
* **`RequirementIsNearFire.cs`**: This requirement checks if the entity is currently near an active fire.
* **`RequirementIsBloodMoon.cs`**: This requirement checks if the current game time is during a Blood Moon event.
* **`RequirementHoldingItemDurability.cs`**: This requirement checks the durability of the item the entity is currently
  holding, allowing for buffs to be conditional on weapon/tool wear.
* **`RequirementEveryXHourSDX.cs`**: This requirement checks if a specific number of in-game hours has passed, allowing
  for time-based buff application or removal.
* **`RequirementEveryXDaySDX.cs`**: This requirement checks if a specific number of in-game days has passed, similar to
  the hour-based requirement but on a daily cycle.
* **`RequirementEveryDawn.cs`**: This requirement checks if it is currently dawn in the game, allowing for buffs to be
  tied to the start of a new day.
* **`HoldingItemDurability.cs`**: This seems to be a helper class related to checking item durability, likely supporting
  requirements like `RequirementHoldingItemDurability.cs`.
* **`FactionRelationshipValue.cs`**: This requirement checks the relationship value between the entity and another
  faction, allowing buffs to be conditional on faction standing.

These custom buff requirements provide modders with granular control over when and how buffs affect entities, enabling
complex and dynamic gameplay scenarios.

Here are conceptual XML examples for each of the custom Buff Requirements, illustrating how they might be used within a
`<requirement>` tag in your `buffs.xml`.

**Note**: These are conceptual examples based on the C\# script's parsing logic and typical `7 Days to Die` modding
patterns. The exact context (e.g., within a specific `effect_group` and `buff` definition) is omitted for brevity.

### 1\. `TargetCVarCompare`

Checks if a CVar on the target entity meets a comparison condition.

```xml

<requirement name="TargetCVarCompare, SCore" target="self" cvar="myTargetCVar" value="10" operator="GTE"/>
```

**Explanation**: Requires `myTargetCVar` on the target to be Greater Than or Equal to 10. `operator` can be `LT`, `LTE`,
`GT`, `GTE`, `EQ`, `NEQ`.

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

**Explanation**: Requires the entity's relationship with the "bandits" faction to be Greater Than or Equal to 500.
`operator` can be `LT`, `LTE`, `GT`, `GTE`, `EQ`, `NEQ`.

### 14\. `RequirementQuestObjective, SCore`

This requirement checks all in-progress quests for specific in-progress objectives, allowing modders to create
conditional logic based on a player's active quest progression [cite: user\_provided\_code].

```xml

<requirement name="RequirementQuestObjective, SCore" id="cntBuriedLootStashChest"/>
<requirement name="RequirementQuestObjective, SCore" objective="TreasureChest"/>
<requirement name="RequirementQuestObjective, SCore" objective="ObjectiveTreasureChest"/>
```

**Explanation**: This requirement takes the following attributes:

* **`name="RequirementQuestObjective, SCore"`**: The name of the requirement [cite: user\_provided\_code].
* **`id`**: (Optional) Specifies the ID of the objective to check. The interpretation of this `id` depends on the
  objective's type [cite: user\_provided\_code]:
    * For `TreasureChest` objectives, the `id` refers to the **block name** (e.g.,
      `cntBuriedLootStashChest`) [cite: user\_provided\_code].
    * For `EntityKill` objectives, it would be the **entity name(s)**.
    * For `Goto` objectives, it would be the **location name**.
    * The comparison is case-insensitive [cite: user\_provided\_code].
* **`objective`**: (Optional) Specifies the type of objective to check. This can be the full class name (e.g.,
  `ObjectiveTreasureChest`) or a shorthand (e.g., `TreasureChest`), as the code automatically prepends "Objective" if
  missing and converts to lowercase for comparison [cite: user\_provided\_code].
* **`invert`**: (Optional, inherited) A boolean value. If `true`, the requirement evaluates to `true` if *no* matching
  in-progress quest objective is found [cite: user\_provided\_code].

**Special Behavior for `TreasureChest` Objectives:**
When checking for an `objective="TreasureChest"` (or `ObjectiveTreasureChest`), an additional radius check is
performed [cite: user\_provided\_code]. The requirement will only return `true` if the player is currently **within
the `CurrentRadius`** of the active `TreasureChest` objective's location [cite: user\_provided\_code]. This ensures that
the condition is met only when the player is actively near the quest's treasure.

### 15\. `RequirementIsTargetBlock, SCore`

This requirement checks if the `BlockValue` associated with the current `MinEventParams` (i.e., the block at the event's
target location) possesses specific tags. It allows for conditional logic based on the characteristics of the block that
an event is interacting with.

```xml

<requirement name="RequirementIsTargetBlock, SCore" tags="wood,stone" has_all_tags="false" invert="false"/>
```

**Explanation**: This requirement takes the following attributes:

* **`name="RequirementIsTargetBlock, SCore"`**: The name of the requirement.
* **`tags`**: (Required) A comma-separated string of `FastTags` that the target block will be checked against. The block
  must possess these tags.
* **`has_all_tags`**: (Optional) A boolean value. If `true`, the target block must have *all* of the tags specified in
  the `tags` attribute. If `false` (default behavior), the block only needs to have *any* of the specified tags.
* **`invert`**: (Optional, inherited) A boolean value. If `true`, the requirement is inverted, meaning it evaluates to
  `true` if the target block *does not* meet the specified tag conditions.

**Example Usage:**

You could use this requirement in an `effect_group` within an item or buff, or in a `response_entry` in a dialog, to
ensure an action only triggers when a player interacts with a block that has certain properties:

```xml

<effect_group>
    <triggered_effect trigger="onSelfPrimaryActionEnd" action="PlaySound" sound="player_pickup">
        <requirement name="RequirementIsTargetBlock, SCore" tags="wood,stone" has_all_tags="false"/>
    </triggered_effect>

    <triggered_effect trigger="onBlockDamaged">
        <requirement name="RequirementIsTargetBlock, SCore" tags="electric,trap" has_all_tags="true"/>
    </triggered_effect>

    <triggered_effect trigger="onBlockPlaced">
        <requirement name="RequirementIsTargetBlock, SCore" tags="decorative,furniture" has_all_tags="false"
                     invert="true"/>
    </triggered_effect>
</effect_group>
```

### 16\. `RequirementBlockHasHarvestTags, SCore`

This requirement checks if a target block, when harvested, is configured to drop items that possess any of a specified
set of tags. This allows for conditional actions based on the harvestable properties of blocks.

```xml

<requirement name="RequirementBlockHasHarvestTags, SCore" tags="salvageHarvest"/>
```

**Explanation**: This requirement takes the following attributes:

* **`name="RequirementBlockHasHarvestTags, SCore"`**: The name of the requirement.
* **`tags`**: (Required) A comma-separated string of tags. The requirement evaluates to `true` if the target block has
  configured harvest drops, and *any* of those harvestable items possess *any* of the tags specified in this attribute.
* **`invert`**: (Optional, inherited) A boolean value. If `true`, the requirement is inverted, meaning it evaluates to
  `true` if the target block *does not* have harvest drops with the specified tags.

**Example Usage:**

You could use this requirement to trigger a special effect or provide a specific dialog option when a player harvests a
block that yields items with certain characteristics:

```xml

<effect_group>
  <triggered_effect trigger="onSelfHarvestBlock" action="ModifyCVar" cvar="$perkperceptionmastery_lbd_xp" operation="add" value="8">
    <requirement name="HoldingItemHasTags" tags="perkSalvageOperations"/>
    <requirement name="RequirementBlockHasHarvestTags, SCore" tags="salvageHarvest" /> <!-- The Superior Requirement -->
    <requirement name="NotHasBuff" buff="buffLBD_perkPerceptionMastery_HarvestCoolDown"/>
  </triggered_effect>
</effect_group>
```

### 17\. `RequirementIsProgressionLocked, SCore`

This requirement checks whether a specified progression (such as an attribute, perk, or skill) is currently locked for the entity (typically the player) that triggered the event. This allows for conditional content or actions based on the player's progression lock status.

```xml
```

**Explanation**: This requirement takes the following attributes:

* **`name="RequirementIsProgressionLocked, SCore"`**: The name of the requirement.
* **`progression_name`**: (Required) A string representing the ID of the progression to check (e.g., `attPerception` for the Perception attribute, or the ID of a specific perk or skill).
* **`invert`**: (Optional, inherited) A boolean value. If `true`, the requirement is inverted. This means it evaluates to `true` if the specified progression is *not* locked (i.e., it is unlocked). The XML shorthand `!` before the requirement name (e.g., `!RequirementIsProgressionLocked`) is equivalent to setting `invert="true"`.

**Example Usage:**

You can use this requirement to make a recipe available only if a certain skill is unlocked, or to display a dialog option only if an attribute is still locked:

```xml
<buff name="buffLBD_attPerception_LevelUpCheck" hidden="true">
  <stack_type value="ignore"/><duration value="1"/>
  <effect_group>
    <requirement name="!RequirementIsProgressionLocked, SCore" progression_name="attPerception" />
    <triggered_effect trigger="onSelfBuffStart" action="AddProgressionLevel" progression_name="attPerception" level="1"/>
    <triggered_effect trigger="onSelfBuffStart" action="ModifyCVar" cvar="$attperception_lbd_xp" operation="subtract" value="@$attperception_lbd_xptonext"/>
    <triggered_effect trigger="onSelfBuffStart" action="ModifyCVar" cvar="$attperception_lbd_xptonext" operation="multiply" value="1.3"/>
    <triggered_effect trigger="onSelfBuffStart" action="PlaySound" sound="ui_level_up"/>
  </effect_group>
</buff>
```
