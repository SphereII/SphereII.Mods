This feature introduces an advanced item degradation system where an item's quality can decrease when it is repaired.
This functionality is driven by SCore's custom MinEvent system.

### Item Quality Degradation on Repair

**Mechanism:**
A Harmony patch within SCore enables the `onSelfItemRepaired` MinEvent to trigger whenever an item is repaired in the
game. This allows modders to attach custom effects to the repair action.

**Action (`ModifyItem, SCore`):**
The `ModifyItem, SCore` action is specifically designed to handle item quality degradation. When triggered, it will
degrade the quality of the affected item. Importantly, SCore automatically handles the adjustment of mod slots based on
the item's new quality level. Any mods that no longer fit due to reduced slots will be uninstalled and added to the
player's inventory. If the player's inventory is full, these mods will be dropped on the ground.

**Provided XML Snippet Analysis:**
The provided XML snippet demonstrates how `onSelfItemRepaired` can be used.

```xml

<item name="myCustomAxe">
    <effect_group name="Repair items">
        <triggered_effect trigger="onSelfItemRepaired" action="ModifyItem, SCore">
            <requirement name="RandomRoll" min="0" max="100" value="@.repairDegradeChance"/>
            <requirement name="!CompareItemProperty, SCore" property="Quality" operation="Equals" value="1"/>
        </triggered_effect>

    </effect_group>
</item>
```

* The `effect_group` wraps the triggered effects for organization.
* The `triggered_effect` is set to trigger on `onSelfItemRepaired`.
* `action="ModifyItem, SCore"` indicates the specific SCore action that handles quality degradation and mod management.
* **Requirements in the Snippet:**
    * `<requirement name="RandomRoll" min="0" max="100" value="@.repairDegradeChance"/>`: This specifies that the
      quality degradation will only occur if a random roll (between 0 and 100) falls below the value of the item's
      `repairDegradeChance` property. The `@.` syntax means it reads the property from the item itself.
    * `<requirement name="CompareItemProperty, SCore" property="Quality" operation="Equals" value="1"/>`: This crucial
      requirement in the provided snippet means that the `ModifyItem, SCore` action *will only trigger if the item's
      quality is already exactly 1*. This would imply a scenario where the degradation only happens at the lowest
      quality.
* **Commented-out `RemoveItem` Effect:** The commented section shows how an item could be entirely removed (break) when
  repaired at Quality 1, based on an external `CVarCompare` for `RepairQualityOneBreaksAfter`.

**Considerations for General Degradation:**
If the intent is for quality to degrade from any tier (e.g., T6 to T5, T5 to T4), the `CompareItemProperty` requirement
for `Quality="1"` on the `ModifyItem, SCore` triggered effect would need to be changed or removed. Instead, you'd
typically have:

