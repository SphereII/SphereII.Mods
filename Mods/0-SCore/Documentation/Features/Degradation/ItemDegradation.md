### Item Modifier Degradation System 🛠️

This XML configuration enables and customizes a **degradation system for item modifiers**, such as a flashlight or water
purifier. It introduces new properties and effect groups that control how and when a mod's durability decreases.

***
```xml
<configs>
  <!-- Add a requirement to the effect_groups to disable it when the durability is below a level -->
  <!-- Apply them all first, so then we can add our own effect_group -->
  <append xpath="//item_modifier/effect_group[passive_effect]">
    <requirement name="ItemPercentUsed, SCore" operation="LT" value="1"/>
  </append>

  <append xpath="//item_modifier">

    <!-- Enable Quality  -->
    <property name="ShowQuality" value="true"/>

    <!-- Let Them be repaired -->
    <property name="RepairTools" value="resourceRepairKit"/>

    <!-- Set max use per quality tier -->
    <property name="DegradationMaxUse" value="100,600" param1="1,6"/>
    <property name="DegradationPerUse" value="1"/>

    <!-- Break after they completely degrade?-->
    <property name="DegradationBreaksAfter" value="false"/>

    <!-- Damage over time if the item is active -->
    <effect_group name="DamageHooks">
      <requirement name="ItemPercentUsed, SCore" operation="LT" value="1"/>
      <requirement name="IsItemActive"/>
      <triggered_effect trigger="onSelfRoutineUpdate" action="DegradeItemValueMod, SCore"/>
    </effect_group>

    <!-- Damages when onSelfItemDegrade gets triggered -->
    <effect_group name="DamageHooks Degrade">
      <requirement name="ItemPercentUsed, SCore" operation="LT" value="1"/>
      <triggered_effect trigger="onSelfItemDegrade" action="DegradeItemValueMod, SCore"/>
    </effect_group>

    <effect_group name="EquipHooks">
      <triggered_effect trigger="onSelfEquipStop" action="RemoveBuff" buff="buffStatusModBroken"/>
      <triggered_effect trigger="onSelfEquipStart" action="RemoveBuff" buff="buffStatusModBroken">
        <requirement name="ItemPercentUsed, SCore" operation="LT" value="1"/>
      </triggered_effect>
    </effect_group>

  </append>
  
  <!-- Water Purifier -->
  <append xpath="//item_modifier[@name='modArmorWaterPurifier']">
    <effect_group name="Damage During Use Override">
      <requirement name="ItemPercentUsed, SCore" operation="LT" value="1" />
      <requirement name="IsEquipped" />
      <!-- Player hand -->
      <triggered_effect trigger="onSelfAction2End" action="DegradeItemValueMod, SCore"/>
      <!-- Murky water -->
      <triggered_effect trigger="onSelfPrimaryActionEnd" action="DegradeItemValueMod, SCore"/>
    </effect_group>

    <effect_group name="Item Broken">
      <requirement name="IsEquipped" />
      <requirement name="ItemPercentUsed, SCore" operation="GTE" value="1" />
      <triggered_effect trigger="onSelfAction2End" action="AddBuff" buff="buffStatusModBroken"/>
      <triggered_effect trigger="onSelfPrimaryActionEnd" action="AddBuff" buff="buffStatusModBroken"/>
    </effect_group>

    <property name="DegradationMaxUse" value="10,600" param1="1,6"/>
    <property name="DegradationPerUse" value="1"/>
  </append>

  <append xpath="//item_modifier[@name='modArmorWaterPurifier']/effect_group/passive_effect[@name='BuffResistance']">
    <requirement name="ItemPercentUsed, SCore" operation="LT" value="1" />
  </append>

</configs>
```

## Core Properties

These properties define the fundamental behavior of a mod's degradation and can be configured to fit various item types.

* **`ShowQuality`**: Setting this property to `true` allows the mod to display its quality level. This is a prerequisite
  for the degradation system, as durability scales with quality.
* **`RepairTools`**: This property, when set to a specific item like `resourceRepairKit`, allows the mod to be repaired.
* **`DegradationMaxUse`**: This defines the **maximum uses** an item can withstand before it breaks. The value can be a
  single number or a range based on quality. For example, a value of `100,600` with `param1="1,6"` means an item with
  quality 1 has 100 max uses, and one with quality 6 has 600.
* **`DegradationPerUse`**: This property determines the amount of durability the mod loses with each use.
* **`DegradationBreaksAfter`**: This is a boolean property that controls whether the mod is **destroyed** after it has
  completely degraded. Setting it to `false` means the item becomes unusable but can still be repaired later.

***

## Degradation Hooks and Triggers

The system uses `effect_group`s to trigger degradation based on specific events. These `effect_group`s are appended to
all `item_modifier`s, making the system universal.

* **`DamageHooks`**: This hook handles **damage over time** when a mod is in active use. It uses a `triggered_effect`
  with `trigger="onSelfRoutineUpdate"` and `action="DegradeItemValueMod, SCore"` to reduce the mod's durability. This is
  ideal for active mods like a flashlight, which degrades as long as it is on.
* **`DamageHooks Degrade`**: This hook triggers degradation when an `onSelfItemDegrade` event is specifically called. It
  also uses the `DegradeItemValueMod, SCore` action. This is useful for mods that degrade based on an external event,
  like a workbench tool degrading when a recipe is crafted.
* **`ItemPercentUsed, SCore`**: A crucial `requirement` used to ensure effects only apply when the item is in a certain
  state. For instance, a `value="1"` means the effect will not activate when the item's durability is 100% or greater.
* **`EquipHooks`**: These hooks handle the removal of the **`buffStatusModBroken`** buff. This buff is applied to a
  player when a mod on an item breaks, indicating that it is no longer working. The hooks remove the buff when the item
  is unequipped or re-equipped, as long as the item has not degraded.

***

## Examples in Practice

* **Water Purifier Mod (`modArmorWaterPurifier.xml`)**: This mod uses `onSelfAction2End` and `onSelfPrimaryActionEnd`
  triggers to degrade when a player drinks murky water or performs a specific action. [cite_start]Once the mod is
  broken, it applies a `buffStatusModBroken` to the player, which has a tooltip that says, "A mod is broken. It must be
  repaired."[cite: 2].
* [cite_start]**Helmet/Gun Flashlight Mods (`modArmorHelmetLight.xml`, `modGunFlashlight.xml`)**: These mods use a
  `triggered_effect` to turn off the light source once the mod's durability falls below 1%[cite: 1].