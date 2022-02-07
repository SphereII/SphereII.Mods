#0-SCore

The 0-SCore is the key component to enable extra functionality for 7 Days To Die. Using a mixture of Harmony, SDX Patch Scripts, and Scripts, new features are enabled for modders and players to use.

| Folder | Description |
| ----------- | ----------- |
| Config | XML Files that serve as documentation, basic setup for required items. blocks.xml contains many default settings to review and adjust as needed.|
| Harmony | Many harmony scripts to make small adjustments to the game. These scripts, for the most part, can be turned on and off via the blocks.xml|
| Scripts | Many Scripts which include new classes. References to these scripts would be  ```<className>, SCore```  |

[ Change Log ]
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