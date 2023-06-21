Fire Manager
---
The Fire Manager introduces fire mechanics into the game. This includes starting, spreading and extinguishing fires, and
allowing blocks to explode.

# Table of Contents
1. [Summary](#summary)
2. [Fire Manage Global Configuration](#fire-manager-global-configuration)
3. [Block-Specific Configuration](#block-specific-configuration)
4. [Examples](#examples)
## Summary

A block may catch on fire through a variety of means, depending on the mod in question. For example, a thrown molotov would certainly catch
blocks on fire, potentially triggering a cascading fire that may burn a building down.

When a block is on fire, it will have damage dealt to it over time, using the CheckInterval time. 

When a block is checked during its CheckInterval time, neighboring blocks may catch on fire if they are flammable. A FireSpread property can disable this.

When a block has taken more damage than its max damage, the block will downgrade itself to its defined Downgrade block. By default, this is an air block.

When a block has an Explosion.ParticleIndex defined, the Fire Manage will use that data to trigger an explosion.

When a block is extinguished, a smoke particle will be placed on it, showing the player that it has been extinguished. An extinguished block will not immediately
catch fire again.

To determine if a block is flammable or not, several checks are performed. The checks here are listed in the order they are checked in code. Once a block is 
determined to not be flammable, then the other checks are not performed.

- Does the block have tag of "inflammable"?
- Is the position a child of a block? ( think of multi-dim's here)
  - Not Flammable.
- Is the position air?
  - Not Flammable.
- is the position water?
  - Not Flammable.
- Does The block have the tag of "flammable"?
  - Flammable
- Does the Config block have the Property "MaterialID", and does it match our Block?
  - Flammable
- Does the Config block have the Property "MaterialDamage", and does it match our Block?
    - Flammable
- Does the Config block have the Property "MaterialSurface", and does it match our Block?
  - Flammable

The block must return a "Flammable" check to be added to the Fire Manager's system, and be lit on fire.

## Fire Manager Global Configuration

The blocks.xml contains configurations and settings to adjust the fire behaviour from its defaults.

Note: Some of these properties may be defined on the block or material entry to over-ride the default.

#### Logging
```xml
    <!-- Enables or disables verbose logging for troubleshooting -->
    <property name="Logging" value="false"/>
```  
#### FireEnable
```xml
    <!-- Enables or disables the fire manager completely --> 
    <property name="FireEnable" value="false"/>
```  
#### CheckInterval
```xml
    <!-- Determines how often the fire manager should damage a block, determine fire spreading, etc -->
    <property name="CheckInterval" value="20"/>
```  
#### FireDamage
```xml
    <!-- Determines how much damage is done to a block per CheckInterval. -->
    <!-- If this property is defined on a specific block, that value will be used, rather than the default --> 
    <!-- If this property is defined on a materials.xml entry, that value will be used. -->
    <property name="FireDamage" value="10"/>
```
#### SmokeTime
```xml
    <!-- Determines the length of time, in seconds, that the smoke particle will appear on an extinguished block -->
    <!-- If this property is defined on a specific block, that value will be used, rather than the default -->
    <property name="SmokeTime" value="60"/>
```
#### FireSound  
```xml
    <!-- Determines which SoundDataNode entry will be used to play the sound -->
    <!-- If this property is defined on a specific block, that value will be used, rather than the default -->
    <property name="FireSound" value="ScoreMediumLoop"/>
```
#### BuffOnFire
```xml
    <!-- Defines which buff to apply to a player / zombie / NPC when they touch a block on fire -->
    <property name="BuffOnFire" value="buffBurningMolotov"/>
```
#### MaterialID
```xml
    <!-- Defines which materials are considered flammable, by material ID in materials.xml -->
    <property name="MaterialID" value="Mplants, Mcorn, Mhay"/>
```
#### MaterialDamage
```xml
    <!-- Defines which materials are considered flammable, by material Damage in materials.xml -->
    <property name="MaterialDamage" value="wood, cloth, corn, grass, plastic, leaves, cactus, mushroom, hay, paper, trash, backpack, organic"/>
```
#### MaterialSurface
```xml
    <!-- Defines which materials are considered flammable, by material Surface in materials.xml -->
    <property name="MaterialDamage" value="wood, cloth, corn, grass, plastic, leaves, cactus, mushroom, hay, paper, trash, backpack, organic"/>
```
#### SmokeParticle
```xml
    <!-- Defines the Smoke Particle to be used on an extinguished block -->
    <!-- If this property is defined on a specific block, that value will be used, rather than the default -->
    <property name="SmokeParticle" value="#@modfolder:Resources/PathSmoke.unity3d?P_PathSmoke_X"/>
```
#### FireParticle
```xml
    <!-- Defines the Fire Particle to be used on a block that is burning. -->
    <!-- If this property is defined on a specific block, that value will be used, rather than the default -->
    <property name="FireParticle" value="#@modfolder:Resources/gupFireParticles.unity3d?gupBeavis05-Heavy"/>

    <!-- Other particles that may be available, but may cause more performance overhead -->
    <!--
    <property name="FireParticle" value="#@modfolder:Resources/gupFireParticles.unity3d?gupBeavis02-CampFire" />
    <property name="FireParticle" value="#@modfolder:Resources/gupFireParticles.unity3d?gupBeavis03-Cartoon" />
    <property name="FireParticle" value="#@modfolder:Resources/gupFireParticles.unity3d?gupBeavis04-SlowFire" />
    <property name="FireParticle" value="#@modfolder:Resources/gupFireParticles.unity3d?gupBeavis05-Heavy" />
    <property name="FireParticle" value="#@modfolder:Resources/gupFireParticles.unity3d?gupBeavis06-HeavyLight" />
    -->
```
#### FireSpread
```xml
    <!-- Defines is fire can be spread to neighboring blocks -->
    <property name="FireSpread" value="true"/>
```
#### ChanceToExtinguish
```xml
    <!-- The chance, in percentage as defined from 0 to 1 to denote 0% to 100%, for a block to self-extinguish -->
    <!-- This is re-checked on each CheckInterval -->
    <!-- If this property is defined on a specific block, that value will be used, rather than the default -->
    <property name="ChanceToExtinguish" value="0.05"/>
```


## Block-specific Configuration

In addition to the above settings in the Fire Manager's overall blocks.xml, further properties may be optionally added to blocks.xml to further define the behaviour.

These per-block settings will over-ride the global value for that block in which they are defined on.

#### FireDowngradeBlock
```xml
    <!-- When a block is destroyed by fire, it will downgrade to this block -->
    <!-- By default, this is air -->
    <property name="FireDowngradeBlock" value="burnBlock"/>
```
#### Explosion.ParticleIndex
```xml
    <!-- If a block has a defined Explosion.ParticleIndex and is destroyed by fire, the fire manager will set an explosion based on those settings. -->
    <property name="Explosion.ParticleIndex" value="0"/>
```
#### ChanceToExtinguish
```xml
    <!-- The chance, in percentage as defined from 0 to 1 to denote 0% to 100%, for a block to self-extinguish -->
    <property name="ChanceToExtinguish" value="0.05"/>
```
#### FireDamage
```xml
    <!-- Determines how much damage is done to a block per CheckInterval. -->
    <property name="FireDamage" value="10"/>
```



## MinEvents

A few MinEvents have been written to help modders interact with the fire system. These MinEvents may be added to buffs or item actions.

The trigger may change depending on context and use.

#### MinEventActionAddFireDamage

This effect will set a block on fire, using the target's position, and the range. There is an optional delay time, which will delay the fire from starting.
```xml
     <triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamage, SCore" target="positionAOE" range="5" delayTime="10" />
```

#### MinEventActionAddFireDamageCascade

This effect can allow fire to spread to to surrounding blocks. You can set up a filter based on the type of blocks you wanted affected.

```xml
<!-- The same type of block -->
<triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamageCascade, SCore" range="4" filter="Type" />

<!-- Shares the same material -->
<triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamageCascade, SCore" range="4" filter="Material" />

<!-- Shares the same material damage classification -->
<triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamageCascade, SCore" range="4" filter="MaterialDamage" />

<!-- Shares the same material surface classification -->
<triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamageCascade, SCore" range="4" filter="MaterialSurface" />
```

#### MinEventActionCheckFireProximity

Checks to see if there's any fire blocks within the specified range, using the player's position as center.

```xml
<!-- The cvar specified, by default _closeFires, will contain the number of burning blocks in the range. -->
<triggered_effect trigger="onSelfBuffUpdate" action="CheckFireProximity, SCore" range="5" cvar="_closeFires" />
```

#### MinEventActionRemoveFire

Remove Fire within the specified range, using the specified target.

```xml
<!-- This will remove fire from all blocks with a range of 5 from the position of the target. -->
<triggered_effect trigger="onSelfDamagedBlock" action="RemoveFire, SCore" target="positionAOE" range="5"/>
```



## Examples

### XPath

Here are some examples on how to modify the ConfigFeatureBlock, which holds the Fire Management settings, from another modlet.
```xml
<!-- Enable fire -->
<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='FireManagement']/property[@name='FireEnable']/@value">true</set>
<!-- Turn on verbose logging -->
<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='FireManagement']/property[@name='Logging']/@value">true</set>

<!-- Adding materials -->
<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='FireManagement']/property[@name='MaterialDamage']/@value">wood, cloth, corn, grass, plastic, leaves, cactus, mushroom, hay, paper, trash, backpack, organic</set>
<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='FireManagement']/property[@name='MaterialSurface']/@value">wood, cloth, corn, grass, plastic, leaves, cactus, mushroom, hay, paper, trash, backpack, organic</set>
```
 
To adding Fire Damage to the flaming crossbow bolts
```xml
<append xpath="/items/item[@name='ammoCrossbowBoltFlaming']/effect_group" >
	<triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamage, SCore" delayTime="5000"/> <!-- Delay Time in ms -->
</append>
```

### Triggers

```xml
<!-- Adds fire to the block being hit, if it's flammable. -->
<triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamage, SCore" />

<!-- Remove fire from an area -->
<triggered_effect trigger="onSelfPrimaryActionRayHit" action="RemoveFire, SCore" target="positionAOE" range="5"/>

<!-- Spread fire to all surrounding blocks that have the same material as the target being hit, such as cloth -->
<triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamageCascade, SCore" range="4" filter="Material" />
```

