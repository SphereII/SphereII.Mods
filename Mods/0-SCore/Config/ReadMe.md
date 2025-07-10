# ConfigFeatureBlock Settings Summary

The `ConfigFeatureBlock` in `blocks.xml` serves as a central hub for various advanced game features and configurations,
allowing detailed control over mechanics ranging from food spoilage and crafting to player abilities and zombie
behavior.

Here's a summary and detailed description of each feature class and its settings:

## 1. Food Spoilage (`<property class="FoodSpoilage">`)

This class controls the food spoilage system, enabling items to decay over time with specific rules for different
inventory locations and transformation upon spoilage.

* **`Logging`**: `false` - Enables or disables verbose logging for troubleshooting the food spoilage feature.
* **`FoodSpoilage`**: `false` - Globally enables or disables the entire food spoilage system.
* **`UseAlternateItemValue`**: `false` - If `true`, a spoiled item uses an alternate item value for its properties.
* **`Toolbelt`**: `6` - Defines a penalty of 6 points per spoilage tick for items in the toolbelt.
* **`Backpack`**: `5` - Defines a penalty of 5 points per spoilage tick for items in the backpack.
* **`Container`**: `4` - Defines a penalty of 4 points per spoilage tick for items in containers.
* **`MinimumSpoilage`**: `1` - Sets the absolute minimum spoilage applied per tick.
* **`TickPerLoss`**: `10` - Global setting; specifies how many game ticks result in one increase in spoilage.
* **`SpoiledItem`**: `foodRottingFlesh` - The item that a spoiled item will transform into. None is also valid to spoil
  to nothing.
* **`FullStackSpoil`**: `false` - If `true`, the entire stack of an item spoils simultaneously.

## 2. Advanced Sound Features (`<property class="AdvancedSoundFeatures">`)

This class controls advanced functionalities related to in-game sounds.

* **`Logging`**: `false` - Enables or disables verbose logging for sound features.

## 3. Advanced Logging (`<property class="AdvancedLogging">`)

This class provides settings for advanced logging control, primarily to reduce log spam.

* **`LowOutput`**: `true` - If `true`, removes extra lines from the log output (e.g.,
  `AdvancedTroubleshootingFeatures :: UtilityAILogging`).

## 4. Advanced Recipes (`<property class="AdvancedRecipes">`)

This class governs features related to remote crafting and advanced recipe behaviors.

* **`ReadFromContainers`**: `false` - Enables or disables the ability to craft using ingredients directly from nearby
  containers.
* **`BlockOnNearbyEnemies`**: `false` - If `true`, remote crafting is blocked if enemies are detected nearby.
* **`LandClaimContainersOnly`**: `false` - If `true`, only containers within the player's land claim are searched for
  ingredients.
* **`LandClaimPlayerOnly`**: `false` - If `true`, remote crafting only works if the player is within their own land
  claim.
* **`Distance`**: `10` - The radius (in blocks) for searching nearby containers for crafting materials.
* **`Debug`**: `false` - If `true`, shows the name of the opened container/workstation in the log for debugging.
* **`BroadcastManage`**: `false` - If `true`, enables options to disable remote crafting on specific named workstations.
* **`disablereceiver`**: `forge, tbd` - A comma-separated list of workstation names where remote crafting is disabled.
* **`disablesender`**: (empty) - A comma-separated list of container names from which remote crafting is disabled.
* **`Invertdisable`**: `false` - If `true`, inverts the logic of `disablesender`, meaning remote crafting is enabled
  *only* from listed senders.
* **`nottoWorkstation`**: (empty) - Defines specific containers that cannot send items to certain workstations. Format:
  `workstation:container1,container2;...`.
* **`bindtoWorkstation`**: (empty) - Binds specific containers to send items *only* to certain workstations. Format:
  `workstation:container1,container2;...`.
* **`enforcebindtoWorkstation`**: `false` - If `true`, remote crafting is disabled if the workstation is not explicitly
  bound to a container via `bindtoWorkstation`.

## 5. Block Upgrade Repair (`<property class="BlockUpgradeRepair">`)

This class controls features related to repairing and upgrading blocks from containers.

* **`BlockOnNearbyEnemies`**: `false` - If `true`, disables repair/upgrade actions when enemies are nearby.
* **`DistanceEnemy`**: `30` - The distance (in blocks) used to check for nearby enemies for repair/upgrade blocking.
* **`ReadFromContainers`**: `false` - Enables or disables the ability to repair/upgrade blocks using resources from
  nearby containers.
* **`Distance`**: `40` - The radius (in blocks) for searching nearby containers for repair/upgrade materials.

## 6. Advanced Lockpicking (`<property class="AdvancedLockpicking">`)

This class enables and configures an advanced lockpicking system.

* **`RequiredModlet`**: `Locks` - Specifies a required modlet for this feature to be active.
* **`Logging`**: `false` - Enables or disables verbose logging for lockpicking.
* **`AdvancedLocks`**: `false` - Globally enables or disables the advanced lockpicking mechanics.
* **`QuestFullReset`**: `true` - If `true`, a full reset of quests related to lockpicking occurs.
* **`LockPrefab`**: `#@modfolder(Locks):Resources/Locks.unity3d?Lockset01` - Defines the prefab to be used for locks.
* **`MaxGiveAmount`**: `10,8,6,4` - Defines the maximum amount of "picks" (or similar resource) given for locks,
  possibly tiered by difficulty (Easier to most difficult).
* **`BreakTime`**: `1.2,1.0,.8,.6` - Defines the time it takes to break a lock, possibly tiered by difficulty.
* **`Left`**: `Keypad4` - Specifies the key code for "left" action in lockpicking mini-game.
* **`Right`**: `Keypad6` - Specifies the key code for "right" action in lockpicking mini-game.
* **`Turn`**: `Keypad8,Keypad5,Keypad2` - Specifies key codes for "turn" action in lockpicking mini-game.

## 7. Advanced Quests (`<property class="AdvancedQuests">`)

This class provides advanced functionalities for quests, particularly regarding POI management.

* **`ReusePOILocations`**: `false` - If `true`, allows Points of Interest (POIs) to be reused for "GotoPOISDX" quests.

## 8. Advanced Item Features (`<property class="AdvancedItemFeatures">`)

This class introduces more complex repair and scrapping options for items.

* **`Logging`**: `false` - Enables or disables verbose logging for item features.
* **`AdvancedItemRepair`**: `false` - If `true`, enables more complex repair options and recipes for items.
* **`DisableScrapFallback`**: `false` - If `true`, disables the creation of a reduced raw ingredients recipe when
  `AdvancedItemRepair` is `true` but `RepairItems` or `ScrapItems` are not defined. This enforces only explicitly listed
  scrap/repair recipes.
* **`DurabilityAffectsDamage`**: `false` - If `true`, a weapon's damage (for melee) is reduced as its durability
  decreases.

## 9. Advanced Player Features (`<property class="AdvancedPlayerFeatures">`)

This class enhances and modifies various core player behaviors and mechanics.

* **`Logging`**: `true` - Enables or disables verbose logging for player features.
* **`AntiNerdPole`**: `false` - Prevents players from performing the "nerd pole" exploit (jumping and placing blocks
  underneath themselves).
* **`OneBlockCrouch`**: `false` - Allows players to fit through spaces that are only one block high while crouching.
* **`PhysicsCrouchHeightModifier`**: `0.49` - Adjusts the player's crouch height for the one-block crouch feature.
* **`SoftHands`**: `false` - If `true`, damages the player if they hit something with their bare hands.
* **`VehicleNoTake`**: `false` - If `true`, prevents players from picking up vehicles manually.
* **`ExtendedSigns`**: `true` - Enables enhanced sign capabilities, including displaying content from web URLs.
* **`Encumbrance`**: `false` - If `true`, enables an item weight encumbrance system for the player's inventory bag.
* **`MaxEncumbrance`**: `10000` - Defines the maximum weight threshold before encumbrance penalties are applied. Can be
  overridden by a player cvar.
* **`EncumbranceCVar`**: `encumbranceCVar` - The cvar name that will store the player's current encumbrance as a
  percentage.
* **`Encumbrance_ToolBelt`**: `false` - If `true`, items in the toolbelt contribute to encumbrance calculation.
* **`Encumbrance_Equipment`**: `false` - If `true`, equipped items contribute to encumbrance calculation.
* **`MinimumWeight`**: `0.1` - The default weight assigned to items that do not have an `ItemWeight` property defined.
* **`AutoRedeemChallenges`**: `false` - If `true`, automatically redeems completed challenges.
* **`SharedReading`**: `false` - If `true`, allows all players within a party to share the benefits of reading books.

## 10. Advanced Zombie Features (`<property class="AdvancedZombieFeatures">`)

This class provides advanced settings for zombie behavior and appearance.

* **`Logging`**: `false` - Enables or disables verbose logging for zombie features.
* **`HeadshotOnly`**: `false` - If `true`, zombies can only be killed by headshots.
* **`RandomSize`**: `false` - If `true`, zombies will spawn with random sizes for more visual variety.
* **`RandomWalk`**: `false` - If `true`, zombies will have random walk animations/styles.
* **`SmarterEntities`**: `false` - If `true`, gives trap avoidance behavior to entities.
* **`UMATweaks`**: `false` - Enables improvements to reduce load times of UMA (Unity Multipurpose Avatar) zombies.
* **`LowerResolutionUMA`**: `false` - Drastically lowers the resolution of UMA zombies.
* **`EntityPooling`**: `false` - Enables entity pooling for performance optimization.
* **`EnemyActiveMax`**: `-1` - Sets the maximum number of active enemies. Default vanilla is `30`. `-1` disables this
  patch.

## 11. Advanced Progression (`<property class="AdvancedProgression">`)

This class offers settings to modify the player progression system.

* **`Logging`**: `false` - Enables or disables verbose logging for progression features.
* **`ZeroXP`**: `false` - If `true`, disables gaining experience from any source.

## 12. Advanced Pick Up And Place (`<property class="AdvancedPickUpAndPlace">`)

This class provides advanced options for picking up and placing blocks.

* **`Logging`**: `false` - Enables or disables verbose logging for pick up and place features.
* **`Legacy`**: `true` - If `true`, enables legacy pick up and place mechanics.
* **`TakeWithTool`**: `meleeToolRepairT1ClawHammer` - Specifies a tool required to pick up certain blocks.

## 13. Error Handling (`<property class="ErrorHandling">`)

This class provides settings to manage and mitigate various in-game errors and exceptions.

* **`Logging`**: `false` - Enables or disables verbose logging for error handling.
* **`NoExceptionHijack`**: `true` - If `true`, disables the console drop-down on red exceptions.
* **`EnablePoolBlockEntityTransformCheck`**: `false` - If `true`, quietly checks for null block entity transforms.
* **`LogPoolBlockEntityTransformCheck`**: `false` - If `true`, logs every instance of a block entity at a null
  transform (can be very spammy).
* **`TraderDataReadInventory`**: `false` - If `true`, checks for null references when a workstation/vending machine is
  reset during a POI reset.
* **`TileEntityCopyFrom`**: `false` - If `true`, provides protection against errors during `TileEntity.CopyFrom()`
  operations.
* **`BlockEntityDataGetRenderers`**: `false` - If `true`, protects against null reference errors when block entity data
  does not have a valid transform.

## 14. Advanced UI (`<property class="AdvancedUI">`)

This class offers settings to customize various user interface elements.

* **`Logging`**: `false` - Enables or disables verbose logging for UI features.
* **`DisableXPIconNotification`**: `false` - If `true`, disables the UI element for XP notifications.
* **`ShowTargetHealthBar`**: `false` - If `true`, displays a health bar for the targeted entity.

## 15. Advanced World Gen (`<property class="AdvancedWorldGen">`)

This class contains settings related to advanced world generation features.

* **`Logging`**: `false` - Enables or disables verbose logging for world generation features.

## 16. Advanced NPC Features (`<property class="AdvancedNPCFeatures">`)

This class offers extensive control over NPC behavior, interactions, and attributes.

* **`Logging`**: `false` - Enables or disables verbose logging for NPC features.
* **`EnhancedFeatures`**: `false` - If `true`, enables advanced features for NPCs like opening doors.
* **`MakeTraderVulnerable`**: `false` - If `true`, allows Trader NPCs to be killed.
* **`NPCSpeedFix`**: `false` - If `true`, applies a fix for NPC speed issues.
* **`HumanTags`**: `human,bandit,survivor,npc,trader` - Tags that identify an entity as alive and having a "brain" (
  e.g., for AI logic).
* **`UseFactionsTags`**: `human,bandit,survivor,npc,trader` - Tags that determine if an entity should use faction
  relationships for targeting.
* **`FactionRelationshipCVars`**: `false` - If `true`, enables the addition of faction relationship cvars.
* **`AllEntitiesUseFactionTargeting`**: `false` - If `true`, all entities in the game will use faction-based targeting.
* **`AttackVolumeInstantAwake`**: `true` - If `true`, sleeper NPCs in "attack" sleeper volumes instantly wake up,
  regardless of the `SleeperInstantAwake` setting.
* **`DisplayCompanions`**: `true` - If `true`, companions are displayed in the UI.
* **`AdvancedEnemyNPCs`**: `false` - If `true`, indicates that some enemy NPCs are "advanced" and use `EntityAliveSDX`.
* **`EnemyDistanceToTalk`**: `10` - Minimum distance between an NPC and an enemy before dialog options are blocked. Set
  to `-1` or `0` to disable this check.
* **`BlockTimeToJump`**: `0.19` - Time (in seconds) that an NPC waits before attempting a jump after being blocked.
* **`BlockedTime`**: `0.2` - Time (in seconds) for which an NPC considers itself "blocked" after an attempted movement.

## 17. Theme (`<property class="Theme">`)

This class allows for the application of themed settings, affecting the game's atmosphere.

* **`Spook`**: `false` - If `true`, enables "spooky" atmosphere effects.

## 18. Advanced Workstation Features (`<property class="AdvancedWorkstationFeatures">`)

This class provides settings for advanced workstation functionalities.

* **`Logging`**: `false` - Enables or disables verbose logging for workstation features.
* **`EnablePoweredWorkstations`**: `false` - If `true`, enables functionality for powered workstations.

## 19. Advanced Dialog Debugging (`<property class="AdvancedDialogDebugging">`)

This class offers settings specifically for debugging in-game dialogs.

* **`Logging`**: `false` - Enables or disables verbose logging for dialog debugging.

## 20. Advanced Troubleshooting Features (`<property class="AdvancedTroubleshootingFeatures">`)

This class provides various settings to assist with debugging and troubleshooting game issues.

* **`Logging`**: `false` - Enables or disables verbose logging for troubleshooting features.
* **`VerboseXMLParser`**: `false` - If `true`, provides more verbose output from the XML parser.
* **`PhysicsBody`**: `false` - If `true`, enables debugging for physics bodies.
* **`AnimatorMapper`**: `false` - If `true`, enables debugging for animator mapping.
* **`EntitySpeedCheck`**: `false` - If `true`, performs a speed check on each entity spawn (can be very spammy).
* **`ComponentMapper`**: `false` - If `true`, prints all transforms and scripts on an entity when it spawns.
* **`UtilityAILogging`**: `false` - If `true`, enables detailed logging for Utility AI.
* **`UtilityAILoggingMin`**: `false` - If `true`, only displays `Start` and `Stop` messages for AI tasks.
* **`UtilityAILoggingTask`**: `false` - If `true`, enables task-specific logging for Utility AI.

## 21. Advanced Prefab Features (`<property class="AdvancedPrefabFeatures">`)

This class provides settings for modifying prefab behaviors, especially related to traders and building.

* **`Logging`**: `false` - Enables or disables verbose logging for prefab features.
* **`DisableTraderProtection`**: `false` - If `true`, disables protection for traders within their prefabs (allows them
  to be damaged/killed).
* **`AllowBuildingInTraderArea`**: `false` - If `true`, allows players to build within trader protected areas (requires
  `DisableTraderProtection` to be `true`).
* **`DisableWallVolume`**: `false` - If `true`, disables the invisible wall behind traders.
* **`DisableFlickeringLights`**: `false` - If `true`, disables flickering light effects.
* **`PrefabName_NoBuilding`**: (empty) - A comma-delimited list of prefab names where building is disallowed.
* **`PrefabTag_NoBuilding`**: `NoBuild` - A comma-delimited list of prefab tags. If a prefab has any of these tags on
  its XML, building is disallowed within it.

## 22. Advanced Tile Entities (`<property class="AdvancedTileEntities">`)

This class provides general settings for advanced tile entity functionalities.

* **`Logging`**: `false` - Enables or disables verbose logging for tile entity features.

## 23. Cave Configuration (`<property class="CaveConfiguration">`)

This class allows for the dynamic generation and configuration of cave systems within the world.

* **`Logging`**: `false` - Enables or disables verbose logging for cave generation.
* **`CaveEnabled`**: `false` - Globally enables or disables the dynamic cave generation system.
* **`CavePath`**: `@modfolder(0-SCore_sphereii):/Caves/StampsV2/Cave03.png` - Specifies a path to a cave stamp (image)
  used for generation.
* **`CavePrefab`**: `Large` - Defines the type or size of cave prefab to be used.
* **`GenerationType`**: `Legacy` - Specifies the generation algorithm (`Legacy`, `Sebastian` (alternative),
  `FastNoiseSIMD`).
* **`CaveType`**: `Random` - Determines where caves generate (`Mountains`, `Random`, `All`, `DeepMountains`).
* **`MaxCaveLevels`**: `5` - Maximum number of cave levels, ignored if `DeepMountains` is selected.
* **`CaveCluster`**: `50` - Number of cave clusters to create when `CaveType` is `Random`.
* **`CavesClusterSize`**: `20` - Number of chunks per cluster when `CaveType` is `Random`.
* **`FractalType`**: `FBM` - Type of fractal noise used (`RigidMulti`, `Billow`, `FBM`). Default is `RigidMulti`.
* **`NoiseType`**: `SimplexFractal` - Type of noise algorithm used (`Cellular`, `Cubic`, `Perlin`, `Simplex`, etc.).
  Default is `SimplexFractal`.
* **`CaveThresholdXZ`**: `0.20` - Determines the noise threshold for horizontal cave generation (higher numbers for more
  open caves).
* **`CaveThresholdY`**: `0.01` - Determines the noise threshold for vertical cave generation (higher numbers for deeper
  caves).
* **`DeepCaveThreshold`**: `30` - Defines the noise threshold for "deep" parts of the cave.
* **`StartCaveThreshold`**: `15` - How far below the surface caves begin to spawn.
* **`MinStartCaveThreshold`**: `-1` - Minimum height of terrain before generating caves (e.g., if center of chunk
  terrain is higher than this value, no cave generates).
* **`POIRandomRoll`**: `8` - Random roll chance (0-100) for placing POIs within isolated cave blocks.
* **`Octaves`**: `3` - Number of noise layers for fractal noise.
* **`Lacunarity`**: `1` - Controls the frequency multiplier between successive octaves.
* **`Gain`**: `0.5` - Controls the amplitude multiplier between successive octaves.
* **`Frequency`**: `0.09` - The overall frequency of the noise.
* **`CavePOIs`**: `Chiko_SCcave_01,...` - A comma-separated list of POI prefabs to be placed in regular caves.
* **`DeepCavePrefabs`**: `Chiko_SCcave_08,...` - A comma-separated list of POI prefabs to be placed in deep caves.
* **`MaxPrefabPerChunk`**: `1` - Maximum number of prefabs (POIs) that can be placed per chunk.
* **`PrefabSister`**: `garage_02,...` - A comma-separated list of "sister" prefabs; typically used to define associated
  structures.
* **`AllowedBiomes`**: `All` - Biomes where cave generation is allowed (e.g., `All` or specific biome names).

## 24. Crop Management (`<property class="CropManagement">`)

This class provides settings for an advanced crop and farming system.

* **`Logging`**: `false` - Enables or disables verbose logging for crop management.
* **`RequirePipesForSprinklers`**: `false` - If `true`, forces sprinklers and water sources to be connected to pipes to
  function.
* **`CropEnable`**: `false` - Globally enables or disables the advanced crop management system.
* **`CheckInterval`**: `600` - Defines how often (in game ticks) the crop manager checks plants for growth and water.
* **`MaxPipeLength`**: `500` - Maximum length of a water pipe system.
* **`WaterDamage`**: `1` - How much damage is done to a water source when its water is consumed by crops.
* **`WaterParticle`**: `NoParticle` - Default particle effect used when a crop is watered (e.g.,
  `#@modfolder:Resources/guppyFountainDisplay.unity3d?gupFountainDisplay`).

## 25. Fire Management (`<property class="FireManagement">`)

This class controls the advanced fire mechanics, including starting, spreading, and extinguishing fires.

* **`Logging`**: `false` - Enables or disables verbose logging for the fire manager.
* **`FireEnable`**: `false` - Globally enables or disables the fire manager feature.
* **`FirePersists`**: `false` - If `true`, enables saving the state of active fires across game sessions.
* **`CheckInterval`**: `20` - How many seconds pass before the fire manager checks blocks for damage, spreading, etc.
* **`FireDamage`**: `50` - The amount of damage applied to a burning block each time it's checked.
* **`SmokeTime`**: `60` - How long the smoke particle remains on an extinguished block (in seconds).
* **`FireSound`**: `SCoreMediumLoop` - The Sound Data Node to use for fire sounds. Can be overridden per block.
* **`BuffOnFire`**: `buffBurningMolotov` - The buff applied to an entity that steps into flames.
* **`MaterialID`**: `Mplants, Mcorn, Mhay` - A comma-separated list of material IDs that are considered flammable.
* **`MaterialDamage`**: `wood, cloth, corn,...` - A comma-separated list of material damage categories that are
  considered flammable.
* **`MaterialSurface`**: `wood, cloth, corn,...` - A comma-separated list of material surface categories that are
  considered flammable.
* **`SmokeParticle`**: `#@modfolder:Resources/PathSmoke.unity3d?P_PathSmoke_X` - The particle effect used for smoke on
  extinguished blocks.
* **`FireParticle`**: `#@modfolder:Resources/gupFireParticles.unity3d?gupBeavis05-Heavy` - The particle effect used for
  burning blocks.
* **`FireSpread`**: `true` - If `true`, fire can spread to neighboring blocks.
* **`ChanceToExtinguish`**: `0.05` - The chance (0 to 1) for a burning block to self-extinguish during each
  `CheckInterval`.

## 26. External Particles (`<property class="ExternalParticles">`)

The `ExternalParticles` section within the `ConfigFeatureBlock` allows modders to pre-load external particle effects,
making them accessible and usable within the game via a unique index (hash code).

## External Particles Configuration

This section is where you define the paths to your custom particle assets. The `name` attribute of the property serves
as a reference for your notes and is not directly used by the system for indexing. The unique index value, which is a
`GetHashCode()` of the `value` attribute, will be displayed in the game's log during boot-up. Modders can then review
the log file to find this hash and use it as the index for the particle in other configurations.

**XML Example:**

```xml

<property class="ExternalParticles">
    <property name="SmokeParticle" value="#@modfolder:Resources/PathSmoke.unity3d?P_PathSmoke_X"/>
    <property name="FireParticle" value="#@modfolder:Resources/gupFireParticles.unity3d?gupBeavis05-Heavy"/>

    <property name="gupBlueEnergyTransfer"
              value="#@modfolder:Resources/gupParticleIndexes.unity3d?gupBlueEnergyTransfer"/>
    <property name="gupIceExplosion" value="#@modfolder:Resources/gupParticleIndexes.unity3d?gupIceExplosion"/>
    <property name="gupLightningSparks" value="#@modfolder:Resources/gupParticleIndexes.unity3d?gupLightningSparks"/>
    <property name="gupPurpleFireRing" value="#@modfolder:Resources/gupParticleIndexes.unity3d?gupPurpleFireRing"/>
    <property name="gupRedExplosion" value="#@modfolder:Resources/gupParticleIndexes.unity3d?gupRedExplosion"/>
    <property name="gupRockStorm" value="#@modfolder:Resources/gupParticleIndexes.unity3d?gupRockStorm"/>
    <property name="gupSkullBloodMist" value="#@modfolder:Resources/gupParticleIndexes.unity3d?gupSkullBloodMist"/>
    <property name="gupScriptsExplosion" value="#@modfolder:Resources/gupParticleIndexes.unity3d?gupScriptsExplosion"/>
</property>
```

**How to Use:**

1. **Define Particle Paths**: Add `<property>` tags within the `ExternalParticles` class, specifying a descriptive
   `name` and the `value` pointing to the Unity3D asset and particle system within it (e.g.,
   `#@modfolder:Resources/MyParticles.unity3d?MyParticleEffect`).
2. **Retrieve Hash Index**: After launching the game, review your log file. The system will print the unique hash code
   generated for each particle `value`. This hash code is the `index` you will use to reference the particle elsewhere
   in your game's configurations.

