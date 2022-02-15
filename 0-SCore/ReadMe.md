#0-SCore

The 0-SCore is the key component to enable extra functionality for 7 Days To Die. Using a mixture of Harmony, SDX Patch Scripts, and Scripts, new features are enabled for modders and players to use.

| Folder | Description |
| ----------- | ----------- |
| Config | XML Files that serve as documentation, basic setup for required items. blocks.xml contains many default settings to review and adjust as needed.|
| Harmony | Many harmony scripts to make small adjustments to the game. These scripts, for the most part, can be turned on and off via the blocks.xml|
| Scripts | Many Scripts which include new classes. References to these scripts would be  ```<className>, SCore```  |

[ Change Log ]

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