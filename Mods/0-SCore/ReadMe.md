#0-SCore

The 0-SCore is the key component to enable extra functionality for 7 Days To Die. Using a mixture of Harmony, SDX Patch Scripts, and Scripts, 
new features are enabled for modders and players to use.

| Folder | Description |
| ----------- | ----------- |
| Config | XML Files that serve as documentation, basic setup for required items. blocks.xml contains many default settings to review and adjust as needed.|
| Harmony | Many harmony scripts to make small adjustments to the game. These scripts, for the most part, can be turned on and off via the blocks.xml|
| Scripts | Many Scripts which include new classes. References to these scripts would be  ```<className>, SCore```  |
|Features | Features will contain all the code necessary for a particular feature, grouping the code so it can be easily found and extracted. |

### Direct Downloads
Direct Download to the 0-SCore.zip https://github.com/SphereII/SphereII.Mods/releases/latest

### Change Logs
Summary for 2.0 Update:
This release of 0-SCore introduces significant enhancements across several core systems, with a strong emphasis on **Shared Reading**, **NPC behaviors (including farming and combat)**, **block placement controls within POIs**, and **performance optimizations**.
**Key Highlights:**

* Shared Reading System: A major new feature allowing party members to share unlocked content from books and items. This includes new localization entries and fixes for server and client-side issues.
* Improved NPC AI and Behaviors:
	* Farming: Reworked Utility AI for farming tasks, making farmers more reliable, preventing task locks and accidental destruction of farm blocks, and keeping them closer to their farms. Sprinklers can now be individually controlled and detect water sources more effectively, and can even extinguish fires.
	* Combat/General: Fixed issues with NPC bandit weapon handling, enabled patrol points for EntityEnemySDX, and exposed more configuration options for NPC movement (e.g., `BlockTimeToJump`, `BlockedTime`).
	* Dialog: Patches for improved dialog functionality, including displaying statements in the subtitle window for EntityAliveSDX and allowing dialogs to inherit and combine from multiple sources using an "extends" property.
* POI Building Restrictions: Introduced a new patch and configuration options to prevent players from placing blocks within specific POI bounds, based on prefab names or tags. This aims to maintain the integrity of designed POIs.
* Performance and Refactoring:
	* Fire Manager V2 & Food Spoilage V2: Both systems underwent significant AI-assisted refactoring to improve performance, breaking down classes into helper classes and cleaning up code.
* Localization Enhancements: Added a new localization method to ensure localized entries are always retrieved, even if the direct key is missing (checking for `Name` or `Desc` suffixes). Also added support for `<include>` tags in Localization files, allowing for better organization.
* Bug Fixes and Stability: Addressed various null reference errors, spamming issues with spoiled items, durability bar disappearing, and general migration/refactoring for broken references and changed parameters.



[ Change Log ]
Version: 2.1.2.1825

	[ Audio Patch ]
		- Fixed an issue with score running against 2.1

	[ ItemActionLauncherSDX ]
		- Fixed an issue where the NPC's with rocket launchers was not shooting past 1 or 2 rockets.

	[ UAITaskAttackTargetEntitySDX ]
		- Fixed an issue where the NPC's IsReloading() was being incorrectly checked. That functionality does not work on NPCs.

	[ Particles On Blocks ]
		- Fixed an issue where biomeProvider was null.

	[ Documentation ]
		- Restructed XPath and Conditionals documentation.

	[ onSelfItemRepaired ]
		- Added a Harmony patch to trigger the minevent effects on an item being repaired.
			- Patched via the QuestManager's event.

	[ MinEvent Action ]
		- Added a new MinEventAction that handles modifying the quality of items.
				<triggered_effect trigger="onSelfItemRepaired" action="ModifyItem, SCore" >
		- Added new Requirement to check against the quality of an item.
			<requirement name="CompareItemProperty, SCore" property="Quality" operation="Equals" value="1"/>

		[ See More ](Features/ItemRepairDegradation/ReadMe.md)


Version: 2.0.24.1245
	[ Spawn Cube ]
		- Cleaned up SpawnCube2SDX
		- Added new BlockSpawnCubeRepeater, that will tick and spawn entities over time.
	        <block name="DeamonPortal2">
    	        <property name="Extends" value="DeamonPortal"/>
        	    <property name="Class" value="SpawnCubeRepeater, SCore"/>
	            <property name="Model" value="@:Entities/Vehicles/TraderVehicles/traderMountainBikeStaticPrefab.prefab"/>
	            <property name="ModelOffset" value="0,0,0"/>
	            <property name="MaxDamage" value="250"/>

	            <property name="EntityGroup" value="ZombiesAll"/>
	            <property name="SpawnRadius" value="5"/>
	            <property name="SpawnArea" value="15"/>
	            <!-- Spawn 2 each tick -->
	            <property name="NumberToSpawn" value="2"/>
	            <!-- Total number of ticksbefore the block self-destructs -->
	            <property name="MaxSpawned" value="10"/>

	            <!-- How many ticks between the spawn times-->
	            <!-- NumberToSpawn spawns each time it block ticks. -->
	            <property name="TickRate" value="10"/>
	        </block>

	[ Documentation ]
		- Added new Examples documentation that shows examples on how to accomplish 0-SCore features.

Version: 2.0.23.1213
	[ EntityMoveHelper ]
		- Removed old StartJump block based on BlockedTime, which caused NPCs not to jump.

	[ MinEventActionChangeFactionSDX ]
		- Added code to reset attack target, removing temporarily the aggro switch.

	- Added documentation 
		
Version: 2.0.22.1718

	[ Recipes ]
		- Added support for generateing a MinEvenParams package, which allows more MinEvents to be used on the onSelfItemCrafted.
		- These seem to only trigger when looking in a workstation and an item is creted.
		- Now will respect requirements.
		Examples:

		<effect_group name="Sphere Testing">
		    <triggered_effect trigger="onSelfItemCrafted" action="AddAdditionalOutput, SCore" item="resourceYuccaFibers" count="2"/>

		    <triggered_effect trigger="onSelfItemCrafted" action="AddAdditionalOutput, SCore" item="ammoRocketHE" count="2">
			    <requirement name="HasBuff" buff="god"/>
		    </triggered_effect>

		    <triggered_effect trigger="onSelfItemCrafted" action="PlaySound" sound="player#painsm">
			    <requirement name="!HasBuff" buff="god"/>
		    </triggered_effect>
		    
		    <triggered_effect trigger="onSelfItemCrafted" action="AddBuff" buff="buffDrugEyeKandy"/>

	[ EntityAliveSDX ]
		- Changed a walk type from 4 to 21 for crawler.

Version: 2.0.21.1719
	[ Fire Manager ]
		- Fixed a Crash To Desktop ( CTD ) when adding fire particles off main thread.
		- Added support for random fire particles, using "," as a delimiter.

	[ Drop Box ]
		- Fixed another 2 issues with Drop Box eating items when nearby storage was opened.

	[ A Better Life ]
		- Fixed entityclasse references for Fish

	[ Recipes ]
		- Added a new feature to trigger multiple outputs when crafting a recipe.
		- Example: in addition to ammo45ACPCase, this will also produce grass fibres and duct tape.
    <recipe name="ammo45ACPCase" count="30" craft_time="5" craft_area="MillingMachine" tags="workbenchCrafting,PerkHOHMachineGuns">
        <ingredient name="resourceBrassIngot" count="5"/>
        <effect_group name="Additional Output">
			<triggered_effect trigger="onSelfItemCrafted" action="AddAdditionalOutput, SCore" item="resourceYuccaFibers" count="2"/>
			<triggered_effect trigger="onSelfItemCrafted" action="AddAdditionalOutput, SCore" item="resourceDuctTape" count="1"/>
		</effect_group>
    </recipe>

		- Note: The AddAdditionalOutput MinEvent is only usable by this recipes hook. It will do nothing in any other context.
		
Version: 2.0.20.1639
	[ ItemAction Repair ]
		- Fixed multiple null references when attempting to repair an item while the player was wearing it. 

	[ Challenges ]
		- Fixed an issue with the StartFire challenge when there was no fire manager.
		- Fixed an issue with ExtinguishFire

	[ Drop Box ]
		- Fixed an issue where the DropBox was distributing items to containers that was opened by another player, and disappearing.

	[ Trader Currency ]
		- Fixed an issue where the currency wouldn't refresh.

	[ ItemActionMelee ]
		- Added two events for when a zombie misses its hit.
			onSelfPrimaryActionEnd
			onSelfPrimaryActionMissEntity


Version: 2.0.19.1555
	[ Blocks.xml ]
		- Updated a reference to a vanilla block for a mesh

	[ Shared Reading ]
		- Fixed an issue where a connecting player would not share their reading with a player that is hosting.

	[ Blooms Family Farm ]
		- Added conditional for NPC farm to only be available if NPC Core is loaded.

	[ Blood Moon Tweak ]
		- Added new property to AdvancedZombieFeatures configuration block that allows you to increase the default enemy active during blood moons
		- Previously, it was fixed to max out at 30.
                <!-- Vanilla is default to 30. -1 disables this patch. -->
                <propert name="EnemyActiveMax" value="-1" />

	[ SCoreLocalization Helper ]
		- Fixed a dumb implementation in a less dumb way.

EnemyActiveMax

Version: 2.0.17.1140
	[ TileEntity IsAlwaysActive ]
		- Fixed an issue where isAlwaysActive was blocking regular tile entities from showing they are Active.

Version: 2.0.16.2016
	[ Fire Manager ]
		- Fixed an issue where Challenge Objectives related to Fire would null ref if used.

Version: 2.0.15.1644
	[ EntityAliveSDX , EntityNPCBandit ]
		- Removed the Walk Type 8 filter from the Crouch after stun reset.
		- Fixed an issue where the NPC wouldn't use the right/left hand as they were supposed too for the various weapons.

Version: 2.0.14.1511
	[ Block ]
		- Updated block Model reference to support new format.

	[ Fire Manager ]
		- Cleaned up an error when game is exiting and fire manager is not enabled.

	[ Trader Currency ]
		- Added a check to see if the trader was already in the dictionary.
		- Added a check to store the original currency, and use in place of casinoCoin

	[ NPC Core ]
		- Fixed an issue where NPCs would get stuck in a crouch after stun
		- Updated code in EntityAliveSDX and EntityBanditSDX for GetEyeHeight() checks


Version: 2.0.12.1509
	[ Disable Trader Protection ]
		- When DisableWallVolume is enabled, the invisible wall volumes will be removed.
		- New property under AdvancedPrefabFeature to enable it:
			<!-- Disables the invisible wall behind traders -->
			<property name="DisableWallVolume" value="false" />

	[ Fire Manager ]
		- Fixed an issue where a POI reset would cause it to fill with extinguished smoke.
		- Updated reference to the SCoreMedium loop for sounds.

	[ Trader Currency ]
		- Added the ability to change a particular trader's currency, using the alt_currency attribute.
				<trader_info id="8" reset_interval="3" open_time="4:05" close_time="21:50" alt_currency="oldCash">
		- When a player talks with a trader, with the alt_currency, it will update the backpack's currency display to use that value.

	[ SphereII Peace of Mind ]
		- Replaced a few new hanging corpses with empty pillar
		- Fixed issue here zombiePartyGirlCharged was throwing warnings


 Version: 2.0.11.824
	[ Shared Reading ]
		- Removed "Learned" from the Tooltip
		- Added a new Localization method to try to always get a localized entry
			- In some cases, there's no Localization directly from the key
			- Example:  
				craftingRifles
				- In those cases, it'll check for craftingRiflesName, craftingRiflesDesc, and finally craftingRiflesLongDesc

	[ SCoreLocalizationHelper ]
		- Wrote a static helper class to cycle through various Localization attempts.
		- This was added to make the sharedReading code a bit cleaner.

Version: 2.0.10.1059 - PreRelease
	[ Farming ]
		- Converted RequireWater and WaterRange to be Auto properties, allowing them to be changed.

	[ Shared Reading ]
		- Fixed an issue where shared reading wasn't working properly on dedicated servers

Version: 2.0.8.920 - Prerelease
	
	[ Blocks ]
		- Added a new patch that allows blocking placing blocks without POI bounds.
			- This does not affect repairing or upgrading.
		- New Config block entry under AdvancedPrefabFeatures
			<!-- Property to check if the prefab name has a no building flag on it. Goes by prefab name. -->
			<!-- Comma delimited -->
			<property name="PrefabName_NoBuilding" value="" />

			<!-- Property to check if a Prefab has this tag on its XML. Comma delimited. -->
			<property name="PrefabTag_NoBuilding" value="NoBuild" />

		- A prefab that has any of these conditions will block placing any blocks within its bounds:
			- If a Prefab has a Tags set to the PrefabTag_NoBuilding
			- If a prefab has a Localized Name, or filename that patches PrefabName_NoBuilding.
				- This is a contains() check, so be explicit unless you want to cover multiple POIs.

	[ Shared Reading ]
		- Fixed a null reference when a client was reading a book.
		
Version: 2.0.7.1008 - Prerelease
	[ Replace Material ]
		- Added a Debug log for ReplaceMaterial to print out the materials on the entity spawned.
		- This will print a log entry if the Debug "Show Tasks" is enabled for showing EAI information.

	[ Shared Reading ]
		- Added new Feature under Advanced Player Feature for Shared Reading
		- Default is False.
		- Any item read with the property "Unlocks" will share the event with all online party members.
		- If an item has the property "NoSharedReading", it will not share it with party members.
		- New Localization.txt entries:
			sharedReading,"Learned"
			sharedReadingDesc,"Shared Reading from"
			sharedReadingSourceDesc,"Sharing To Party Members"
		- All Party members will see  "Shared Reading from sphereii : Learned <Unlocks Value>"
		- The reader will see "Sharing to Party Members : Learned <Unlocks Value>"

	[ Food Spoilage ]
		- Fixed an issue where spoiled items would get spammed
		- Fixed an issue where spoilage counter would be reset
		- Fixed an issue where the durability bar would disappear.
	
	[ SCoreConstants ]
		- Fixed typo in SCoreConstants

Version: 2.x Initial
	[ Migration ]
		- Updated broken references. 
		- Renamed changed parameters in blocks
		- General refactoring.

	[ EntityPlayerLocal ]
		- Added SharedReading, defaulted to false the blocks.xml
			- If enabled, all items with the property "Unlocks" is shared with party members.

	[ EntityEnemySDX ]
		- Uncommented Read and Write methods to allow patrol points to be used

	[ Patches ]
		- Added Patch for Dynamic Music, triggering a null reference if NPCs are within trader areas
		- Added a patch to prevent null references if a loot list name isn't set for NPCs
			- This triggered Null ref when trying to open up an NPC's death bag.

	[ NPCs ] 
		- Fixed an issue where EntityNPCBandit did not have any weapons in their hand.
		- The dialog window has changed, and is missing a property we relied on for NPCs to speak.
			- A new dialog window will have to be written in the future.
			- Currently using the Subtitle dialog window for this.

	[ Fire Manager V2 ]
		- Major AI assisted refactoring to improve performance.
		- Broke the class into several helper classes and cleaned up the code.
		- New code is in FireV2. Old code is still there, but not included into the build.
		- Added a check to see if there's sprinklers around to extinguish the fire.

	[ Food Spoilage ]
		- Major AI assisted refactoring code to improve performance.
		- New code is in FoodSpoilageV2. Old code is still there, but not included into the build.

	[ Config Blocks ]
		- Exposed The BlockTimeToJump value to XML. 
			- This determines how long an NPC should be blocked before allowing them to jump.
		- Exposed the BlockedTime value to XML. 
			- This determines how long an NPC should be blocked before considering they are blocked and need to do something else.

	[ EntityAlive SDX ]
		- Added new property to change the default window group being opened for dialog.
            <property name="dialogWindow" value="dialog" />

	[ SCore Constants ]
		- Created a new class that can be used to centralize some variables to be used elsewhere.

	[ Utility AI ]
		- Adjusted all the block time checks to use the SCoreConstants class.
		- Added NotHasHomePosition as an inverse check
		- Added new TerritorialSDX task that wanders within the home range.
		- Integrated https://github.com/SphereII/SphereII.Mods/issues/83
                    // If the weight of this action is lower than the current high score, then even More actions
                    // the highest consideration score of 1 can't make this the winning action.
		

		[ UAI Farming Task ]
			- Modified Consideration HasHomePosition as a simple if its set or not.
			- Re-factored UAIFarming Task to handle new piping, and restructuring.
			- Fixed an issue where the farmer would get task locked
			- Fixed an issue where the farmer would destroy random farm blocks.
			- Modified the Bloom's Utility AI with new considerations and default tasks
				- The default tasks will be used when NPC Core is not loaded
			- Added Territorial to default, keeping the farmer close to the farm while still wandering about it.

	[ Farming Systems ]
		- Adjusted BlockWaterSourceSDX to better detect when water is available to the sprinkler
		- Added the ability to interact with it to turn off individual sprinklers.
		- Added the ability for water to extinguish nearby fires
		- Added the ability for sprinklers to rescan when a pipe is removed or added.

	[ Dialog ]
		- Added patch to allow EntityAliveSDX to display dialog statements in the Subtitle window
		- Added a patch to enable extends on dialogs, to inheirt and combine multiple dialogs.
		- The extends is a comma delimited list of other dialogs to merge.
        	<dialog id="FrankieFarmerDialog" startstatementid="start" extends="trader,GenericZombieLoreDialog">

	[ Localization ]
		- Added a patch to support <include for Localization files
		- The <include could be added any place in the Localization.txt file.
		- Each included localization file needs to have a header.
			Key,english
			<include filename="Dialogs/Farmer/Localization.txt"/>
			<include filename="Dialogs/General/ZombiesDialog.txt"/>
			<include filename="Dialogs/General/TraderSurvival.txt"/>
	

Version: 1.3.24.1230
	[ NPCs ]
		- Added support for the Auto-Stach buttons to work with NPCs.

	[ AdvancedItemsFeatures ]
		- Added a new property to the AdvancedItemsFeature in the blocks.xml
	        <property name="DisableScrapFallback" value="false"/>
		- This feature is only enabled AdvancedItemRepair is set to true.
		- If this feature is enabled, but RepairItems / ScrapItems is not defined, then the item will scrap using vanilla.
		- Current behaviour allows an item to be scrapped using a reduced recipe based on its ingredients, without defining RepairItems / ScrapItems.

		- Added ScrapItems support for blocks.

	[ Version Checker ]
		- Received donated Code from Yakov that allows modders to add a versioncheck.xml file to their overhauls
		- This versioncheck.xml file can be anywhere in the Mods folder
		- This versioncheck.xml file contains the expected game version, along with optional messages to display.
		- If this versioncheck.xml file is found in the Mods folder,
			- Reads the current game version, and compares it to the expected game version.
			- If the version matches, the main menu loads normally.
			- If the version mismatches, a message box is shown to the players, saying the version does not match.
			- The message box has a Quit and Continue button, allowing the user to keep going if they really want too.
		- Example file can be found under Features/VersionCheck/versioncheck.example
		- Added localization support.


Version: 1.3.7.1037
	[ Challenges ]
		- Added multi-option to the loot_list= for the Gather Challenge. This is a comma-delimited list.
	
	[ Craft From Containers ]
		- Added new Config option in Blocks.xml to check if a container is within Landclaim bounds, rather than distance.
		    <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedRecipes']/property[@name='LandClaimContainersOnly']/@value">true</set>
		- Default is false, do not restrict to landclaim, but rely on distance.

		- Added new Config option in Blocks.xml to check if the player is within landclaim bounds.
		    <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedRecipes']/property[@name='LandClaimPlayerOnly']/@value">true</set>
		- Default is false, do not restrict player from being within a landclaim bounds.

	[ Dynamic Bone ]
		- Added test code for a new dynamic bone system.

Version: 1.3.2.1535
	- No Code changes. Rebuilt against Game Version 1.3.

Version: 1.2.68.1007
	[ Material Modifier ]
		- With approval from Zilox, consumed the Material Modifier mod into 0-SCore to take over maintenance going forward.
			<triggered_effect trigger="onSelfFirstSpawn" action="ReplaceMaterial, SCore" 
				target_material_name="HD_Arlene_Radiated" replace_material="#@modfolder:Resources/ww_zeds_1.unity3d?HD_Arlene_Rad"/>
	
	[ Challenges ]
		- Added an item_tags to the CraftWithIngredient Challenge to check the tag for an ingredient
			<objective type="CraftWithIngredient, SCore" count="2" item_tags="tag"/>
			- This can work with ingredient= as well, and mix and match.
			- Tag is checked first, then ingredient name.
			- If tag is found, then it counts towards the challenge, but does not again check the ingredient name.

		- Added a PlaceBlockByTag objective that checks for block tag when a block is placed.
			<objective type="BlockPlaceByTag, SCore" count="2" block_tags="myTag"/>

		- Added a loot_list attribute to GatherTags. It varies that the loot container is opened, with the loot_list name, before counting.
			- This will not be 100% accurate, as it counts the items you have, and not necessarily how many you are grabbing from the loot container itself
	            <objective type="GatherTags, SCore" loot_list="garbage"  item_tags="junk" count="10"/>




Version: 1.2.61.2007
	[ Fire Manager ]
		- Fixed a potential threading issue with fire particles.

Version: 1.2.59.838
	[ Fire Manager ]
		- Fixed a net package setup that was causing bad performance
		- Adjusted how the netpackages are sent to the clients and recieved by the clients
		- Reduced the information distributed via net packages

Version: 1.2.57.733
	[ On Block Added ]
		- Fixed a null reference in onBlockAdded patch when loading prefab editor.

Version:1.2.56.900
	[ NPCs ]
		- Fixed a few issues with null references when adding NPCs to storage.
		

Version: 1.2.54.1354
	[ NPCs ]
		- Fixed an issue where NPCs could be quick stacked into Drop box and other storage units

	[ Challenges ]
		- Fixed an issue where Stealth Kills would trigger twice, resulting in credit of 2 for 1 kill.

Version: 1.2.53.1812
	[ Challenges ]
		- Added in a missing block tag check on the BlockUpgrade challenge

Version: 1.2.52.1518

	[ Challenges ]
		- Fixed various issues with counting killed entities towards challenges
		- Refactored the SCore base class a bit to handle the bugs
		- Stealth kills should be working as expected now.
		- Decaptation challenges should be working better.
		- KillWithItem challenges should be working better.
		- Fixed issues where Fire-related challenges was not running for clients connecting to servers.

	[ Goto POI SDX ]
		- Fixed an issue where the POI's name was not being localized.

	[ Requirements ]
		- Fixed an issue with the IsBloodMoon requirement check

	[ Auto Redeem Challenges ]
		- Fixed a potential null reference.
			- Probably just a timing issue, but better safe then sorry.

	[ UAI ]
		- Applied fixes to the UAI Farming Task
			- Changed how the Entity determines its looking at the target plot
			- Changed the distance check kto determine if its close enough to the target plot.

	[ Fire Manager ]
		- Changed FireManager from a polling method to a Monobehaviour, attached to the GameManager's transform.
			- This should reduce it spinning on Updates every frame.
		- Fixed a few issues with netpackages, and distributions. Maybe.
		- Refactored a few calls to make them easier to call from other scripts.
		- Added in a new property, called "FirePersists". 
			- If this is set to true, fire will be saved.
			- Default is not saved when the game is unloaded / loaded.

		- Added new patch to OnBlockAdded to check for property to start a block on fire.
		- When this property is on a block, and is set to true, it'll automatically catch fire.
			<property name="RegisterToFireManager" value="true" />

		- Changed how a block picks its Fire Particle.
		- A Fire Particle can be defined on a block's material, a block, or in the global block configuration, using this syntax:
			<property name="FireParticle" value="@modfolder:..." />

		- Fire particle that will be used on any given block will now be determined by in this order of priority: 
			- A block's material
			- A block
			- Default fire particle defined in global block configuration.
				- If Random Fire Particle is set to true, a random fire particle will be used instead of the global block configuration.
			


Version: 1.2.37.1146
	[ NPCs ]
		- Reverted a patch that was causing null ref on player death.
		- This patch was meant to block people stashing NPCs.

Version: 1.2.36.1142
	[ NPCs ]
		- Fixed an issue where a null reference would happen in a harmony patch for the Stash All.

	[ Cave Spawning ]
		- Fixed an issue where cave spawning was too sensitive.

Version: 1.2.35.852
	[ NPCs ]
		- Fixed an issue where NPCs could be added to a storage box using the Stash All button.

	[ Path Finding ]
		- Accidentally reverted the sign for path finding.
		- Intentionally restored the change.

	[ Entity Factory Patch ]
		- Added a patch to EntityFactory to catch "GetEntityType slow lookup for" for SCore-related classes


Version: 1.2.29.952
	[ Fire Mod ]
		- Additional performance fixes when playing on servers

	[ Caves ]
		- Fixed issues with decorations being too few
		- Cleaned up old staglamites which do not exist anymore
		- Fixed cave spawning issues, and refactored the class

	[ EntityAliveSDX ]
		- Changed the default name of Bob to empty string.

	[ Error Handling ]
		- Added a ConfigBlock entry for a null reference in BlockEntityData.GetRenderers()
			- This error would be thrown sometimes when a POI was being reset
			- This feature must be set to true to guard against the null check. Default is false.
				<property name="BlockEntityDataGetRenderers" value="true" />


Version: 1.2.8.1136
	[ NPCs ]
		- Fixed an issue where NPCs could be added to Drone

Version: 1.2.4.1601
	[ Block Triggered SDX ]
		- Fixed an issue with the ActivateOnLook check

Version: 1.2.2.2032
	- Updated Cave Spawning to fix against 1.2
	- Updated other broken code from the 1.2... minor adjustments

	[ Lock Pick ]
		- Added a null check on entityalive before checking if they have a cvar.

	[ Challenges ]
		- Fixed a grammar issue in a comment

Version: 1.1.62.918
	[ Repair Counter ]
		- Added a new Harmony patch to monitor how often an item can be repaired, before blocking the repair.
		- This only works on Items, repaired through ItemActionEntryRepair.
		- Two formats are supported:
			- Comma delimited value. This will allow you to adjust max repairs based on quality
			- For non-quality items, or to simplify, a single value can be used for all teirs.
	 		
  				<append xpath="/items/item[@name='meleeToolRepairT0StoneAxe']">
					<!-- The first value is quality 1. The last value is quality 6 -->
  					<property name="RepairLimit" value="1,2,3,4,5,6" />
  				</append>

				<append xpath="/items/item[@name='meleeToolRepairT0TazasStoneAxe']">
					<property name="RepairLimit" value="5" />
				</append>
		- Localization Key is: repair_limit_reached

	[ Block Spawn Cube 2SDX ]
		- Adjusted the code again to try to spawn just a single entity.

Version: 1.1.54.933
	[ Fire Manager ]
		- Fixed the laws of physics or whatever laws there are that govern how fire spreads.
		- Fire now spreads.

Version: 1.1.53.1237

	[ Fire Manager ]
		- Functionality should be the same, but performance should be increased quite a bit.
		- Added Smoke Time to be on its own timer
		- Changed the main check loop to only process 1 block per frame
		- Changed blocks that are destroyed or extinguished so they are collected as in a list.
			- At the end of the Check Update, a single netpackage is sent with all the changes.
			- Previously, a net package was sent for each block in the loop.
		- Rather than playing sounds at the location of fire, another check is done every second to see if any player is near fire,
			- If a player is near fire, it'll play the defined FireSound in the player's head.
			- This was the biggest source of improvement.

	[ SpawnCube2SDX ]
		- Modified the OnBlockAdded to only add a Tick event if there's more than 1 entity to spawn.

Version: 1.1.49.1701
	[ SpawnCube2SDX ]
		- Added an additional check to see if an entity has already spawned, and blocks further spawns.

	[ Challenges ]
		- Fixed an issue with Craft With Ingredient, where an item had no recipe, causing a null reference.

	[ EntityAliveSDX ]
		- Removed a debug log about Weapon not found, but was actually there.

	[ Take And Replace ]
		- Added a new property that will trigger the drop event Harvest.
       		<property name="HarvestOnPickUp" value="true" />
		- If this property is set to true, the following drop event style will be triggered:
            <drop event="Harvest" name="resourceCrushedSand" count="9" tag="oreWoodHarvest"/>
            <drop event="Harvest" name="resourceClayLump" count="9" tag="oreWoodHarvest"/>
		- The block itself will only do the harvest; it will not give you the PickUpValue back.
		- By default, Harvest On pick up is false.

Version: 1.1.42.847
	[ ConfigurationBlock ]
		- Added new section called "AdvancedQuests" to allow more control over quests.

	[ Fire Manager ]
		- Added a null check for the NetPackage for AddFirePosition
		- Removed extra checks that may have been block fire from being cleared on quest reset

	[ GotoPOISDX ]
		- Added new Property block in ConfigurationBlock called AdvancedQuests
		- New Property value in AdvancedQuests block in ConfigurationBlock for re-using quest locations
		- If "ReusePOILocations" is set to true, it will not filter quest locations based on if they were already visited.

	[ SpawnCube2SDX ]
		- Added potential fix for duplicate spawns.

Version: 1.1.36.1627
	[ Client Kill Event ]
		- Changed ClientKill() patch to be a Prefix vs Postfix to fix an issue where it'd fire multiple times
			- ie, chopping up a dead body would count as a kill for each hit.

	[ Fire Manager ]
		- Fixed an issue where a molotov would not trigger the OnStartFire
		- This caused a molotov not to trigger the Start A Fire Challenge
		- Fixed an issue with the NetPackage for calling AddBlock instead of Add(), skipping player assigning
			- Fixes an issue with the challenge Fire Started on dedicated servers.

	[ Remote Crafting ]
		- Added an additional property to AdvancedRecipes for Checking if Enemy is nearby
		- BlockOnNearbyEnemies is now available in both BlockUpgradeRepair and AdvancedRecipes.
				<property name="BlockOnNearbyEnemies" value="false"/>
		- Previously, this toggle was defined in BlockUpgradeRepair only, but used to block crafting as well.

	[ Error Handling ]
		- Default is false, these errors are NOT handled. Change to true to use them, under ErrorHandling.

		- Added a ConfigBlock entry for a null reference in TraderData.ReadInventoryData()
			- This error would be thrown sometimes when a POI was being reset, and a workstation / vending machine
			went from working / non-working. 
			<property name="TraderDataReadInventory" value="false" />

		- Added a ConfigBlock entry for TileEntity.CopyFrom()
			- This error could occur during POI resets. Base class for CopyFrom throws an exception. This blocks it.
				<property name="TileEntityCopyFrom" value="false" />
			- When set to true, it will also log an entry in the log file saying which block its causing on.
			- It may be a sign the block is not configured correctly.
                Debug.Log($"ErrorHandling::TileEntityCopyFrom::Prefix:: {_other.blockValue.Block.GetBlockName()}. No Defined CopyFrom()");


Version: 1.1.28.1028
	[ Farming ]
		- Added "MuteSound" to the BlockWaterSourceSDX to turn off sprinkler sound.
			<property name="MuteSound" value="true" />
		- Default is false, the sound is not muted.

		- Added GetWaterRange(), RequireWater(), and WillWilt() public methods as part of the BlockPlantGrowingSDX
			- No functionality change, just makes it easier for others to read values through code.

Version: 1.1.22.1530
	[ Challenges ]
		- Added a description_override attribute to completely over-ride the Localization key to the following Challenges:
		- if description_override= does not exist, a generated Localized entry will be used.
			- ChallengeObjectiveBlockDestroyed
			- ChallengeObjectiveBlockUpgrade
			- ChallengeObjectiveCompleteQuestStealth
			- ChallengeObjectiveCraftWithIngredients
			- ChallengeObjectiveCraftWithTags
			- ChallengeObjectiveCVar
			- ChallengeObjectiveDecapitation
			- ChallengeObjectiveEnterPOI
			- ChallengeObjectiveGatherTags
			- ChallengeObjectiveKillWithItem
			- ChallengeObjectiveStealthKillStreak

	[ Events ]
		- Fixed an issue with OnBuffAdded not parsing multiple buffs
		- Removed Debug Log
	

Version: 1.1.21.1123
	[ Challenges ]
		- Fixed an issue where Block Upgrade did not do properly localization
		- Fixed an issue where the WearTags was not working properly on mods

		- Added a description_override attribute to completely over-ride the Localization key to the following Challenges:
		- if description_override= does not exist, a generated Localized entry will be used.
			- ChallengeObjectiveHarvest
			- ChallengeObjectiveWearTags
			- ChallengeObjectiveCraftWithTags
			- ChallengeObjectiveCraftWithIngredient

Version: 1.1.20.1108

	[ Challenges ]
		- Expanded support for WearTags Objective to support installable_tags and modifier_tags.
             <objective type="WearTags,SCore" item_tags="armorHead"/>
             <objective type="WearTags,SCore" item_mod="modGunBarrelExtender"/>
             <objective type="WearTags,SCore" installable_tags="turretRanged"/>
             <objective type="WearTags,SCore" modifier_tags="modGunBarrelExtender"/>

Version: 1.1.18.1635

	[ Documentation ]
		- Added some Documentation on Challenges and MinEvents

	[ OnBuffAdded Event ]
		- Added a if buff is null to the OnBuffAdded event, silently failing if the requested buff does not exist.

	[ ObjectiveBuffSDX Quest Objective ]
		- Added a if buff is null check, silently failing if the requested buff does not exist.

	[ Min Events ]
		- Found a few MinEvents that were combined in a single file.
		- Seperated so that each MinEvent is in its own file.
		- No changes necessary. This is just a clean up.

	[ Challenges ]
		- WearTags : Takes an item_tags, rather than an Item name.
			- Also searches for the tag in Mod / Cosmectic slots.

			Default Localization Key: challengeObjectiveWearTags
            <objective type="WearTags,SCore" item_tags="armorHead"/>

		- GatherTags : Takes an item_tags instead.
			Default Localization Key: challengeObjectiveGatherTags
            <objective type="GatherTags, SCore" item_tags="junk" count="10"/>

		- Craft With Tags
			Default Localization Key: challengeObjectiveCraftWithTags
			<objective type="CraftWithTags, SCore" count="2" item_tags="tag01"/>

		- Get CVar
			Default Localization Key: challengeObjectiveOnCVar
			<objective type="CVar, SCore" cvar="myCVar" count="20" description_key="onCVar" />

	[ Nexus Release ]
		- Fixed an issue where the zip files were not in the correct format for Vortex.
 
	[ Block Ground Patch ]
		- There is a bug with the PathingCubes and quickly moving into their chunk while being loaded.
		- Throws a Block.GroundAlign null error because there is no ebcd (yet?)
			NullReferenceException
				at (wrapper managed-to-native) UnityEngine.Component.get_gameObject(UnityEngine.Component)
				at Block.GroundAlign (BlockEntityData _data) [0x0001f] in <e8e43063270440388d2e6b7642da1a62>:0
				at ChunkManager.GroundAlignFrameUpdate () [0x00028] in <e8e43063270440388d2e6b7642da1a62>:0
				at GameManager.gmUpdate () [0x00393] in <e8e43063270440388d2e6b7642da1a62>:0
				at GameManager.Update () [0x00000] in <e8e43063270440388d2e6b7642da1a62>:0

Version: 1.1.10.1307
	[ Food Spoilage ]
		- Fixed an issue when using PreserveBonus -99, where a full stack would instant spoil.

Version: 1.1.9.2008
	[ Faction Manager ]
		- Added a Harmony Patch to GetFactionByName() to catch for invalid factions.
		- If a faction is requested from an entityclass, but it's not defined in npc.xml,
			the undead faction is used.
		- A message in the console is printed when a faction was not found.

	[ POI Error Check ]
		- Added in two Harmony patches, gated by two new blocks.xml entry.
		- Under the ErrorHandling section:
				EnablePoolBlockEntityTransformCheck
				LogPoolBlockEntityTransformCheck
		- Some POis were throwing errors about block entity's without a proper transform:
			BlockEntity {0} at pos {1} null transform!
			2: {0} on pos {1} with empty transform/gameobject!
		- These were being thrown in the Chunk class.
		- These two patches block that error from being thrown, and silently returns.
		- The LogPoolBlockEntityTransformCheck will throw an error, but it'll tell you which block it's failing at.
		- Both these should be false, unless you are specifically having a problem
			
	[ TileEntitySign Gif ]
		- Fixed an issue where some older signs did not have the correct amount of transforms
		- ie, pathing cubes

	[ Challenges ]
		- Fixed an issue with the StartAFire / Extinguish Fire where any entity would contribute

	[ Fire Manager ]
		- Updated the Fire Manager's StartFire / ExtinguishFire event takes an entity ID.

Version: 1.1.4.1542  
	[ Entity Targetting ]
		- Updated the code for the ItemItemAction to first check if it's hitting an EntityAlive
		- Then checks if the entity alive is dead. If so, let the damage through.
		- If it's an EntityAlive, and it's alive, do the faction checks.

Version: 1.1.4.1015
	[ SCore Options ]
		- Hide SCore Options window when in prefab editor

	[ ConfigBlock ]
		- Pathing cube reverted to 1,1,1 multidim.

	[ Challenges ]
		- Fixed a typo in the comment section of StealthKill showing an example.

	[ EntityAliveSDX ]
		- Changed the requirements for when to check if an EntityAliveSDX is near a campfire.
		- Moved UpdateBlockStatusEffect from EntityAliveSDX to EntityUtilities

	[ Entity Alive Patch ]
		- Added a patch to EntityAlive's OnUpdateLive()
		- If an entity has this cvar "UpdateBlockStatusEffect" with a value of non-0, then it will process the BlockStatus Effect
		- This means it'll pick up buffs and effects from blocks like BlockAoE.

	[ One Block Crouch ]
		- Exposed the crouch height modifier to xml, with the default being still 0.49 in code, and through xml.
		- Any numbers below 0.10 will be set to 0.10.
		- No max threshold is protected.
		- This is adjusted in the AdvancedPlayerFeatures's section of the Config Block called PhysicsCrouchHeightModifier
		    <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedPlayerFeatures']/property[@name='PhysicsCrouchHeightModifier']/@value">0.45</set>

	[ Merged in Raycast change from khzmusik ]
		From Commit Notes: 
		- Disable ray hit events when entities can't be damaged
		- This is a Harmony patch on `ItemActionDynamic.hitTarget` to disable the firing of "on[SelfPrimary|Secondary]Action[Ray|Graze]Hit" events 
			if the holding entity can't damage the target entity.

		- This is the cause of the stun baton "shock" issue with friendly NPCs. The stun baton itself wasn't hitting or doing damage, but the 
			baton was still charging, and when fully charged, it still shocked the friendly NPC.

		- I also refactored the logic to determine when an entity should use faction targeting. It was already repeated in two 
			places in the code, and I would need to repeat it a third time in my patch.
		- I did test to make sure that there were no regressions. I tested with a friendly NPC (Baker), and enemy NPC (Harley), and animal (bear), 
			and zombies (mainly Boe). I made sure that I could still shock everyone except the Baker, and that when they attacked each other, they did damage.

Version: 1.0.94.1336
	[ Challenges ]
		- Created an Interface for challenges to help future development work

	[ Entity Alive Ground Detection ]
		- Added a patch to stop a vanilla custom trader that was stuck in a fall post.
			- Fix is to add vanillatrader tag to the trader

	[ Maintenance ]
		- Cleaned up some code that was never used, that was failing on 1.1.

Version: 1.0.93.1757
	[ Challenges ]
		- Fixed an issue with Decapitation challenge
		- Fixed an issue with the BlockUpgrade challenge

Version: 1.0.93.1534
	[ Challenges ]
		- Added another patch to protect against potential errors when loading saves
	
	- KillWithItem:
		- Changed main localization entry to challengeKillWithItemDesc

	[ Caves ]
		- Initial add for new cave system based on a texture2D.
		- GenerationType is "Texture2D" to activate and test. 
		- Sample Cave03.png is provided.
		- Red Spots are POIs
		- No support yet to control depth.

Version: 1.0.89.1020
	[ Challenges ]
		- Added BlockUpgradeSCore, SCore
			- Adds the ability to specify a block_tag/block_tags attribute.
			- Adds the ability to filter based on biome.
			
		 	<objective type="BlockUpgradeSCore,SCore" block="frameShapes:VariantHelper" count="10" 
				held="meleeToolRepairT0StoneAxe" needed_resource="resourceWood" needed_resource_count="8" />
			<objective type="BlockUpgradeSCore,SCore" block_tags="wood" count="10" 
				held="meleeToolRepairT0StoneAxe" needed_resource="resourceWood" needed_resource_count="8" />
			<objective type="BlockUpgradeSCore,SCore" block_tags="wood" count="10" biome="burnt_forest" />

Version: 1.0.88.1030
	[ Challenges ]
		- Added target_name_key parsing to KillWithItem.
		- Added Localization key for KillWith Item
			challengeKillZombiesWithItemDesc

		- Added Item_tags to be validated, on the Harvest Objective.
			- If item="" and item_tags="" are both defined, they both need to pass 
			
	[ NPCs ]
		- merged 2 fixes from  khzmusik 
			- Fixed a see cache issue
			- Fixed a faction tracking issue

Version: 1.0.86.1506
	[ Challenges ]
		- Another patch to fix challenges being corrupted

Version: 1.0.86.1304
	[ Challenges ]
		- Fixed an issue where the Challenges were getting corrupted by conflicting enum values
		- This probably needs a new Save to work.
		- Added a new ObjectiveSCoreBase to allow us to make more challenges without duplicating a bunch of repeated tags.

		- Added a new Challenge that fires whenever an NPC is being hired.
			<objective type="HireNPC, SCore" count="20" />
    	    <objective type="HireNPC, SCore" count="5" target_name="npcNurseKnife"/>
        	<objective type="HireNPC, SCore" count="5" entity_tags="female"/>
        	<objective type="HireNPC, SCore" count="5" entity_tags="male"/>
        	<objective type="HireNPC, SCore" count="5" entity_tags="male" biome="burnt_forest"/>
        	<objective type="HireNPC, SCore" count="5" entity_tags="female" item="resourceCropGoldenrodPlant"/>
        	<objective type="HireNPC, SCore" count="5" entity_tags="male" item_tags="brass"/>
        	<objective type="HireNPC, SCore" count="5" entity_tags="male" item_material="Mwood"/>

		- Added a new challenge to give more control over harvest
     		<objective type="Harvest, SCore" count="20" item="resourceWood" held_tags="axe" biome="burnt_forest" />
			<objective type="Harvest, SCore" count="20" item="resourceWood" held_tags="axe" block_tag="challenge_pallet" />

Version: 1.0.83.950
	[ Events ]
		- Fixed a null reference with the OnClientKill when the game would trigger the event after the game killed.

	[ Challenges ]
		- Added an optional cvar attribute for StealthKillStreak.
		- This cvar will hold the longest recorded kill streak.
			<objective type="StealthStreak, SCore" count="2" cvar="longestStreakCVar" />

	[ SCore Utilities ]
		- Created a window group for the SCore Utilities button
		- Added it to the xui.xml, rather than the existing in game-menu
		- Removed the background and cleaned up the SCore Utilities creen.

Version: 1.0.82.935

	[ Fire Mod ]
		- Added Events:
			- OnExtinguish - Triggered when a fire is extinguished
			- OnFireUpdate - Triggered when a CheckBlocks on the fire mod finishes
			- OnStartFire - Triggered when a new block is put on fire.

	[ Challenges ]
		- Fixed a null reference with the KillWith
		- Fixed up LocalizationKey entry for some Challenge Objectives

		- Decapitation
			- New challenge to keep track of decapitations. Supports xml attributes from KillWithItem
	            <objective type="Decapitation, SCore" count="10" item_tag="gun"  />
            	<objective type="Decapitation, SCore" count="10" item_tag="knife,machete"  />

		- Craft With Ingredient
			- New challenge objective to allow a player to craft with a certain ingredient, rather than a recipe itself.
				<objective type="CraftWithIngredient, SCore" count="2" ingredient="resourceLegendaryParts"/>

		- Burn Down a Building
			- New challenge to reward a player for burning down a house using the fire mod.
	            <objective type="BlockDestroyedByFire, SCore" count="20" />

		- Start a Fire
			- New challenge to reward a player for starting a fire.
	            <objective type="StartFire, SCore" count="20" />

		- Out of Control Fire
			- New challenge to reward a player for having a large fire
	            <objective type="BigFire, SCore" count="20" />

		- Extinguish Fire
			- New challenge to reward a player for extinguishing some blocks.
	            <objective type="ExtinguishFire, SCore" count="20" />
			- World's more boring challenge for a pyro

		- Break a block or a material on a block, and further define if it's in a certain biome and/or poi / poi_tag.
			<objective type="BlockDestroyed, SCore" count="20" block="cntRetroFridgeVer1Closed" />
			<objective type="BlockDestroyed, SCore" count="20" block="cntRetroFridgeVer1Closed" biome="burn_forest"  />
			<objective type="BlockDestroyed, SCore" count="20" block="cntRetroFridgeVer1Closed" biome="burn_forest" poi="traderJen" />
     		<objective type="BlockDestroyed, SCore" count="20" material="Mmetal" biome="pine_forest" poi_tags="wilderness" />

		- Updated SphereII Challenges with examples. 

Version: 1.0.81.1118

	[ SCore Events ]
		- Added two new events:
		- OnRallyPointActivated
			- When a Rally Point is activated, this event is called
		- OnClientKill
			- When an entity gets killed, this event is called.
			- This event differs from the vanilla's Kill event, as it includes the DamageResponse.
				- Vanilla event fires too early to have the Damage Response included in it.

	[ Project ]
		- Added new SphereII Challenges project.
		- This includes a full page of Challenges that involve the new SCore events and new Challenge Objectives

	[ Process Options ]
		- Fixed an issue where the graphic settings were not running
		- Added a check for the blocks.xml for some settings, before reading the cvar.
	
	[ Lock Picking ]
		- Added a buff check to see if the Jail Breaking buff was active
			- If active, break time is drastically increased, as well as the max angle to turn.
		- Feel a bit limited on how to further cheese a skilled based game on candy.

	[ Remote Crafting / Repair ]
		- Added a tool tip to display if there's an enemy nearby, blocking you from pulling
		- Fixed an issue where you could not repair even from your backpack, when enemies were near.

	[ Challenges ]
  	
		-CompleteQuestStealth, SCore
			A new challenge objective to monitor your stealth kills during a quest.
			To pass this challenge, you must do consecutive stealth kills until you've reached the desired count.
			If the stealth kill chain is broken, the Challenge is reset.

			If the intention is that the full quest be done 100% stealth, set the count to be higher than the expected number of zombies
			Once the Sleeper volumes are all cleared for the QuestObjectiveClear, then the challenge will complete, regardless if
			the Count is equaled to the count specified.

			Examples:
				<!-- Kill two entities in a row with a stealth kill, during a quest. -->
				<objective type="CompleteQuestStealth, SCore" count="2"/>

				<!-- Kill all entities in a row with a stealth kill, during a quest. -->
				<objective type="CompleteQuestStealth, SCore" count="1000"/>

		-KillWithItem, SCore

			To pass this challenge, you must killed zombies with the specified item. This extends the KillByTag objective, and thus supports
			all those attributes as well. Multiple item="" can be listed as a comma-delimited list.

			<!-- Kill two zombies in a row with a gunHandgunT1Pistol -->
			<objective type="KillWithItem, SCore" count="2" item="gunHandgunT1Pistol" />

			Rather than item name itself, you could also use item_tag
			<objective type="KillWithItem, SCore" count="2" item_tags="handgunSkill"  />

			ItemName is checked first, then item tags. If either passes, then vanilla code is checked for the other tags and checks.
			You may also add the option entity tags and target_name_key for localization.

			By default, the entity_tags and target_name_key is zombie and xuiZombies, respectively. These can be over-ridden via xml.

			<objective type="KillWithItem, SCore" count="2" item="gunHandgunT1Pistol" entity_tags="zombie" target_name_key="xuiZombies" />

			Other attributes available are:
				target_name="zombieMarlene"
				biome="snow"
				killer_has_bufftag="buff_tags"
				killed_has_bufftag="buff_tags"
				is_twitch_spawn="true/false"

			Note About Traps: 
			If the item is a trap, such as a landmine, then the owner ID is not set on it. There is no way to track down, or give exp to a player
			for the kill. I did add a check for this, so that you can use a landmine for a challenge. 

			If a zombie dies from a trap, and the player is within 50 blocks, it will count towards the challenge.

		- StealthStreak Challenge 
			A new challenge objective to monitor your stealth kills
			To pass this challenge, you must do consecutive stealth kills until you've reached the desired count.

			If a kill is registered, and is not a sneak kill, then the challenge resets.
		
			<!-- Kill two entities in a row with a stealth kill -->
			<objective type="StealthStreak, SCore" count="2"/>

			This challenge extends from KillWithItem, and supports all those tags as well.

Version: 1.0.75.721
	[ Farming ]
		- Fixed a null reference error when world is null / not ready.

Version: 1.0.72.659
	[ Process Options ]
		- Added additional checks again a player being null or being on dedi.

Version: 1.0.71.2148
	[ Remote Crafting / Repair ]
		- Fixed an issue where 2x the amount of resources was available. Display only bug.
		- Removed debug lines.

Version: 1.0.70.1352
	[ Remote Crafting / Repair ]
		- Fixed an issue where ItemActionEntryRepair wasn't detecting ingredients from an open container.
		- Added checks for IsUserAccessing to check if the current user has a chest open, allowing them to pull resources from that container.

	[ CanSway ]
		- Fixed a null reference when exiting a game.

Version: 1.0.65.1554

	[ No Changes to SCore, only Better Life ]

	[ SphereII A Better Life ]
		- 1.0.19.1554
		- Fixed an issue where errors would occur without NPC Core installed, due to factions.


Version: 1.0.64.2041
		[ SCore Options ]
			- Added toggle for WeaponSway
			- Added Toggle for gfx st set 0
				- Game needs to be restarted after checked.
				- Every start after will have that automatically running.
			- Added Toggle for gfx pp enable 0
				- Game needs to be restarted after checked.
				- Every start after will have that automatically running.

		[ Weapon Sway ]
			- Added patches to vp_Weapon / vp_Camera to block swaying and bob code from running.
			- This feature is configured via a cvar called "$WeaponSway". 
				- Value of 0 will enable sway.
				- Value of 1 or more will disable sway.
			- This is included into 0-SCore, and as a standalone mod.
				- The standalone mod disables it completely, with no option to toggle it back on.
			- This feature can be toggled using the SCore Utilities, or through a console command
			- Added new console Command weaponsway
				- "weaponsway true" will turn on weapon and camera sway.
				- "weaponsway false" will turn off weapon and camera sway.

		[ Zombie Random Walk Type ]
			- Removed default Spider / Crawler walk types
			- Spider's head position look wonky, and crawlers were too slow.
			

		[ A Better Life ]
			- Added notrample tag on fish
			- Merged in new plants from blue name.

Version: 1.0.60.1241
		[ MinEvent ]
			- Added a new MinEvent to allow changing local transform / rotation on a particular transform on an entity.
				<triggered_effect trigger="onSelfBuffUpdate"
					action="AdjustTransformValues, SCore"  
					parent_transform="AK47"
					local_offset="-0.05607828,0.07183618,-0.02150292"
					local_rotation="-3.98,-9.826,-5.901"
					<!-- Optional. Defaults to false -->
					debug="true"  
				/>

		[ Console Command ]
			- Added a new console command to assist testing of the above MinEvent. 
			- Use this cautiously. 
			- Example:
				ReloadSCore buffs
				ReloadSCore entityclasses
			
		[ Particles On Block ]
			- Fixed a few issues where particles were being loaded incorrectly, causing a hard crash
			- Added a patch on the init to pre-load Particles
			- Added a check for to keep a particle upon removal of its block
				<property name="PeristAfterRemove" value="false" />

			- Somewhat realistic example:
		    <append xpath="/blocks/block[contains(@name,'emberPile')]">
        		<property class="Particles" >
            		<property name="OnSpawnParticle" value="#@modfolder(0-SCore_sphereii):Resources/gupFireParticles.unity3d?gupBeavis02-CampFire,#@modfolder(0-SCore_sphereii):Resources/gupFireParticles.unity3d?gupBeavis03-Cartoon,#@modfolder(0-SCore_sphereii):Resources/gupFireParticles.unity3d?gupBeavis04-SlowFire,#@modfolder(0-SCore_sphereii):Resources/gupFireParticles.unity3d?gupBeavis06-HeavyLight"/>
            		<property name="OnSpawnProb" value="0.1"/>
					<property name="PeristAfterRemove" value="false" />
        		</property>
    		</append>


Version: 1.0.59.1007
		[ Food Spoilage ]
			- Added missing FreshnessOnly check on the ModifyCVar minevent patch.
			- The FreshnessOnly patch to execute if the item has the "FreshnessBonus" property on the item, and it's set to true.


Version: 1.0.58.1256
	[ Resharpen ]
		- Fixed an issue where I confused UseTimes with Quantity.
		- When you sharpen an item, it'll remove a random amount from the UseTime, capped at 20% of the total max usages
		- Added 2 no location entries to be over-ridden in Localization.txt
			TooDamagedToSharpen,"This item is too worn out to be resharpened.",""
			NotDamagedEnoughToSharpen,"This item is still in pretty good shape.",""
		- Cleaned up the code to make it seem like it wasn't developed by a drug addled raccoon.
		- If you have used the item 70 times, with a max usage of 250
			- A Random number between 51 and 70 is generated.
			- 51 comes from 250 max usage * 0.2.
			- At most, the Use times will be reduced to 51, with a possible minimum of 69.

	[ Powered Workstation ]
		- Added a check to see if fuel slots were null before checking if anything was in them.

	[ EntityVehicle ]
		- Added Harmony patch to EntityVehicle's Kill() to trigger EntityAlive's dropCorpseBlock()
		- If the entity vehicle is configured using the following parameters, it'll drop a corpse block.
			<property name="CorpseBlock" value="goreBlockAnimal"/>
			<property name="CorpseBlockChance" value="1"/>

	[ Events ]
		- Added two new Mod events to subscribe too
			- OnBloodMoonStart / OnBloodMoonEnd.	
		- This feature is still under development.

	[ Solution ]
		- Added Peace of Mind and Sample Project to SphereII.Mods Solution

	[ Peace of Mind ]	
		- Fixed an issue where the noose block was causing a warning [ thanks blue name ]

	[ Sample Project ]
		- Fixed an a block warning [ thanks blue name ]



Version: 1.0.56.1453
	[ Sprinklers ]
		- Fixed an issue where water sprinklers were not turning on and off on dedicated servers

	[ Freshness ]
		- Removed test buff of "buffFreshnessSCore" when using the Freshness system and Food Spoilage.

	[ Portals ]
		- Fixed a null reference when adding a portal key to the text
		- Fixed another null reference where there was not a smart mesh on the block
		- Fixed an issue where the player would exist in the same block space as the portal, causing the player unstuck message
			- Moved player position up by 1 on teleport.

	[ Advanced Items - Sharpen ]
		- Changed the Sharpen feature to put new item right into backpack / ground, rather than through the crafting queue
		- Previously, if you cancelled the resharpen from the crafting queue, you'd get the ingredients of the item back, and not the item you were sharpening.

	[ Spawn Particle On Block ]
		- Experimental
		- Added a series of Harmony patches under Features/Particles/Harmony/Blocks.xml
		- OnBlockDamaged, OnBlockAdded, OnBlockRemove
		- This will add the specified particle to the block whenever those events are added, if defined on the block.
		- The following syntax is supported:

		<block name="blah" >
    		<property class="Particles" >
				<!-- Use this particle when the block is added to the world. -->
				<!-- comma delimited, if you want to randomize which particle. -->
				<!-- otherwise, just a single bundle reference.
       			<property name="OnSpawnParticle" value="unitybundle,unitybundle2"/>

				<!-- If you want to change the particle based on biome?-->
				<!-- Use OnSpawn_<biome name> -->
       			<property name="OnSpawnParticle_pine_forest" value="unitybundle,unitybundle2"/>
				<!-- Probably of each block that gets a particle -->
       			<property name="OnSpawnProb" value="0.2"/>

       			<!-- If you want a different particle for when it gets damaged -->
       			<property name="OnDamagedParticle" value="unitybundle,unitybundle2"/>
       			<property name="OnDamagedParticle_snow" value="unitybundle,unitybundle2"/>
       			<property name="OnDamagedProb" value="0.2"/>
    		</property>
		</block>

Version: 1.0.51.1516
	[ Resharpen ]
		- Fixed an issue where the option would not be properly enabled.
			- Was looking for the wrong property name.
		<property name="SharpenItem" value="sharpeningStone" />

	[ SphereII A Better Life ]
		- Many fixes to the models to improve and add hit boxes ( xyth )
		- Enables more fishes from pipermac. (xyth)
		- Fixed the ModInfo.xml's name entry so it's properly ordered.

Version: 1.0.49.1202

	[ Repair From Containers ]
		- Adjusted ordering of Nearby Enemy filter
			- Check to see if in Party
			- Check if nearby entity is in party
			- Check if nearby entity is an ally
			- Then check if nearby entity can damage you.
		- Previously, the "can damage you" check was before the Party check

	[ Food Spoilage ]
		- Moved Food Spoilage to a Feature folder
		- Food Spoilage items will have a new MetaData called "Freshness", which is a percentage of freshness less max durability.
			currentSpoilage / maxSpoilage
		- Freshness percentage is stored as 0,1 value, with 0.1 being 10%, and 1 being 100% fresh.
		- The following vanilla code should be able to compare it through a requirement.
			<triggered_effect trigger="onSelfPrimaryActionEnd" action="AddBuff" buff="buffStillFresh">
				<requirement name="CompareItemMetaFloat" operation="GTE" value="0.1" key="Freshness"/>
			</triggered_effect>

		- New Harmony Patches to allow custom Item Type Icons.
		- If the Freshness % is above 0%, it will use the AltItemTypeIcon, if it's defined.
		- If no AltItemTypeIcon is defined, nothing will display.
		- Once freshness reaches 0%, the ItemTypeIcon will be used, if it's defined.
		- Example item entry.
				<!-- If there's freshness still available, display the campfire icon -->
				<property name="AltItemTypeIcon" value="campfire"/>
				<!-- If there's no freshness left, display the cold  icon -->
				<property name="ItemTypeIcon" value="cold"/>


	[ FreshnessOnly - A Food Spoilage Sub-Feature ]
		- Added a Freshness Only property for ItemClass entry.
			<property name="FreshnessOnly" value="true" />
		- This FreshnessOnly property unlocks a light-weight food spoilage implementation.
		- The concept of this feature is to provide an incentive to eat food fresh, without punishing players who are not interested in it.
		- If the FreshnessCVar property is defined, then only the cvars listed will be executed again with the multiplier.
			<property name="FreshnessCVar" value="cvar1,cvar2" />
		- By default, FreshnessCVar is "all". 
		- If FreshnessCVar is "none", then all ModifyCVars will be ignored from the multiplier.

		- Items with this property set to true will not downgrade to another item when spoiled.
		- All items in the same stack will lose its freshness at once.
		- If this freshness value is 0%, then it will no longer perform calculations
		- If this freshness value is 0%, the durability bar will disappear, and the item will appear normally.
		- If a freshness value is greater than 0%, then the value of the consumable will be multiplied by that percentage

		- Example:
			<triggered_effect trigger="onSelfPrimaryActionEnd" action="ModifyCVar" cvar="$waterAmountAdd" operation="add" value="20"/>
			With a freshness value of 0.5 ( 50% )
				- the $waterAmountAdd will be increased by 20
				- the $waterAmountAdd will be further increased by 10.    20 * (.50 + 1 )
				- The total $waterAmount will be 30

		- When a consumable is ate when it's still fresh, a buff will be applied, letting the player know a bonus is active.
		- Example Implementation:
			<append xpath="/items/item[@name='drinkJarBlackStrapCoffee']">
				<property name="Spoilable" value="true" />
				<property name="SpoiledItem" value="drinkJarBlackStrapCoffee" />
				<property name="ShowQuality" value="false" />
				<property name="SpoilageMax" value="1000" />
				<property name="FreshnessOnly" value="true" />
				<!-- if specified, it'll only re-trigger the following cvars -->
				<property name="FreshnessCVar" value="cvar1,cvar2" />
				<!-- If there's freshness still available, display the campfire icon -->
				<property name="AltItemTypeIcon" value="campfire"/>
				<!-- If there's no freshness left, display the cold  icon -->
				<property name="ItemTypeIcon" value="cold"/>
			</append>
			
	[ SphereII Larger Party ]
		- Added a XUi/Config/windows.xml to increase the side of the window for more players.

Version: 1.0.46.1010
	[ EntitySwimingSDX / EntitySwimmingSDX ]
		- Fixed an issue where fish were leaving the water.
		- Each fish searches for water blocks in its area, and uses that to validate it's pathing.
		- If a fish has less than 20 water blocks, it'll despawn.
		- If a fish leaves the water, it'll despawn.
		- Added new class reference to fix spelling error in Swimming.
			EntitySwimmingSDX and EntitySwimingSDX are the same, code-wise.
		- Kept spelling error to maintain references
		

	[ A Better Life 1.0.0.732 ]
		- Adjusted the ModInfo.xml's Name value
		- Added the ability to auto-generate the version number.
		- Adjusted the entityclasses.xml for class reference for extends.
		

Version: 1.0.45.1058
	[ Check Items For Valid Containers ]
		- Fixed another null reference when blocking an item from the NPC's loot container.
		- Added filter for ItemValue's MetaData to exclude them from being added to a chest
		- If an ItemValue has a Meta Data of "NoStorage", with a value greater than 0, it will be blocked.
	
	[ NPCs ]
		- Added a few new properties to NPCs to block them from being added to storage.
		- These are processed regardless of tags on the storage container or item.
		- When an NPC is being picked up, NoStorage is read from the entityclass.
		- By default, without this property, storage is allowed.
		- npcNoStorage localization is added to the 0-SCore's Localization

		Example syntax:
			<append xpath="/entity_classes/entity_class[@name='npcMeleeTemplate']">
				<property name="NoStorage" value="true" />
				<property name="DisallowedKey" value="npcNoStorage" />
			</append>
	
Version: 1.0.45.853
	[ Check Items For Valid Containers ]
		- Fixed a null reference when opening up zombie loot bags, because they do not have block properties.
		- Questioning life choices that I did not name this feature correctly, and will be forever doomed calling it "Check Items For valid Containers"
		- Added additional check for block at the tile location's position

	[ ModEvents ]
		- Removed a usefuless warning about duplicate mods folder ( this is the standard now )
		- This provides a quick hash of all installed mods, allowing overhauls to quickly see at a glance if it matches the expected value.
		- The hash is made up of modlet name + version number. 
		- If more mods are installed than expected, or it has different version numbers, the hash will change.
		- Changed the mod hash from a GetHashCode() to a truncated Sha256 value
			Modlet List Hash: 7E5FF

Version: 1.0.44.1602
	[ Check Items For Valid Containers]
		- Introduced a new feature through a series of Harmony patches that allows you to filter items from storage containers.
		- Allows you to filter items being added to any given Loot Container based on tags.
		- A container with the AllowTags set to "all", will allow all items.
		- A container with no AllowTags or DisallowTags will not be checked at all, and act normally.

		- If a loot container has the following property, then it will only allow items that have those tags to be stored.
			<!-- Only melee, axe, and repairTool items are allowed to be added -->
			<!-- But it will not accept any lightarmor -->
			<property name="AllowTags" value="melee,axe,repairTool" />
			<property name="DisallowTags" value="lightarmor" />

			<!-- Items.xml entry -->
			<item name="meleeToolRepairT0StoneAxe">
				<property name="Tags" value="axe,melee,light,tool,longShaft,repairTool,miningTool,attStrength,perkMiner69r,perkMotherLode,perkTheHuntsman,canHaveCosmetic,harvestingSkill,corpseRemoval"/>

			<!-- Example -->
			<block name="cntSphereTagTest">
				<property name="Extends" value="cntWoodWritableCrate"/>
				<property name="LootList" value="playerWoodWritableStorage"/>
				<property name="AllowTags" value="melee,axe,repairTool" />
			</block>

		- If an item is blocked, a denied UI sound will be triggered.
		- If an item is dragged and dropped in a blocked container, a tooltip will also display.
		- Shift clicking on an item to move it will play the denied UI.
	
		- If a block has the following Property, this will be used to check for localization and display a custom blocked message.
			<property name="DisallowedKey" value="NoPickUpForNPCs" />
		- This property can also exist on the Item entry as well, and will over-ride the block's message, if it's set.
		- If not otherwise set, the default localization entry will be displayed.
			
Version: 1.0.43.1207
	[ Fire Manager ]
		- Removed the DynamicMeshChunk update, which was likely needless, and likely caused a performance issue.

		- Added new property on a block to replace the target block with an ExtinguishedUpgradeBlock.
			<property name='ExtinguishedUpgradeBlock' value='burntBlock' />
		- When a block is extinguished, it will turn into this block.
		- Damage from the original block is transfered over to the new block
		- If the damage from the original block is greater than the new block's max health, it'll turn into an air block.
		

	[ SphereII Larger Parties ]
		- New modlet that allows Parties up to 100 members.
		- Added a Minimum exp value for shared exp.
		- Default Minimum Exp is 200.
		- Not included into 0-SCore, because there is a transpiler patch.

Version: 1.0.39.746

	[ One Block Crouch ]
		- Disabled one block crouch by default.
	
	[ Take And Replace ]
		- Fixed an issue where the original default behaviour was broken.
		- A new Configuration Block entry called "Legacy" is set to true, going to default behaviour.

		<!-- To enable advance configuration settings. -->
		<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedPickUpAndPlace']/property[@name='Legacy']/@value">false</>

Version: 1.0.38.1615

	[ Food Spoilage ]
		- Adjusted references within the patches to use their full __instance values, rather than creating shorter forms for cleanliness
		- Fixed an issue where a stack was spamming unlimited rotten meat

	[ 0-SCore Blocks ]
		- Added new class to blocks.xml to support PickUpAndReplace features. 
			<property class="AdvancedPickUpAndPlace">
				<property name="Logging" value="false"/>
				<property name="TakeWithTool" value="meleeToolRepairT1ClawHammer" />
			</property>

	[ Take And Replace ]
		- When a TakeWithTools property is set, the default item of clawHammer gets cleared.
		- Fixed the ordering of material checks according to the xml defined order.
			-> is Block a Valid Material to pick up?
			-> Do we need to check the hand item for pick up?
				-> Are we holding one of those tools?
			-> Does our current hand item have the proper material tag?

		- Trying a new design pattern.
			-> Hold unto your hats.

		- Created some helper shortcuts in order to reduce hard coding many references in the blocks.xml/shapes.xml
		- If the TakeWithTool property exists on the block / shape, then a lookup against the AdvancedPickUpAndPlace class is triggered.
			-> ValidMaterials operates the same style of lookup.
		- This lookup uses the value of the TakeWithTool from the block/shape as a key to an entry in AdvancedPickUpAndPlace.
			For Example:
				<!-- blocks.xml -->
				<property name="TakeWithTool" value="WoodenFenceTools" />
				<property name="ValidMaterials" value="woodMaterial"/>
			
				<!-- Config Block -->
				<property class="AdvancedPickUpAndPlace">
      				<property name="Logging" value="false" />
					<property name="TakeWithTool" value="meleeToolRepairT1ClawHammer" />
      				<property name="WoodenFenceTools" value="meleeToolRepairT0StoneAxe,meleeToolRepairT0TazasStoneAxe,meleeToolRepairT1ClawHammer,meleeToolAxeT1IronFireaxe,meleeToolAxeT2SteelAxe,meleeToolAxeT3Chainsaw">
					<property name="woodMaterial" value="Mwood_weak,Mwood_regular"/>
			    </property>

		- You may append to the class with your own unique keys
			<append xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedPickUpAndPlace']">
				<property name="WoodenFenceTools" value="meleeToolRepairT0StoneAxe,meleeToolRepairT0TazasStoneAxe,meleeToolRepairT1ClawHammer,meleeToolAxeT1IronFireaxe,meleeToolAxeT2SteelAxe,meleeToolAxeT3Chainsaw" />
			</append>
		
Version: 1.0.37.1432
	[ Localization ]
		- Updated some missing localization entries.

	[ Food Spoilage ]
		- Fixed an issue where a stack was spamming unlimited rotten meat
		- Issue was related to the Spoilage meter on an item not being reset properly after each spoiled item.

	[ Jiggle Adjustments ]
		- If you are under 18, stop reading.
		- This is mostly for xyth, who is more assuredly considered old enough to view.
		- Exposed two properties for EntityAlive's:
			<!-- Always enable the jiggle script, regardless of distance -->
			<property name="AlwaysJiggle" value="true" />

			<!-- Never downscale the AI, regardless of distance -->
			<property name="NeverScaleAI" value="true" />


Version: 1.0.32.940

	[ CompoPackTweaks ]
		- main host for the CompoPackTeaks, which is used by the CompoPack Team
		- You do not need to install this yourself for compopack; they supply it.
		- Removes the warning for the DMS when new traders are added.
		- Can be used by other groups for the same purpose.

	[ ObjectiveGiveBuffSDX ]
		- Removed the hard-coded "buff" word after the "Get <Buff>" string.

	[ Take And Replace ]
		- Added a property option to block / shape to allow filtering based on hand item.
			<property name="TakeWithTool" value="clawHammer,stoneAxe" />
		- If this property is specified, the Take Prompt will only show when you are holding that item.
			- This happens after we've checked ValidMaterial, and passed the material check for pick up.
		- If this property is not specified, then the Take Prompt will show up for those blocks.
		- Note: HoldingItem property is still valid. The item(s) specified in this property will half the time it takes for the block. 

        - If the item that you are holding has the tag "silenttake", then no sound will be played when a block is taken.
			- This happens regardless if the tool is specified in TakeWithTool or HoldingItem.
			- If you can take a block, and have that tag on your hand item, no sound will be played.
		- Updated the "Take Sound" to be more appropriate for the material you are taking.
			- Wooden sound for wood materials. Steel sound for steel materials.

		- Added a property option CheckToolForMaterial, with the default being false.
			<property name="CheckToolForMaterial" value="true"/
		- If this property is defined and set to true, then the block will check for a tag on the holding item
		- The tag on the holding item must be the material ID that is allowed to be picked up.
			<item name="meleeToolRepairT0StoneAxe">
				<!-- Allow the stone axe to pick up any block that is Mwood_weak or Mwood_regular.
    			<property name="Tags" value="<ommitted for clarity>,Mwood_weak, Mwood_regular"/>

		<!-- full example : -->
		<append xpath="/shapes/shape[@name='windowBoarded']">
			<property name="Class" value="TakeAndReplace, SCore"/>
			<property name="CanPickup" value="true"/>
			<property name="TakeDelay" value="8"/>
			<property name="PickUpBlock" value="woodShapes:VariantHelper"/>

			<!-- Only allow picking up window board blocks that are weak or regular wood -->
			<property name="ValidMaterials" value="Mwood_weak,Mwood_regular"/>

			<!-- Only allow picking up blocks with this tool being held. -->
			<property name="TakeWithTool" value="meleeToolRepairT0StoneAxe,meleeToolRepairTazaStoneAxe"/>
				
			<!-- Check the currently held tool's tag for the material of the block / shape being picked up -->
			<!-- note: That means that the stone axes must have a Mwood_weak tags in order to pick up those blocks -->
			<property name="CheckToolForMaterial" value="true" />
		</append>



Version: 1.0.31.1121
	[ Events ]
		- Added a new folder Scripts/Events
		- Added new event EventOnBuffAdded.OnBuffAdded, which is triggered whenever a buff is added.
			- This event was not strictly necessary for the ObjectiveBuffSDX quest, but left just in case it's useful.
		- Moved EventOnEnterPoi.OnEnterPoi to this new folder. This event is fired whenever a player enters the POI bounds.

	[ Quests ]
		- Fixed an issue with ObjectiveBuffSDX by adding in the necessary event hooks
		- ObjectiveBuffSDX will be checked whenever a buff is added, or if a buff is already present, when the objective is current.
		
		- Cleaned up QuestActionGiveBuff
		- Added example syntax in quests.xml of 0-SCore

			<objective type="BuffSDX, SCore" >
				<property name="phase" value="3" />
				<property name="buff" value="buffIsOnFire" />
			</objective>

			<action type="GiveBuffSDX, SCore">
				<property name="value" value="buffRadiation01" />
				<property name="on_complete" value="false"/>
				<property name="phase" value="4" />
			</action>

	[ Take and Replace ]
		- Added a material filter to the PickUpAndReplace block.
		- If the block or shape does not have the listed material, the prompt will not show up, nor will the block work.
		- Removed some default shapes and blocks from the Take And Replace modlet.
		- Default: "Mwood_weak,Mwood_weak_shapes,Mwood_shapes";
		- If a block / shape has the following property, it will use them over the default.
			<property name="ValidMaterials" value="" />
		- It is a comma-delimited list. 
		
Version: 1.0.30.1042

	[ NPCs ]
		- Fixed an issue when NPCs could not move around, and were stuck in inital pose.
	

Version: 1.0.29.1609

	[ SmartText Mesh ]
		- Added a Harmony patch to protect against null reference due to the Signs Feature.
		- Sign Feature hides the wooden mesh and Text from showing up. This harmony patch protects against
			when the text is changing, and the TextMesh isn't enabled (yet).

	[ Signs ]
		- Fixed issues with signs not generating correctly.
		- ImageWrapper, the basis of the image signs, has been modified to support a VideoPlayer
		- Gifs are no longer supported unfortunately due to shader issues.
		- Gif and Gifv links are converted by ImageWrapper to be a mp4 URL.
			- This probably only effectively works if you use an imgur link.
		- This is then fed to the VideoPlayer as a Source URL.
		- When the http:// link is removed, the sign is reverted back to vanilla texture and mesh.
		
	
			
Version: 1.0.28.1841

	[ EntityAliveSDX ]
		- Fixed a null reference when picking up an NPC and selecting their item.
			- It was storing a LootListName, which may be null, causing the null ref.

	[ Signs ]
		- Fixed an issue with http:// images not displaying correctly on signs.
		- Problem was identified that the new shader on the signs weren't allowing it.
			- Added a SCoreModEvents.GetStandardShader() that returns a Standard shader.
			- When using a custom http:// link, it will switch to this shader.
		- A Redesign is necessary.
		- Known Issue: URL text still shows up. Sometimes image doesn't.

Version: 1.0.25.1115
	- Rebuilt, and fixed against b326.

	[ Take And Replace ]
		- Fixed a null reference if the PickedUpItemValue was null on a block.

Version: 1.0.23.1434

	[ Soft Hands ]
		- Fixed an issue where Soft hands was not negated by the new equipment system

	[ Take And Replace ]
		- Modifed the class to better support shapes.
		- If a property called PickUpBlock is available, it'll give you whatever resource is listed there.
		- If the property is not there, it'll give you the same block you pulled off.
		- Example:
            <append xpath="/shapes/shape[@name='windowBoarded']">
                <property name="Class" value="TakeAndReplace, SCore"/>
                <property name="CanPickup" value="true" />
                <property name="TakeDelay" value="8"/>
                <property name="PickUpBlock" value="resourceWood"/>
            </append>
		-> Order of reading PickUpBlock:  current block, PickUpBlock from Shape, and PickUpBlock from block.

	[ Portals ]
		- Fixed a Portal Null reference when trying to access the block.

Version: 1.0.22.1055

		- Cleaned up old debug statements.

	[ NPC ]
		- Fixed a null reference when opening a backpack from a dead NPC

	[ Broadcast ]
		- Fixed an issue where the button was not working properly

	[ Localization ]
		- Fixed some localization settings.

Version: 1.0.19.1056
	- Restructured Repo to include a top level Mods folder.

Version: 1.0.19.944
	[ Powered Workstations ]
		- Fixed a bug where workstations would get free power if they did not have RequirePower set to true.

Version:1.0.18.1549

	[ Pathing Cube ]
		- Fixed an issue where a Pathing cube would throw a null reference.
			- Got an updated block from xyth
			- Updated the code to work with TextMesh.

	[ Challenges ]
		- Added a challenges.xml file, all commented out.
		- Shows how to add a new category, group, and challenges.

Version: 1.0.18.2207

	- Rebuild against b317

	[ UAI ]
		- Fixed broken reference in UAITaskMoveToTarget.
			- This used to set a reference to the closes enemy and player. However, those fields are no longer available.
			- Player reference is now set up for aiClosestPlayer.

Version: 1.0.17.1452
	- Recompiled against latest experimental build
	- Fixed a few over-rides that changed in EntityAliveSDX, and AddBuff()

	[ Known Issues ]
		- Pathing Cubing Errors out on save.

	[ Advanced Items - Scrap ]
		- Fixed another issue where the temporary recipe was adjusting the master recipe, causing scrap information to be lost.

	[ Food Spoilage ]
		- Documenting a global Block Property for FullStackSpoil. This will cause the entire stack to spoil at once, rather than individual.
		- <property name="FullStackSpoil" value="true" />
		- This property may be defined on individual items to over-ride the global result.

	[ ItemValue Clone() ]
		- Modified the Patch to ItemValue to be a Postfix, and only re-does the Metadata dictionary, if conditions are correct to execute. 
		- See previous release notes about the Meta Data for details.

Version: 1.0.15.1402

	[ Food Spoilage ]
		- Fixed an issue where PreserveBonus was not being calculated correctly.
		- NOTE
			- Identified a potential issue with the ItemValue's Meta data is not being properly disconnected when a stack is split.
			- The result of that is when we store the Item's next spoilage tick is shared between the stack.
			- If a stack is split, the next spoilage tick is still synced up with all stacks from the original
			- When spoilage is applied to the first stack, all child stacks will also spoil at the same rate, regardless of location.

			- As a workaround, I have added a new blocks.xml property that patches ItemValue's Clone() call.
			- This patch is only fired when Food spoilage is enabled, and only if the item supports food spoilage itself.
			- Default is false, do not apply this patch regardless.
			
			<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='FoodSpoilage']/property[@name='UseAlternateItemValue']/@value">true</set>


	[ Advanced Items - Scrap ]
		- Fixed an issue when scrapping an item, it would allow unlimited crafting of that item.
			- oops.
		- Cleaned up code a bit.

Version: 1.0.14.1543
	[ Anti Nerd Pole ]
		- Fixed an issue where a block was consumed on a failed placement.
			- Still needs testing on MP.
	
	[ Lock Code ]
		- Removed a Debug statement about Mouse 0.

	[ EntityAliveSDX ]
		- Added a CanCollideWith() check, from EntityAliveV2.
			- To enable this, set <property name="CanCollideWithLeader" value="false" />
			- Default is true, it can collide with leader.

	[ Food Spoilage ]
		- Refactored Food Spoilage to use ItemValue's MetaData, instead of UseItem
		- Created several helper methods, and cleaned up references
		- Updated variables to follow project standards.

Version: 1.0.10.1107
	[ SCore Utility Settings ]
		- Added a Sirillion modified window
		- Added a toggle to force vanilla lock picking, if Locks is installed
		- Added a toggle to force Locks to be used, if installed, regardless of keyboard / controller
		- Added a toggle to mute Trader Rekt
		- Added a toggle to mute all Trader's voices.
		- Added a toggle to mute NPC foot steps
		- This screen can be manipulated through xpath to remove features in overhauls that may not be desirable.

	[ Advanced Lock Picking ]
		- New-ish UI window for Advanced Lock picks with some cleaner description.
		- Added new lines in the blocks.xml to allow customized key bindings
		- These are comma delimited list, so you may add more to the list.
				<property name="Left" value="Keypad4" />
				<property name="Right" value="Keypad6" />
				<property name="Turn" value="Keypad8,Keypad5,Keypad2" />
		- Defaults are still hard coded:
			- A / D / Space
			- Joy stick support
			- Mouse support - Scroll up / down to move, click to try.
			- Follows mouse position as well.

	[ Challenges ]
		- Added support for custom challenges
		- Features/Challenges
			- Uses a Harmony patch to allow proper checks

 		- Added new Enter POI Challenge.
			POIs can be referenced by prefab name using the prefab attribute.
				<objective type="EnterPOI, SCore" prefab="abandoned_house_04" count="10"/>
			POIs can also be referenced by POI Tags.
				<objective type="EnterPOI, SCore" tags="wilderness" count="1"/>
			Objectives that list both name, and tags, must match both to pass the objective.
				<objective type="EnterPOI, SCore" prefab="abandoned_house_04" tags="wilderness" count="1"/>
				

	[ Auto Redeem Challenges ]
		- Removed a check on the MinEventActionAutoRedeemChallenges that was checking if there was any any challenges to redeem
			- In reality, this was just duplicating the same loop as it was going to do anyway.

		- Added Harmony Patch and cvar for AutoRedeeming, available through the SCore Utility Screen

	[ Disable Flickering ]
		- Added a Harmony patch to disable flickering lights and lightening flashes.
		- See Config/blocks.xml to toggle on or off.

	[ Block Configuration ]
		- Added a new optional parameter to CheckFeatureStatus().
		- Defaults to false, which is typical behaviour of checking the Configuration Block for the setting.
		- If true is passed as the third parameter, then it'll check the local player's cvar.
		    public static bool CheckFeatureStatus(string strClass, string strFeature, bool checkCVarFirst = false)
		- This is useful if there's a feature you want to expose to the SCore Utility Settings screen

	[ Transmogrifier ]
		- Added a FastTag check for Crawler to exclude them

	[ TriggeredSDX ]
		- Fixed a typo that could cause a null ref if a property was not specified
		- Added support for passing cvar from player to animators
			<property name="CopyCVarToAnimator" value="cvar1;cvar2;cvar3" />

		- This will loop around the animator, doing a SetFloat() on the animator, using the cvarname, and cvar value.

	[ ToggleButtonCVar ]
		- Added a new controls.xml entry for ToggleButtonCVar
		- This allows you to define a cvar and have it tied to a cvar on the local player
		- This is used in the SCoreUtilities to allow adding toggle buttons quicker, without modifying c# code.
		- Add in a new toggleButtonCVar to SCoreUtilities window.

		<togglebuttonCVar name="xuiSCoreUtilsLockPick" caption_key="xuiSCoreUtilsLockPick" cvar="LegacyLockPick"
        	tooltip_key="xuiLockPickingToolTip" width="290" height="32" depth="3" />

Version: 1.0.5.826

	[ Farming ]
		- Added code to disable sound from a sprinkler when being destroyed.
		- Merged Yakov's changes for optional setting for Sprinklers to require to be connected to pipes to work.
				<property name="RequirePipesForSprinklers" value="false" />
			- Default is false; no change from original.

	[ TriggeredSDX ]
		- Cleaned up class a bit
		- Fixed an issue where the prompt would not show up when needed.

	[ SphereII Peace of Mind ]
		- Merged SphereII PG13 into Peace of Mind modlet
		- Mutes Rekt's audio to clean up his abbrasive attitude
			- This was easier than trying to go through each of his audio dialogs, and finding out which ones
				where least offensive.
		- Removed Noose blocks, replacing them with Air.
		- Changes the Party Girl's model to be that of Marlene's.
			- Quests, Challenges, etc are still working.
		- Removed the block with bodies hanging from pillar.
		- If 0-SCore is available, flickering / pulsating lights will stay constantly on.
		- Removes the lightning flashes.


Version: e1.0.1.1224

	Available: https://github.com/SphereII/SphereII.Mods/tree/1.0e

	- Initial release of 0-SCore for 1.0 experimental.
	- There is probably a TON of broken stuff still
	- Overhaulers can start to consume and test out their features.

	[ 1.0 Port ]
		- Update public field changes in the console commands
		- Update many FastTags
		- Removed UMA Patches
		- Probably much to do 

	[ Fire Manager ]
		- Converted CheckBlocks into a coroutine.
			- Will process 10 blocks per frame now.
			- Changes are still distributed to all clients at the same time
		- Added a rainfall check.
			- Gives a bonus to extinguish chance when raining. 
			- Doubles the Change To Extinguish.
			- This might not stay.
		- If FireSound is set to None, then no sound will play.
		- Changed CheckFireProximity to default to -1 if not close to fire.

	[ MinEffect ]
		- Added an AutoRedeemChallenges mineffect. Add to something like buffstatus01
		- Auto redeems all completed challenges.
			<triggered_effect trigger = "onSelfBuffUpdate" action="AutoRedeemChallenges, SCore"  />

	[ Remote Crafting / Repair ]
		- Updated code to use the TEFeature for lootable
		- Cleaned up repair code
		

	[ skipnewsscreen ]
		- Added command line parameter to skip starting news screen
			-skipnewsscreen
		- When the score's -autostart command argument is used, it will also skip the news screen.


	[ Powered Workstation ]
		- Adjusted fuel checks on wireless power feature.
		- If there's alternative fuel, such as wood, this will be used as a priority.
		- If there's no alternative fuel, then it will check for wireless power.
			- If wireless power is found, then burn time will be set to 15f.
			- If a wireless power is not found, then burn time will be set to 0.

	[ Spawn Entity On Death ]
		- Fixed an issue where, when using an entitygroup, was always picking the first entry

Version: 21.2.205.1412

	[ Fire Manager ]
		- Adjusted the random expiry time for extinguished smoke.
		- Removed an additional RemoveFire() from the Extinguish MinEffect.
			- It was removing the fire, then extinguishing the fire, which also removed the fire.
		- Fixed an issue where resetting a POI with a fire trap on would extinguish the fire trap, although keep its damaging effect.
			- Changed the SetBlock to check if the burning block is in the Fire Map before resetting it's particle.
			- This may have a side effect where there is a potential that not all fire will be extinguished from a quest-resetted POI.

	[ Powered Workstation ]
		- Removed a check on if the current fuel is below 0.5. 
		- Powered workstation will now check on each Update Tick, and add 15 fuel
		- Removed a call to get the block from the chunk position, rather than using the locally available block information.

	[ Spawn On Death ]
		- The Spawn on Death hooks have been expanded to support passing cvar and buffs over to the new entity.
		- This change also introduces a new method to spawn entities that need this functionality.
		- The following new properties are supported on the source entity (ie, the entity that is dying)

			<!-- Spawn in using a method from FuriousRamsay to simplying spawning -->
			<!-- This can be set without actually copying over cvars and buffs -->
			<property name="SpawnOnDeathAltSpawn" value="true" />
			
			<!-- If any of the following properties are not found, then nothing is done for them -->

			<!-- Copy all cvars over from the source to the new entity. -->
			<property name="SpawnOnDeathCVarFilter" value="all" />

			<!-- Copy all cvars that contain the word guppy. Position of the * does not matter. It's a contains check. -->
			<property name="SpawnOnDeathCVarFilter" value="guppy*" />

			<!-- Copy all buffs over from the source to the new entity. -->
			<property name="SpawnOnDeathBuffFilter" value="all" />

			<!-- Copy all buffs that contain the word guppy. Position of the * does not matter. It's a contains check. -->
			<property name="SpawnOnDeathBuffFilter" value="guppy*" />

			<!-- Both style of filters are comma delimited. -->
			<property name="SpawnOnDeathBuffFilter" value="buffStatus, buffIsOnFire, buffIs*" />

Version: 21.2.170.702

	[ Fire Manager ]
		- Fixed miswording on comment on smoke particle
		- Made a change to particle rotation in an attempt to fix sideway smokes.
		- Changed the ordering of fire/smoke particles to read RandomParticles first, then checks if blocks / materials had overrides
			- Previously, if Random Particles was used, they would over-ride individual blocks
		- Documenting that RandomFireParticle and RandomSmokeParticle exist.
			- This is the documentation evidence.
			- This will randomly select one of the particles in each one to display.
			- You may use "NoParticle" to add a blank, ie skip, a particle.

			<append xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='FireManagement']">
				<property name="RandomFireParticle" value="#@modfolder(0-SCore_sphereii):Resources/guppySmokeParticles.unity3d?gupSmoke1,#@modfolder(0-SCore_sphereii):Resources/guppySmokeParticles.unity3d?gupSmoke2,#@modfolder(0-SCore_sphereii):Resources/guppySmokeParticles.unity3d?gupSmoke5" />
				<property name="RandomSmokeParticle" value="#@modfolder(0-SCore_sphereii):Resources/guppySmokeParticles.unity3d?gupSmoke3,#@modfolder(0-SCore_sphereii):Resources/guppySmokeParticles.unity3d?gupSmoke4" />
			</append>

	[ Quests ]
		- Fixed an issue in the ObjectiveGotoPOISDX where the random prefab name was being selected at initialiation,
			rather than each time the quest is triggered.
		- Before searching for the prefab, if the property name PrefabNames is set, it'll randomly pick one.
		- Removed extra debug logs from ObjectiveBuffSDX

Version: 21.2.151.1612

	[ Quests.xml ]
		- Fixed an issue where a starting comment was missing.

	[ Encumbrance ]
		- Added a check for mods on items, and calculate those item weights appropriately.
		- Refactored a bit of the code to clean it up, and reduce duplicate checks.

	[ One Block Crouch ]
		- Added a Is Crouched check before lowering the camera.

	[ Code Added, but not compiled ]
		- NPCv2 code has been added, but excluded from building

Version: 21.2.101.931

	[ NPCs ]
		New Property in the blocks.xml called EnemyDistanceToTalk, under AdvancedNPCFeatures.
				<property name="EnemyDistanceToTalk" value="10" />

		This value is used to determine how close an enemy needs to be, to block the NPC from talking with you.

			A value of 0 or less will block this check, allowing you to talk to NPCs in all conditions.
			A value of 5 will block NPC dialog options if an enemy is within 5 blocks of it.
			Default is a value of 10.

	[ Dialog ]
		Added new requirement to dialog called EnemyNearBy. This is meant to work in conjunction with the EnemyDistanceToTalk,
			allowing you to disable an enemy check for the overall dialog, while also providing the ability to hide certain dialog 
			responses that may not be appropriate when in a battle.

		This will show the requirement if it the enemy is within 10 blocks.
			<requirement type="EnemyNearby, SCore" id="10" requirementtype="Hide" />

		This will hide the requirement if it the enemy is within 10 blocks.
			<requirement type="EnemyNearby, SCore" id="10" value="not" requirementtype="Hide" />

	[ Remote Crafting ]
		Added additional check for EnemyNearBy for remote crafting to take in consideration if a player is nearby,
			if they are in the same party, or are friends with each other.


Version: 21.2.80.1026
	[ NPCs ]
		- Merged khzmusik's fix to protect a player's followers from their own explosions.


Version: 21.2.78.935

	[ Lock Pick ]
		- Removed Debug.Log in Lock picking, and replaced with a Logging check to display.

	[ Particle Attractor ]
		- Refactored FromAttackTarget to enhance functionality.

			Example:

				Minimum Example:
					<triggered_effect trigger="onSelfBuffUpdate" action="SetParticleAttractorFromAttackTarget, SCore" />

				Typical Example:
					<triggered_effect trigger="onSelfBuffUpdate" action="SetParticleAttractorFromAttackTarget, SCore" cansee="true" />

				Full Example:

				<triggered_effect trigger="onSelfBuffUpdate" action="SetParticleAttractorFromAttackTarget, SCore" 
					cansee="false"   <!-- Does the zombie need to see the player? --> 
					speed="10"   <!-- Optional. Default to 5.  How fast the particle should go.  -->
					transform="Particle attractor"    <!-- 
															Optional. If not specified, it'll look for the first particleAttractorLinear script.
															Searches through the entire zombie for the transform with this name.
															It actually searches for all Particle Systems, then checks the transform name
															so the effect is the same. 
															** If the particleAttractorLinear does not appear on the transform, it'll add it **
													-->
					target_transform="hips" />		<!-- 
														Optional. If not specified, it'll attach to the Head.
														This allows you to specify a target transform, which transform to attach the particle too.
														If this transform does not actually exist, it won't attach to any.
														"head" and "hips" are shortcuts to GetHeadTransform() and GetPelvisTransform().
														All others search.  -->
							


Version: 21.2.54.843
	[ Disable Flickering Lights ]
		- Added new Feature for Flickering Lights, and a matching Config Block Entry
		- This will change all the Flickering lights to be static lights.
		- This will disable lightning effects.

	
		<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedPrefabFeatures']/property[@name='DisableFlickeringLights']/@value">true</set>

	[ Peace Of Mind 21.2.1.0 ] 
		- Updated Peace of Mind to only remove the Noose block with an air, leaving the other ropes available.
		- Added Disabling of Flickering lights, if 0-SCore is available.
			- If SCore is not available, it'll display a harmless warning.

Version: 21.2.52.1028
	[ Dialog ]
		- Merged in khzmusik's dialog changes:
			- Added new DialogRequirement of IsSleeper
				Requires that the dialog's NPC must have been spawned into a prefab sleeper volume.
			- Added `IDialogOperator` interface for requirements and actions that accept an "operator" attribute
			- Updated Harmony patches to `DialogFromXML` to use the interface
				- `DialogActionAddCVar`, `DialogRequirementFactionValue`, `DialogRequirementHasCvar`, and `DialogRequirementNPCHasCVar` now all implement that interface
 			- Major refactoring of `DialogRequirementNPCHasCVar`
			- `DialogRequirementHasCVar` handles "not" operator better
			- `DialogRequirementLeader` supports "not" as a value, which checks that the NPC is not hired by the player talking to it (as opposed to "HiredSDX" which just checks whether or not the NPC is hired by _any_ player)
			- Added XPath in `dialogs.xml` that will test the various values of "HasCVarSDX" (the XPath is commented out)
			
	[ Entity Alive SDX ]
		- Updated WeaponSwap to force an avatar update on placement
			- It was observed that NPCs were not full initializing their weapons properly when the weapon
				was using a different hand to hold it in.

	[ Particle Attractor ]
		- Added new feature "Particle Attractor"
			- This allows running a particle between two points, one being the source entity and then the target entity.
			- The script runs on the zombie, and needs to know a "target" to set the destination for the particle.

		- To add the particleAttractorLinear to the zombie, the following line can be used:
				<!-- if the zombie has the "Particle attractor" transform, it'll attach the script to it. -->
				<triggered_effect trigger="onSelfBuffStart" action="AddScriptToTransform, SCore" 
					transform="Particle attractor" script="particleAttractorLinear, SCore"/>

			
		- Added 3 new MinEventActions to support this feature, along with examples:
			- Note: the trigger="" is just used as an example. Add the appropriate trigger for your use case.

			<!-- If the zombie has an in-game attack target, such as a player, this will set the "target" in the particleAttractorLinear to it. -->
			<!-- This will cause the particle to go from the zombie to the target, ie, the player. -->
			<!-- An optional cansee flag can be toggle, which will do a visibility check before applying the target -->
			<triggered_effect trigger="onSelfBuffUpdate" action="SetParticleAttractorFromAttackTarget, SCore" cansee="false"/>

			<!-- This MinEvent runs on the Player, and can use the position AoE to find zombies in range which may have
				an ParticleAttractor. If a zombie is found with the ParticleAttractor, it'll set the player as the target. -->
			<triggered_effect trigger="onSelfBuffStart" action="SetParticleAttractorFromPlayer, SCore"/>

			<!-- This MinEvent runs on the zombie, and can use the position AoE to find a player in range. 
				If the zombie has a ParticleAttractor, it'll set the first player it finds as the target. -->
			<triggered_effect trigger="onSelfBuffStart" action="SetParticleAttractorFromSource, SCore"/>

Version: 21.2.49.751

	[ Dialog ]
		- Added new Dialog Requirement that checks cvar on the NPC.
		- Follows the same format as the Player's HasCVar check.
        	<requirement type="NPCHasCVarSDX, SCore" value="1" requirementtype="Hide" operator="GTE" 
				id="quest_Samara_Diary" />

	[ NPCs ]
		- Adjusted the TeleportNow feature to be a bit more fault tolerant from teleporting NPCs into walls.
		- Added new cvar to a few of the order buffs:
				<triggered_effect trigger="onSelfBuffStart" action="ModifyCVar" cvar="Guarding" operation="set" value="1"/>
		- This is meant to set a flag so we know if the NPC is supposed to be guarding, and will ignore commands.

	[ MinEvent ]
		Added new Min Event called TeleportNow
			<triggered_effect trigger="onSelfBuffUpdate" action="TeamTeleportNow, SCore" />
		- Any NPC within a 50 block range will teleport to you immediately. This is a short cut to the Come Here! command
			in the Companion screen.
		- If the NPC has the Guarding cvar, it will ignore this order.
		- Added buff buffTeleportCooldown after TeleportNow as a cool down. Default is 5 seconds.
		- Added item scoreTeleportNow for an example.
			
Version: 21.2.47.1657

	[ MinEventActionOpenWindow ]
		- To Better support modders' use of the Companions screen, a new minevent has been created.
			<triggered_effect trigger="onSelfPrimaryActionEnd" action="OpenWindow, SCore" window="SCoreCompanionsGroup"/>
		- This will open the xui.xml's Window Group

		- A test item called scoreOpenNPCWindow has been added showing an example:

     		<property class="Action0">
        		<property name="Class" value="Eat"/>
				<!-- Consume is set to false so the item doesn't get used up -->
        		<property name="Consume" value="false"/>
        		<property name="Delay" value="1.0"/>
        		<property name="UseAnimation" value="false"/>
        		<property name="Sound_start" value="read_mod"/>
        		<property name="Sound_in_head" value="true"/>
      		</property>
      		<effect_group tiered="false">
        		<triggered_effect trigger="onSelfPrimaryActionEnd" action="OpenWindow, SCore" window="SCoreCompanionsGroup" />
      		</effect_group>
		
Version: 21.2.47.1616

	[ XUiC ]
		- Added a "Come Here!" to the list of commands.
			- This is a Range-based, as are the other commands.
			- It is intended to be used when the NPC is in a different room than you, and are stuck.
			- This applies the Follow Buff to the NPC, so if they were told to Stay previously, they'll still come to you.
			- This teleports to the Player position + 1. It's a very tight teleport.
		

Version: 21.2.46.1336

	[ Fire Manager ]
		- Added a check in Chunk.SetBlock(), that if the SetBlock is from a POI Reset, to remove the fire from any blocks that may be burning.

	[ EntityAliveSDX ]
		- Added a buff check in LeaderUpdate() to prevent a timing issue when you are dismissing an NPC

	[ XUiC ]
		- Shrunk the Utilities screen, and moved it to the center of the screen.
		- Added new Companion Screen. These are defined in the xui.xml.
		- Access to these screens are available in the Esc menu in-game.

		- Companion screen shows all hired NPCs, their name, their distance, and allows you to give them orders.
			- Orders are based on range, roughly 50 blocks.
			- If the NPC is more than 50 blocks away, the commands are ignored.
			- Dismissing an NPC through this screen is instant, and has no range check.

Version: 21.2.44.1541

	[ Fire Manager ]
		- Another fix for Fire not damaging players on a dedicated server
		- Fixed various typoes in a single debug log caused by a stroke.

	[ EntityAliveSDX ]
		- Removed one of the checks preventing you from talking to an NPC that was in battle. 
			- Previously, there was a check to see if the NPC had an attack target. This was removed.
			- Another check, right after that one, is still active. It scans to find enemies within 10 blocks, and prevents talking then.

	[ NPC ]
		- Small tweak to the CheckBlocked() to determine if the NPC will jump off the building after you.
			- Previously, this just inched the NPC forward until they gracefully fell, but it may not have been enough in all cases.

Version: 21.2.42.1753

	[ XUiC ]
		- Added new XUiC_SCoreUtilities controller to allow personalization of player experiences
			- Personalization is limited to SCore features that some users may not like, and offer
				no benefit other than comfort.
			- Currently feature:
				The ability to turn off the Lock MiniGame, if Locks is available.
				The ability to turn off hired NPC foot steps, so you aren't hearing it constantly.

			- By default, this screen is added to the inGameMenu window. 
				- Pressing ESC to see the Exit / Option window will show it.
			

	[ Farming Manager ]
		- Adjusted GetWaterPercent() to be a lower value to determine if its a plantable area.

	[ Fire Manager ]
		- Another potential fix for fire not hurting a player when in a multipler server.

	[ EntityAlive SDX ]
		-Merged in Pull Request 85 from khzmusik
		- Remove the prompt to talk with enemy NPCs

		- New blocks.xml Property to 
			<!-- Set to true if some enemy NPCs are "advanced" NPCs that use EntityAliveSDX. -->
			<property name="AdvancedEnemyNPCs" value="false" />
			- When enabled, it'll remove the prompt to talk to non-friendly NPCs.
			
Version: 21.2.40.1024

	[ EntityAliveSDX ]
		- Fixed an issue with Weapon Swapping that would get triggered when an NPC only has an HandItem specified, and not
			given an item through ItemsOnEnterGame.
			- This generated the "Item Not Found" error.

	[ Fire Manager ]
		- Blind potential fix for fire not hurting a player, when in a Multiplayer server.
			- When the fire buff is applied now to the player, the player itself will be the source of the damage (instigator id ).

Version: 21.2.37.1618

	[ TileEntitySign ]
		- Fixed an issue where png files were not being displayed.
		
		Note: Gifs hosted on Imgur may not work. This is being looked at.

	[ EntityAliveSDX ]
		- Fixed an issue where the UseTimes on an item was not being preserved by an NPC
			- This caused the UseTime to be reset when you pick up and place down an NPC.
			- This was mostly evident by giving an NPC a partially spoiled item, then picking up and placing down the NPC to reset the spoilage counter.
			- This was actually fixed in 21.2.31.1132

Version: 21.2.31.1132

	[ EntityAliveSDX ]
		- Added new property StartingItems to specify items that will appear in the NPC's loot container on spawn in.

			Example:
				<property name="StartingItems" value="drinkJarBoiledWater=2,foodCanChili,medicalFirstAidBandage,meleeToolTorch,keystoneBlock,noteDuke01"/>

			- Once added, it sets a cvar called "InitialInventory" to flag it was already done.
			- Also updated DeployNPCSDX to support this.

		- Fixed an issue where weapon swap got broken over a bad check

		- Added new property PickUpItem to allow a custom item to be selected.
			<property name="PickUpItem" value="MyNPCItem" />
	
			If the property is not there, it will use the default spherePickUpNPC.

	[ DeployNPCSDX ]

		- Added 2 new properties for the PickUp Item, to allow placing down an NPC pre-configured.

			- This will allow you to recieve an NPC as an item.
			- If the AutoHire property is set to true, it will auto-hire the NPC to the player placing it down.
			- If the AutoHire property is not set, the NPC will not be hired on placing it down.
			- If the property EntityName is set, it will assign that name to the NPC. 

			Example:
				<item name="spherePickUpNPC2">
      				<property name="Extends" value="spherePickUpNPC" />
      				<property name="EntityClass" value="npcBakerClub" />
      				<property name="AutoHire" value="true" />
				    <property name="EntityName" value="John Wayne" />
    			</item>
		

	[ Fire Manager ]
		Added a check to disable fire when Particle index is set to 0.
			if (___explosionData.ParticleIndex == 0) return;



Version: 21.2.27.1542

	[ Trader Protection ]
		- Added a check to allow you to talk to the trader during the night, when trader protection is off.

	[ Portals ]
		- Added a check for invalid portal positions

Version: 21.2.18.1001
	[ Trader Protection ]
		- Added a check to disable Trader Teleport when crossing the bounds of a trader.
	
	[ Portal ]
		- Added the ability to define a buff to activate the portal.
			<property name="ActivateBuff" value="myBuff" />
		- If this buff is defined, the buff must be active in order to use the teleport.
			- When denied, the Portal will chime a Denied sign, and show the localization entry for xuiPortalDenied.
			- If the Localization entry for xuiPortalDenied is an empty string, it will not chime, or show a tool tip on denial.
		- For Powered Portals, this check happens after checking if the portal is powered.

	[ EntityAliveSDX ]
		- Added a sanity check for loot container size.

	[ Spook ]
		- Fixed an issue where the sky was not as dark as it was supposed to be.

Version: 21.2.11.1647
	[ Fire Manager ]
		- Fixed an issue where fires could not be extinguished on servers.
		- Fixed a potential issue with an excessive amount of net packages being sent.

	[ NPC Weapon Swap ]
		- Changed NetPackage for weapon swap
		- Added new helper method in EntityAliveSDX to better handle it
		- Added a preventative null check on FindWeapon()


Version: 21.2.1.1646
	[ Rebuild ]
		- Re-built and re-linked against Alpha 21.2

	[ Remote Crafting Utils ]
		- Fixed a change in the TryStackItem in the loot container.

Version: 21.1.111.950
	[ Merging changes ]
		The EntityName property setter is called in (at least) two cases for NPCs:

			During EntityFactory.Create, when it is set to the entity class name
			When the NPC entity is being re-created after being picked up and put down
		
		We should not set the private _strMyName in the first case, since that erases the random name chosen from the "Names" property in the entity class XML. 
		But in the second case, the name that is being set was previously taken from the EntityName property, so should be the correct value chosen from "Names".

Version: 21.1.110.1032
	[ Buff / Quest From Sounds ]
		- Fixed an issue where buffs / quests were not being properly read by sounds.

	[ Hire CVars ]
		- Added new cvar "CurrentHireCount" to keep track of the number of hired NPCs.
		- This is cvar is updated when:
			- An Entity is hired, the CheckStaleHires is ran, updating the cvar.
			- When the MinEvent for ClearStaleHires fires:
				<triggered_effect trigger = "onSelfBuffUpdate" action="ClearStaleHires, SCore"  />

		- This cvar can be used in the dialog system as well, for requirements
	        <requirement type="HasCVarSDX, SCore" requirementtype="Hide" id="CurrentHireCount" operator="LTE" value="3" /> 

	[ Quest Objective Block Destroy ]
		- Fixed an issue because I was calling the wrong net package, and changes were not being distributed properly to the party.

Version: 21.1.97.930
	[ Farming ]
		- Fixed an issue where valves did not work properly on dedi

	[ Dialog ]
		- Added support to trigger an action from a statement, rather than only a response.

	[ RequirementInVehicleSDX ]
		- Added a fix for the name space. 
			- This is waiting for a fix in vanilla before it functions.

	[ Inert ]
		- Fixed an issue where Inert was making invisible entities when paused.

Version: 21.1.89.1227
	[ Game Events ]
		- Fixed typo in Vehicle.
		- Added RequirementInVehicleSDX game events that takes a tag.
			I don't even know if you can make this call from here. But the script is written.

			<requirement class="InVehicleSDX , SCore">
				<property name="invert" value="true" />
				<property name="entity_tags" value="mytags" />
			</requirement>

Version:21.1.89.946
	[ UAI ]
		- Fixed an issue where NPCs could loot, but wouldn't keep any of the items it looted.
		- Rolled back UAI change that caused the considerations to be calculated differently.

	[ Game Events ]
		- Added RequirementInVehicleSDX game events that takes a tag.
			I don't even know if you can make this call from here. But the script is written.

			<requirement class="InVehicleSDX , SCore">
				<property name="invert" value="true" />
				<property name="entity_tags" value="mytags" />
			</requirement>

Version: 21.1.73.1834

	[ MinEvent ]
		Added new MinEventActionConverItem ( Completely Untested. Should be okay. Probably. )
			The intended usage for this is to allow a limited use item that is not connected to the durability,
			and allows it to change into another item.
			I didn't even test this one.
			This MinEvent will add a "MaxUsage" meta data entry using MaxUsage as a starting value.
			Each time the event is fired on the item, it will count the usage down.
			Not even joking.
			When the item is down to 0, it will turn the item into the specified downgradeItem.

		Example:
			<triggered_effect trigger="onSelfPrimaryActionEnd" action="ConvertItem, SCore" downgradeItem="meleeClub" maxUsage="10" />

	[ UAI ]
		- Fixed an issue where the UAI considerations were a bit off. Thanks khzmusik

	[ Tools ]
		- Added Unity Debugging DLLs for Alpha 21.x. Thanks to Yakov.

Version: 21.1.56.1705

	[ Requirements ]
		Added invert support for RequirementIsBloodMoon.
			<requirement name="RequirementIsBloodMoonDMT, SCore" invert="true" />

	[ Farming ]
		- Added an additional check to see if a farm plot is empty.
		- Updated the UAI Farming Task
			- Consumes seed items in the inventory now
			- Resets farm plots after they are all visited, so that the farmer will revisit them
			- Fixed an issue where Farmers won't tend to a farm plot that you have harvested.
			- Fixed an issue where the farmer wasn't looking at the plant correctly enough
			- Fixed an issue where the farmer was too far away from a farm plot to visually have an effect on it.
		- To help with testing, the farmer will place the smoke particle on the farm plot its currently working on.
			- This will be removed after some tests are complete.

	[ Bloom's Family Farming ]
		- Updated the MyFarmer's test entity properties with notrample tag, more UAI, and allow spawns from menu.
Version: 21.1.55.858

	[ Drop Box Storage ]
		- Fixed a bug where items were not removed from drop box in multiplayer

	[ Spawn On Death ]
		Added new Property to allow leaving of the body when spawning a new entity.
			<property name="SpawnOnDeathLeaveBody" value="true" />

Version: 21.1.53.1324
	[ Trader Protection ]
		- Added a new option to allow placing blocks within a Trader Area, under the AdvancedPrefabFeatures ConfigBlock
			<property name="AllowBuildingInTraderArea" value="false" />
		- Disable Trader Protection must be set to true, in addition to this setting, to allowing placing of blocks.
		- Land Claims are still denied from being claimed, to prevent hijacking of a trader by a player.


Version: 21.1.52.1544
	[ Enhanced Signs ]
		- Fixed hard crash when an invalid URL was used.

Version: 21.1.52.1357
	[ Food Spoilage ]
		- Added support for SpoiledItem's value being "None" to skip downgrading the item into something else.
			<property name="SpoiledItem" value="None" />

Version: 21.1.51.1344
	[ EntityAliveSDX ]
		- Re-Added Exp Sharing.
			- If the Hired NPC has a Player, but the Player does not have a Party, the NPC will share exp directly with that player.
			- If the Player has a party, the exp will share with the entire party.
		- Fixed an issue with weapon swapping. 

	[ Broadcast Storage ]
		- Drop Box(tm) Support
			- Added the ability to set up a container to be considered a drop box to distribute items to other storage boxes.

			- Any container with the Property "DropBox" with a value of true is handled specially.
	  			<property name="DropBox" value="true" />

			- Optionally, there is a new block class that can be used.
				<block name="cntSphereDropBoxTest">
					<!-- Doesn't need to be signed container, but might make the most sense -->
					<property name="Extends" value="cntStorageGenericSigned"/>
					<property name="Class" value="DropBoxContainer, SCore"/>
					<!-- Default is 100 ticks -->
					<property name="UpdateTick" value="200" />
					<!-- Optionally trigger the distribution when the user closes the box -->
					<property name="DropBox" value="true" />
					<!-- Optional Distance this box can scan to redistribute. -->
					<!-- If not defined, the value from AdvancedRecipes's distance is used. -->
					<property name="Distance" value="30" />
				</block>

				- Blocks that use the DropBoxContainer class will trigger redistribution at regular intervals, 
					in case the player makes changes to surrounding boxes.


			- When this type of container is closed, it will scan the local TileEntities using the same rules as the Items From Containers for crafting.
				- Rules include broadcasting is enabled, player is allowed to access, if the container is not opened, and within the distance.
				- Other containers with the property "DropBox" are ignored as potential targets.

			- It will try to fill any partial stacks.
			- If existing stacks are full, it will add a new stack to the container.
			- Any items that cannot be added, either partially or fully, will be left in the DropBox.
			- If you are adjust your storage containers to add items, the DropBox must be opened, and closed for it to distribute to the new containers.

Version: 21.1.50.839
	[ Broadcast Storage ]
		- Removed pre-check cuz I'm dumb, and introduced a free-crafting mechanic.

Version: 21.1.48.1604
	[ Broadcast Storage ]
		- Added a few null checks for when player backpack or tool belt may not be available.
		- Added a pre-check if the player has the required items in their backpack / tool belt, that it'll use those first
			- Should provide better performance, and protect against the above mentioned null ref.

Version: 21.1.48.955
	[ Broadcast Storage ]
		- Fixed another issue where toolbelt items were not being consumed when riding in a vehicle and crafting.
			- The original code would read the localPlayer's bag and toolbelt slots for items. These were empty.
			- The fix was to pass the bag and toolbelt from the XUiM_PlayerInventory

Version: 21.1.47.1805

	[ Broadcast Storage ]
		- Fixed another issue where materials would not be returned upon cancellation when pulled from container
		- Fixed another issue where crafting would be approved without all the ingredients.

Version: 21.1.39.1623

	[ Water System ]
		- Fixed an issue where the block's custom description wouldn't show up depending on DisplayInfo
		- Fixed an issue where water range wasn't being properly used for crops.

Version: 21.1.39.1422

	[ Broadcast Storage ]
		- Fixed an issue when a cancelling a recipe that had an ingredient with a quality value would result in no returned items.

Version: 21.1.34.952

	[ EntityAlive SDX ( NPCs )]
		- Fixed an issue where inventory was not being preserved or distributed to clients.
		- Code clean up on class
		- Fixed an issue where the NPC could continue to use a weapon that no longer exists in thein loot container.

	[ SCore Integrity Check ]
		- Added a new feature to SCore's ModEvents.Init() to provide a quick look integrity check.

		Intention:
			- This checks all the modlets installed locally.
			- It's intended to be a Quick Look to see if an overhaul is installed correctly.
			- Each release of an overhaul would have a unique, and repeatable Hash Code.
			- A Hash Code is a 10 digit numeric code.
			- If a modlet is added, removed, or is a different version, the Hash Code will be different.
			- This will not stop a game from loading ( You do you, boo ).

		What It Checks:
			- This will add new entry's to the game load, printing after the "Loaded Mod:" step.
			- This will check, and print the path, if there's a Mods folder in the UserDataFolder or local folder.
			- If there's a mods folder in both, it will display a warning in the log file:
				WARNING: Loaded Mods From two Folders!
			- This isn't a fatal warning, but rather helps you at a glance to see if they are potentially loading mods from two different places.
				- Loading Mods from two locations is not an error, or bug.
				- Some people install a mod to UserDataFolder / Local Mods folder and forget about it.

			- It will loop around all Loaded Mods, do a GetHashCode() on the Modlet Name and Version number.
			- GetHashCode() returns an 10 digit numeric code.
			- All the HashCode for the Loaded Mods are added to a string.
			- At the end, it will print a GetHashCode() from the combined string, giving a summary.
		
		Example:
			2023-09-02T07:49:59 4.454 INF SCore:: Checking Installed Modlets...
			2023-09-02T07:49:59 4.454 INF Loaded Mods From C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/7DaysToDie_Data/../Mods
			2023-09-02T07:49:59 4.455 INF Modlet List Hash: 1788120558
			2023-09-02T07:49:59 4.455 INF SCore:: Done Checking Installed Modlets
		
	[ UAI ]
		- Aded the consideration / Task to the VC Proj of SCore

	[ Build Tooling ]
		- Added a new obj Clean Target to be triggered after "AfterBuild".
		- This will remove the contents of the obj file after the building / linking.
		- Any project that loads the Directory.Build.targets file as a reference.


Version: 21.1.24.928

	[ EntityEnemySDX ]
		- Integrated khzmusik's changes
			- Added a EntityEnemySDX check for an AvatarController Patch
			- Rebased EntityEnemySDX against EntityHuman
	
	[ UAI ]
		- Integrated khzmusik's changes
			- Added new Consideration: HasinvestigatePosition
			- Added UAI Task: ApproachSpotSDX
			- Cleaned up UAI Task AttackTargetEntity
	
Version: 21.1.22.727
	[ Transmogrifier ]
		- Moved the crawler gate up a bit, as it was getting random walk types when it really shouldn't have.

Version: 21.1.21.1021

	[ UAI ]
		- Removed Debug statement for UAIAttackTargetEntity

	[ DecoAoE ]
		- Added additional code in an attempt to make them quest resets work.
			- Further updates may be needed

Version: 21.1.20.1053
	Note: When reference an asset bundle from another modlet, you must use the Name value from the ModInfo.xml
	For example:
		<xml>
		  <Name value="0-SCore_sphereii" />
		  <Author value="sphereii" />
		  <Description value="SCore Mod" />
		  <DisplayName value="0-SCore" />
		  <Website value="" />
		  <Version value="21.1.20.945" />
		</xml>

		<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='FireManagement']/property[@name='FireParticle']/@value">#@modfolder(0-SCore_sphereii):Resources/gupFireParticles.unity3d?gupBeavis06-HeavyLight</set>
	
	[ Water System ]
		- Fixed a possible null ref when reading from a quest-reset
		- Added a OnBlockLoaded for the sprinkler.

	[ NPCs ]
		- Disabled a call that created a player party if they hired an NPC, potentially causing other party invites to fail.

	[ UAI ]
		- Added a + 1 to the y value when doing the IsInside consideration to give it a chance to get out a block or entity to do the check.

	[ Deco Block ]
		- Added a ischild check for multi-dim blocks
		- Added a IsTileEntitySavedInPrefab() call to persist in resets

Version: 21.1.16.1542

	[ NPCS ]
		- Fixed an issue where the NPC wasn't facing at its target
		- Fixed an issue where a bag drop may throw null ref.
		- Fixed an issue where the NPC wouldn't turn around to face the target, when backing up.

Version: 21.1.12.1115

	[ NPCs ]
		- Fixed an issue with a null ref on the read/write for loot containers
		- Fixed an issue where NPC backpacks weren't preserving on dedi.

Version: 21.1.10.1805  ( For A21.1 stable )
	[ NPCS ]
		- Quite a few failed attempts at making inventory persists reliably / weeapon select on respawn

	[ Locks ]
		- Fixed an issue where players could not lock their own doors.

Version: 21.1.3.950
	[ NPCs ]
		- Fixed an issue where the NPCs were not preserving their inventory (for realisies?)

Version: 21.1.1.830
	[ NPCs ]
		- Dialog Fixed for the PickMe Up functionality.
			- Now works on dedicated and single player.
		- Dialog fixed for weapon swap.
		- Fixed an issue where NPCs were not preserving their inventory
		- Fixed an issue where the NPCs were not respawning with the right weapon.

	[ Trader Protection ]
		- Re-implemented a Remove Trader Protection Patch
			- Allows compound to be destructive
			- Traders never close
			- Zombies can wander into compound, but do not spawn in the compound.
			- Quests work.

	[ MinEvent ]
		- Added AnimatorFireTriggerSDX
		- It will recursively look for all animators on each target, firing the trigger one each animator.

		Example:
			<triggered_effect trigger="onSelfBuffStart" action="AnimatorFireTriggerSDX, SCore" trigger="triggerName"/>

	[ One Block Crouch ]
		- Adjusted the crouch adjustment from -0.40 to -.31f. Camera will be slightly higher, but still avoid clipping
		- Added a safety check to not run if player is in a vehicle.

	[ Power Block ]
			It allows to use the wire tool anywhere on a MultiDim Block - from ocbMaurice
			<property name="MultiDimPowerBlock" value="true"/>

Version: 21.0.51.708
	[ Portals ]
		- Added all portals to be chunk observers.
		- Updated the teleport position to be one block above the portal itself.

Version: 21.0.49.1542
	[ Portals ]
		- Fixed an issue where portals would error out on game reload, without exiting the game completely.

	[ Documentation ]
		- Removed documentation to save on space.

Version: 21.0.49.958
	[ HoldingItemDurability ]
		- Commented out debug statement

	[ Locks ]
		- Fixed an issue where cop cars would triggers errors when lock picking, resulting in no loot.
			- Added a check for LockpickDowngradeBlock property. If it's a non-air block, it will use that downgrade option instead.

	[ Remote Crafting ]
		- Fixed an issue when a recipe required multiple ingredients, not all ingredients would be consumed.

	[ Caves ]
		- Flipped the conditional check for the MinThreshold; it does opposite of what it did before.

	[ UAI ]
		- Merged in changes for the UAI tasks to trigger alert, sense, and give up sounds.

Version: 21.0.46.2014
	[ EAI ]
 		- Remove debug statement

Version: 21.0.46.1832

	[ Remote Crafting ]
		- Added null checks to items before trying to use their upgrade sound.
		- Fixed an issue where item dupes were possible.
			- Items are now removed properly from the backpack, toolbelt, then from Tile entities correctly.

	[ Bugs ]
		- Fixed an null ref on the animator when playing a game, exiting to main menu, then re-loading.
		- Fixed a null ref when exiting the game when a drone and hired NPC are available.
			- This was due to the fact that NPCs were being added as an OwnedByPlayer, and the DroneManager
				did not consider it would have been a non-vehicle entity.
	[ Fire Manager ]
		- Disabled fire manager in prefab editor / play testing worlds.

	[ Better Life ]
		- Some fixes for the fish swimming are in the SCore, with some modification to the entityclasses.xml
			- EAI isn't used by fish, so they do not need to be defined.

	[ UAI ]
		- Updated the IsInFront check to make sure the NPC is facing a target correctl, replacing the old, buggy one.

Version: 21.0.45.1205
	
	[ UAI ]
		- Adjusted the EntityUtilities.Stop() to have an optional parameter: full, default is false.
			- When a full stop is requred, it will zero out the animator's speed / strafe
			- Full is triggered when Dialog is open, and when the Idle Task is triggered

	[ Animator ]
		- Added a postfix to adjust the animator's speed / strafe when dealing with very small numbers.

	[ Better Life ]
		- Changed MecanimSDX to AvatarAnimalController to fix the animator bug.
		- Could still be issues

Version: 21.0.42.1007
	[ Dialog ]
		- Added new Dialog Requirement For Tag. This will check the listed tags in the value against the NPC you are talking to.
			<requirement type="HasTag, SCore" requirementtype="Hide" value="zombie" />
			<requirement type="HasTag, SCore" requirementtype="Hide" value="zombie,human" />

Version: 21.0.41.1943

	[ Remote Crafting ] 
		- Fixed an issue where items weren't being properly consumed from the backpack / toolbelt.

Version: 21.0.39.859

	[ UAI ]
		- Fixed an issue where the NPCs would shuffle in place
		- Fixed an issue where non-hired NPCs wouldn't face you.
	
Version: 21.0.38.1508

	[ Caves ]
		- Added new property to determine when a deep cave threshold would be considered
				<!-- Spawning and cave decorations would be considered _Deep when at this level or below -->
				<!-- This setting is used for spawning as well -->
				<property name="DeepCaveThreshold" vallue="30" />
		- Added new property to determine how deep the first cave level is from the top
				<!-- Determines how far below the surface to begin spawning caves. -->
				<property name="StartCaveThreshold" value="15" />
		- Added new property to determine minimum terrain height to start generating caves
				<!-- Default value is -1, no consideration of terrain height -->
				<property name="MinStartCaveThreshold" value="-1" />

				<!-- If the terrain height is 40, but the threshold is 41, do not generate a cave -->
				<property name="MinStartCaveThreshold" value="41" />

		- Fixed an issue where the max levels wasn't being respected.
		
	[ UAI ]
		- Cleaned up IdleSDX

Version: 21.0.37.1546
	[ Caves ]
		- Fixed an issue where caveAir was used instead of regular air, so decorations weren't being placed.
		- Added arramus' fixes for the prefabs (thanks!)
		- Added new property to to the Config Block:

			- This will generate caves in all biomes ( after all the other checks for caves are passed )
				<property name="AllowedBiomes" value="All" />

			- This will only generate caves in wasteland and pine_rest biomes 
				<property name="AllowedBiomes" value="wasteland,pine_forest" />

	[ NPCs ]
		- Expanded the attack angle from -15 and 15 to -30 and 30, to give a wider angle to attack from.
		

Version: 21.0.30.1335
	[ Entity Alive Patch ]
		- Added patch from Zilox to fix the animation issue on animals that do not use root motion, on servers.
	[ Fixes ]
		- Fixes for the LootContainer's spawn calls, which have seen a parameter change in A21 b232.

Version: 21.0.28.902
	[ Remote Crafting ]
		- Fixed a broken reference to the broadcast button

	[ Food spoilage ]
		- Integrated khzmusik's food spoilage adaptage to use the ItemValue dictionary.
			[ This change may be a save-breaking change ]


	[ UAI ]
		- Adjusted the ConsiderationCanSeeTarget to remove the 20 distance change.
			- This will now use the CanSee Distance from the entity itself.
		- Code formatting clean up for Wander Task

Version: 21.0.24.1146
	[ Farming ]
		- Code moved to Features
		- Added support for new water system
		- Added readme.md description its functionality.

	[ Remote Crafting ]
		- Code moved to Features
		- Code refactored for clarity

Version: 21.0.22.1422

	Introducing the Features Folder
		Features folder will contain the harmony and scripts for an isolated feature set, rather than spread across the Harmony and Scripts folder now.
		This will help with future extraction into seperate DLLs.

	[ Locks ]
		- Fixed Unity crash when pick locking doors. 
				- Sound was trying to play on a non-main thread.
		- Code moved to Features

	[ Fire Manager ]
		- Modified the Walk On Fire trigger to fire off the block, rather than the entity.
		- Code moved to Features
		- Added documentation to the Features/Fire/Readme.md

Version: 21.0.21.1610
	[ Broadcast Feature ]
		- Fixed an issue where the game would allow you to craft if you had a single ingredient.
	
	[ UAI ]
		- Fixed an issue where the NPC would not face you when following
		- Added a rotation of 90f in the TaskMoveToTargetSDX for when the entity is blocked.

	[ NPCs ]
		- Added yet another fix to the jumping.

Version: 21.0.20.1553
	[ Dialog ]
		- Added a toolbar / bag search for the NPCHasItemSDX Condition

	[ NPC ]
		- Fixed an issue where the NPC would stare at a dead body
		- Added the ability to add items directly into the NPC's private bag slot, defined in the entityclass.
			<property name="BagItems" value="itemMeleeStoneAxe" />
			<property name="BagItems" value="itemMeleeStoneAxe, foodWater=2" />
		- If the leader has the cvar "quietNPC" set, the hired NPCs will not make foot steps.
		- Added Shared EXP, so hired NPCs will share their exp with you, and the party members.
		- Fixed a few issues with angles
		- Fixed shared exp with party
		- Fixed issue where dead zombies stayed standing
		
	[ Broadcast Storage ]
		- Added FuriousRamsay's suggestion to filter button when in entity or vehicle.

	[ Console Command ]
		- Created a new console command to set cvar's on the player.
			setcvar <cvarName> <cvarValue>

		- To Remove a cvar, set it to 0
			setcvar <cvarName> 0  

		Example:
			setcvar quietNPC 1  
			setcvar FailOnDistanceToLeader 10

	[ UAI ]
		- Updated the FailOnDistanceToLeader consideration to check the distance of the target entity, to see if the entity is outside of the allowed range.
			- This should prevent the NPC from walking halfway to a zombie, then turning around, as it'd take it out of this range.
		- Updated the FaceToEntity code to try to look at the player a bit more often.
		- When the NPC starts the Follow Task, it will clear its attack / revenge targets, as not to obsess over them.
		- Adjusted the yaw and pitch of NPCs to be 8f,8f, which should line them up better. 
Version 21.0.15.1058
	[ Upgrade log ]
		- Updated all XMLAttributes to XAttributes. Gosh
		- Removed QuestTags and converted over to FastTags
		- Oh geeze, XMLAttributes are everywhere

	[ Dialog ]
		Added a DialogActionSwapWeapon to allow a NPC to change weapon
			<action type="SwapWeapon, SCore" id="gunNPCPipeShotgun"  />
		Added DialogActionAnimatorSet to change parameter.
			<action type="AnimatorSet, SCore" id="PistolUser" value="0" />
		Added a DialogActionRemoveBuffNPCSDX to remove a buff from a NPC
			<action type="RemoveBuffNPCSDX, SCore" id="PistolUser" />
		Added a DialogActionPickUpNPC, allowing you to pick up an NPC as an Item
			<action type="PickUpNPC, SCore" />
			- Item is hard coded, and defined in SCore's items.xml
		Added A DialogActionDisplayInfo to dump the NPCs Buffs and CVars to thelog file.
			<action type="DisplayInfo, SCore" />

		Reminder: Console command  dialog   or dialogs    will reload dialogs.xml without restarting the game.

	[ EntityAlive SDX ]
		Added New Feature to allow NPCs to change weapons on the fly.
		Added a UpdateWeapon( item ) call to allow an NPC to change weapons
		Added PickUp NPC option.
			- This preserves ownership and name of Entity.
			- Entity gets created into an items.xml entry (defined in SCore's Items.xml
		Re-added Progression for NPCs under the following conditions:
			- If they are EntityAliveSDX
			- If they do not hava a cvar called "noprogression" with a value greater than 0
			- if they do not have a tag called "noprogression"
		Fixed an issue where the PhysicsTransform was not active, allowing NPCs to clip inside of each other.

	[ MinEvent ]
		Added MinEventActionAnimatorSetFloatSDX
		Added MinEventActionAnimatorSetIntSDX
			- Triggers as UpdateInt / UpdateFloat on the animator, updating the parameter with the supplied value.
			- If the value starts with a @, the cvar value will be used.
			Example:
				<triggered_effect trigger="onSelfBuffUpdate" action="AnimatorSetIntSDX, SCore" property="HoldType" value="@WeaponType" duration="1" /> 
		Added MinActionSwapWeapon
			Causes the NPC to have the specified Item.
			Example:
				<triggered_effect trigger="onSelfBuffUpdate" action="SwapWeapon, SCore" item="meleeClub" />

	[ Entity Player ]
		Fixed One Block Crouch to prevent clipping in terrain.

	[ Winter Project Snow ]
		- Fixed an issue where terrain was bumpy around POIs when buried in snow ( Winter Project Only )
		- Fixed an issue with snow material that caused collapses.

	[ Caves ]
		- Added Pillars from bedrock to strengthen POIs weakened SI

	[ Lock Picking ]
		- Fixed a hard crash when trying to access Progression

	[ Fire Manager ]
		- Added the ability to specify on a per block basis the chance to extinguish itself.
			<property name="ChanceToExtinguish" value="0.05" /> <!-- 5% chance to exintguish -->
		Note: This is checked per block, per CheckInterval.

Version: 20.6.471.1518
	[ Quest Utils ]
		- Uncommented code that was accidentally commented out.

Version: 20.6.470.1151

	[ Quests / Entity Targetting ]
		- Merged in a bug fix for khzmusik.

		The revenge targets were being set on all entities in bounds, including the player hires. 
		This is now fixed, and in addition the revenge targets also will not be set on other players in the party, 
			provided those players are protected from friendly fire.

		To detect whether an entity is an ally of the player's party, I created a new IsAllyOfParty method in EntityTargetingUtilities. 
		I did not change any existing methods so there should be no risk of breaking anything.

		I also fixed an issue where the POI's full area was not covered (the Bounds constructor shrinks the size vector 
			argument in half so only a quarter of the prefab was covered).

	[ MinEvent ] 
		-Added a MinEvent to attach scripts to entity transforms.
		- Note: This should not be used yet. I've added it for xyth's testing for getting zombies to create foot prints in the snow.

		Example:
			<effect_group>
				<triggered_effect trigger="onSelfFirstSpawn" 
					action="AddScriptToTransform, SCore" 
					transform="RightFoot"    // The Game Object's Name to target.
					script="GlobalSnowEffect.GlobalSnowCollisionDetector, SphereII_Winter_Project"  // The script you want to attach:  Namespace.Script, Assembly
				/>
				<triggered_effect trigger="onSelfFirstSpawn" action="AddScriptToTransform, SCore" transform="LeftFoot" script="GlobalSnowEffect.GlobalSnowCollisionDetector, SphereII_Winter_Project"/>
			</effect_group>

Version:  20.6.467.917

	[ Quests ]
		- Merged khzmusik's new Quest Action.

			This quest objective sets the revenge targets of all entities in range.

			<action type="SetRevengeTargetsSDX" id="party" value="location" phase="2" />

	[ Lock Pick ]
		- Adjusted MaxGiveAmount to be 2 * the perk level

	[ Music Boxes ]
		- Reformatted, and refactored

	[ IsActive ]
		- Fixed some issues with the TileEntity AlwaysActive
		- Re-enabled full Winter-Project support

	[ Inert ]
		- When an entity is inert, the entity is paused. No animations, no sound, no attacks, and takes no damage.
		- When an entity has the following property, it will only be active at night. Otherwise, it'll be considered inert.
		      <property name="EntityActiveWhen" value="night" /> <!-- alternative is day -->
		- Added a check for TargetIsAlive if the entity is Inert or not. If it is, it's ignored.
		- Added checks for IsEnemyNearby, and CanSeeTarget to do Inert checks.
		- Added patches so inert entities will not make a sound.

	[ Fire Manager ]
		- Tagged Material: Mhay to be flammable.
		- Re-factored FireManager to be clearer.
		- Added some performance tweaks:
			- Sound will not always play on each fire block. Instead, each block will have a random chance to either play a sound or not. (10% chance to play a sound)
			- Added 5% chance for a block to self-extinguish.	
			- These checks are re-evaluable on the CheckInterval time.
		- Added a check for Explosion.
			- If a block has an explosion property, and is set as flammable, the block will trigger an explosion when it downgrades.
		
	[ Explosion Particles ]
		- The vanilla prefabExplosions array that maintains a list of possible explosion particles via the Explosion.ParticleIndex is set to 20.
		- When a game starts, the SCore will check it's ConfigurationBlock's ExternalParticles node for external particles.
			Example:
				<property class="ExternalParticles">
					<!-- The name is not used by the system. The index value will be displayed in the log during a game boot up. -->
					<!-- Review the log to find out your index. -->
					<!-- The Index is a GetHashCode() on the value. -->
					<property name="SmokeParticle" value="#@modfolder:Resources/PathSmoke.unity3d?P_PathSmoke_X" />
					<property name="FireParticle" value="#@modfolder:Resources/gupFireParticles.unity3d?gupBeavis05-Heavy" /> 
				</property>

		- If a particle entry is detected, this particle will be registered with the main ParticleEffect.RegisterBundleParticleEffect.
			- This is the same particle effect that is used elsewhere in the system, including the Fire Manager.
		- The index of this particle will be the bundle's GetHashCode(). 
			- This value can be viewed in the log file:
				Registering External Particle: Index: -87591912 for #@modfolder(0-SCore):Resources/PathSmoke.unity3d?P_PathSmoke_X

			- This index is what you should use in your Explosion.ParticleIndex.
				<property name="Explosion.ParticleIndex" value="-87591912"/> 

		- This check is applied via a Harmony patch to GameManager's ExplosionClient, and will be triggered if the ParticleIndex is not within a range of 0 and 20.

		- Append to SCore's ConfigurationBlock's ExternalParticles Entry.
		

Version: 20.6.453.1912

	[ Effect Group Requirement ]
		- Fixed a bug where it just didn't work.

		Tested Configuration, other combinations may work:

			<effect_group name="RETURN BROKEN">
				<triggered_effect trigger="onSelfPrimaryActionStart" action="CreateItemSDX, SCore" item="resourceBone" >
					<requirement name="HoldingItemDurability, SCore" operation="Equals" value=".1"/>
				</triggered_effect>
			</effect_group>

	[ Fire Manager ]
		- No code change, however wanted to say that the SetLightOff was moved from the GameUpdate loop, and moved behind the check interval.
		- Performance increase potential.

Version: 20.6.453.1540

	[ Fire Spread ]
		- New property in Config/blocks.xml.
		- If FireSpread is false, fire will not spread to neighboring blocks.
		- Default is true, fire will spread.

	[ EntityAliveSDX ]
		- Merged in FuriousRamsay's changes
			- Remove the colliders on death, so you can keep bodies around afterwards
			- Added lootable corpses.

	[ Spook Theme ]
		- Fixed an issue where Spook wasn't spookie. Effects easier to tell at night.


	[ Effect Group Requirements ]
		- Added a new Requirement, meant to be used in the effect_group / triggered events.
		- Note: This is completely untested.
	
			<!-- True when the item is 100% broken -->
		 	<requirement name="HoldingItemDurability, SCore" value="1"/>

			<!-- True when the item is 50% broken -->
		 	<requirement name="HoldingItemDurability, SCore" value="0.5"/>

		Example on an items.xml reference:
			<effect_group name="Check For Broken" >
				<requirement name="HoldingItemDurability, SCore" value="1"/>
				<triggered_effect trigger="onSelfSecondaryActionStart" action="CreateItemSDX, SCore" item="whatever" />
			</effect_group>


Version: 20.6.442.1932

	[ Food Spoilage ]
		- Added an additional check for PreserveBonus -99 to not spoil when taken out.

	[ Lock Picking ]
		- If keyboard is not detected as the primary input device (ie, a controller is), then disable the lock mini game, and fall back to vanilla
		
		- Added a cvar check to fall back to regular lock picking
			- If the player has the cvar LegacyLockPick  of greater value than 0, default lock picking will be used, skipping the mini-game.

	[ MinEffect ]
		Added Soleil Plein's new Console command that executes commands and passes cvar values to it.
			<triggered_effect trigger = "onSelfBuffStart" action = "ExecuteConsoleCommandCVars, SCore" command = "testCommand {0} {1}" cvars = "cvar1,cvar2" />

	[ Encumbrance ]
		- Needed a way to re-calculate the encumbrance when the cvar for max encumbrance triggers.
		- Not many good ways came to me for this, so I wrote a MinEffect. This will recalculate the encumbrance values.

			<triggered_effect trigger = "onSelfBuffStart" action = "RecalculateEncumbrance, SCore"  />

	[ Legacy Distance Terrain ]
		- Disable Legacy Distant Terrain in GameModeEditWorld mode due to reported lag.

Version: 20.6.247.845
	[ Vehicle No Pick Up ]
		- Fixed spelling error in NoVehicleTake.cs
			- Left old name as an empty, mispelled file to be removed for A21... if I remember.

	[ Farming ]
		- Added property check for Direct Water Source.
			<property name="WaterType" value="Unlimited" />
		- Any block with that set will be an unlimited water source, and will not take damage from crops.
		- Add to bedrock, or any other block you want.

	[ Quests ]
		- ObjectiveBuffSDX now displays the localized name_key for the buff.

Version: 20.6.422.831

	[ Vehicle No Pick Up ]
		- Fixed a bug where removing the "take" option just shifted the index, not remove the functionality.

		- Added a few filtering options, allowing modders to fine tune which vehicles can be picked up or not. 
		- The follow conditions allow for vehicle pick up, listed by the order in which they are evaluated, if the feature is enabeld in the Config Block's VehicleNoTake
			- If the vehicle has the tag: takeable
			- If the Player has the cvar: {EntityVehicleName}_pickup, and this value is greater than 0.  
				Example:     vehicleBicycle_pickup = 1
			- If the Player has the cvar: PickUpAllVehicles, and this value is greater than 0.
		- In all other cases, the vehicle's "take" command will be disabled.



Version: 20.6.420.840
	
	[ Encumbrance ]
		- Added a check that for block weight.
			- Previous was only checking if the Item entry had a weight.
			- Now checking ItemWeight, and if not found, will check the Block weight.
			- If both item and block has different weight, the Item weight will take priority

	[ Repair From Container ]
		- Fixed the issue where the tool belt items were not being consumed.
		- Activated original Repair Block code. Code was not added to project and not built.
			- Disabled my own crummy implementation which was causing more bugs.


Version: 20.6.416.1123

	[ MinEvent ]
		Added MinEventActionSetDateToCVar
			This will set the Current Day to the CVAR $CurrentDay.

			Example: 
				<triggered_effect trigger="onSelfBuffStart" action="SetDateToCVar, SCore" target="self"/> 

				CVar $CurrentDay will have the current game day.

		
	[ Farming ]
		- Added debug information to water pipes, farm plots, and crops.
			- If the player holds down the "Activate" key, it will display the information about water.

			- Ie: holding down <E> by default will show information about some blocks.

		- Adjusted the way that the Farming Task extracts Harvest / Items

			Seeds:
				- The first planted*1 item it finds will be the seed which is replanted.

				Example:
					<drop event="Destroy" name="plantedBlueberry1" count="1" prob="0.5"/>

				- All planted*1 items will be removed from the harvest / destroy lists.
				- The Farming Task will generate a new item to be used for planting.

			Harvest:
				- By default, all harvesting will continue as expected. 
				- If the NPC has a cvar for the Harvest item, it will set the min and max count to that value.
		
				Example:
					If MyFarmer has a cvar called "foodCropBlueberries", with a value of 5, MyFarmer will get 5 blueberries from each harvest,
						regardless of prob or min / max count defined in the Harvest line.

		- Adjusted Farming Task Scanning to include tending to wilted plants.

		- Updated BlockWaterSourceSDX to be able to supply unlimited water, without requiring a water block itself, or doing damage.

			<property name="WaterType" value="Unlimited" />

			Default is limited, and is equivalent to 
				<property name="WaterType" value="Limited" />

		- The above property can also be applied to a BlockLiquidv2, and will have the same effect.
			Default is limited.
		

Version: 20.6.414.1626
	[ Farming ]
		- Fixed a bug where the Farmer would not see FarmPlots
			- The BlockFarmPlosSDX was not setting IsNotifyOnLoadUnload to true, thus it was not registering being loaded and unloaded.

	[ Encumbrance ]
		- Fixed a bug where the encumbrance of equipment was not being done correctly

Version: 20.6.413.1556

	[ Craft / Repair From Containers ]
		- Adjusted permission for Broadcast feature to allow members of the same party to access locked containers.


Version: 20.6.412.1907

	[ Craft / Repair From Containers ]
		- Rolled back a fix when count of items were doubled.
			- Seems this fixes it for some containers, but for others, it displays no items.

	[ Fire Manager ]
		- Integrated FuriousRamsay's fix for the MinEffectAddFireDamage.
			- In some cases, a melee weapon will set fire to contents on the other side of a wall or door, if the ray cast went through the block.

	[ Farming ]
		- Fixed an issue where a water plant was doing damage to a sprinkler, instead of water block
		- Added an additional scan for UAITaskFarming to do a wider scan if it doesn't find anything interesting.

Version: 20.6.409.1626

	[ Craft / Repair From Containers ]	
		- If a container is open, do not include the contents in the broadcast scan.

	[ Farming ]
		- Fixed an issue where a Plant could not consume water from the water, through the sprinkler.
		- Added a fix to help improve Farmer to reach corner farm plots
		- If a PlantGrowingSDX block has a PlantGrowing.Wilt, it will flag the plant to wilt if there's no water

		- Added new Config Block Entry called WaterParticle on CropManagement node.

			- When a plant consumes water, this particle will be applied.
			- When a plant is first planted, this particle will be applied.
			- Default particle in Bloom's Family Farming is from Guppycur. 
				It will run for 5 seconds, then stop looping.
				The particle is not removed until the plant is removed.

			Default: 
				<property name="WaterParticle" value="NoParticle" />

			Bloom's Family Farming XPath:
				<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='CropManagement']/property[@name='WaterParticle']/@value">#@modfolder:Resources/guppyFountainDisplay.unity3d?gupFountainDisplay</set>
			
			- Each Plant can over-ride the particle at the block level, using the same property. 
				<!-- No particle for this particular plant -->
				<property name="WaterParticle" value="NoParticle" />

Version: 20.6.408.1442

	[ Craft From Containers 
		- Fixed an issue where craft / repair from containers was checking tool belt, then containers. 
			It was checking to see if the item was in the backpack, but not consuming it.


Version: 20.6.408.1121

	[ Craft From Containers ]
		- Fixed an issue where GetItemCount() patch was searching for ingredients twice, resulting in mis-reporting (2x amount actually available )

	[ Encumbrance ]
		- Added basic encumbrance.
		- Encumbrance check fires when something is added / removed from back pack, toolbelt, and equipment.
		- Once encumbrance is calculated, it's total value is added to a cvar, specified in the Config block. By default, this is encumbranceCVar.
		- The value of the cvar is based on percent.  When set to 1f, the player is considered at MaxEncumbrance. 
			- A value of 1.5 means the player is considered to b e 50% over encumbered.
		- Maximum Encumbrance is read from the Config Block. However, if the CVar "MaxEncumbrance" is set, it will use that value instead.
			- If Max Encumrabrance drops below 0, it will be reset to the Config Block Entry.
			- The CVar will not be reset, only the value being used to perform the calculation.

		- New configuration options added to SCore's Config block
				<!-- Enables item weight encumbrance on the Player bag -->
				<property name="Encumbrance" value="false" />

				<!-- how much encumbrance before "max" threshold is set, and penalties are incurred. -->
				<property name="MaxEncumbrance" value="10000" />

				<!-- This cvar value will be placed on the player and will be a percentage of encumbrance. -->
				<!-- 1f = at max encumbrance. 1.5, 50% over encumbrance -->
				<property name="EncumbranceCVar" value="encumbranceCVar" />
				
				<!-- Include Tool belt? -->
				<property name="Encumbrance_ToolBelt" value="false" />
				
				<!-- Include equipment ? -->
				<property name="Encumbrance_Equipment" value="false" />
				
				<!-- Each item that does not have a ItemWeight property will be weighed at this value. -->
				<property name="MinimumWeight" value="0.1" />


			Recommend XPath:
				<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedPlayerFeatures']/property[@name='Encumbrance']/@value">true</set>
				<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedPlayerFeatures']/property[@name='Encumbrance_ToolBelt']/@value">true</set>
				<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedPlayerFeatures']/property[@name='Encumbrance_Equipment']/@value">true</set>
				<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedPlayerFeatures']/property[@name='MinimumWeight']/@value">1</set>

		- Each item can have a property called ItemWeight which will over-ride the MinimumWeight from the SCore's block's entry.
				<property name="ItemWeight" value="10" />

		- ItemWeight can be 0, and can also be negative numbers.



	[ Farming ]
		- Fixed a bug where a water sprinkler was acting as an independent water source. 
			-> Water sprinkler is now checking to see if its connected to a valid water source
		- Fixed an issue where crops could be planted after a sprinkler was removed, and the area was no longer watered.
		- Added debug information on pipes to show water source, and how much water is left.
		- Added a Custom description for BlockPlantGrowing to show water information. This is for testing purposes.
				<property name="DisplayInfo" value="Custom"/>
		- Fixed an issue where Farmer would get bored and do nothing.


Version:  20.6.405.854
	[ Farming ]
		- Fixed a bug where the water range check was incorrectly using the water range of air, rather than the plant.

Version: 20.6.403.1003

	[ Farming ]
		- Fixed an issue where the sprinkler blocks were not registering themselves to the Valve System.
			- Plants will check all valves ( sprinklers ) for their water range, then calculate if its within range of the valve.
			- If there are no valves within range, plant will check for surrounding blocks for water.


Version: 20.6.403.2043

	[ Broadcast Feature ]
		- Added code for Repair / Upgrade from storage
		- Added check to see if enemy was nearby for Repair / Upgrade from storage

	[ UAI Farming Task ]
		- Moved seed decrements from UAI Task to the Manage method, to more accurately subtrack seed usage.
		- Added water check to see if NPC can actually plant at the location.
		- Added a Water Valve Check
			- When a water block will scan for a water source, it'll check all the valves registered, and check if the plant is within range of the water block's water range
			- In order for this water valve to be registered, it must be on. Just having a water valve that is not on, will not be sufficient for plant needs.
					- This is different behaviour than using a regular non-valve water source.
			- If the original valve is off when a plant checks, it will look for other valves to see if there's any water available on them.
			- If none are found, it'll resort to legacy behaviour, and scan for water sources.

			- BlockWaterSourceSDX has a new property. Default is 5f.
				<property name="WaterRange" value="5" />


	[ Trample Code ]
		- Entities that have "notrample" cvar value greater than 0, will not cause crops to be damaged, if the crops damaged feature is enabled.
		- Entities that have a "notrample" tag will not cause crops to be damaged.

	[ Quest ]
		- Added Localization support.
			ObjectiveBuffSDX_keyword  ( fall back is ObjectiveBuff_keyword, which is simply Get in English )
			<buff> If the buff name has a localization setting, it'll use that, in conjunaction with the keyword  Get <my buff>

Version: 20.6.381.1359

	[ Merge from khzmusik ]
			- Fixed a null ref when an NPC would shoot a drone.

			- Enhanced support for NPC Guard

			The Guard command, when issued from a pathing cube, sets the NPC order to Guard. Since it is not issued by the NPCs leader or owner, the guard position is set to (exactly) where the NPC is standing.

			This is different from the "GuardHere" response (aka "Stay where I am standing") in the hired NPC dialog menu. That dialog response sets the current order to Stay, and since it is issued by a player, 
				the guard position is set to the center of the block where the player is standing.

			This is all done by buffs. The buff from the pathing cube was already named "buffOrderGuard" in the C# code, but there was never a buff by that name in buffs.xml. So, I created a new buff for use by that code.

			The buff used by the dialogs was "buffOrderGuardHere", which matched an existing buff in buffs.xml. That buff is unchanged.

			Because the order is Guard, any NPC Core utility AI tasks that will not fire when the order is Stay, will still fire. This means NPCs will still run after enemies, reload, etc.

			In order to have them stay where they are when initially spawned in, and return to their guard positions after they're done attacking enemies, a new AI task will need to be 
				added in utilityai.xml. But this is not done in SCore.

			No existing behavior should be affected by these changes; only POIs that have pathing cubes with "task=guard" will need these changes, and to my knowledge nobody is doing that.


Version: 20.6.368.1433

	[ Food Spoilage ]
		- Fixed an issue with -99 not stopping spoilage when using PreserveBonus -99

	[ RandomDeathSpawn ]
		Bug Report #61:	Wrong spawns on multiplayer of "Burn Victim" #61 (https://github.com/SphereII/SphereII.Mods/issues/61 )
			- Added isEntityRemote check before determine to spawn or not.

	[ ObjectiveBlockDestroySDX ]
		Added a NetPackage to help distribute the block being destroyed count to the party for shared accumulation

	[ Fire Manager ]
		- Disabled the extinguished check to see if a block has been flagged as extinguished or not.
		- Added a check where extinguished removed a block from the fire map.
		- New functionality is that each time a block is extinguished, it'll set the expired time, regardless if it's already in the list, counting down.
			- If BlockA was extinguished, it can smoke for 20 seconds.
			- After 20 seconds, BlockA can catch fire again.
			- If BlockA is re-extinguished before the 20 seconds are past, expired time will be set back to 20.

		- Old functionality is that each time a block is extinguished, it'll expire after the expired time, allowing it to re-ignite.
			- If BlockA was extinguished, it can smoke for 20 seconds.
			- After 20 seconds, BlockA can catch fire again.
			- If BlockA is re-extinguished before the 20 seconds are past, it won't reset that time period.
			

Version: 20.6.297.1109

	[ Enitity Alive SDX ]
		- changes by kgiesing
		
		- With these changes, both EntityAliveSDX and EntityEnemySDX will implement that interface. 
			- This should allow EntityEnemySDX to behave similarly to EntityAliveSDX

			- This creates a new interface named IEntityOrderReceiverSDX, which should be used on any entity that can accept orders.

			- Also, order-related blocks, MinEvents, and AI tasks are updated to use the interface type, rather than EntityAliveSDX directly.

Version: 20.6.295.807

	[ Broadcast Manager ]

	- possible fix for Broadcastmanager not saving on Multiplayer games : by FuriousRamsay

Version: 20.6.285.831

	[ Broadcast Manager ]

	- Broadcastmanager will now save and cleanup on exiting

	- It should now be possible to broadcast from storages put down by other players
		(storage must be locked if you don't want other players be able to remote craft from your storage)
	
	- Changed Button code for better support in custom UIs.
	
	[ Remote Crafting / Repairs ]
	
	- added remote repair/upgrade on  blocks by FuriousRamsay
	- added blocking repair/upgrade when enemies nearby


Version: 20.6.259.937

	[ BlockClockDMT ]
		- added in BlockClockDMT, which gives a working clock, updating an animator on the unity object every hour.
		      <property name="Class" value="ClockDMT, SCore"/>

	[ Spawn On Death ]
		- Commented out code that would destroy the gore block, which may not be relevant anymore.

	[ Remote Crafting ]
		- Solved bug where broadcast button was not being hidden when broadcastmanager was disabled

Version: 20.6.250.1020

	[ Fire Manager ]
		- Fixed a potential null ref when the fire check is firing before the game is fully loaded.

Version: 20.6.249.1333

	[ Random Death Spawn ]
		- Updated spawn code to reflect the changes from SpawnEntity MinEffect to avoid duplicating entities

	[ Linux Fixes ]
		- Fixed null reference errors when running on Linux client for crop trample and IsAlwaysActive

Version: 20.6.247.1047

	[ Entity Stats ]
		Using patch from haidr'gna.  Sets cvars on the character that contains the current rain fall, current snow fall, and current cloud percent

		CVar:
			_sc_rain
			_sc_snow
			_sc_cloud

		    __instance.Entity.Buffs.SetCustomVar("_sc_rain", rain, true);
            __instance.Entity.Buffs.SetCustomVar("_sc_snow", snow, true);
            __instance.Entity.Buffs.SetCustomVar("_sc_cloud", cloud, true);

	[ Fire Manager ]
		
		- If an entity has a property set for SpreadFire set to false, an explosion from it will not spread the fire.
			<property name="SpreadFire" value="false" />

		- If an entity has a cvar called SpreadFire, and is set to -1, an explosion from it will not spread the fire.

		- Commented out OnEntityWalkingPatchFire, as I believe UpdateCurrentPositionAndValue patch is sufficient to catch an entity on fire.

	[ Block Utilities ]
		- Added a check to not add particles if the system is on a dedicated server


Version: 20.6.242.1151

	[ Cave Manager ]
		- Adjusted math for Heigh map

Version: 20.6.240.913

	[ Fire Manager ]
		- Potential fix for stack error on Linux

	[ Cave Manager ]
		- Adjusted math for HeightMap mode
		- Added new default height map as cave1.png.

Version: 20.6.238.1937

	[ Cave System ]
		- Initial implementation of Heighmap cave system.
		- Samples under 0-SCore/Caves/Stamps
		- To enable Height Map Caves using xpath:

			<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='CaveConfiguration']/property[@name='CaveEnabled']/@value">true</set>
			<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='CaveConfiguration']/property[@name='GenerationType']/@value">HeightMap</set>
			<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='CaveConfiguration']/property[@name='CavePath']/@value">@modfolder:/Caves/Stamps/cave11.png</set>
			<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='CaveConfiguration']/property[@name='CavePrefab']/@value">Large</set>

		- Default is cave11.png

		- Teleport to 0 0, then look for underground. The top corner of your target png is currently set to 0,0
			-> You can also look at the log for:
				Cave Teleport: 

				This will give you the coordinates to teleport to. Example:
					Cave Teleport: 57 0

					teleport 57 0

		- Log file is kind of spammy.
			- During initial load up, it will display lines of numbers for the cave mapping as the system sees it.
			- When a cave is being drawn, it'll display Target Depth.

		- The CavePrefabs placeholders are currently defined as follows:
			Large:  3x4x3 block of air
			Medium: roughly 3x1x3
			Small:	1x1x1

			- This will be fleshed out more, but this is what you can start off with. I don't find Medium and Small very useful, but your heightmaps may need adjustments


Version: 20.6.238.1305

	[ Fire Manager ]
		- Updated two Harmony patches that give buffs to check if the block is on fire and has a particle.

	[ Random Death Spawn ]
		- Removed blocking Check:
			if (__instance.Buffs.HasCustomVar("NoSpawnOnDeath")) return true ;
		
			- Added another line of guppy code to prevent spawns:
			if (strSpawnGroup == "SpawnNothing") return true;

		- Resynced Guppycur's changes

		- If an entity has a custom cvar called RandomSize, and a custom cvar called SpawnCopyScale, the newly spawned entity will spawn with the same scale as the original

	[ UMA ]
		- Brought forward some UMA patches that are disabled by default; toggle in Config/blocks.xml
			-> If UMAs are set to type="Zombie", create the Data/UMATextures Folder
				-> If UMATweaks is enabled, this folder is created when a type is Zombie.
			-> If UMAs are set to type="Player", you do not need a Data/UMATextures folder

Version: 20.6.235.1604

	[ ObjectiveBlockDestroySDX ]
		-> Added support for a comma-delimited list for the block names that are valid.
			<objective type="BlockDestroySDX, SCore" id="woodChair1,officeChair01VariantHelper,woodChair1Broken" value="1" phase="2"/>
		
		-> Added support for comma-delimited list of tags. If the objective's ID does not match any known block, the objective will assume the id is a potential comma-delimited list of tags.
			<objective type="BlockDestroySDX, SCore" id="ore,deepOre" value="1" phase="2"/>

		-> Note: Order of checks is: comma-delimited block name, block name, comma-delimited tag. This is important because some tags will match a block name (terrGravel, wha u doin')

		-> Note: Count is unified, so a woodChair1 and woodChair1Broken counts as 2 towards this objective 

	[ XUI Menu ]
		-> Added a Config/XUi_Menu/windows-Template.xm to expose over-ride options using w00kie n00kie's Custom Game Option mod: https://gitlab.com/wookienookie/CustomGameOptions.git
			-> other settings can be exposed with just XML settings. Some documentation exists in the windows.xml on how to map the settings to the xui.
			-> This file is just a template file. Using it as-is is NOT recommended, as it will not display properply.
		-> This is an optional feature that will not work without the CustomGameOptions.

Version: 20.6.234.911

	[ EntityAlive Patch ]
		- Added a chunk of Guppycode I missed

	[ SpawnCubeSDX ]
		- If the keep variable is on the Config line (the value does not matter.. keep=0), then it will not self-destroy
			-> When keep is available, it'll set a scheduled update into the future.

	[ Entity Swimming ]
		- Fixed an issue where fish were outside of water

	[ A Better Life ]
		- Adjusted spawn rate in the biomes.xml for fish spawns.

Version: 20.6.233.1938

	[ Entity Alive Patch ]
		- Added new cvar check to disable SpawnOnDeath
			-> If a particular entity has a cvar called: NoSpawnOnDeath, then it will skip the SpawnOnDeath

	[ Headshot Only Patch ]
		- Fixed typos

	[ Fire Manager ]
		- If SmokeTime is set to 0 or below, no smoke will be placed.

	[ 0-SCore Development ]
		- Re-visited AssemblyInfo.tt and fixed up a issues, simplyfing it and adding leading 0s

	[ SpawnCubeSDX ]
		- Added new property called keep=0. This will keep the spawn block from destorying itself, which was affecting water spawned entities.

		<block name="terrWaterSpawner">
		  <property name="Extends" value="SpawnCube"/>
		  <property name="Class" value="SpawnCube2SDX, SCore" />
		  <property name="Config" value="eg=AnimalSwiming;buff=buffOrderStay;pc=0;keep=0" />
	  </block>

Version: 20.6.231.98

	[ Entity Alive Patch ]
		- Moved SpawnOnDeath() to a prefix rather than postfix, to remove ragdoll

	[ Fire Manager ]
		- Fixed a threading issue where a zombie hand attack would crash if it tried to add fire.
		- Fixed an issue where a Material does not have MaterialDamage setting (error reported by magejosh)

Version: 20.6.230.1610

	[ Entity Alive Patch ]
		- Fixed an issue where entity's using the Vulture class were stuck in their jump pose.

		- Added a ForceDespawn() call to the SpawnOnDeath() feature.

	[ Fire Manager ]
		- Moved registering of particles to the Block.Init()
			-> Checks the block and material for FireParticle and SmokeParticle.


Version:  20.6.230.86

	[ Remote Storage ]
      - Fixed Bug where items with ItemQuality where not removed from containers from matteo

	[ Fire Manager ]
		- Removed test code that turned terrain into burn forest when burned.
		- Updated Explosion patch.
			<property name="Explosion.BlockDamage" value="0"/>  <!-- Setting it to 0 neither starts nor puts out a fire -->
			<property name="Explosion.BlockDamage" value="-1"/>  <!-- extinguishes. Value less than 0 -->
			<property name="Explosion.BlockDamage" value="1"/>  <!-- lights fire. Value higher than 0 -->

				
Version: 20.6.229.1021

	[ Fire Manager ]
		- Added optional cvar flag to MinEffect for CheckFirePRoxity. By default, this is _closeFires
			<triggered_effect trigger="onSelfBuffUpdate" action="CheckFireProximity, SCore" range="5" cvar="_closeFires" />_
		- This allows you to specify different cvars for different ranges

		- Added support for a FireDowngradeBlock property on a block.
			<property name="FireDowngradeBlock" value="terrSnow" />

			-> This will downgrade a block that is damaged by fire only.
			-> If set, the downgraded block will go through the blockplaceholders for other options.
			-> If the new block is flammable, it will remain on fire.
			-> If the new block is not flammable, it will get extinguished.


Version: 20.6.229.2113

	[ Fire Manager ]
		- Fixed an issue where zombies do not catch fire walking through fire.


Version: 20.6.229.2113
	[ Fire Manager ]
		- Hot fix to prevent crashing when adding sound (like trees)

		- Exposed new properties in Config/blocks.xml
				<property name="SmokeTime" value="60" />					<!-- How long the smoke will stay on a block. -->
				<property name="FireSound" value="FireMediumLoop" />		<!-- Sound Data Node to use. Can be over-ridden by individual block -->
		- FireSound can be over-written on a block by basis.		

Version: 20.5.228.1520

	[ Fire Manager ]
		- Added new buff Requirement to determine if you were close to a fire block
			<requirement name="RequirementIsNearFire, SCore" range="5" />

		- Added a new MinEffect to determine how many burning blocks are within the player.
			<triggered_effect trigger="onSelfBuffUpdate" action="CheckFireProximity, SCore" range="5"  />		

			-> This sets a cvar called _closeFires with how many burning blocks are near by.
		
		- Added sound effects for fire.

	[ Remote Storage ]
    	- Added Invert feature to disabledsender
			-> If Enabled only mentioned containers share inventory
        - Added grouping of workstations to nottoworkstation and bindtoworkstation
			-> It is now possible to group multiple workstations to a group of storages


Version: 20.5.227.1659

	[ Fire Manager ]
		- Updated the code to set fire to players to use AddBuff instead of AddBuffNetwork

	[ SpawnCube2SDX ]
		- Added two new mineffect to set and clear owner of the SpawnCube2SDX

			<triggered_effect trigger="onSelfDamagedBlock" action="ClearOwner, SCore" />
			<triggered_effect trigger="onSelfDamagedBlock" action="SetOwner, SCore" />

Version: 20.5.227.925
	
	[ Remote Recipes ]
		- Merged changes from matteo
			added features requested by pipermac...
			updated searchnearbycontainers since checks are already done by gettileentities.
			changed some methods to use searchnearbycontainers instead of gettileentities.

Version:  20.5.227.95
	
	[ SpawnCube2SDX ]
		- Added Harmony patch to preserve ownership during upgrades / downgrades
			-> Downgrade / Upgrade targets must use the same SpawnCube2SDX class
				<property name="Class" value="SpawnCube2SDX, SCore" />
			-> Config property line can be ommitted, or blanked, for non-spawning spawnCube.
				



Version: 20.5.226.2037

	[ Fire Manager ]
		- Fixed an issue with the FireManager not tracking fires on dedi correctly
		- Fixed an issue where melee attack does not set fires correctly (only partially applying fire )
		- Updated default Config/blocks.xml's entry for what is flammable or not.
		- Added protection gate for MinEffects when fire manager is off line.
		

Version: 20.5.226.1035
	
	[ Fire Manager ]
		- Moved fire buff on walk to Entityalive patch rather than block
		- Fire hurts now.

	[ Config/blocks.xml ]
		- Cleaned up an old Broadcastmanager node.

Version: 20.5.224.1353
	[ Broadcast feature ]
		- Update from matteo, removing second button, updating sprites


Version: 20.5.224.1021

	[ Broadcast feature ]
		- Fixed an issue with a null ref when the xui.currentworkstation is empty

	[ Quests ]
		- Added new ObjectiveBlockDestroySDX, SCore. Example in Config/quests.xml

			<objective type="BlockDestroySDX, SCore" id="frameShapes" value="1" phase="2"/>

		
Version: 20.5.223.110
	[ Broadcast feature ]
		- New Broadcast Manager feature by matteo ( https://youtu.be/BGPSIc5HUgg )
		- New NetPackages to support broadcast feature

	[ Remote Containers ]
		- Fix by matteo with regards to pulling ingredients for forge

Version: 20.5.221.934

	[ Remote Containers ]
		- Merged fixes from matteo

	[ EntityEnemySDX ]
		- Merged fix for IsAlwaysAwake


Version: 20.5.220.1149

	[ Remote Containers ]
		- Fixed an issue where items were not being removed from the container.

Version: 20.5.218.1914

	[ Fire Manager ]
		- Fixed a null ref when fire manager is first started when DyanmicMesh system isn't online

		- Added optional feature where randomized fire and smoke particles can be used, using a comma-seperated format.
			If this property is not defined on the Configuration block, it uses default Fire/Smoke particle.

			Example in Config/blocks.xml, but also here:

					<property name="RandomFireParticle" value="#@modfolder:Resources/gupFireParticles.unity3d?gupBeavis05-Heavy,#@modfolder:Resources/gupFireParticles.unity3d?gupBeavis02-CampFire,#@modfolder:Resources/gupFireParticles.unity3d?gupBeavis03-Cartoon,#@modfolder:Resources/gupFireParticles.unity3d?gupBeavis04-SlowFire,#@modfolder:Resources/gupFireParticles.unity3d?gupBeavis06-HeavyLight" />

Version: 20.5.218.174

	[ Fire Manager ]
		- Added a call to update dynamic mesh so a burned down prefab looks as you'd expect.
		- Merged in ocbMaurice's light changes.
			- These will turn on and off lights from the fire to improve performance

	[ Portals ]
		- Disabled ChunkObserver setting for Portals, as it was causing an error on the dedicated servers



Version: 20.5.216.1632

	[ Fire Manager ]

		- Expanded SmokeParticle, FireParticle, and FireDamage to be read from the block's material entry.

			<property name="FireDamage" value="50" /> 				<!-- How much damage each time it checks will do the block. -->
			<property name="SmokeParticle" value="#@modfolder:Resources/PathSmoke.unity3d?P_PathSmoke_X" />		<!-- Fire particle to use -->
			<property name="FireParticle" value="#@modfolder:Resources/gupFireParticles.unity3d?gupBeavis05-Heavy" /> <!-- Fire particle to use -->

			Priority Order:  
				Material Entry
				Block Entry
				Global Entry

		- Added a delayTime to the MinEffectAddFireDamage. 
			
			The value is in milliseconds. Default is 0, with no delay.

			<triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamage, SCore" target="positionAOE" range="5" delayTime="1000" /> <!-- 1 second before fire starts -->
		
		

Version: 20.5.216.1451

	[ Entity ]
		- New MinEffect to spawn an entity. The actual spawn location is hit position + 1 block up.
		
			For example, adding this to the ammo of a cross bolt will spawn an entity at the hit location.

			<triggered_effect trigger="onProjectileImpact" action="SpawnEntityAtPoint, SCore" SpawnGroup="ZombiesBurntForest"  />


Version: 20.5.215.1938
	[ Fire Manager ]

		- Removed verbose log output

Version: 20.5.215.1628

	[ TileEntity AOE ]
		- Fixed an invalid cast from bad copy and paste job.

	[ Fire Manager ]
		- Added debug output to MinEffects, enabled via turning Logging on in the FireManagement



Version: 20.5.214.1558
	[ Ingredients From Container patches by matteo ]
	
		https://github.com/SphereII/SphereII.Mods/pull/46
			changed some stuff to linq for better readabilty.

		Note: Please report any kind of performance slows downs.	

	[ Fire Manager ]
		- Updated to use NetPackages.
			-> Particles should work on Fires, extingusihes, and auto-expire

		- Added new NetPackages to distribute the server's data to client's

Version: 20.5.214.743
	
	[ Ingredients From Container Patches by matteo ]
	
		https://github.com/SphereII/SphereII.Mods/pull/45
			fixed some issues with items not being removed correctly.
		
	[ Entity Swiming ]
		- Pushed a fix to avoid spawning fish randomly in the world.

	[ Fire ]
		Note: Fire System is being refactored to fix a variety of issues.

		- Fixed particles not showing on dedi / P2P clients.

		- Changed Fire Instance start to ModEvent for GameStartDone.

		- Added Cleanup call to clear possibly stale data on client restart.

		- Added Reset() to clear all existing game blocks that are on fire or smoldering.
		- Added new console command called:  fireclear
			-> Removes all recorded blocks that were on fire or extinguished, and removes their particles.

		- Fixed an issue with the NetPackageRemoveParticleEffect() with a useless write call.

		- Explosions that cause BlockDamage -1, will extingish fires. BlockDamage other than -1 will set fires.
			<property name="Explosion.BlockDamage" value="-1"/>

		- Added new property to Config/blocks.xml to control the amount of smoke time at the end of a fire, in seconds.
			<property name="SmokeTime" value="60" />

		- Removed smoke particles from fires that burn themselves out. Reserving them now when you intentionally extinguish them.

		- Modified MinEvent FireDamage / FireDamageCascade to support ranged targets.

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