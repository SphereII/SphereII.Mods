#0-SCore

The 0-SCore is the key component to enable extra functionality for 7 Days To Die. Using a mixture of Harmony, SDX Patch Scripts, and Scripts, new features are enabled for modders and players to use.

| Folder | Description |
| ----------- | ----------- |
| Config | XML Files that serve as documentation, basic setup for required items. blocks.xml contains many default settings to review and adjust as needed.|
| Harmony | Many harmony scripts to make small adjustments to the game. These scripts, for the most part, can be turned on and off via the blocks.xml|
| Scripts | Many Scripts which include new classes. References to these scripts would be  ```<className>, SCore```  |


[ Change Log ]
Version: 20.5.211.1016

	[ Fire ]

		- Added new Config/blocks.xml entry to allow filtering Material ID.				
			<property name="MaterialID" value="Mplants, Mcorn" /> 	<!-- Checks the material's id to see if it should ignite  -->

		- Order of Material Checks, in terms of priority:
			ID
			DamageCategory
			DamageSurface

		- New block properties have been exposed, over-riding global defaults for FireParticle / SmokeParticle
			<property name="FireParticle" value="#@modfolder:Resources/gupFireParticles.unity3d?gupBeavis05-Heavy" />
			<property name="SmokeParticle" value="#@modfolder:Resources/PathSmoke.unity3d?P_PathSmoke_X" />

			Using NoParticle will skip the particle effect:
				<property name="FireParticle" value="NoParticle" />
				<property name="SmokeParticle" value="NoParticle" />

			To mass add custom particles above to blocks, consider using an xpath similar to this:

			<!-- Search for all blocks that have a Material property called Mcloth, and add in the properties to that block -->
			<append xpath="/blocks/block[ property[@name='Material' and @value='Mcloth'] ]">
				<property name="FireParticle" value=#@modfolder:Resources/gupFireParticles.unity3d?gupBeavis05-Heavy"" />	
				<property name="SmokeParticle" value="#@modfolder:Resources/PathSmoke.unity3d?P_PathSmoke_X" />
			</append>

		- Add new NetPackage in attempt for particles on dedi. Needs testing.

		- Added new MinEvent called AddFireDamageCascade. Defaults to block type.
			This minevent works similar to the Extinguish Minevent, in which multiple blocks of the same type around the target is affected. Rather than extinguish, it ignites those blocks instantly.

			Supported formats:

				<!-- The same type of block -->
				<triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamageCascade, SCore" range="4" filter="Type" />

				<!-- Shares the same material -->
				<triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamageCascade, SCore" range="4" filter="Material" />

				<!-- Shares the same material damage classification -->
				<triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamageCascade, SCore" range="4" filter="MaterialDamage" />

				<!-- Shares the same material surface classification -->
				<triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamageCascade, SCore" range="4" filter="MaterialSurface" />

			Example, if you hit a curtain block, it will catch fire. In addition, all curtain blocks in the range of 4 are also immediately lit.




Version: 20.5.210.1950

	[ Fire ]
		- Changed default buff when you step into the fire to be buffBurningMolotov 
		- Added new particle from guppycur ( commented in Config/blocks.xml )

		- Fire will not be added to trader-protected zones. We have to protect Jen.

		- Added new Config/blocks.xml properties to configure materials and surface categories to xml. Case sensitive.
			Example, and default:
				<property name="MaterialDamage" value="wood, cloth" /> <!-- Checks the material's damage category to see if it should ignite  -->
				<property name="MaterialSurface" value="wood, cloth" /> <!-- Checks the material's surface category to see if it should ignite -->

		- Added new Config/blocks.xml to configure heat per burning block. Default is 1. Setting to 0 disables it.
			<property name="HeatMapStrength" value="1"/> 			<!-- Determines how much each block contributes to heat -->

		- Added new block's property to control how much fire damage the block is to take. Integer.
			This over-rides the default global configuration.

			<property name="FireDamage" value="40" />



Version: 20.5.210.1020
	[ Fire ]
		- Fixed an issue with the particles not working at all. Great job, SphereII.


Version: 20.5.210.859
	
	[ Fire ]
		- Fixed MinEvent on the RemoveFire damage to work at aim point, rather than the barrel of extinguisher
		- Put in a potential fix for dedi particles. Unlikely to work.

Version: 20.5.209.1530

	[ Fire Manager ]
		- Applying Fire Particle on hit, instead of delayed
		- Adjusted the CheckInterval from 10 to 20
		- Updated fire particle from guppycur
		- Updated default fire particle to be Heavy


Version: 20.5.209.1414

	[ Fire Manager ]
		- Added initial implementation for lighting blocks on fire.

			- When a block is added to the FireManager, it is added to a loop that checks:
				-> Is it still flammable?
				-> Has it been extinguished?
				-> Does damage on the block
				-> If damage exceeds the block, it gets downgraded, or replaced with air. (uses block placeholder)
				-> Once all blocks are checked and calculated, it updates the chunks.

		What makes a block flammable or not?

			If it's a child, it won't catch fire. If its air, water, or is near water, it will not catch fire.		
			
			If the block has a tag of 'inflammable', it will not be burnable.

			If the block has a tag of 'flammable', it will be marked as burnable. 
				<append xpath="/blocks/block[@name='terrGravel']/property[@name='Tags']/@value">,flammable</append>
        
			If there is not tag, and passes the other checks, it'll check the block material from materials.xml.
				if its DamageCategory is wood, or if its SurfaceCategory is plant, it will be flammable.

		- New config entry in Config/blocks.xml
		- New Harmony patch on OnEntityWalking, which checks if the block is burning and delivers a buff to an entityalive that walks through it.
		- New Harmony patch to Explosion to include blocks affected by them to catch fire.

		- New test items:
			sphereTorch - When a block is struck, it is lit on fire.
			sphereExtinguish - When a block is struck, blocks in a range of 2 of the strike position is considered extinguished.

	[ Triggered Effect ]
		- Added two new triggered effects to provide support for Fire Manager

			<!-- When an item has this triggered effect hits a block that is flammable, the block will catch fire -->
			<triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamage, SCore" />

			<!-- When an item has this triggered effect hits a block that is on fire, the fire still be extinguished. -->
			<triggered_effect trigger="onSelfDamagedBlock" action="RemoveFire, SCore" target="positionAOE" range="5"/>


Version: 20.5.207.98

	[ Block ]

		New Block to throw an AoE at all times. Full example xml in Config/blocks.xml.

			Example:
			<block name="sphereiiAoETest">
				<property name="Class" value="DecoAoE, SCore" />
				<property name="ActiveRadiusEffects" value="buffCampfireAOE(3)"/>


Version: 20.5.194.1350

	[ Crop Management ]
		- Disabled unlimited water from bedrock block
			- If desired to have bedrock as an unlimited water source, add the following property to the bedrock block:
				<property name="WaterSource" value="true" />

		- Added new property to global crop data that determines how much damage to a water block happens when a crop consumes.
			- This property is defined in the CropManagement block of the SCore's configuration block.
			- Individual plants can over-ride this property if they have it defined on their block.
				- This is done on a block by block basis. If your plant has 3 growth cycles, you will have to define it for each block, with various possible amounts.
			- Default is 1.
			
			<property name="WaterDamage" value="1" />

		- If a water source has a the WaterSource property, it is considered an unlimited source of water, so be mindful of this.

		- When a water pipe is removed, and the particle is still enabled, the particle will now be removed.

Version:  20.5.177.1415

	[ Sleepers ]
		- Merged pull request to wake up sleepers automatically in Active sleeper volumes.

Version: 20.5.174.1938

	[ Portals ]
		- Fixed an issue where the requiredPower wasn't being checked correctly for the animation.
			- Animation may still be a bit wonky, but should be slightly improved.
		- Fixed an issue where the sign data wasn't being saved correctly.

Version: 20.5.174.218

	[ Sleepers ]
	
		- Added the EntityEnemeySDX to the SetSleeperActive(). This was already being done for EntityAliveSDX.

Version: 20.5.169.98

	[ Portals ]
		- Changed location for when animations on portal starts

	[ Quests ]
		- Changed how the EntityEnemySDX and EntityAliveSDX kills are recorded; more in line with vanilla now for MP compatibility.


Version: 20.5.165.1145
	[ Portals ]
		- Fixed a bug with the dialog teleport

	[ Storage ]
		- Reduced default range from 30 to 10


Version: 20.5.162.1349

	[ Portals ]
		- Reproduced and fixed teleporting through buff / dialog and landing on a roof
		- Added samplePortal06 which acts like a non-powered, player-editable test portal.

	[ Storage ]
		- Highly Experimental Feature
		- New property in blocks.xml to turn this highly experimental feature on.
			<property class="AdvancedRecipes" >
				<property name="ReadFromContainers" value="true"/>
				<property name="Distance" value="30" />  <!-- This is not 30 blocks, but rather a distance between you and the container. This is roughly about 6 or 7 blocks.-->
			</property>

		- When set to true, this highly experimental feature will read from containers around the player, pulling items out, to be used in crafting recipes.
			- If crafting is cancelled, this highly experimental feature will dump the ingredients into the player backpack, rather than their original location.

		- With regards to testing this highly experimental feature, high concentrations of containers may induce lag. We may need to put in limits on how many containers it will check.

		Note: This is highly experimental feature. 

	[ Food Spoilage ]
		- If the container's Preserve bonus is set to -99, no food spoilage will occur.
			<property name="PreserveBonus" value="-99" />


Version: 20.5.155.1359:

	[ Quests ]
		Fixed a bug with ObjectiveEntityEnemySDXKill not recording.

Version: 20.5.155.1247

	[ Portals ]
		- Added ability to define a Prefab's nameon the location line. Full example in blocks.xml as samplePortal05.
		- If the destination= portal isn't registered, and the prefab is defined on the location line, the Portal Manager
			will search inside of the first prefab instance it finds, with that name, with that portal.

			<!-- I am samplePortal05, I go to samplePortal03, which is inside a prefab called farm_02 -->
			<property name="Location" value="source=samplePortal05,destination=samplePortal03,prefab=farm_02" />
		
		- Fixed a bug where requiredPower wasn't being accurately checked.
		- Made Portals be ChunkObservers

Version: 20.5.151.934
	
	[ Lock Picks ]
		- Added a new property to be read from Doors. If set to false, the doors are not pickable, and revert to vanilla
			<property name="Pickable" value="false" />

		- If the door has an Owner ( Player-placed), the door will not be pickable.

	[ Quests ]
		- Added new EntityEnemySDX objective
			<objective type="EntityEnemySDXKill, SCore" id="npcHarleyEmptyHand" value="2" phase="3"/>

	[ Food Spoilage ]
		- No code changes, documentation only:
			- Added an example items.xml entry to show how to specify food spoilage on the food items. Turning on the food spoilage in the blocks.xml does not fully activate food spoilage.
			- The blocks.xml needs to enable the food spoilage, and the foot items must contain new properties to allow spoiling.
			- khzmusik has an excellent modlet for this: https://gitlab.com/karlgiesing/7d2d-a20-modlets/-/tree/main/khzmusik_Food_Spoilage

	[ Portals ]
		- Added a check to see if the teleport request is either legacy support or the newer style.
			- This should fix the MinEvent Teleport and other one-way teleports.
			- If a location= only contains a single value, it's determined to be legacy
			- It will check all the portals for destination=, and then check its source=

	[ Dialogs ]
		- Added in a crude extend option which will be expanded more. I wouldn't use this yet, if I were you.

Version:20.5.149.1038
	[ Portals ]
		- When a new portal is added, it checks if the source location already exists, and how many times. If there's already 2, the portal is marked as invalid, and is not linked in.
			- Operates on first set basis. New portals will NOT over-write existing portal locations. They must be removed before they can be re-assigned.
		- Sunsetting Portal, SCore blocks. It'll now pass through to PoweredPortal, SCore
			- Note: Portal2, SCore exists in code to replicate the original code, but not intended to be used.

		- The difference between a Powered Portal and non-Powered Portal is the RequiredPower value.
			<property name="RequiredPower" value="7"/>
			When the property is not set, it defaults to -1, and the portal does not need power to operate.
			Set to 0 to disable a portal from getting power through extends.

		MinEffectTeleport and DialogActionTeleport should accept  location="destination=portal2". 
			- Source is not set up for these methods, unless someone has a use-case.

		- If a PoweredPortal requires power, then the destination must be powered to go there.
			- If its not powered, teleport does not occur.

		Added new samplePortal01b block. Source is samplePortal01cb, but destination is samplePortal01b
			- Add buff buffTeleportTest to teleport to this portal

	[ Quests ]
		- Added SCoreQuestEventManager to manage custom events for future quests
		- Added ObjectiveEntityAliveSDXKill, modelled off ObjectiveAnimalKill
			<objective type="EntityAliveSDXKill, SCore" id="npcHarley" value="2" phase="3"/>
			- Example in quests.xml

		- Merged khzmusik's quest changes
			Adds an objective that is a drop-in replacement for RandomPOIGoto except you can specify:

			tags a POI must have to be included in the search, in the "include_tags" property
			tags a POI cannot have to be included in the search, in the "exclude_tags" property
			A distance from the quest giver, as either a max distance or range (in-game meters)
			All of the properties from RandomPOIGoto are also supported.

			A sample quest is included.

			For servers, a new Net Package implementation is included.


Version: 20.5.147.746

	[ Portals ]
		- Fixed null reference when using Legacy portal in PortalManager
		- Added IsPowered check to Teleport method, and to animate method
		- Added check to confirm destination is powered and available before teleporting.

Version: 20.5.146.1672

	[ Portals ]
		- Added Powered Portals. May need model work to add the 'electrical connection.'
			<block name="samplePortal04">
				<property name="Extends" value="portalMaster"/>
				<property name="Class" value="PoweredPortal, SCore" />
				<property name="DisplayType" value="blockMulti"/>
				<property name="MultiBlockDim" value="3,3,3"/>
				<property name="Model" value="Entities/Electrical/power_switchPrefab"/>
				<!--property name="Model" value="#@modfolder:Resources/gupFuturePortal.unity3d?guppyFuturePortal"/-->
				<property name="Display" value="true" />
			</block>

Version: 20.5.145.728

	[ Caves ]
		- Added sanity check against a negative value for max range - Chasing possible bug with vehicle + cave connection

	[ Portals ]
		- New configuration option for blocks.xml entry for portals

			If the Display property is false, nothing will show up to the user when they are looking at the portal.
			<property name="Display" value="true" /> 

			If the Display is set to true, and the player has the buff "buffyours", they will see "Teleport To <Portal Destination>"
			<property name="DisplayBuff" value="buffyours" />

			If Display is set to true, but the player does not have the buff, they will see "Telport To..."

		- New <property name="Location" value="" /> Changes
			If the value is simply Portal01, then it will act as a two-way portal to the other Portal with the same name.
			MinEffect and Dialog teleport triggers use this syntax as well.

			- New syntax:
				<property name="Location" value="source=Portal01,destination=Portal02" />  I am Portal01, but I send the player to Portal02
				<property name="Location" value="source=Portal01,destination=NA" />  I am Portal01, but I have no destination. I am a one way portal (people can port to me, but not from me)

		- Fixed an issue with PortalMaps not being cleared from one game to another

Version: 20.5.143.1141

	[ Quests ]
		Added in GiveBuff as quest award
			<reward type="GiveBuff, SCore" id="yourbuff" />

	[ Portals ]
		- Removed OnBlockCollided, and OnWalk on triggers. Players must interact with it now.
		- Added 2 second delay before teleport begins, starting after cooldown buff is specified.
			<property name="CooldownBuff" value="buffTeleportCooldown" />
			<property name="Delay" value="1000" /> <!-- Micro seconds, whole numbers only. Default is 1000ms  -->

		- Added XML configuration for fixed portals
			<property name="Location" value="Portal01" />

	[ MinEvent ]
		- Added new MinEvent to teleport the player to a certain location
			<triggered_effect trigger="onSelfBuffStart" action="Teleport, SCore" location="Portal01" />

	[ Dialog ]
		- Added new Dialog action to trigger a teleport. It supports the following, most of which is not tested.

			Teleports to a Portal
			<action type="Teleport, SCore" id="Portal01" />
 
			Teleports to the player's current bedroll
			<action type="Teleport, SCore" id="Bedroll" />
  
			Teleports to the player's landclaim
			<action type="Teleport, SCore" id="Landclaim" />
  
			Teleports to the player's backpack
			<action type="Teleport, SCore" id="Backpack" />
	
	[ Faction Manager ]
		- Fixed null ref for faction manager's update call

	[ XML ]
		- Commented out the error with the PortalPrefab


Version: 20.4.126.2131
	[ NPC ]
		- Allowed override on a per-entity base for 
				<property name="AllEntitiesUseFactionTargeting" value="true" />
	[ Portals ]
		- Preliminary code for setting Portals ready for testing. Portals are fancy signs. Add the same Text to two Portals, and they will be linked.
			- In the case of multiple named portals, the first one discovered will be used.

			Video Example: https://youtu.be/cvYxVzY_lO4

			2 Portal Blocks have been defined in blocks.xml, using models provided by guppycur
				guppyPortalMagic
				guppyFuturePortal
		

Version: 20.4.119.941
	[ NPC ]
		- Added Chunk visible feature for the NPCs.
			When the chunk is not visible, the NPC's gravity will be turned off.
			When thec hunk is visible, the NPC's gravity will be turned on.
			This is a potential fix for NPCs disappearing at bases, with the theory that they are falling out of the world.
			Thanks to closer_ex

	[ UAI ]
		- New Utility AI task. 
			- Look for a PathingCode on the NPC, and start patrolling about between all the codes.
			- NPC will scan for all PathingCubes nearby for a code that matches its Cvar PathingCode.

			- Optional buff will fire when the NPC reaches its target point.
				- Task will not end until the buff expires
				- Once buff expires, NPC will go to its next location.
				- If you want an NPC to walk to a point, and stand there for 10 seconds, have the buff's duration set to 10.


			Basic Example:

			<action name="Patrol" weight="3" entity_filter="IsSelf" >
				<task class="Patrol, SCore" run="false" buff="IsGathering"/>

				<consideration class="HasPathingCode, SCore" />
				<consideration class="EnemyNotNear, SCore" distance="15"/>
			</action>

			Setting pc=5 on a SpawnCube will set nearby NPC to path to cubes with the number 5.


Version: 20.4.116.1950

	[ Blocks ]
		- Re-added crop trample
			Add  fcropsDestroy to the FilterTags to enable:

			<!-- To make all crops destroy on touch -->
			 <append xpath="/blocks/block/property[@name='FilterTags' and contains(@value, 'SC_crops')]/@value">,fcropsDestroy</append>

		- Added Guppycur's sprinkler to Bloom's Family Farming
		- Added the Farmer NPC back for MyFarmer entity in Bloom's

Version: 20.3.113.106

	Demo and water / crop overview: https://youtu.be/ApcwwfexxWU

	"Bloom's Family Farming" Modlet contains sample XML configuration to:
		- Turn on water requirement feature
		- Update all crops to require water
		- Updated metalPipe's to water pipe class
		- Added My Farmer NPC ( Uses Nurse model from 1-NPCCore (required))
		- Added Crop Control Panel
		-Add Farm UAI

		- Use this Modlet with the latest SCore and latest 1-NPCCore for a complete test environment.

	[ Crop Management ]
		- Improved the valve system to be a bit more reliable
		- Fixed BloomTest01 Prefab used for testing: 
			- Includes water tower, valves, pipes, and farm plots
	
	[ UAI ]
		- Improved Farming Task
			- Farmer task now harvests and replants crops

Version: 20.3.111.x
	
	[ Crop Management ]
		- Fixes for the UAI Task for planting/ checking
		- NPCs will be able to:
			a) Look in their inventory to find seeds
			b) Plant those seeds
			c) Once all seeds are gone, they maintain the crops
				- Checking for bugs ( SCore doesn't have bugs, but the plants may)
				- Potentially watering the crops, avoiding water waste
			d) Harvest crops when they are ready, and store crops in their inventory.
				- Any seeds harvested will be replanted.

Version: 20.3.110.1815
	[ Blocks ]
		BlockTakeAndReplace has a new property to allow you to specify which item is allowed. This is a comma delimited list.
			<property name="HoldingItem" value="meleeToolRepairT1ClawHammer" />
			<property name="HoldingItem" value="meleeToolRepairT1ClawHammer,meleeStoneTool" />

		Updated BlockUtilities to add particles centered to the block, instead of at the edge.

	[ Crop Management ]
		- Added new Blocks for support:
			BlockCropControlPanel - Used to debug and control the pipe system
			BlockFarmPlotSDX - Used to update the vanilla's FarmPlots
		-Added new FarmPlotData to hold FarmPlot information
		-Added PlantData to hold plant data
		-Added Pipe class to hold piping information
		-Added FarmPlotManager and WaterPipeManager to handle their tasks.
			- Removed pipe management from crop


	[ Utility AI]
		- Added IsNearFarm consideration 
		- Added UAITaskFarming task

	[ Modlet ]
		Added new Bloom's Family Farming modlet to the SphereII.Mods repo
		This modlet contains XML to turn on and manage the crop management system with examples
		Also includes a prefab called BloomTest01
			- From menu, go into Prefab Tools and load up BloomTest01, then Play test.
		Added SphereiiFlat world to be used for durability testing.

Version: 20.3.105.1149

	[ Quests ]
		- FuriousRamsay has been working on fixing the distance restriction bug on quests
		- New Objectives:   
			RandomGotoSDX, SCore
				<objective type="RandomGotoSDX, SCore" value="700-800" phase="1">
					<property name="completion_distance" value="50"/>
					<property name="nav_object" value="quest" />
				</objective>

			RandomPOIGotoSDX, SCore
				<objective type="RandomPOIGotoSDX, SCore" value="400-800">
					<property name="phase" value="1"/>
					<property name="nav_object" value="quest" />
				</objective>

		- NetPackageQuestGotoPointSDX and NetPackageQuestGotoPointSDX to allow distance checks to work on dedicated servers ( FuriousRamsay )

	[ MinEvents ]
		MinEventActionChangeFactionSDX2: Same as the MinEventActionChangeFactionSDX, except it resets the entity's attack and revenge target ( FuriousRamsay )
			<triggered_effect trigger="onSelfBuffStart" action="ChangeFactionSDX2, SCore" target="self" value="undead" /> 
		
		MinEventActionResetTargetsSDX: Resets the target's attack and revenge targets.
			 <triggered_effect trigger="onSelfDamagedOther" action="MinEventActionResetTargetsSDX, SCore"/>


	[ NPCs ]
		- Added FuriousRamsay's fixes for SetDead(), and OnMission collision ( should be able to fly with NPCs now )
	
	[ Caves ]
		- Fixed issue where random blocks would appear underground during the decoration phase.
		- Modified Cave prefab locations to help avoid isolated prefabs
		- Fixed an issue where the first level of caves did not have decorations.
		- Currently, no cave openings are available when cave types are Mountain or DeepMountain.

	[ Crop Management ]	
		- Initial Implementation of Advanced Crop Management features. This will likely be buggy.

		- There's a lot to unpack here. Buckle up.

		- When enabled, crops will need to be within a 5x5 block radious of water block. 
			This is configurable with the WaterRange on a per-plant basis. Default is 5.
			Each plant will record where it's water blocks are. 
			The CheckInterval is used to determine how often the plant will check its water source.
				- If the water is gone when checked, the plant will rescan for a new block.
				- If no new water block is found, it will wilt.
				- Whenever a plant successfully checks in with water, it will do 1 point of damage to the water.
					- When a water block's damage exceeds its max damage (100 by default), it will turn into air.
			Crop data is not persisted to disk. Rather, if data is missing, it simply rechecks and re-discovers.

		- SCore's blocks.xml contains a new entry:
			<!-- Turns on support for the PlantGrowingSDX, SCore features for more advanced crop -->
			<property class="CropManagement" >
				<!-- Turns on logging to help debug -->
				<property name="Logging" value="false"/>

				<!-- Controls if crops are to be managed -->
				<property name="CropEnable" value="false"/>
				
				<!-- How often the crops should check for water -->
				<property name="CheckInterval" value="600" />

				<!-- How many pipes to scan -->
				<property name="MaxPipeLength" value="500" />
			</property>

			As always, it is not recommended to change the SCore's blocks.xml directly when adjusting, but rather use a modlet:
				<!-- Turning on Crops-->
				<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='CropManagement']/property[@name='CropEnable']/@value">true</set>

				<!-- Checks how often the Crop Manager will check in with the plants. -->
				<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='CropManagement']/property[@name='CheckInterval']/@value">120</set>

			
		- Crops that you want to manage under the advanced crop management must use the new classes:
			BlockPlantGrowingSDX

			It supports the following properties:
				RequireWater: [ true/false ]
					Allows invidiaul plants to control whether they need water or not. 
					- This can be read using extends, but overridden by the individual block
				WaterRange: [ 5 ]
					Allows individual plants to control how far away their water can be.
					- This can be read using extends, but overridden by the individual block
				PlantGrowing.Wilt: [ treeDeadPineLeaf ]
					If a plant goes without water for tool long, it will wilt into this block.
					- Can be air as well.
				Wilt: [ true / false ]: default false.
					- If a plant cannot find a water source, it will wilt and destroy itself.

			Example XML to convert all growable crops to use water.

				<!-- Setting up defaults -->
				<!-- Changing all crops to using the PlantGrowingSDX class -->
				<set xpath="/blocks/block[@name='cropsGrowingMaster']/property[@name='Class']/@value">PlantGrowingSDX, SCore</set>
				<append xpath="/blocks/block[@name='cropsGrowingMaster']">
					<!-- If the crop needs water to survive. Default: false -->
					<property name="RequireWater" value="true" /> 

					<!-- how far away that block can be from a water source: Default is 5 -->
					<property name="WaterRange" value="5" />  

					<!-- The block the crop downgrades too if its dead. Default: Air-->
					<property name="PlantGrowing.Wilt" value="treeDeadPineLeaf"/>

					<!-- if set to true, the plant will die when there is no water near by. -->
					<property name="Wilt" value="true" />
				</append>

		- In order to better support crops, and make them more fun, support for water pipes have been added.
			Two new classes have been written:
				BlockWaterPipeSDX: This block designates a block as a water carrier. It is not a water source itself, but can allow water to 'move' through. 
					- One section of the pipe must touch a water source block.

					- In this example, I convert all metalPipe's to be used as water pipes
						<append xpath="/blocks/block[starts-with(@name, 'metalPipe')]">
							<property name="Class" value="WaterPipeSDX, SCore" />
						</append>

				BlockWaterSourceSDX: This block designates a block as a water distributor. It is not a water source itself.
					- if the BlockWaterSourceSDX is connected to a series of BlockWaterPipeSDX which is connected to a water source block, it will act like a water source block.

					- In this example, I'm using the metalPipeValue as a distributor
						<set xpath="/blocks/block[@name='metalPipeValve']/property[@name='Class']/@value">WaterSourceSDX, SCore</set>


			A water source block is currently defined as:
				BlockLiquidV2 : Water from a river, lake, or dumped from a bucket
				terrBedRock: if you connect a pipe to bedrock, it will act as an unlimited water source.

				Any block is the property WaterSource is set on it, and set to true
					<property name="WaterSource" value="true" />

			




Version: 20.3.101.172
	- Fixed recursive method that was causing crashes on MarkToUnload()
	- Modified TileEntityAlwaysActive patch.
		If any TileEntity has this XML property, it will be Always On.
			<property name="AlwaysActive" value="true" />
		This feature can be used on any other TileEntities to distribute a buff similar to the CampFire's warming buff.
		
		Note: This excludes Forge and Workstation Tile Entities.
	- Disabled the IsQuestGiver on the Entities

Version: 20.3.100.1629
	- Fixed SpawnCube2 issue on dedi where preview was not being cleared.
	- Fixed GotoPOISDX to prefer exact names, rather than wild card

Version: 20 3.100.1044
	- Reverted Sebastian Cave changes, and changed default to Legacy
	- Added working cave entrances
	- Widened cave system to allow more room, and some chaos.
	- Fixed cave spawning of zombies.
		- Spawning may be a bit sparse, due to the many levels of the cave systems and max zombies.
		Suggestions on fleshing it out more would be to add in SpawnCubes to the blockplaceholders that spawn more zombies.
	- Fixed double spawns on SpawnCube on dedi
	- Turned off SmarterEntities by default in blocks.xml, to get rid of the path node warnings.
		- Max revisit later if functionality has changed due to turning it off.
	- Turned off Sound when NPC is on a mission.


Version: 20.3.93.840
	- Reduced the height position of the SetPosition() to calm their leader when on a mission.
	- Overrode ProcessDamageResponse() not to process damage if NPC is on a mission
	- Overrode IsImmuneToLegdamage, returning true if OnMission.
	- Fixed possible null ref in IsInertEntity()
	- Updated MarkToUnload to go to base class when NPC is ordered to Stay or Patrol. 
		- This is an attempt to fix the disappearing NPCs at the bases.

Version: 20.3.84.x

	- When driving in a vehicle, NPCs will no longer teleport to you after a distance, but does a SetPosition() on a tighter range
	- NPCMOD-FT-0045 : Added DespawnNPC MinEffect: <triggered_effect trigger="onSelfBuffRemove" action="DespawnNPC, SCore" />
	- Added new Property for entities, which controls if they run "leader checks" for increase performance. Defaults to True.
		<property name="Hirable" value="false" />
	- Added new Property for entities, which controls if they can give or process quests. Defaults to true.
		<property name="IsQuestGiver" value="false" />


Version: 20.2.83.1240
	
	- Performance Tweaks:
		- Disabled DrawLine in Utils from raycasting path finding
		- Disabled Progression on NPCs
	- Updated Pathing Cube to disallow non-owners to edit
	- Similar change to the SpawnCube with above change
	- Added TimeToDie for EntityEnemySDX
	- Added DialogAction for AddItemSDX, SCore

Version: 20.2.60.742

	- Merged khzmusik's fix for infinite resource bug

 Version: 20.2.57.x
	- Loot task work.
	- SpawnCube breaks after trigger now.

Version: 20.2.56.x
	- Added more enhancements for SpawnCube2SDX. See blocks.xml for examples on advanced configuration.
	- Fixed issue with Sleepers not getting the correct buffs on waking up.
	- Fixed issue with Attack UAI being locked when target was still visible but not accessible.

Version: 20.2.55.x
	- Fixed issue with bad ray cast, causing NPCs to shoot through doors and walls
	- Multiple fixes to the BlockSpawnCube2SDX. This cube will allow you to spawn in a one-time entity.
		Example Usage:
	        <property name="Class" value="SpawnCube2SDX, SCore" />
			<!-- Will spawn NPC furiousRamsayBristonStart_FR, give them the buffOrderStay, pathing cube 0, and auto-assigns yourself as their leader -->
            <property name="Config" value="ec=FuriousRamsayBristonStart_FR;buff=buffOrderStay;pc=0;leader=true" />
			
			<!-- Will spawn the NPC and ordered to Stay. They do not have a pathing code, no leader -->
			<property name="Config" value="ec=FuriousRamsayBristonStart_FR;task=stay" />


		Note: There is a delay of about 5 seconds on MP servers for the leader to be auto-assigned.

	- Added support for new cvar called FailOnDistance.
		- If this cvar is set on the player, all hired NPCs will use this FailOnDistance range, rather than the one hard coded in the utilityai.
		- If this cvar is set on the NPC, then that NPC will use that range, instead of the one on the player or utilityai.


Version: 20.2.45
	- Added IsAlwaysAwake to be false. This allows Sleeping NPCs to be fully awake on spawn in.
		XML Property can be toggled with the following line:
			<property name="SleeperInstantAwake" value="false" />
			<property name="SleeperInstantAwake" value="true" />

	- Reduced the Auto-scan for pathing cubes to be a lot smaller (about a block away)
	- Default PathingCode is -1, so avoid this number, as its equivalent to not having a code.
	- Gave sleeping NPCs hearing for a threhold to wake them up.
	- Disable UAI when NPCs are sleeping

	- Slightly changed the consideration for InWeaponRange, to take in effect if it should shoot a player, if it could see it, but not reach it.
[ XUI ]
	- Added a few more bindings to npc stat window

Version: 20.2.44

	- Added toggle to turn on and off Advanced signs (0-SCore/Config/blocks.xml) to turn off gif / img support 
	- Moved Sign reading data to a universal class to allow EntityAliveSDX to re-use code
	- Added EntityAliveSDX to read from a nearby Pathing Cube
		Using the PathingCube (or PathingCube2) blocks, you can place an invisible sign near a sleeper
		Once placed, you can configure it as follows
			Task=stay  [ wander, guard, follow ] 
				- This will give the nearby EntityAliveSDX a starting order. 

				Example, you could put in Task=stay for NPCs that you want to behave like a turret

				Internally, the above tasks gives SCore's based-buffs  ( buffOrderStay, buffOrderWander, etc)
				Task can also be used to specify a general buff.

		Example: https://youtu.be/IuK_sN3WHZ4

	- Fixed a null ref where NPC following a NPC leader
		With this error fixed, the old herd spawner now works. This spawner is a special EntityClass that you can add to your sleeper volume, or in entitygroups, and will spawn multiple entities.
		This can be used, for example, to set up a Solider Group, which could have an officer, along with a few grunts. The soldiers will follow the officer around as they would if hired by a player.

		For this to work, you must give them the NPCHired, or equivalent, UAI. These do not need to be hirable by the player, but must contain the follow tasks.

		When you spawn in the SoldierPackMelee entity, it'll spawn the leader and followers.

		Example: https://youtu.be/OSlfwuX9r6Y

		EntityClasses.xml example:

		<append xpath="/entity_classes">
			<entity_class name="spawnerStub">
				<property name="Mesh" value="Gore/gore_block1_bonesPrefab"/>
				<property name="ModelType" value="Custom"/>
				<property name="Prefab" value="Backpack"/>
				<property name="Class" value="EntityAliveEventSpawnerSDX, SCore"/>
				<property name="Parent" value="Animals"/>
				<property name="TimeStayAfterDeath" value="1"/>
				<property name="IsEnemyEntity" value="false"/>
				<property name="LootListOnDeath" value="banditLoot"/>
				<property name="Faction" value="animals"/>
			</entity_class>
		
			<!-- the Leader is spawned in first, then the Followers. The Followers set their Leader cvar to that of the Leader. -->
			<entity_class name="SoldierPackMelee" extends="spawnerStub">
				<property name="Prefab" value="Backpack"/>
				<property class="SpawnSettings" >
				<property name="Leader" value="survivorOfficerDPistol" />
				<!-- Params 2,3. This means that 2 to 3 of each of the followers will be spawned. -->
				<property name="Followers" value="survivorSoldier1Club,survivorSoldier2Knife,survivorSoldier4Machete" param1="2,3"/>
				</property>
			</entity_class>

			<entity_class name="SoldierPackRanged" extends="spawnerStub">
				<property name="Prefab" value="Backpack"/>
				<property class="SpawnSettings" >
				<property name="Leader" value="survivorOfficerTRifle" />
				<property name="Followers" value="survivorOfficerAK47,survivorOfficerSMG,survivorSoldier1TRifle,survivorSoldier3AK47" />
				</property>
			</entity_class>
		</append>

Version: 20.2.43.147

	- Version bump to catch up to the Alpha 20.2

[ NPC Fixes ]

	- Due to the refactoring for the damage to block code, some issues are listed as potentially fixed only.

	- NPCMOD-FT-0038        NPCs be given the ability to damage blocks

		Tested:
				<action name="MeleeAttackBlock" weight="3">
					<task class="AttackTargetEntitySDX, SCore" action_index="0"/>
					<consideration class="PathBlockedSDX, SCore"  />
					<consideration class="TargetType" type="Block"/>
				</action>
		- Recommended that NotPathBlockedSDX, SCore for approach-style tasks
	
	- NPCMOD-SP-0071        Hired NPC names show up twice above their heads ( Potentially )
	- NPCMOD-SP-0062        Melee NPCs run max distance away from their target then begin their approach ( Potentially )
	- NPCMOD-SP-0075        NPCs will get stuck on certain blocks (like trees) ( Potentially )
	- NPCMOD-SP-0048        NPCs can talk to the player while fighting, until they are hit by an enemy
		- Added a call to the UAI System IsEnemyNearBy() call (10 distance).
	- NPCMOD-DEDI-0006    NPCs can be hired by another player after you've hired them and log out
		- Changed the GetLeaderOrOwner() call to just check for existence of cvars as a gap solution, 
			rather than expecting the entity to be online in order for it to be valid.

	- Added a new fun task ( Work in Progress. Don't use in production )
				<!-- Allows the entity to place a timed charge on a door -->
				<action name="DoorBuster" weight="4">
					<task class="BreakBlock, SCore" action_index="0"/>
					<consideration class="PathBlockedSDX, SCore"  />
					<consideration class="TargetType" type="BlockDoorSecure"/>
				</action>

	- Many, many UAI tweaks, cleaning up old code, ironing out behaviours.
	- Gound work for expanding the available ItemActions 2,3,4.
	- Added MeleeSDX to allow non-EntityEnemy's target blocks effectively.

[ UI ]
	- Fixed the NPCStatWindow to update properly
	- Fixed Respondent Name to display

Version: 20.0.39

[ NPC Fixes ]
	Initial implementation of Break Blocks functionality.

[ Block Changes ]
	- Added the ability to trigger SpawnCube when placed by a player.

	Sample XML, which will spawn an empty handed nurse on placement, and ordered to stay:
		<block name="TestSpawnCube">
			<property name="Extends" value="SpawnCube"/>
			<property name="Config" value="ec=npcNurseEmptyHand;buff=buffOrderStay;pc=0" />
		</block>

Version: 20.0.38

[ NPC Fixes ]
	NPCMOD-SP-0072  Hired NPC names disappear from life bars when you enter a vehicle
	NPCMOD-SP-0076  Limit opening of doors to IsHuman or IsHired
	NPCMOD-SP-0073  Hired NPC footsteps can be heard when you hop on a bike and travel
					- Also disable footsteps when flying tags is used
	NPCMOD-SP-0075  Potentially fixed:  NPCs will get stuck on certain blocks (like trees) 

	Hired NPCs which are ordered to Stay will unload when a chunk is unloaded, and re-load back in.
	Merged Food Spoilage patch


Version: 20.0.33.1824

[ Dialog ]
	- Made Dialog requirement Hired simpler

[ NPC Fixes ]
	- Removed NPCs that were told to guard / stay not to show up in Companion screen
		- This leaves room for NPCs that are currently with you.

[ Fixes ]
	- Fixed a bug with QuickContinue when steam wasn't running at all.

Version: 20.0.32.x

[ Debug Information ]
	- Added Logging for when NPC has died, it will display its active buffs in the log file

[ NPC Fixes ]
	- Fixed an issue where NPCs would get confused when two players were nearby
	- Fixed an issue where NPCs wouldn't talk to you without a very long delay after a fight

Version: 20.0.30.x

[ Configuration Change ]
	- Disabled Advanced Workstation from being enabled by default

[ NPC Fixes ]
	- Hired NPCs will drop their backpacks, if anything is in them
	- Fixed Bandit loot containers



Version: 20.0.29.x

[ Fixes ]
	- Fixed null ref when trying to get a block outside of world bounds in GetGamePath.
	- Fixed Spook's SkyManager reference with IsBloodMoonVisible.
	- Connected progression into lock picking, and adjusted default difficulties ( adjustments in the Configuration Blocks.xml of the SCore)
	- Fixed a potential issue with Buried Supplies not working when Trader Protection is removed
		- Note to Self: If you want to test Clear Zombies quest, remember to enable Enenmy Spawning. It'll save a lot of confusion.

[ XUI Changes ]
	- Added new XUiC_NPCStatWindow for Sirillion hooks
	- Fixed issue where a null reference would happen on empty strings.

[ Quest Changes ]
	- Added NetPackage to better handle GotoPOISDX
	- Added Biome Filtering for GotoPOISDX:
		<property name="biome_filter_type" value="ExcludeBiome" />
		<property name="biome_filter" value="wasteland" />
	- Added additional debug for GotoPOISDX. Enter in dm mode to see.
	- Note: If a quest does not show up on the map, confirm you have nav_object property inside the objective
		<property name="nav_object" value="return_to_trader" />
	- Changed GetPOIPrefabs() to GetDynamicPrefabs() for searching for GotoPOISDX
	- Fixed null ref when PrefabName was not set.
	- Created a QuestUtils static class to share code between NetPackage and GotoPOIS

[ NPC Changes ]
	- Added hired NPCs to show up in Companions window (Configuration block option to toggle)
	- Color adjusted the hired NPCs on compass to match companion window, when enabled.
	- Partial hook up for NPCs to earn experience
	- If an entity is dead, pass the CanTakeDamage checks. The dead get no reprieve.
	- Fixed wander where the position was in the air, causing NPCs to stop trying to move there.	
	- Hired NPCs will no longer take damage from traps (BlockDamage). 
		If they are smart enough to get paid, they are smart enough to step carefully through barb wire
	- If NPCs have anything in their bag (backpack)  or loot container (what the player has access too), a bag will drop with the contents.
		-  If no contents, then no bag drops.

[ Buffs.xml Changes ]
	- New Buff orders:
		buffOrderGuardHere Sets cvar to Order 2, and sets guardPosition to where the player is standing.

	- Updated Buff Order:
		buffOrderStay updated to set guardPosition to be where the NPC is standing.
		

[ Dialog Changes ]
	- Added new Dialog requirements:  
		<requirement type="HasPackage, SCore" requirementtype="Hide" value="NPCAnimalBasic" /> <!-- true if this UAI Package is on this NPC-->
		<requirement type="HasPackage, SCore" requirementtype="Hide" value="!NPCAnimalBasic" /> <!-- true if this UAI package is NOT on this NPC -->
		<requirement type="HasTask, SCore" requirementtype="Hide" value="LootBasic" /> <!-- True if this UAI Action Name is on the NPC -->
		<requirement type="HasTask, SCore" requirementtype="Hide" value="!LootBasic" /> <!-- True if this UAI Action name is NOT on this NPC -->

[ UAI Changes ]

	- Added new UAI Task called Guard. This task can replae the Stay action using IdleSDX.
		- Guard is similar to Stay and can be a replacement for Stay.
		- Two new MinEffectActions have been created for this, and is applied by the buff in the SCore's buffs.xml
			- MinEventActionGuardHere : This sets the NPC's guard position to match the player's current position, as well as the direction the player is looking.
			- MinEventActionGuardThere: This sets the NPC's guard position to match where the NPC is currently staying, with the look direction the same.
	- Added additional feature for the Wander UAI task:
		<task class="WanderSDX, SCore" interest="Loot:cntBirdnest"/>
	
		When interest is specified, there is a 30% chance triggering a TileEntity scan matching the value.
	
		For example, interest="Loot" will search for all Loot containers in the area, and pick one.
	
		If the TileEntity's entry has a :, that further allows filtering based on block name.
		
			<task class="WanderSDX, SCore" interest="Loot:cntBirdnest"/> <!-- scan for any loot container that has a block name that starts with cntBirdNest -->


		If interest is not specified, a standard wander is triggered.
		
	- Fixed a bug in AttackTarget where an NPC that was told to stay, would take a few steps out of position to attack

[ Mecanim Animation ]
	- Added simple null check when the entity using Mecanim is invalid.