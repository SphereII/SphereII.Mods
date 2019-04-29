SDX SpawnFrom Entity Group
==========

This class attempts to provide an entity spawning mechanism based on an entity. Rather than using the block spawner, this entity will spawn in a specified group of entities, assign a leader to them. It's designed to be a 
Herd or Bandit / NPC group spawner


EntityAliveEventSpawnerSDX
---------------------

This Entity class has a SpawnSettings class, that has properties for Leader and Follower entities. The Leader will spawn in first, then the Followers will spawn in, assigning their Leader ID to Leader entity. This will 
cause the followers to follow their leader, and treat them as such. If the leader gets attacked, for example, the followers will defend it.

###Properties:###

	Leader: This specifies which entity will be the leader.
	value: This is either a single entity name, or multiple ones, separated by commas. Note: There can only be one leader.
	Followers: This specifies the follow entities in its value attribute.  The value for follower may also be a single entitygroup reference, rather than entity class.
	Param1: Param1 is used in the name="Follower-*" to determine how many entities to spawn

Example Usage:

~~~~~~~~~~~{.xml}
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
        <property name="Leader" value="animalBull" /> 
        <property name="Followers" value="animalCow,animalCow,animalCow,animalCow"  />  
    </property>
</entity_class>


<entity_class name="invisibleCowHerdSpawner2"> 
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
        <property name="Leader" value="animalBull" /> 
        <property name="Followers" value="animalCow"  param1="2,4"/>   <!-- entity with a range  -->
    </property>
</entity_class>
~~~~~~~~~~~
