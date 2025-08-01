The `ObjectiveBlockDestroySDX` is an advanced quest objective that tracks the destruction of specific blocks. It extends
the standard block destruction objective by offering flexible criteria for identifying target blocks, including by name,
partial name, alternate blocks, or tags.

## Functionality

This objective becomes active when its quest phase begins and listens for `BlockDestroy` events in the game world. When
a block is destroyed, the objective checks if it matches the specified `id` or tags. If a match occurs and quest
requirements are met, the objective's progress increases. It also supports tracking progress for players within the same
party.

### Properties

You can configure `ObjectiveBlockDestroySDX` within your `quests.xml` file using the following properties:

* **`type`**: `BlockDestroySDX, SCore` - Specifies that this is an SCore custom block destruction objective.
* **`id`**: `string` - The primary identifier for the block(s) to be destroyed. This can be:
    * A single block name (e.g., `frameShapes`).
    * A comma-delimited list of block names (e.g., `woodChair1,officeChair01VariantHelper,woodChair1Broken`). The count
      is unified across all listed blocks.
    * A block name that contains a colon (e.g., `blockname:variant`), it matches the part before the colon.
    * A tag (e.g., `ore`, `deepOre`, `ore,deepOre`). If a direct name match fails, it checks for matching tags.
* **`value`**: `int` - The total number of blocks that need to be destroyed to complete the objective.
* **`phase`**: `int` - The quest phase to which this objective belongs.

## XML Examples

Here are examples of how to define `ObjectiveBlockDestroySDX` in your `quests.xml`:

```xml

<objective type="BlockDestroySDX, SCore" id="frameShapes" value="1" phase="2"/>

<objective type="BlockDestroySDX, SCore" id="woodChair1,officeChair01VariantHelper,woodChair1Broken" value="1"
           phase="2"/>

<objective type="BlockDestroySDX, SCore" id="deepOre" value="1" phase="2"/>

<objective type="BlockDestroySDX, SCore" id="ore,deepOre" value="1" phase="2"/>
```

---

The `ObjectiveBuffSDX` is a quest objective that requires the player or another specified entity to have a particular
buff active for the objective to be completed [cite: uploaded:ObjectiveBuffSDX.cs].

## Functionality

This objective listens for buffs being added to entities. Once the quest is active, if the target entity (usually the
player or an entity specified by `SharedOwnerID` on the quest) gains the required buff, the objective is marked as
complete. It does not rely on a continuous update loop, instead using event hooks for
efficiency [cite: uploaded:ObjectiveBuffSDX.cs].

### Properties

You can configure `ObjectiveBuffSDX` within your `quests.xml` file using the following
properties [cite: uploaded:ObjectiveBuffSDX.cs]:

* **`type`**: `BuffSDX, SCore` - Specifies that this is an SCore custom buff objective.
* **`buff`**: `string` - The name of the buff that the entity must have for the objective to be completed. This value is
  case-insensitive [cite: uploaded:ObjectiveBuffSDX.cs].
* **`id`**: (Inherited) - Used for localization of the objective's display name.
* **`value`**: (Inherited) - While present due to inheritance, its direct use for completion is handled by the presence
  of the buff itself.

## XML Example

Here's an example of how to define `ObjectiveBuffSDX` in your `quests.xml` [cite: uploaded:ObjectiveBuffSDX.cs]:

```xml

<objective type="BuffSDX, SCore" id="have_my_buff_objective" buff="buffImmunity" value="1" phase="1"/>
```

**Explanation**: This objective requires the player (or the quest's `SharedOwnerID` entity) to have the `buffImmunity`
active to be completed. The `value` attribute is typically set to `1` to indicate that the buff must be present.

-----

The `ObjectiveGotoPOISDX` is a quest objective designed to direct players to specific Points of Interest (POIs) or to
randomly selected POIs from a defined list. It extends the base POI objectives to provide more control over target
selection.

## Functionality

This objective's primary function is to determine a target POI for the player to visit. It can either target a single,
named prefab or choose one randomly from a list of specified prefabs. Once a POI is selected (or confirmed from previous
quest data), its position is calculated, and the quest updates to direct the player to that location. Completion
typically occurs when the player enters the designated POI area within a certain distance.

### Properties

You can configure `ObjectiveGotoPOISDX` within your `quests.xml` file using the following properties:

* **`type`**: `GotoPOISDX, SCore` - Specifies that this is an SCore custom POI objective.
* **`value`**: `string` - While it can represent a count, in the provided example, it's shown as a distance range (e.g.,
  `500-800`), likely indicating the desired distance from the quest giver for the POI to be found.
* **`phase`**: `int` - The quest phase to which this objective belongs.
* **`PrefabName`**: `string` - Specifies the exact name of the POI prefab that the player must go to. If `PrefabNames`
  is also present, this property will be overridden by a random selection from that list.
* **`PrefabNames`**: `string` - A comma-delimited list of POI prefab names. If this property is used, the objective will
  randomly select one POI from this list as the target. If the list is empty, a warning is logged.
* **`completion_distance`**: `float` - (From example comment) Defines how close the player needs to get to the POI for
  the objective to be marked as complete.

## XML Examples

Here are examples of how to define `ObjectiveGotoPOISDX` in your `quests.xml`:

```xml

<objective type="GotoPOISDX, SCore" value="500-800" phase="1">
    <property name="completion_distance" value="50"/>
    <property name="PrefabName" value="abandoned_house_02"/>
</objective>
```

**Explanation**: This objective directs the player to the `abandoned_house_02` POI, searching for it between 500 and 800
units away from the quest giver. The objective completes when the player gets within 50 units of the POI.

```xml

<objective type="GotoPOISDX, SCore" value="1000-2000" phase="1">
    <property name="PrefabNames" value="trader_joel_01,trader_jen_01,trader_bob_01"/>
    <property name="completion_distance" value="75"/>
</objective>
```

**Explanation**: This objective directs the player to a randomly selected trader POI (Joel, Jen, or Bob), searching
between 1000 and 2000 units away. It completes when the player gets within 75 units of the selected POI.

-----

The `ObjectiveRandomGotoSDX` is a quest objective that directs the player to a randomly determined location within the
world. It's designed to create dynamic exploration tasks where the destination is not fixed but generated based on
parameters.

## Functionality

This objective's primary function is to calculate a random point in the game world relative to the quest owner's
position. It then tasks the player with traveling to this generated location. The objective's progress is tracked by the
player's distance to this random point, and it completes once the player gets within a specified `completion_distance`.
The random point generation ensures it is a valid location (e.g., not in water or inside a POI).

### Properties

You can configure `ObjectiveRandomGotoSDX` within your `quests.xml` file using the following properties:

* **`type`**: `RandomGotoSDX, SCore` - Specifies that this is an SCore custom random go-to objective.
* **`value`**: `string` - (`PropDistance`) Defines the distance from the quest giver where the random point will be
  generated.
    * Can be a single float (e.g., `value="500"`) for a fixed distance.
    * Can be a range (e.g., `value="500-1000"`) for a random distance within that range.
* **`completion_distance`**: `float` - (`PropCompletionDistance`) The distance (in blocks) the player needs to be from
  the generated random point for the objective to be marked as complete. Default is `10` blocks.
* **`phase`**: `int` - The quest phase to which this objective belongs.

### XML Example

Here's an example of how to define `ObjectiveRandomGotoSDX` in your `quests.xml`:

```xml

<objective type="RandomGotoSDX, SCore" value="500-800" phase="1">
    <property name="completion_distance" value="20"/>
</objective>
```

**Explanation**: This objective will generate a random point between 500 and 800 blocks away from the quest giver. The
objective will be completed when the player gets within 20 blocks of this generated point.

-----

The `ObjectiveRandomPOIGotoSDX` is a quest objective that directs the player to a randomly selected Point of Interest (
POI) within the world. It's a foundational custom random POI objective, serving as a base for more advanced versions (
like `ObjectiveRandomTaggedPOIGotoSDX`).

## Functionality

This objective functions by attempting to find a random POI near the quest owner's position. It can consider a specified
distance or range when searching for the POI. Once a POI is identified (or retrieved if already set), the objective's
progress is updated based on the player's proximity to this POI, completing when the player gets within a set
`completion_distance`. The server handles the POI selection, and the client updates its UI accordingly.

### Properties

You can configure `ObjectiveRandomPOIGotoSDX` within your `quests.xml` file using the following properties:

* **`type`**: `RandomPOIGotoSDX, SCore` - Specifies that this is an SCore custom random POI objective.
* **`value`**: `string` - Defines the search distance for the random POI from the quest giver.
    * Can be a single float (e.g., `value="1000"`) for a maximum distance.
    * Can be a range (e.g., `value="500-2000"`) defining a minimum and maximum search distance. The objective attempts
      to find a POI within this range.
* **`completion_distance`**: `float` - The distance (in blocks) the player needs to be from the target POI for the
  objective to be marked as complete. This is inherited from the base objective class.
* **`phase`**: `int` - The quest phase to which this objective belongs.

## XML Example

Here's an example of how to define `ObjectiveRandomPOIGotoSDX` in your `quests.xml`:

```xml

<objective type="RandomPOIGotoSDX, SCore" value="1000-3000" phase="1">
    <property name="completion_distance" value="50"/>
    <property name="POIName" value="myPrefab"/>
</objective>
```

**Explanation**: This objective will find a random POI located between 1000 and 3000 units away from the quest giver.
The objective will be completed when the player gets within 50 units of the selected POI.

-----

The `ObjectiveRandomTaggedPOIGotoSDX` is an enhanced quest objective designed as a replacement for the vanilla
`ObjectiveRandomPOIGoto`. It provides modders with significantly more control over the selection of random Points of
Interest (POIs) for quests.

## Key Features

This objective offers several key improvements over its vanilla counterpart:

* **POI Tag Filtering**: You can specify which POI tags a target POI must `include` or `exclude`.
* **Custom Search Distance**: The objective allows defining a precise search distance for POIs, either as a maximum
  distance or as a range (minimum to maximum distance).

## Configuration Properties

You can configure `ObjectiveRandomTaggedPOIGotoSDX` within your `quests.xml` file using the following properties:

* **`include_tags`**: `string` - A comma-separated list of POI tags. A prefab must have at least one of these tags to be
  considered a valid target for the quest.
* **`exclude_tags`**: `string` - A comma-separated list of POI tags. If a prefab has any of these tags, it will be
  excluded from the search for quest targets.
* **`distance`**: `string` - Specifies the search distance for POIs from the quest giver.
    * Can be a single value (e.g., `value="2000"`) representing the maximum distance.
    * Can be a range (e.g., `value="300-1000"`) defining both minimum and maximum search distances.
    * Defaults to a minimum of 50 and maximum of 2000 if not specified.

## Functionality

The objective's core logic (`GetPosition` method) overrides the vanilla behavior to incorporate the new tag and distance
filters. It utilizes utility methods like `QuestUtils.GetRandomPOINearTrader` or `QuestUtils.GetRandomPOINearEntityPos`
to find a suitable POI based on the specified criteria. This ensures that quests guide players to POIs that fit the
desired thematic or difficulty requirements.

## XML Example

Here's an example of the syntax for `ObjectiveRandomTaggedPOIGotoSDX` within a quest definition:

```xml

<objective type="RandomTaggedPOIGotoSDX, SCore">
    <property name="include_tags" value="downtown,industrial"/>
    <property name="exclude_tags" value="rural,wilderness"/>
    <property name="distance" value="300-1000"/>
</objective>
```

---

The `QuestActionGiveBuffSDX` is a quest action that applies a specified buff to the player upon the completion of a
quest stage or the quest itself.

## Functionality

When this action is triggered, it accesses the quest's owner (typically the player) and applies a buff to them using the
`AddBuff` method. The name of the buff to be applied is determined by the `value` property defined in the XML.

### Properties

You can configure `QuestActionGiveBuffSDX` within your `quests.xml` file using the following properties:

* **`type`**: `GiveBuffSDX, SCore` - Specifies that this is an SCore custom buff-giving quest action.
* **`value`**: `string` - The name of the buff that will be applied to the player (or the quest owner) when this action
  is performed.

## XML Example

Here's an example of how to define `QuestActionGiveBuffSDX` in your `quests.xml`:

```xml

<action type="GiveBuffSDX, SCore" value="buffFastRunner"/>
```

**Explanation**: This action will apply the `buffFastRunner` to the player when this quest action is executed.

-----

The `QuestActionGiveCVarBuffSDX` is a quest action that modifies a specified CVar (Console Variable) on the player upon
the completion of a quest stage or the quest itself.

## Functionality

When this action is triggered, it reads a `value` from the XML and attempts to add it to an existing CVar identified by
its `id`. If the CVar already exists on the player, the new `value` is added to its current amount; otherwise, the CVar
is set to the provided `value`. This allows for dynamic tracking of player progress or stats directly via CVars during
quests.

### Properties

You can configure `QuestActionGiveCVarBuffSDX` within your `quests.xml` file using the following properties:

* **`type`**: `GiveCVarBuffSDX, SCore` - Specifies that this is an SCore custom CVar modification quest action.
* **`id`**: `string` - The name of the CVar on the player that will be adjusted.
* **`value`**: `float` - The numerical value to add to (or set) the specified CVar.

## XML Example

Here's an example of how to define `QuestActionGiveCVarBuffSDX` in your `quests.xml`:

```xml

<action type="GiveCVarBuffSDX, SCore" id="playerQuestPoints" value="2" phase="3"/>
```

## **Explanation**: This action will add `2` to the

`playerQuestPoints` CVar on the player when this quest action is executed (typically in `phase` 3).

---

The `QuestActionPlaySoundSDX` is a quest action that plays a specified sound effect upon the completion of a quest stage
or the quest itself. Optionally, it can also apply a buff to the player.

## Functionality

When this action is triggered, it plays a one-shot sound effect using the `ID` property as the sound's identifier.
Additionally, it checks if a `Value` property is defined in the XML. If a `Value` (representing a buff name) is present
and the player does not already have that buff, the buff is applied to the player.

### Properties

You can configure `QuestActionPlaySoundSDX` within your `quests.xml` file using the following properties:

* **`type`**: `PlaySoundSDX, SCore` - Specifies that this is an SCore custom sound-playing quest action.
* **`id`**: `string` - The `ID` of the sound to be played. This should correspond to a sound definition in `sounds.xml`.
* **`value`**: `string` (Optional) - The name of a buff that will be applied to the player when this action is
  performed, provided the player doesn't already have it.

## XML Example

Here's an example of how to define `QuestActionPlaySoundSDX` in your `quests.xml`:

```xml

<action type="PlaySoundSDX, SCore" id="ui_quest_complete" value="buffQuestCompletionBonus"/>
```

**Explanation**: This action will play the `ui_quest_complete` sound and apply the `buffQuestCompletionBonus` to the
player when executed.

-----

The `QuestActionReplaceEntitySDX` is a quest action that replaces a specified entity in the world (typically the entity
holding the quest, identified by `SharedOwnerID`) with one or more new entities upon the completion of a quest stage or
the quest itself. This allows for dynamic transformations or boss fight mechanics where one entity "evolves" into
another.

## Functionality

When this action is triggered, it identifies the entity associated with the quest (the quest holder). It then proceeds
to spawn one or more new entities, selected randomly from a predefined list of entity IDs (`ID`). The `value` property
determines the quantity of new entities spawned, which can be a fixed number or a random count within a specified range.
After the new entities are spawned, the original quest-holding entity is marked for unloading, effectively replacing it.
The spawning occurs with a slight delay between each new entity if multiple are specified.

### Properties

You can configure `QuestActionReplaceEntitySDX` within your `quests.xml` file using the following properties:

* **`type`**: `ReplaceEntitySDX, SCore` - Specifies that this is an SCore custom entity replacement quest action.
* **`id`**: `string` - A comma-separated list of entity class names (e.g., `zombieMale,zombieFemale`). One entity from
  this list will be randomly chosen for each new spawn.
* **`value`**: `string` - The number of entities to spawn.
    * Can be a single integer (e.g., `value="1"`).
    * Can be a range specified as "min-max" (e.g., `value="2-4"`), from which a random integer count will be chosen.

## XML Example

Here's an example of how to define `QuestActionReplaceEntitySDX` in your `quests.xml`:

```xml

<action type="ReplaceEntitySDX, SCore" id="zombieBoss,zombieGiant" value="1-2"/>
```

**Explanation**: This action will replace the quest-holding entity with either one or two new entities. Each new entity
will be randomly selected to be either a `zombieBoss` or a `zombieGiant`.

-----

The `QuestActionSetRevengeTargetsSDX` is a quest action that dynamically sets the revenge targets of all living entities
within a specified range upon the completion of a quest stage or the quest itself.

## Functionality

When this action is performed, it identifies a set of target entities (either the quest owner or random members of their
quest-sharing party). It then finds all living, non-player, non-hired, non-allied entities within a defined area around
the player (or the quest location). For each of these entities, it sets a randomly selected target from the previously
identified set as their `RevengeTarget`, and sets a `RevengeTimer` of 5000 ticks, making them hostile towards that
target.

### Properties

You can configure `QuestActionSetRevengeTargetsSDX` within your `quests.xml` file using the following properties:

* **`type`**: `SetRevengeTargetsSDX, SCore` - Specifies that this is an SCore custom action to set revenge targets.
* **`id`**: `string` - Defines who the revenge targets will be set to:
    * `owner`: Sets the revenge targets to the quest owner only.
    * `party`: Sets the revenge targets randomly to either the quest owner or a random member of the owner's party who
      shared the quest and is within range.
    * If omitted, defaults to `owner`.
* **`value`**: `string` - Defines the distance (diameter) of the area around the player (or quest location) in which
  entities will have their revenge targets set:
    * A number (e.g., `"20"`, `"50"`): Represents the diameter in blocks.
    * `"location"`: The distance will be the size of the quest location (typically a POI's bounding box size).
    * If omitted, or if `"location"` is used and the quest location cannot be determined, it defaults to `15` blocks.
* **`phase`**: `int` - The quest phase in which this action will be performed.

## XML Examples

Here are examples of how to define `QuestActionSetRevengeTargetsSDX` in your `quests.xml`:

```xml

<action type="SetRevengeTargetsSDX, SCore" id="owner" value="20" phase="4"/>
```

```xml

<action type="SetRevengeTargetsSDX, SCore" id="party" value="location" phase="2"/>
```

```xml

<action type="SetRevengeTargetsSDX, SCore" phase="1"/>
```

-----

The `QuestActionSpawnEntitySDX` is a quest action that spawns one or more new entities into the game world upon the
completion of a quest stage or the quest itself. This allows modders to create dynamic encounters, reinforce areas, or
reward players with new companions.

## Functionality

When this action is triggered, it identifies a list of possible entities to spawn from its `ID` property. It then
determines how many entities to spawn based on its `value` property (which can be a fixed number or a random count
within a range). The entities are spawned with a slight delay between each if multiple are specified. Each entity is
created at a random position close to the quest holder (usually the player), with a rotation that matches the quest
holder, and is marked as a static spawner source.

### Properties

You can configure `QuestActionSpawnEntitySDX` within your `quests.xml` file using the following properties:

* **`type`**: `SpawnEntitySDX, SCore` - Specifies that this is an SCore custom entity spawning quest action.
* **`id`**: `string` - A comma-separated list of entity class names (e.g., `zombieMale,animalWolf`). One entity from
  this list will be randomly chosen for each new spawn.
* **`value`**: `string` - The number of entities to spawn.
    * Can be a single integer (e.g., `value="1"`).
    * Can be a range specified as "min-max" (e.g., `value="2-4"`), from which a random integer count will be chosen.

## XML Example

Here's an example of how to define `QuestActionSpawnEntitySDX` in your `quests.xml`:

```xml

<action type="SpawnEntitySDX, SCore" id="zombieRadiated,zombieBiker" value="1-3"/>
```

**Explanation**: This action will spawn between 1 and 3 new entities when executed. Each spawned entity will be randomly
chosen to be either a `zombieRadiated` or a `zombieBiker`.

-----


The `RewardGiveBuff` is a quest reward that applies a specified buff to the player upon the completion of a quest.

## Functionality

When this reward is triggered, it accesses the quest's owner (typically the player) and applies a buff to them using the
`AddBuff` method. The name of the buff to be applied is determined by the `id` property defined in the XML.

### Properties

You can configure `RewardGiveBuff` within your `quests.xml` file using the following properties:

* **`type`**: `GiveBuff, SCore` - Specifies that this is an SCore custom buff-giving quest reward.
* **`id`**: `string` - The name of the buff that will be applied to the player (or the quest owner) when this reward is
  given.

## XML Example

Here's an example of how to define `RewardGiveBuff` in your `quests.xml`:

```xml

<reward type="GiveBuff, SCore" id="yourbuff"/>
```

**Explanation**: This reward will apply the buff specified by `yourbuff` (e.g., `buffStaminaRegen`) to the player when
the quest is completed.

-----

The `RewardGiveCvarDMT` is a quest reward that modifies a specified CVar (Console Variable) on the player upon quest
completion.

## Functionality

When this reward is given, it reads a `value` from the XML and attempts to add it to an existing CVar identified by its
`id`. If the CVar already exists on the player, the new `value` is added to its current amount; otherwise, the CVar is
set to the provided `value`. This allows for dynamic tracking of player progression or stats directly via CVars upon
quest completion. This reward is `HiddenReward = true` by default, meaning it won't be displayed in the quest rewards
list unless explicitly changed.

### Properties

You can configure `RewardGiveCvarDMT` within your `quests.xml` file using the following properties:

* **`type`**: `GiveCvarDMT, SCore` - Specifies that this is an SCore custom CVar modification quest reward.
* **`id`**: `string` - The name of the CVar on the player that will be adjusted.
* **`value`**: `float` - The numerical value to add to (or set) the specified CVar.

## XML Example

Here's an example of how to define `RewardGiveCvarDMT` in your `quests.xml`:

```xml

<reward type="GiveCvarDMT, SCore" id="yourcvar" value="1"/>
```

**Explanation**: This reward will add `1` to the `yourcvar` CVar on the player when the quest is completed.

-----

The `RewardGiveNPCSDX` is a quest reward that allows players to receive an NPC upon quest completion. This can either
involve hiring the NPC who gave the quest or spawning a new NPC from a specified entity group.

## Functionality

This reward operates in two primary modes:

1. **Hiring the Quest Giver**: If the `id` property is left empty, the reward attempts to set the `QuestGiverID`'s NPC
   as the `Owner` of the player who completed the quest. This effectively "hires" the quest-giving NPC.
2. **Spawning a New NPC**: If an `id` is provided (which should be an `entityGroup` name), the reward attempts to spawn
   a new NPC from that group at the player's position. This new NPC is then added to the world, and if it's an
   `EntityAliveSDX`, a "JoinInformation" UI window might be opened for the player.

### Properties

You can configure `RewardGiveNPCSDX` within your `quests.xml` file using the following properties:

* **`type`**: `GiveNPCSDX, SCore` - Specifies that this is an SCore custom NPC-giving quest reward.
* **`id`**: `string` (Optional) - If provided, this should be the name of an `entityGroup` from which a new NPC will be
  spawned. If omitted, the quest giver NPC will be hired instead.

## XML Examples

Here are examples of how to define `RewardGiveNPCSDX` in your `quests.xml`:

```xml

<reward type="GiveNPCSDX, SCore" id="myCustomNPCS"/>
```

**Explanation**: This reward will spawn an entity from the `myCustomNPCS` entity group to become the player's NPC.

```xml

<reward type="GiveNPCSDX, SCore"/>
```

**Explanation**: This reward will attempt to hire the current NPC who gave the quest.

-----


The `RewardItemSDX` is a quest reward that grants a specified item to an entity associated with the quest, typically the
shared quest owner, rather than directly to the player who completed the quest.

## Functionality

When this reward is given, it checks if an entity is linked to the quest via its `SharedOwnerID`. If such an entity
exists and is alive, the specified item (`id`) in the given `value` quantity is added directly to that entity's
inventory. This is particularly useful for quests that involve NPCs or companions who should receive items as a reward.

### Properties

You can configure `RewardItemSDX` within your `quests.xml` file using the following properties:

* **`type`**: `ItemSDX, SCore` - Specifies that this is an SCore custom item-giving quest reward.
* **`id`**: `string` - The ID or name of the item to be given as a reward.
* **`value`**: `int` - The quantity of the item to be given.

## XML Example

Here's an example of how to define `RewardItemSDX` in your `quests.xml`:

```xml

<reward type="ItemSDX, SCore" id="casinoCoin" value="1"/>
```

**Explanation**: This reward will add 1 `casinoCoin` to the inventory of the entity linked by `SharedOwnerID` when the
quest is completed.

-----

The `RewardQuestSDX` is a quest reward that awards another quest to a specified entity (typically the quest owner or a
shared quest entity) upon the completion of the current quest. This feature is essential for creating quest chains and
sequential storytelling.

## Functionality

When this reward is given, it creates a new quest identified by its `id`. If the current quest has an `OwnerQuest` (
meaning it's part of a chain), the new quest's `PreviousQuest` property is set to the `ID` of the just-completed quest,
maintaining the quest chain integrity. The newly created quest is then added to the quest journal of the entity
specified by `OwnerQuest.SharedOwnerID` (or the `OwnerJournal.OwnerPlayer` if `SharedOwnerID` is not applicable).

### Properties

You can configure `RewardQuestSDX` within your `quests.xml` file using the following properties:

* **`type`**: `QuestSDX, SCore` - Specifies that this is an SCore custom quest-awarding reward.
* **`id`**: `string` - The ID of the new quest to be awarded. This ID should correspond to a defined quest in your
  `quests.xml`.
* **`IsChainQuest`**: `bool` (Inherited from `BaseReward` and handled in `Clone()`) - This property indicates if the
  reward is part of a quest chain.

## XML Example

Here's an example of how to define `RewardQuestSDX` in your `quests.xml`:

```xml

<reward type="QuestSDX, SCore" id="quest_next_chapter_of_story"/>
```

**Explanation**: This reward will award the quest with the ID `quest_next_chapter_of_story` to the appropriate entity
when the current quest is completed.

-----

The `RewardReassignNPCSDX` is a quest reward designed to reassign NPCs, particularly by transferring leadership of other
NPCs from the quest-giving NPC to the player.

## Functionality

When this reward is granted, it first identifies the NPC that gave the quest. It then searches the area around this
quest-giving NPC for other `EntityAliveSDX` entities. If it finds any such NPCs that have the quest giver assigned as
their "Leader" (via a CVar), it will reassign the ownership of those follower NPCs to the player who completed the
quest. This allows for seamless transfer of companion groups to the player.

### Properties

You can configure `RewardReassignNPCSDX` within your `quests.xml` file using the following properties:

* **`type`**: `ReassignNPCSDX, SCore` - Specifies that this is an SCore custom NPC reassignment quest reward.

* This reward does not require any specific `id` or `value` attributes in the XML, as its functionality is based on the
  quest giver and dynamically found NPCs.

## XML Example

Here's an example of how to define `RewardReassignNPCSDX` in your `quests.xml`:

```xml

<reward type="ReassignNPCSDX, SCore"/>
```

**Explanation**: This reward, when triggered, will reassign any NPCs currently following the quest giver (if the quest
giver has followers defined by the "Leader" CVar) to become followers of the player.

-----

Of course. Here is the documentation for the `ObjectiveFetchByTags` class, formatted in the style of your examples file.

-----

The `ObjectiveFetchByTags` is a quest objective that requires the player to collect a certain number of items that have
specific tags. It is a flexible alternative to the standard fetch objective, which tracks items by their specific name.

## Functionality

This objective becomes active during its quest phase and completes when the player has the required number of items with
the specified tag(s) in their backpack and toolbelt. The objective periodically refreshes by scanning the player's
inventory and counting all items that match the given tags, updating the quest status accordingly. The count is
cumulative across all items that match the tags.

### Properties

You can configure `ObjectiveFetchByTags` within your `quests.xml` file using the following properties:

* **`type`**: `FetchByTags, SCore` - Specifies that this is an SCore custom fetch-by-tags objective.
* **`value`**: `int` - The total number of items with the specified tags that the player needs to collect.
* **`phase`**: `int` - The quest phase to which this objective belongs.
* **`tags`**: `string` - A comma-delimited list of item tags. The objective will count any item in the player's
  inventory that possesses at least one of the tags listed.
* **`Description`**: `string` (Optional) - A localization key to provide a custom description for the objective in the
  quest log.

## XML Example

Here's an example of how to define `ObjectiveFetchByTags` in your `quests.xml`:

```xml

<objective type="FetchByTags, SCore" value="50" phase="1">
    <property name="tags" value="ore"/>
    <property name="Description" value="gather_ores_objective"/>
</objective>
```

**Explanation**: This objective requires the player to gather a total of 50 items that have the `ore` tag. This could be
a mix of iron ore, lead ore, coal, etc. The quest log will display the text associated with the `gather_ores_objective`
localization key.