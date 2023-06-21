Blocks
---

The following new block types are supported by the SCore.

# Table of Contents
1. [BlockDecoAoE](#BlockDecoAoE)
2. [BlockFarmPlotSDX](#BlockFarmPlotSDX) ( Advanced Farming Feature )
3. [BlockCropControlPanel](#BlockCropControlPanel) ( Advanced Farming Feature )
4. [BlockMortSpawner](#BlockmortSpawner)
5. 
### BlockDecoAoE

This block allows a buff to be emitted from it, such as the campfire's heat.  The Block is always active, thus always radiating this buff when a player is nearby.


Usage: 
```xml
<property name="Class" value="DecoAoE, SCore" />
<property name="ActiveRadiusEffects" value="buffCampfireAOE(3)"/>
```

Full Example:
```xml
<block name="sphereiiAoETest">

	<property name="Class" value="DecoAoE, SCore" />
	<property name="ActiveRadiusEffects" value="buffCampfireAOE(3)"/>

	<property name="Material" value="Mwood_weak"/>
	<property name="Tags" value="coalBurnedDmg"/>
	<property name="StabilitySupport" value="false"/>
	<property name="Shape" value="Ext3dModel"/>
	<property name="Texture" value="293"/>
	<property name="Mesh" value="models"/>
	<property name="Model" value="OutdoorDecor/ember_pile_1" param1="main_mesh"/>
	<property name="IsTerrainDecoration" value="true"/>
	<property name="BuffsWhenWalkedOn" value="buffBurningEnvironment"/>
	<property name="Collide" value="melee,bullet,arrow,rocket"/>
	<property name="VehicleHitScale" value="999"/>
	<property name="FilterTags" value="MC_outdoor,SC_decor"/>
</block>
```


### BlockFarmPlotSDX

This block is a replacement to vanilla's FarmPlot block, with additional hooks into the FarmPlotManager. The FarmPlotManager allows plants and planters to require water to grow.

Usage:

```xml
<property name="Class" value="FarmPlotSDX, SCore" />
```

XPath Usage, to update all vanilla farmplots to FarmPlotSDX:

```xml
<!-- Farming Plot hooks -->
<append xpath="/blocks/block[starts-with(@name, 'farmPlot')]">
	<property name="Class" value="FarmPlotSDX, SCore" />
</append>
```

### BlockCropControlPanel 
[Advanced Farming Feature] (../../Documentation/CropManagement/ReadMe.md) 

This block is a test control panel, allowing players to turn on water particles and test to see if water is available on connected pipes.

Usage:
```xml
<property name="Class" value="CropControlPanel, SCore" />
<property name="ControlPanelName" value="debugcontrolpanel" />
```

Full Example:
```xml
<block name="cropControlPanelBase01">
	<property name="Extends" value="controlPanelBase01"/>
	<property name="Shape" value="ModelEntity"/>
	<property name="Model" value="Entities/Industrial/controlPanelBase_02Prefab"/>
	<property name="CustomIcon" value="controlPanelBase01" />
	<property name="Class" value="CropControlPanel, SCore" />
	<property name="ControlPanelName" value="debugcontrolpanel" />
	<property name="ActivateSound" value="button4"/>
</block>
```


### BlockMortSpawner

This block will act as an entity spawner. 

Note: It's an older class made by Mortelentus and brought forward by sphereii. No new features are being added, and as time goes on, functionality may be reduced.

Usage:
```xml
<property name="Class" value="MortSpawner, SCore" />

<!-- this determines how fast zombies are spawned (s) -->
<property name="TickRate" value="30" />  

<!-- this is the spawn trigger area -->
<!-- if a player gets inside this are it starts spawning -->
<property name="SpawnRadius" value="15" />

<!-- spawning area it will try to spawn the entities inside this area only -->
<property name="SpawnArea" value="5" />

<!-- If you dont want it to make radiation damage, set this values to 0 -->
<property name="RadiationArea" value="5" />
<property name="RadiationDamage" value="5" />

<!-- If you want any buff to be applied inside the radiation area, uncomment next line -->
<!-- <property name="Buffs" value="radiation1" /> -->
<!-- spawns a maximum of 4 zombies at a time -->
<property name="NumberToSpawn" value="4" />

<!-- Maximum zombies allowed inside the spawnarea -->
<property name="MaxSpawned" value="8" />

<!-- area to check for max spawned -->
<property name="CheckArea" value="20" />

<!-- Time to Pause spawning, in minutes, after the total number of spawned zombies have reached NumberToPause -->
<!-- If this number is >= 999, the spawning will stop forever -->
<!-- this WILL NOT STOP radiation damage and buffs -->
<property name="PauseTime" value="0" />

<!-- Total number of zombies to pause the spawning. -->
<property name="NumberToPause" value="0" />

<!-- EntityGroup used to choose the entity to spawn -->
<property name="EntityGroup" value="ZombiesAll" />

<!-- Lootable -->
<property name="Lootable" value="false" />
<property name="LootList" value="10" />

<!-- empty since it's not lootable -->
<!-- What to do to particles when the nest is stopped -->
<!-- None, Start particles, Stop particles -->
<property name="ParticleAction" value="None" />
<property name="LPHardnessScale" value="8" />
<property name="Group" value="Decor" />
<drop event="Destroy" count="0" />
<property name="debug" value="false" />
```



