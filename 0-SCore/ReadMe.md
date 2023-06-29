﻿#0-SCore

The 0-SCore is the key component to enable extra functionality for 7 Days To Die. Using a mixture of Harmony, SDX Patch Scripts, and Scripts, new features are enabled for modders and players to use.

| Folder | Description |
| ----------- | ----------- |
| Config | XML Files that serve as documentation, basic setup for required items. blocks.xml contains many default settings to review and adjust as needed.|
| Harmony | Many harmony scripts to make small adjustments to the game. These scripts, for the most part, can be turned on and off via the blocks.xml|
| Scripts | Many Scripts which include new classes. References to these scripts would be  ```<className>, SCore```  |
|Features | Features will contain all the code necessary for a particular feature, grouping the code so it can be easily found and extracted. |

Note: An auto-mirror is available on gitlab that allows you to download individual mods: https://gitlab.com/sphereii/SphereII-Mods

### Direct Downloads
Direct Download to the 0-SCore.zip available on gitlab mirror:

[ 0 - SCore (Alpha 20 ) ]: https://gitlab.com/sphereii/SphereII-Mods/-/archive/master/SphereII-Mods-master.zip?path=0-SCore
[ 0 - SCore ( Alpha 21) ]: https://gitlab.com/sphereii/SphereII-Mods/-/archive/alpha21-experimental/SphereII-Mods-alpha21-experimental.zip?path=0-SCore

### Change Logs

[ Change Log ]
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