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