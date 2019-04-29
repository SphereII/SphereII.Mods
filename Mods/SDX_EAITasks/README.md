EAI Tasks and Targets
=====================

The SDX_EAITasks class introduces new AI tasks for entities to perform. Some of the tasks were changed so they rely on entity factions, rather than specifying the enemies through their Entity classes.

EAIApproachAndFollowTargetSDX
-------------------

This AI task is useful for getting followers, based on potential incentives.

Incentives can be a block name that you are holding, an item name that you are holding, a buff, or a CVar value. You do not need to specify which
type of incentive it is. The class will go through all the various combinations to try to identify which of your incentives is active.

For the cvar checks, it stored the EntityID of the target. For example, Leader is the one who has hired the entity, so it's the player.entityID. For Mother, 
it's the entity ID of the mother that spawned it.

###Parameters:###

	Param1: This parameter is the target's incentive
	Param2: Not Used

Example Usage:
~~~~~~~~~~~{.xml}
      <property name="AITask-4" value="ApproachAndFollowTargetSDX, Mods" param1="Mother,Leader,hayBaleBlock,foodCornOnTheCob"/>
~~~~~~~~~~~


EAIPatrolSDX
------------------

This AI Task is useful for asking followers and NPCs to patrol a path for you, once they are hired.

To set a patrol task, the Order must be SetPatrol. By default, this is set through opening up a dialog chat by interacting with the NPC. Once SetPatrol is set, walk the path you want the entity to follow. The entity will record every step you make.

Once the path is set, interact with them again, and ask them to Patrol. They will turn around and retrace their steps to their starting position, and start looping back and forth until interrupted.

###Parameters:###

	Param1: Not Used
	Param2: Not Used

Example Usage:

~~~~~~~~~~~{.xml}
		<property name="AITask-3" value="PatrolSDX, Mods"/>
~~~~~~~~~~~

EAIWanderSDX
---------------

This AI Task is useful for assigning to NPCs and animals alike. By default, the base Wander Ai task will begin attacking a block that is in front of it. The WanderSDX task stops the entity from wandering in the direction that its stuck in, rather than trying to break through. Very useful for farm animals that you don't want to break out of your fences.

###Parameters:###

	Param1: Not Used
	Param2: Not Used

Example Usage:
~~~~~~~~~~~{.xml}
		<property name="AITask-9" value="WanderSDX, Mods"/>
~~~~~~~~~~~

EAIMaslowLevel1SDX
---------------

This AI Task enables rules based on losely on Maslow laws. It allows the entity to react to its own stats, including seeking food and water if hungry or thirsty, sanitation needs, and even farm-animal production, such as laying an egg or producing milk from a cow.

###Parameters:###

	Param1: Not Used
	Param2: Not Used

Example Usage:
~~~~~~~~~~~{.xml}
		<property name="AITask-7" value="MaslowLevel1SDX, Mods"/>
~~~~~~~~~~~


EAILootLocationSDX
---------------

This AI task is useful for assinging to NPCs who will help you loot a POI with you. They will scan for all the loot containers in the bounds of a prefab, and start searching them. You may intereact with the NPC and ask to see their Inventory to see what they have picked up.

This is experimental, and not recommended.

###Parameters:###

	Param1: Not Used
	Param2: Not Used

Example Usage:
~~~~~~~~~~~{.xml}
		<property name="AITarget-5" value="LootLocationSDX, Mods" />
~~~~~~~~~~~


EAISetAsTargetIfHurtSDX
-------------------

This AI Target helps assign whether there is an revenge target if something attacks it. However, if its your leader, you forgive them...

###Parameters:###

	Param1: Vanille: This sets a filter based on what type of Entity is allowed to be targetted. 
	Param2: Not Used

Example Usage:
~~~~~~~~~~~{.xml}
		<property name="AITarget-1" value="SetAsTargetIfHurtSDX, Mods" param1="Entity"/>  <!-- Anything that attakcs it, can be attacked back -->
~~~~~~~~~~~

EAISetAsTargetIfLeaderAttackedSDX
---------------------

This AI Target helps assign whether the Leader, if assigned, is being attacked and if they should become the attack target of the entity.

###Parameters:###

	Param1: Vanille: This sets a filter based on what type of Entity is allowed to be targetted. 
	Param2: Not Used

Example Usage:

~~~~~~~~~~~{.xml}
		<property name="AITarget-3" value="SetAsTargetIfLeaderAttackedSDX, Mods" param1="Entity"/>  <!-- Anything that attakcs the leader, can be attacked back -->
~~~~~~~~~~~

EAISetAsTargetNearestEnemySDX
---------------------

This AI Target helps assign whether any entities that are close by belong to an enemy faction, and attacks accordingly.


###Parameters:###

	Param1: Vanille: This sets a filter based on what type of Entity is allowed to be targetted. 
	Param2: Not Used

Example Usage:

~~~~~~~~~~~{.xml}
		<property name="AITarget-4" value="SetAsTargetNearestEnemySDX, Mods" param1="Entity"/> <!-- It will attack anything it hates, as defined by the NPC factions -->
~~~~~~~~~~~