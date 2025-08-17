### Item Degradation for 7 Days to Die

Item degradation is a useful mechanic for keeping players engaged over a longer period of time, as it forces them to
look for replacement parts with better quality. SCore provides a few small patches to help create a more flexible
system.

#### How It Works

* **Trigger Condition**: A patch will trigger to degrade the durability of an `item_modifier` if that `item_modifier`
  has `Quality`.
* **Mechanism**: The patch works by looping through each modification and degrading its durability per its
  `passive_effect`.
* **Outcome**: Once the degradation is complete, an item will either break, or its `passive_effect`s will be disabled.

#### Using the `ItemPercentUsed` Requirement

A new `ItemPercentUsed` requirement has been added for use within `item_modifications`. This allows modders to control
the effects of a modifier based on the item's remaining durability percentage.

You can use this requirement within an `effect_group` to enable or disable effects when an item's durability falls below
a certain threshold.

Consider the following `item_modifier.xml` patch as an example:

```xml
<configs>
  <!-- Add a requirement to the effect_groups to disable it when the durability is below a level -->
  <!-- Apply them all first, so then we can add our own effect_group -->
  <!--
      <append xpath="//item_modifier/effect_group[passive_effect]">
          <requirement name="ItemPercentUsed, SCore" operation="LT" value="1"/>
      </append>
      -->
  <append xpath="//item_modifier/effect_group">
    <requirement name="ItemPercentUsed, SCore" operation="LT" value="1"/>
  </append>

  <append xpath="//item_modifier">
    <!-- Enable Quality for all item_modifier -->
    <property name="ShowQuality" value="true"/>

    <!-- Let Them be repaired -->
    <property name="RepairTools" value="resourceRepairKit"/>

    <!-- Break after they completely degrade?-->
    <property name="DegradationBreaksAfter" value="false"/>

    <effect_group name="Quality Durability" >
      <passive_effect name="DegradationMax" operation="base_set" value="300"/>
      <passive_effect name="DegradationPerUse" operation="base_set" value="100"/>
    </effect_group>

    <effect_group name="DamageHooks">
      <requirement name="ItemPercentUsed, SCore" operation="LT" value="1"/>
      <requirement name="IsItemActive"/>
      <triggered_effect trigger="onSelfRoutineUpdate" action="DegradeItemValueMod, SCore"/>
    </effect_group>

    <effect_group name="BrokenHooks">
      <requirement name="ItemPercentUsed, SCore" operation="GTE" value="1"/>
      <requirement name="IsItemActive"/>
      <triggered_effect trigger="onSelfRoutineUpdate" action="AddBuff" buff="buffStatusModBroken"/>
    </effect_group>

    <effect_group name="EquipHooks">
      <triggered_effect trigger="onSelfEquipStop" action="RemoveBuff" buff="buffStatusModBroken"/>
      <triggered_effect trigger="onSelfEquipStart" action="RemoveBuff" buff="buffStatusModBroken">
        <requirement name="ItemPercentUsed, SCore" operation="LT" value="1"/>
      </triggered_effect>
    </effect_group>

  </append>
</configs>
```

