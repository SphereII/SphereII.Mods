### Item Degradation for 7 Days to Die 🛠️

Item degradation is a key mechanic in **7 Days to Die** for keeping players engaged by encouraging them to find
replacement parts with better quality. The SCore mod provides a flexible system for this.

-----

### How It Works

* **Trigger Condition**: The system triggers when an **item\_modifier** has a **Quality** attribute.
* **Mechanism**: A patch loops through each modifier, degrading its durability based on its **passive\_effect**.
* **Outcome**: When an item's durability reaches zero, it either breaks or its **passive\_effect**s are disabled.

-----

### The `ItemPercentUsed` Requirement

A new `ItemPercentUsed` requirement has been introduced, allowing modders to control a modifier's effects based on the
item's remaining durability percentage. This requirement can be used within an `effect_group` to enable or disable
effects when an item's durability falls below a certain threshold.

**Example: `item_modifier.xml` Patch**

```xml

<configs>
    <append xpath="//item_modifier/effect_group">
        <requirement name="ItemPercentUsed, SCore" operation="LT" value="1"/>
    </append>

    <append xpath="//item_modifier">
        <property name="ShowQuality" value="true"/>
        <property name="RepairTools" value="resourceRepairKit"/>
        <property name="DegradationBreaksAfter" value="false"/>

        <effect_group name="Quality Durability">
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

