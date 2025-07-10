UAI (Utility AI) Tasks represent the specific actions or behaviors that an entity can perform within the Utility AI system. Once the AI has evaluated various considerations, it selects the task with the highest score to execute.

Here's an overview of the custom UAI Tasks available:

* **UAITaskApproachSpotSDX**: Directs the entity to approach a specific designated spot or location.
* **UAITaskAttackTargetEntitySDX**: Commands the entity to attack its current target, focusing on specific entity types or conditions.
* **UAITaskBackupFromTargetSDX**: Instructs the entity to move away or back up from its current target, often used for ranged attackers or defensive maneuvers.
* **UAITaskBreakBlocks**: Directs the entity to break specific blocks, which can be part of pathing or harvesting behaviors.
* **UAITaskFarming**: Enables an entity (e.g., an NPC) to perform farming-related activities, such as tending to crops.
* **UAITaskFireBuff**: Allows the entity to fire a buff, potentially applying a status effect to itself or others.
* **UAITaskFollowSDX**: Instructs the entity to follow a specified target, typically the player or a leader NPC.
* **UAITaskGuard**: Directs the entity to guard a specific position or area, defending it from threats.
* **UAITaskHealSDX**: Enables the entity to heal itself or other targets, often based on health thresholds.
* **UAITaskHealSelf**: Specifically directs the entity to apply healing effects to itself.
* **UAITaskIdle**: Commands the entity to remain idle or perform passive behaviors when no other high-priority tasks are present.
* **UAITaskLearn**: Enables the entity to "learn" or gain progression, potentially interacting with skill systems or knowledge acquisition.
* **UAITaskLoot**: Directs the entity to search for and collect loot from containers or dropped items.
* **UAITaskMoveToAttackTargetSDX**: Instructs the entity to move towards its current attack target.
* **UAITaskMoveToExplore**: Directs the entity to move towards unexplored areas, often used for exploration or scouting behaviors.
* **UAITaskMoveToHealTarget**: Commands the entity to move towards a target that needs healing.
* **UAITaskMoveToHealTargetSDX**: An extended version of `UAITaskMoveToHealTarget`, with enhanced logic for reaching healing targets.
* **UAITaskMoveToInvestigate**: Directs the entity to move towards a position that requires investigation, often triggered by sounds or visual cues.
* **UAITaskMoveToSDX**: A general task for moving an entity to a specific location.
* **UAITaskMoveToTargetSDX**: Directs the entity to move towards its assigned target.
* **UAITaskPatrol**: Instructs the entity to follow a predefined patrol path or route.
* **UAITaskTerritorial**: Commands the entity to patrol or guard a specific territory, reacting to intruders.
* **UAITaskWanderSDX**: Directs the entity to wander randomly within an area, potentially seeking specific points of interest or resources.

In addition to the core `UAITask` types, the following EAI (Enemy AI) Tasks specific to NPCv2 provide further specialized behaviors for companions and NPCs:

* **EAIApproachAndAttackTargetCompanion**: Directs a companion to approach and attack its target.
* **EAILookCompanion**: Commands a companion to look at a specific target or direction.
* **EAISetAsTargetIfHurtCompanion**: Sets a target for the companion if it or its leader gets hurt.
* **EAISetNearestEntityAsTargetCompanion**: Sets the nearest entity as the companion's target.
* **EAIWanderCompanion**: Directs a companion to wander within an area.