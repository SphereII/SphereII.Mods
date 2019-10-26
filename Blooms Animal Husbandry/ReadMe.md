Blooms Animal Husbandry
=======================

The Bloom's Animal Husbandry Mod provides sample xml for a cow, along with support XML items, such as hand item, buff sequences to control its thirst and hunger, as well as reproduction.

Using a combination of A custom AI task ( EAIMaslowLevel1 ) and Buffs, animals can be guided through their entire life cycle. 

Finding
-------------------
Animals can be found in the wild or earned through potential quest lines from NPCs, using the RewardGiveNPCSDX quest reward system. Animals can spawn in small herds, usually with a male and a few female animals. Male animals have an
AoE buff that affects only the female version of the animal, re-inforced by the faction system. This allows the opportunity for the animals to breed.


Incentives
-------------------
Unlike full NPCs, you are not expected to hire or gain the loyalty of a farm animal.  Basic animal handling is control using the incentive feature of the EAIApproachAndFollowTargetSDX. 

An incentive is something that each individual entity finds appealing. This can be a combination of buffs, cvars, block or item names. For animals, it makes that they would be attracted to certain blocks (hay bales) and
food. If a baby is spawned from an entity, then it has a Mother cvar, which has the entity ID of the mother entity. This gets removed as the entity gets older through the buff system, but when it's young, 
it'll follow its mother around.

A spawned in herd will usually contain a male version of the animal, or however the EntityAliveEventSpawnderSDX is configured for them. The "Leader" is spawned in first, and its EntityID is stored with the Followers, allowing them to 
follow their leader.
 
~~~~~~~~~~~~~~~~{.xml}
<property name="AITask-7" value="ApproachAndFollowTargetSDX, Mods" param1="Mother,Leader,hayBaleBlock,foodCornOnTheCob"/>
~~~~~~~~~~~~~~~~

Breeding
-------------------
Breeding is handled throuhgh a buffs. For breeding to be successful, there needs to be a Male and Female version of the same animal, and they need to share the same faction. 

When the adults recieve the buffAnimalAdult buff, they'll be affected by one of two effect groups, depending on their gender:

If they are female, they'll have a random chance to get pregnant. One of the conditions, besides random chance, to getting pregnant is having the buffAnimalFertility buff.

If they are male, they'll have the buffAnimalFertility AoE. The buffAnimalFertility only affects Females that are close by ( Default is 4 blocks ).


~~~~~~~~~~~~~~~~{.xml}
<buff name="buffAnimalAdult" hidden="true">
    <stack_type value="ignore"/>
    <duration value="1000"/>

    <!-- Effects that only need to happen on the females -->
    <effect_group name="Female Effects">
		<requirement name="IsMale" invert="true"/>

		<!-- Conditions for triggering pregnancy-->
		<triggered_effect trigger="onSelfBuffUpdate" action="AddBuff" target="self" buff="buffAnimalPregnant">
			<requirement name="RandomRoll" seed_type="Random" target="self" min_max="0,100" operation="LTE" value="1"/>
			<requirement name="NotHasBuff" buff="buffAnimalPregnant"/>
			<requirement name="NotHasBuff" buff="buffAnimalNewMother"/>
			<requirement name="NotHasBuff" buff="buffBloodMoonEffect" />
			<requirement name="HasBuff" buff="buffAnimalFertility"/>
		</triggered_effect>
    </effect_group>

    <!-- if the entity is a male, then give it the fertility buff -->
    <effect_group name="Male Effects">
		<requirement name="IsMale" />
		<triggered_effect trigger="onSelfBuffUpdate" action="AddBuff" target="selfAOE" range="4" buff="buffAnimalFertility" target_tags="farmanimal"/>
    </effect_group>

    <!-- Effects and triggers for both genders -->
    <effect_group name="Universal effects">
		<triggered_effect trigger="onSelfBuffStart" action="ModifyCVar" cvar="$sizeScale" operation="set" value="1"/>
		<triggered_effect trigger="onSelfBuffStart" action="PumpQuestSDX, Mods" target="self"  />
    </effect_group>

    <effect_group name="Exit Buffs">
		<triggered_effect trigger="onSelfBuffFinish" action="AddBuff" target="self" buff="buffAnimalSenior" />
		<triggered_effect trigger="onSelfBuffFinish" action="RemoveBuff" target="self" buff="buffAnimalHarvestable" />
		<triggered_effect trigger="onSelfBuffFinish" action="RemoveBuff" target="self" buff="buffAnimalFertility" />
    </effect_group>
</buff>
~~~~~~~~~~~~~~~~


Herds
-------------------

Initial spawning is handled through a special entity called EntityAliveEventSpawnerSDX class.  Through this special entity, you may spawn in a particular entity as a leader, then spawn in followers. These followers will set
their cvar Leader to their leader, and follow it everywhere. If the leader gets attacked, then the followers will protect it.

The invisibleCowHerdSpawner would be configured through an entity group or through biome spawning like a typical zombie or animal.

~~~~~~~~~~~~~~~~{.xml}
    <entity_class name="invisibleCowHerdSpawner">
      <property name="Mesh" value="Gore/gore_block1_bonesPrefab"/>
      <property name="ModelType" value="Custom"/>
      <property name="Prefab" value="Backpack"/>
      <property name="Class" value="EntityAliveEventSpawnerSDX, Mods"/>
      <property name="Parent" value="Animals"/>
      <property name="TimeStayAfterDeath" value="1"/>
      <property name="IsEnemyEntity" value="false"/>
      <property name="LootListOnDeath" value="4"/>
      <property name="Faction" value="animals"/>
      <property class="SpawnSettings" >
        <property name="Leader" value="animalFarmCowMale" />
        <property name="Followers" value="animalFarmCow,animalFarmCow,animalFarmCow" />
      </property>
    </entity_class>
~~~~~~~~~~~~~~~~

