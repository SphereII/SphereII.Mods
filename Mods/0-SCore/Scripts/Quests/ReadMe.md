Here's a summary of the custom Quest Objectives and Quest Actions available, which extend the game's questing system for
more diverse and dynamic mission design.

### Quest Objectives

These define various conditions that players must fulfill to complete a quest stage.

* **ObjectiveBlockDestroySDX**: Requires the player to destroy a specific block or number of blocks.
* **ObjectiveBuffSDX**: Requires the player or another entity to have a specific buff active for the objective to be
  completed.
* **ObjectiveEntityAliveSDXKill**: Requires the player to kill a specific type or number of `EntityAliveSDX` entities.
* **ObjectiveEntityEnemySDXKill**: Requires the player to kill a specific type or number of `EntityEnemySDX` entities.
* **ObjectiveGotoPOISDX**: Requires the player to travel to a specific Point of Interest (POI).
* **ObjectiveRandomGotoSDX**: Requires the player to go to a randomly determined location within the world.
* **ObjectiveRandomPOIGotoSDX**: Requires the player to go to a randomly selected POI.
* **ObjectiveRandomTaggedPOIGotoSDX**: Requires the player to go to a randomly selected POI that has specific tags.
* **ObjectiveFetchByTags**: Requires the player to gather items bia tag.

### Quest Actions

These define various effects or events that occur upon the completion of a quest stage or the quest itself.

* **QuestActionGiveBuffSDX**: Applies a specified buff to an entity.
* **QuestActionGiveCVarBuffSDX**: Applies a buff to an entity based on the value of a CVar.
* **QuestActionPlaySound**: Plays a specified sound effect.
* **QuestActionReplaceEntitySDX**: Replaces a specified entity with another entity type.
* **QuestActionSetRevengeTargetsSDX**: Sets a new revenge target for specified entities.
* **QuestActionSpawnEntitySDX**: Spawns a specified entity at a given location.

These custom rewards allow modders to grant players various benefits beyond standard XP or items, including buffs, CVar adjustments, and even NPC reassignments.

* **RewardGiveBuff.cs**: Applies a specified buff to the player or another entity upon quest completion.
* **RewardGiveCvarDMT.cs**: Sets a CVar (Console Variable) on the player, allowing for custom quest progression tracking or dynamic state changes.
* **RewardGiveNPCSDX.cs**: Rewards the player with an NPC, potentially hiring them or adding them as a companion.
* **RewardItemSDX.cs**: Rewards the player with a specific item or set of items.
* **RewardQuestSDX.cs**: Awards another quest to the player upon completion of the current one, facilitating quest chains.
* **RewardReassignNPCSDX.cs**: Reassigns an NPC, possibly changing their role, location, or owner, upon quest completion.

