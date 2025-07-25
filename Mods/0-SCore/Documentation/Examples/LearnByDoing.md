## LBD Perk Documentation: Advanced Engineering

### Overview

The `AdvancedEngineering` perk is designed to reward players for fabricating complex items. Your skill in this perk
increases as you craft items at high-level workstations, reflecting your growing expertise with advanced tools and
schematics.

* **How to Level**: Gain experience by crafting any item using a Workbench, Forge, or Chemistry Station.
* **Attribute Synergy**: Every time you gain experience for `AdvancedEngineering`, you also gain a small amount of "
  synergy" experience towards leveling up your main `attIntellect` attribute.

-----

### How It Works (XML Implementation)

The logic for `AdvancedEngineering` is handled by appending an `effect_group` to the main `buffLBD_IntellectManager`.
This group activates whenever the player successfully crafts an item.

1. **Initialization (`Init.xml`)**: When the LBD system initializes, it sets the starting XP and the amount of XP needed
   for the first level of this perk.

   ```xml
   <triggered_effect trigger="onSelfBuffStart" action="ModifyCVar" cvar="$perkadvancedengineering_lbd_xp" operation="set" value="0"/>
   <triggered_effect trigger="onSelfBuffStart" action="ModifyCVar" cvar="$perkadvancedengineering_lbd_xptonext" operation="set" value="2000"/>
   ```

2. **XP Gain (`AdvancedEngineering.xml`)**: The system listens for the `onSelfItemCrafted` trigger. If the crafting
   occurred at a `forge`, `workstation`, or `chemistryStation`, it adds experience to the perk's CVar (
   `$perkadvancedengineering_lbd_xp`) and applies a short cooldown to prevent spam.

3. **Level Up**: Once the accumulated XP meets the required amount, a temporary level-up buff is triggered, which grants
   the player a level in `perkAdvancedEngineering` and increases the XP required for the next level.

-----

### XML Examples

#### `AdvancedEngineering.xml`

This file would contain the core logic for granting XP to the perk.

```xml

<configs>
    <append xpath="/buffs/buff[@name='buffLBD_IntellectManager']">
        <effect_group name="Advanced Engineering">
            <requirement name="RequirementRecipeCraftArea, SCore" craft_area="forge,workstation,chemistryStation"/>

            <triggered_effect trigger="onSelfItemCrafted" action="LogMessage"
                              message="LBD DEBUG: Advanced Engineering - Attempting Base Crafting XP (+@$lbd_xp_special_base)">
                <requirement name="NotHasBuff" buff="buffLBD_perkAdvancedEngineering_XPCoolDown"/>
                <requirement name="HasBuff" buff="god"/>
            </triggered_effect>
            <triggered_effect trigger="onSelfItemCrafted" action="ModifyCVar" cvar="$perkadvancedengineering_lbd_xp"
                              operation="add" value="@$lbd_xp_special_base">
                <requirement name="NotHasBuff" buff="buffLBD_perkAdvancedEngineering_XPCoolDown"/>
            </triggered_effect>

            <triggered_effect trigger="onSelfItemCrafted" action="AddBuff"
                              buff="buffLBD_perkAdvancedEngineering_XPCoolDown">
                <requirement name="NotHasBuff" buff="buffLBD_perkAdvancedEngineering_XPCoolDown"/>
            </triggered_effect>

            <triggered_effect trigger="onSelfItemCrafted" action="AddBuff"
                              buff="buffLBD_perkAdvancedEngineering_LevelUpCheck">
                <requirement name="CVarCompare" cvar="$perkadvancedengineering_lbd_xp" operation="GTE"
                             value="@$perkadvancedengineering_lbd_xptonext"/>
            </triggered_effect>
        </effect_group>
    </append>
</configs>
```

#### `buffLBD_perkAdvancedEngineering_LevelUpCheck`

This buff handles the actual process of leveling up the perk. This code would typically be placed in the same file or a
centralized buff file.

```xml

<configs>
    <append xpath="/buffs">
        <buff name="buffLBD_perkAdvancedEngineering_LevelUpCheck" hidden="true">
            <stack_type value="ignore"/>
            <duration value="1"/>
            <effect_group>
                <requirement name="!RequirementIsProgressionLocked, SCore" progression_name="perkAdvancedEngineering"/>
                <triggered_effect trigger="onSelfBuffStart" action="AddProgressionLevel"
                                  progression_name="perkAdvancedEngineering" level="1"/>
                <triggered_effect trigger="onSelfBuffStart" action="ModifyCVar" cvar="$perkadvancedengineering_lbd_xp"
                                  operation="subtract" value="@$perkadvancedengineering_lbd_xptonext"/>
                <triggered_effect trigger="onSelfBuffStart" action="ModifyCVar"
                                  cvar="$perkadvancedengineering_lbd_xptonext" operation="multiply"
                                  value="@$lbd_xp_curve_multiplier"/>
                <triggered_effect trigger="onSelfBuffStart" action="ShowPerkLevelUp, SCore"
                                  perk="perkAdvancedEngineering" sound="read_skillbook_final"/>
            </effect_group>
        </buff>
    </append>
</configs>
```

#### Synergy with `attIntellect` (`Intellect.xml`)

As shown in your `Intellect.xml` file, crafting at these stations *also* grants a small amount of XP to the main
attribute, ensuring that specializing in crafting makes you more intelligent overall.

```xml

<triggered_effect trigger="onSelfItemCrafted" action="LogMessage"
                  message="LBD DEBUG: Intellect Attribute - Advanced Crafting Synergy XP (+@($lbd_xp_attribute_synergy_base))">
    <requirement name="RequirementRecipeCraftArea, SCore" craft_area="forge,workstation,chemistryStation"/>
    <requirement name="NotHasBuff" buff="buffLBD_attIntellect_XPCoolDown"/>
    <requirement name="HasBuff" buff="god"/>
</triggered_effect>
<triggered_effect trigger="onSelfItemCrafted" action="ModifyCVar" cvar="$attintellect_lbd_xp" operation="add"
                  value="@($lbd_xp_attribute_synergy_base)">
<requirement name="RequirementRecipeCraftArea, SCore" craft_area="forge,workstation,chemistryStation"/>
<requirement name="NotHasBuff" buff="buffLBD_attIntellect_XPCoolDown"/>
</triggered_effect>
```