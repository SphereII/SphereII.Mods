SDX EntityAlive
==================

This project contains a few different classes to enable more flexible NPCs and animals.

EntityAliveSDX
--------------

This is the new base class, which inherits from EntityNPC and all the sub classes. It has new features, such as allowing them to be hired and execute your orders.

NPCs can accept orders by interacting with them and selecting one of pre-made options. Using a combination of XML dialog settings and code, some dialog options will not appear unless they make sense. For example, 
when you first meet an NPC, you will not be able to give them orders. 

NPCs can be hired either by paying their Hire Cost, completing a quest line, or have some sort of incenntive for them to follow.

Using the new Maslow AI Task, along with the buff system, NPCs can consume food and water.

##Hiring NPCs##

Each unique NPC can have its own hiring cost and currency, available through the XML properties.  By default, this is 1000 casino Coins. The HireCurrency is any available Item, and that item will be removed 
from your inventory when you hire them. 

####Examples:####

Default:
~~~~~~~~~~~~~~~~~{.xml}
 <property name="HireCost" value="1000"/>
 <property name="HireCurrency" value="casinoCoin"/>
~~~~~~~~~~~~~~~~~

Unique Item:
~~~~~~~~~~~~~~~~~{.xml}
 <property name="HireCost" value="1"/>
 <property name="HireCurrency" value="BloomsFamilyHeirloom_1"/>
~~~~~~~~~~~~~~~~~

Currently, all NPCs are permanent hires. Once NPCs are hired, your entity ID is stored with the entity as a cvar "Leader". As soon as they are hired, they'll begin to follow you, and try to keep pace with your speed.

Initially, unhired NPCs will only show a few options in the dialog. However, when they are hired, they can do more for you.

###Orders###

\li ShowMe - This displays information about the entity in a tool tip window, and in the console. This will show you their health, name, and hunger levels.
\li ShowAffection - This just displays a tool tip. TODO: have it positively impact the entity
\li FollowMe  - The entity will follow you, keeping a distance of 3 to 5 spaces behind you. They'll try to match your walk and run speed.
\li Stayhere - The entity will stay in place
\li GuardHere - The entity will stand in your place, facing your initial direction. This will set the GuardPosition, allow the guard to return to that spot after a fight.
\li Wander - The entity will walk around the area
\li SetPatrol - The entity will begin following your foot steps, recording them as Patrol Coordinates
\li Patrol - The entity, if it has Patrol Coordinates, will begin tracing the coordinates back and forth. This option only shows if it has Patrol Coordinates
\li Hire - If unhired, the entity will allow you to hire them. Their cost depends on their XML settings.
\li OpenInventory - This will open their inventory
\li Loot - This tells the entity to Loot the POI you are in now.

Options can be filtered through using the SDX_Dialog class that adds new conditions to show statements based on if they are hired. Not all NPCs need to support all the tasks.

Here's a sample Dialog XML that show's the Hire option, if the NPC is not hired, and will show ShowMe and StayHere orders if it is hired.

####Sample Dialog####

~~~~~~~~~~~~~~~~~{.xml}
<response id="Hire" text="I am interested in hiring you." >
	<requirement type="HiredSDX, Mods" requirementtype="Hide" value="not"/>
	<action type="OpenDialogSDX, Mods" id="Hire" />
</response>
      
<response id="FollowMe" text="Follow me" >
	<requirement type="HiredSDX, Mods" requirementtype="Hide"/>
	<action type="ExecuteCommandSDX, Mods" id="FollowMe" />
</response>

<response id="ShowMe" text="Show Me your inventory" >
	<requirement type="HiredSDX, Mods" requirementtype="Hide" />
	<action type="ExecuteCommandSDX, Mods" id="OpenInventory" />
</response>

<response id="StayHere" text="Stay here" >
	<requirement type="HiredSDX, Mods" requirementtype="Hide" />
	<action type="ExecuteCommandSDX, Mods" id="StayHere" />
</response>
~~~~~~~~~~~~~~~~~


###Recommended AI Tasks###
-------------------
The following AI tasks were used in testing well rounded NPCs. It is recommended taht you start off with this AI Tasks, and then customize as needed.

~~~~~~~~~~~~~~~~{.xml}
<property name="AITask-1" value="BreakBlock"/>
<property name="AITask-2" value="Territorial"/>
<property name="AITask-3" value="ApproachAndAttackSDX, Mods" param1="Entity,0" param2=""  /> 
<property name="AITask-4" value="ApproachAndFollowTargetSDX, Mods" param1="Leader,foodCornOnTheCob"/> 
<property name="AITask-5" value="PatrolSDX, Mods"/> 
<property name="AITask-6" value="MaslowLevel1SDX, Mods"/> 
<property name="AITask-7" value="Look"/> 
<property name="AITask-8" value="WanderSDX, Mods"/> 
<property name="AITask-9" value="" />
<property name="AITarget-1" value="SetAsTargetIfHurtSDX, Mods" param1="Entity"/> 
<property name="AITarget-2" value="SetAsTargetIfLeaderAttackedSDX, Mods" param1="Entity"/> 
<property name="AITarget-3" value="SetAsTargetNearestEnemySDX, Mods" param1="Entity,80"/>
<property name="AITarget-4" value="" />
~~~~~~~~~~~~~~~~

###Maslow###
-------------------
Losely modelled around Maslow's Hierachy of Needs, the NPCs have requirements based on a combination of buffs and a new AI task. This buff is applied through the entityclass entry, and controls the NPCs' food and 
drink requirements. Over time, the NPCs will get hungry and thirsty, and, using the EAIMaslowLevel1SDX class, will seek out food and water through its configuration.

####Food and Water####

NPCs will look for food and water through their configured bins. Triggered by the HungryBuffs and ThirstyBuffs, the AI task will seek out food and water as needed, and consume it. Once satisfied, it'll continue
 to wander.
~~~~~~~~~~~~~~~~{.xml}
<!-- which containers to look for food in -->
<property name="FoodBins" value="cntSecureStorageChest,cntStorageChest" />

<!-- what it can drink out of -->
<property name="WaterBins" value="water,waterMoving,waterStaticBucket,waterMovingBucket,terrWaterPOI" />

<!-- Default thirsty and hungry buffs -->
<property name="ThirstyBuffs" value="buffStatusThirsty1,buffStatusThirsty2" />
<property name="HungryBuffs" value="buffStatusHungry1,buffStatusHungry2" />
~~~~~~~~~~~~~~~~

Each entity can be configured to eat a certain food type. The NPCs will scan for the FoodBins, then search inside and consume one of the listed food items. The value of food they recieve is hard coded in the EAIMaslowLevel1
class initially, since some food items may not be have a food value, such as the hayBaleBlock. All foods satisfy the same amount of hunger.

Once an entity becomes too hungry or thirsty, it will start taking damage until it eventually dies.

~~~~~~~~~~~~~~~~{.xml}
<!-- Food items and bins that this entity will eat, if using the right AI Task -->
<property name="FoodItems" value="hayBaleBlock,resourceYuccaFibers,foodCropYuccaFruit,foodCornBread,foodCornOnTheCob,foodCornMeal,foodCropCorn,foodCropGraceCorn"/>
~~~~~~~~~~~~~~~~

#### Sanitation ####

As an optional feature, NPCs can be affected by a sanitation system. Over time, a sanitation level rises through a buff. Once this value reaches a threshold, the entity will need to use the bathroom. If
a bathroom is not available, they will do their business on the ground.

If sanitation is turned on, the entity will search for a ToiletBlocks that is nearby. If found, it will go to the ToiletBlock and reset its sanitation level. This is useful for NPCs, but might not work out so well for animals.

If an entity does not have a toilet block, it will eventually drop a SanitationBlock. This will be useful in the future for different types of fertilizer.

By default, it is disabled.

~~~~~~~~~~~~~~~~{.xml}
<property name="ToiletBlocks" value="cntToilet01,cntToilet02,cntToilet03" />
<property name="SanitationBlock" value="terrDirt" /> <!-- Poop block. If ToiletBlocks is configured, it'll use those rather than generate this block. -->
~~~~~~~~~~~~~~~~

#### Home Block ####

If configured, a Homeblock may be placed which will reset the entity's Home location. The blocks listed here will set the entity's Home Position, allow it to patrol around it using the vanilla Territorial AI task

~~~~~~~~~~~~~~~~{.xml}
<!-- This is the block it'll call home -->
<property name="HomeBlocks" value="cntCowHomeBlock,hayBaleBlock"/>
~~~~~~~~~~~~~~~~

#### Starting Buffs ####

NPCs are controlled mainly through the buff system. The default buffs are listed on the entity class, and are used to start off its hunger / thirsty stats, it's sanitation stats, etc.

~~~~~~~~~~~~~~~~{.xml}
<!-- The starting buffs. Semi-colon delimited! Example for Animal Buffs -->
<property name="Buffs" value="buffAnimalStatusCheck;buffStatusCheck;buffAnimalBaby;buffSanitationStatusCheck;buffAnimalCow" />
~~~~~~~~~~~~~~~~

#### Names ####

Each entity may have one or more names, configured through the XML files. When the ntity is first spawned in, a name is randomly picked by the XML. The entity will be known as "Name the EntityName" in log files, along
with its entity ID. 

~~~~~~~~~~~~~~~~{.xml}
<property name="Names" value="Bully,Duke,Hydro,Homer,Earl,Disel,Horns,Armor,Bob,Sampson,Tank,Kristof,Angus,Midnight,Nitrous,Red Bull,Moogan Freeman,Meatloaf,T-Bone,Mooshu,Leonardo DiCowprio,Cheeseburger,LovaBull,Hugh Heifer,Moossolini,Grassyella,Moo Rock,Rick Raws The Sause Bause,SteakHouse" />
~~~~~~~~~~~~~~~~
