Farming Overhaul System
---
The Farming Overhaul System enables additional depth to the vanilla farming instance. 

# Table of Contents
1. [Summary](#summary)
2. [Fire Manage Global Configuration](#fire-manager-global-configuration)
3. [Block-Specific Configuration](#block-specific-configuration)
4. [Examples](#examples)
## Summary

The code within this section adds additional layers to farming. Plants may require water to grow, and allows the players to create elaborate network of pipes to distribute
water to the farm plots. It contains hooks and calls that allows NPCs to manage your farm.

This system relies on blocks being adjacent to each other. In other words, the blocks must be within a one block radius of each other to be considered 'connected'.

The visual orientation of the blocks are not enforced, considered, or used. 

## Overview

In order to plant seeds into a farm plot, the farm plot must be within range of a water source, such as a pond, river, or lake. 

The default range of water for a farm plot is 5 blocks, so each farm plot must be within 5 blocks of a water source.

As the seed grows, it will check to see if there is still water around it, and will consume (damage) a part of that water block. If there's multiple water blocks
within range for that farm plot, then it will randomly pick a water source to consume from. 

If the crop checks for water, and does not find water, the crop will either not grow or it will wilt into a dead plant.

It may not always be practical to have the farm plots close to water, either because of distance or because of space around the water.

In those situations, the player may craft / harvest metal pipes to place, connecting the water source to the farm area. The pipes must be adjacent to each other in order
to be considered a flowing water source. Likewise, the end pipe must be also be adjacent to a water source.

The pipes can distribute water to farm plots that are adjacent to it. 

This is an important note. While farm plots can seek water from a range of 5, a pipe is not considered a water source beyond it's adjacent check. 

That means a farm plot that is touching a pipe may get it's water from it. However, the farm plot that is one block away will not see the pipe as 
a water source, and not be able to get any water from it.

To get around this limitation, the player may craft a 'sprinkler' that can be connected to a water pipe. This sprinkler, if it's connected to a pipe with water access,
may act like a water source itself. These sprinklers have a range that may supply water to many crops in a wider area. 

When a plant needs to check for water, it will see the sprinkler as its water source, and confirm that the sprinkler itself is connected to water through the pipe system.

The plant will consume water from this sprinkler, and the sprinkler will pass this consume to its end water block.

Reference Videos: https://www.youtube.com/playlist?list=PLJeCuPbkcF5SwR7x_kAa9TiJvVDaLmsue


## Breakdown

Crop Manager:
  This manager is in charge to monitoring crops for their needs. Currently, this involves just their watering information, and when they were last maintained. Last maintained
is a concept introduced to allow NPCs to tend to your crops by visiting the crops, providing water, pruning, and removing bugs.

The Crop Manager uses the PlantData class to keep track of a particular plant. It contains the logic to consume water from a water source.

Farm Plot Manager:
  This manager is in charge of monitor farm plots themselves.

The Farm Plot Manager uses FarmPlotData class to keep track of a particular farm plot. This farm plot can check, if it's connected to water. It also contains
a manage hook that is designed for NPCs to harvest the crops, and replant as needed.

Water Pipe Manager:
  This manager is in charge of monitoring water supplies and its associated piping. It's in charge of finding water, and checking if a particular position is connected to water.


## Crop Manager Global Configuration

The blocks.xml contains configurations and settings to adjust the crop behaviour from its defaults.

#### Logging
```xml
    <!-- Enables or disables verbose logging for troubleshooting -->
    <property name="Logging" value="false"/>
```  
#### CropEnable
```xml
    <!-- Enables or disables the Farming managers completely --> 
    <property name="CropEnable" value="false"/>
```  
#### CheckInterval
```xml
    <!-- Determines how often the managers should check for watering needs -->
    <property name="CheckInterval" value="600"/>
```  
#### WaterDamage
```xml
    <!-- Determines how much damage is done to a block per CheckInterval. -->
    <!-- If this property is defined on a specific block, that value will be used, rather than the default --> 
    <!-- If this property is defined on a materials.xml entry, that value will be used. -->
    <property name="WaterDamage" value="10"/>
```
#### MaxPipeLength
```xml
    <!-- Determines the number of pipes to monitor -->
    <property name="MaxPipeLength" value="500"/>
```
#### WaterParticle
```xml
    <!-- Defines the Water Particle to be used when watering a plant. block -->
    <!-- ie: #@modfolder:Resources/guppyFountainDisplay.unity3d?gupFountainDisplay from Bloom's -->
    <property name="WaterParticle" value="NoParticle"/>
```



## Block-specific Configuration

In addition to the above settings in the Crop Manager's overall blocks.xml, further properties may be optionally added to blocks.xml to further define the behaviour.

### Crops

The following properties are meant to be added to a crop or plant that you want to manage. 

#### Class
```xml
    <!-- Adds support for the crop to be managed -->
    <property name="Class" value="PlantGrowingSDX, SCore" />

    <!-- xpath example to add to all growing plants. -->
    <set xpath="/blocks/block[@name='cropsGrowingMaster']/property[@name='Class']/@value">PlantGrowingSDX, SCore</set>
```

#### RequireWater
```xml
    <!-- Determines if this crop needs water -->
    <property name="RequireWater" value="true"/>
```
#### Wilt
```xml
    <!-- Defines if a plant will wilt and die, if there is no water source available when it tries to grow. -->
    <property name="Wilt" value="true"/>
```
#### WaterRange
```xml
    <!-- How far this plant can detect water -->
    <property name="WaterRange" value="5"/>
```
#### PlantGrowing.Wilt
```xml
    <!-- If set to wilt, the plant will change into this block when it cannot find water. -->
    <property name="PlantGrowing.Wilt" value="treeDeadPineLeaf"/>
```

#### Example

Generally, it may suffice to just modify the crops master, rather than adding to individual plants. 

```xml
	<set xpath="/blocks/block[@name='cropsGrowingMaster']/property[@name='Class']/@value">PlantGrowingSDX, SCore</set>
	<append xpath="/blocks/block[@name='cropsGrowingMaster']">
			<property name="RequireWater" value="true" /> <!-- If the crop needs water to survive. Default: false -->
			<property name="WaterRange" value="5" />  <!-- how far away that block can be from a water source: Default is 5 -->
			<property name="PlantGrowing.Wilt" value="treeDeadPineLeaf"/> <!-- The block the crop downgrades too if its dead. Default: Air-->
	</append>
```


### Pipes

Water Pipes are blocks considered to be able to carry or pass along water information to its neighbors. Bloom's Family Farm uses metal pipes, 
but any block will take these settings. Pipes will allow farm plots to access water that are directly connected to a pipe.

#### Class
```xml
    <!-- Any block with this class will be registered as a water pipe -->
    <property name="Class" value="WaterPipeSDX, SCore" />
```


### Water Source

A Water Source, in this context, is not an actual direct source of water. Rather, it should be considered a sprinkler, which can further spread water 
out to a farther range than a standard pipe

```xml
    <!-- Any block with this class will be registered as a water source -->
    <property name="Class" value="WaterSourceSDX, SCore" />
```

### Example

In the below example, we use the pipe valve model to act as a sprinkler. We also update it's model to use guppycur's sprinkler prefab.

```xml
	<set xpath="/blocks/block[@name='metalPipeValve']/property[@name='Class']/@value">WaterSourceSDX, SCore</set>
	<set xpath="/blocks/block[@name='metalPipeValve']/property[@name='Model']/@value">#@modfolder:Resources/gupSprinklerBlock.unity3d?gupLargeSprinkler.prefab</set>
```

### Farm Plot

#### Class
```xml
    <!-- Registers the farm plot into the Farm Plot manager -->
    <property name="Class" value="FarmPlotSDX, SCore" />
```

### Water Source

A Water Source is flag for a non-liquid / non-water block that may be used to simulate water. 

This would be useful if the player can craft or get water from alternative sources, or if the modder decides that any pipe connected to bedrock would be 
an unlimited source of water.

#### WaterSource
```xml
  <!-- Any block that has this property can be considered a source of water -->
  <property name="WaterSource" value="true" />
```

#### WaterType
```xml
  <!-- Optional property. This indicates the block will supply an unlimited source of water, and will not take damage. -->
  <property name="WaterType" value="unlimited" />
```



### XPath

Here are some examples on how to modify the ConfigFeatureBlock, which holds the Crop Management settings, from another modlet.
```xml
	<!-- Turning on Crops-->
    <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='CropManagement']/property[@name='CropEnable']/@value">true</set>

    <!-- Change interval to every 5 minutes -->
    <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='CropManagement']/property[@name='CheckInterval']/@value">60</set>
    <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='CropManagement']/property[@name='Logging']/@value">true</set>
```
 
To add water requirements to all crops:
```xml
	<!-- Changing all crops to using the PlantGrowingSDX class -->
    <set xpath="/blocks/block[@name='cropsGrowingMaster']/property[@name='Class']/@value">PlantGrowingSDX, SCore</set>

    <!-- Setting up defaults -->
    <append xpath="/blocks/block[@name='cropsGrowingMaster']">
        <property name="RequireWater" value="true" /> <!-- If the crop needs water to survive. Default: false -->
        <property name="WaterRange" value="5" />  <!-- how far away that block can be from a water source: Default is 5 -->
        <property name="PlantGrowing.Wilt" value="treeDeadPineLeaf"/> <!-- The block the crop downgrades too if its dead. Default: Air-->
    </append>

    <!-- Speed up plant growth for testing -->
    <set xpath="/blocks/block[@name='cropsGrowingMaster']/property[@name='PlantGrowing.GrowthRate']/@value">10</set>
```

To configure the pipes:
```xml
	<append xpath="/blocks/block[starts-with(@name, 'metalPipe')]">
		<property name="Class" value="WaterPipeSDX, SCore" />
	</append>
```

To configure sprinklers:
```xml
	<set xpath="/blocks/block[@name='metalPipeValve']/property[@name='Class']/@value">WaterSourceSDX, SCore</set>
	<set xpath="/blocks/block[@name='metalPipeValve']/property[@name='Model']/@value">#@modfolder:Resources/gupSprinklerBlock.unity3d?gupLargeSprinkler.prefab</set>
```

To configure farm plots:
```xml
    <!-- Farming Plot hooks -->
    <append xpath="/blocks/block[starts-with(@name, 'farmPlot')]">
        <property name="Class" value="FarmPlotSDX, SCore" />
    </append>
```

To add bedrock as an unlimited water source:
```xml
    <append xpath="/blocks/block[@name= 'terrBedrock']">
        <property name="WaterSource" value="true" />
        <property name="WaterType" value="unlimited" />
    </append>
```
