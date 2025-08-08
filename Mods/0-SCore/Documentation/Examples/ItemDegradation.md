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

#### Using the `ItemModDurability` Requirement

A new `ItemModDurability` requirement has been added for use within `item_modifications`. This allows modders to control
the effects of a modifier based on the item's remaining durability percentage.

You can use this requirement within an `effect_group` to enable or disable effects when an item's durability falls below
a certain threshold.

Consider the following `item_modifier.xml` patch as an example:

```xml

<configs>
    <!-- This will disable the effect_group from executing if the durability is below the threshold,
        ie, the passive effects.
    -->
    <append xpath="//item_modifier/effect_group">
        <requirement name="ItemModDurability, SCore" operation="GTE" value="0.9"/>
    </append>

    <!-- This sets up the quality for the item, and sets up its DegradationPerUse -->
    <append xpath="//item_modifier">

        <!-- Enable Quality for all item_modifier -->
        <property name="ShowQuality" value="true"/>

        <!-- Let Them be repaired -->
        <property name="RepairTools" value="resourceRepairKit"/>

        <!-- Break after they completely degrade? -->
        <property name="DegradationBreaksAfter" value="true"/>

        <property name="ShowQuality" value="true"/>
        <effect_group name="Quality Mods">
            <passive_effect name="DegradationMax" operation="base_set" value="300,400" tier="1,6"/>
            <passive_effect name="DegradationPerUse" operation="base_set" value="1"/>
        </effect_group>

        <!-- This is optional, just to let you show a log message -->
        <effect_group name="Item Broken">
            <requirement name="ItemModDurability, SCore" operation="LTE" value="0.9"/>
            <triggered_effect trigger="onSelfPrimaryActionRayHit" action="LogMessage"
                              message="Mod is damaged, it's not longer working.">
            </triggered_effect>
        </effect_group>
    </append>
</configs>
```

