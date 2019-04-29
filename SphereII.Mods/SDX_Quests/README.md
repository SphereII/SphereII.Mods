SDX Quests
==========

The follow classes have been added to allow more fine tuning of the Quest system, by introducing new Objectives and action types.  

Note: The new Objectives are not tied into the official quest "pump" that gets triggered to check if the objective is checked. For this reason, the MinEventActionPumpQuestSDX was created, to pump the quest chains to see if the
objectives have been met. 


ObjectiveBuffSDX
---------------------

This new Objective type allows you to set a condition on a buff before continueing. In the below example, in order to transition from Phase 1 to Phase 2, the entity must have the buff called "buffAnimalAdult".

###Properties:###

	phase: Vanilla. 
	buff: This is the buff that the entity must have before this phase will progress.

Example Usage:

~~~~~~~~~~~{.xml}
<objective type="BuffSDX, Mods">
	<property name="phase" value="1" />
	<property name="buff" value="buffAnimalAdult" />
</objective>
~~~~~~~~~~~


ObjectiveGotoPOI
---------------------

This new Objective type allows you to target a particular prefab by name.

###Properties:###

	PrefabName: This is the prefabName you wish to target, which is the filename without the extension.

Example Usage:

~~~~~~~~~~~{.xml}
<objective type="GotoPOISDX, Mods" value="500-800" phase="1">
	<property name="completion_distance" value="50" />
    <property name="PrefabName" value="prefabName" />
</objective>
~~~~~~~~~~~


QuestActionGiveBuffSDX
---------------------

This new Action for the quest will give a buff to the recipient.

###Properties:###

	type: This is the SDX Class of the GiveBuff
	value: This is the buff you want to be applied
	phase: Vanilla. Determines at which phase the buff is applied.

Example Usage:

~~~~~~~~~~~{.xml}
      <action type="GiveBuff, Mods" value="buffAnimalPregnant" phase="3" />
~~~~~~~~~~~


RewardGiveNPCSDX
---------------------

This reward gives you the quest NPC to the player as a follower.

###Properties:###

	type: This is the SDX Class of the GiveNPCSDX
	id: If provided, a new NPC will be spawned from this entity group, and assigned to you. If no id is specified, the current NPC is assigned.

Example Usage:

~~~~~~~~~~~{.xml}
 <reward type="GiveNPCSDX, Mods" id="entityGroup"  />  // Spawns in an entity from the group to be your NPC
 <reward type="GiveNPCSDX, Mods"  />  // Hires the current NPC
 ~~~~~~~~~~~


RewardItemSDX
---------------------

This reward gives you target item.

###Properties:###

	type: This is the SDX Class of the GiveNPCSDX
	id: Provides the player with that item
	value: Is the number of the item to provide. 

Example Usage:

~~~~~~~~~~~{.xml}
<reward type="ItemSDX, Mods" id="casinoCoin" value="10" />
~~~~~~~~~~~


QuestActionSpawnEntitySDX
---------------------

This QuestAction allows you to spawn an entity, spawning it beside the SharedOwnerID's entity. Unlike SpawnEnemy, it won't automatically set the player or spawn entity trigger as ane enemy. Like SpawnEnemy, 
it can accept comma delimited entities in the id="animalFarmCow,zombieArlene".
		
Example Usage:

~~~~~~~~~~~{.xml}
<action type="SpawnEntitySDX, Mods" id="zombieBear" value="1" phase="3" />       <!-- This will spawn in an zombieBear entity during Phase 3 -->
~~~~~~~~~~~


RewardQuestSDX
---------------------
This RewardQuest allows you to give a Quest to the SharedOwnerID entity. This is a non-Player reward.

###Properties:###

	type: This is the SDX Class of the RewardQuestSDX
	id: The quest name you want to give.

Example Usage:

~~~~~~~~~~~{.xml}
<reward type="QuestSDX, Mods" id="Progression_Quest" />
~~~~~~~~~~~



Quest Line Example
-------------------

Here's an example of a Quest line that works based on buff progression. 

~~~~~~~~~~~{.xml}
<quest id="Progression_Quest">
	<property name="name_key" value="Progression_Quest" />
	<property name="subtitle_key" value="Progression_Quest" />
	<property name="description_key" value="Progression_Quest" />
	<property name="icon" value="ui_game_symbol_zombie" />
	<property name="repeatable" value="true" />
	<property name="category_key" value="challenge" />
	<property name="offer_key" value="Progression_Quest" />
	<property name="difficulty" value="veryeasy" />

	<!-- Before progressing, make sure the entity is at infection1 -->
	<objective type="BuffSDX, Mods">
		<property name="phase" value="1" />
		<property name="buff" value="buffIllInfection1" />
	</objective>

	<!-- Before progressing, make sure the entity is at infection2 -->
	<objective type="BuffSDX, Mods">
		<property name="phase" value="2" />
		<property name="buff" value="buffIllInfection2" />
	</objective>

	<!-- Before progressing, make sure the entity is at infection3 -->
	<objective type="BuffSDX, Mods">
		<property name="phase" value="3" />
		<property name="buff" value="buffIllInfection3" />
	</objective>
      
	<!-- This will spawn in an zombieBear entity during Phase 3 -->
	<action type="SpawnEntitySDX, Mods" id="zombieBear" value="1" phase="3" />
      
	<!-- Once the entity is spawned, give back the same quest, and give it a casino coin for a good job. -->
	<reward type="QuestSDX, Mods" id="buffProgression_Quest" />
	<reward type="ItemSDX, Mods" id="casinoCoin" value="1" />

</quest>
~~~~~~~~~~~

As mentioned before, in order for the Objectives to properly fire, you need to use the MinEventActionPumpQuestSDX in the buffs. In the  effect_group  "Starting Effects", there's a PumpQuestSDX, Mods. This fires the refresh
call for each of the quests that belong to the entity to refresh and check their status.

~~~~~~~~~~~{.xml}

<buff name="buffInfectedAnimal1">
    <stack_type value="ignore"/>
    <duration value="50"/>
    <effect_group name="Starting Effects">
		<triggered_effect trigger="onSelfBuffStart" action="PumpQuestSDX, Mods" target="self"  />
		<triggered_effect trigger="onSelfBuffStart" action="RemoveBuff" target="self" buff="buffAnimalHarvestable" />
		<triggered_effect trigger="onSelfBuffStart" action="RemoveBuff" target="self" buff="buffAnimalFertility" />
    </effect_group>

    <!-- Give the animal a chance to fight the infection -->
    <effect_group name="Updates">
		<triggered_effect trigger="onSelfBuffUpdate" action="RemoveBuff" target="self" buff="buffInfectedAnimal1" >
			<requirement name="RandomRoll" seed_type="Random" target="self" min_max="0,100" operation="LTE" value="1"/>
		</triggered_effect>
    </effect_group>

    <!-- handles the buffs that we are adding and removing together-->
    <effect_group name="General Matched buffs">
		<triggered_effect trigger="onSelfBuffStart" action="AttachParticleEffectToEntity" particle="p_onFire" local_offset="0,-.2,0" parent_transform="Hips"/>
		<triggered_effect trigger="onSelfBuffFinish" action="RemoveParticleEffectFromEntity" particle="p_onFire"/>
		<triggered_effect trigger="onSelfBuffRemove" action="RemoveParticleEffectFromEntity" particle="p_onFire"/>
    </effect_group>

    <effect_group name="Exit Buffs">
		<triggered_effect trigger="onSelfBuffFinish" action="AddBuff" target="self" buff="buffInfectedAnimal2" />
		<triggered_effect trigger="onSelfBuffRemove" action="AddBuff" target="self" buff="buffAnimalHarvestable" />
    </effect_group>
</buff>
	~~~~~~~~~~~
