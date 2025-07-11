This documentation outlines new capabilities for crafting recipes in 7 Days to Die, allowing for more dynamic and
conditional outputs or effects when a player crafts an item. These features are powered by SCore modifications.

-----

# Advanced Recipe Crafting: Multiple Outputs & Conditional MinEvents

This section details how to configure recipes to produce multiple items and execute conditional `MinEvents` (
`triggered_effect`s) specifically when an item is crafted in a workstation.

## 1\. Triggering Multiple Outputs from a Recipe (`AddAdditionalOutput, SCore`)

You can now configure recipes to yield more than just the primary crafted item. This new feature allows you to specify
additional items that will be added to the player's inventory (or the workstation's output) when a recipe is
successfully crafted.

* **Action**: `AddAdditionalOutput, SCore`
* **Trigger**: This action is specifically designed to be used with the `onSelfItemCrafted` trigger within a recipe's
  `<effect_group>`.
* **Parameters**:
    * `item`: The `name` of the additional item to produce.
    * `count`: The quantity of the additional item to produce.

### Example: `ammo45ACPCase` with Additional Output

In this example, crafting `ammo45ACPCase` will also grant the player `resourceYuccaFibers` and `resourceDuctTape`.

```xml

<recipe name="ammo45ACPCase" count="30" craft_time="5" craft_area="MillingMachine"
        tags="workbenchCrafting,PerkHOHMachineGuns">
    <ingredient name="resourceBrassIngot" count="5"/>
    <effect_group name="Additional Output">
        <triggered_effect trigger="onSelfItemCrafted" action="AddAdditionalOutput, SCore" item="resourceYuccaFibers"
                          count="2"/>
        <triggered_effect trigger="onSelfItemCrafted" action="AddAdditionalOutput, SCore" item="resourceDuctTape"
                          count="1"/>
    </effect_group>
</recipe>
```

-----

## 2\. Advanced `onSelfItemCrafted` MinEvents with Requirements

The `onSelfItemCrafted` trigger within recipes now supports a more robust `MinEventParams` package. This enhancement
allows for more complex and conditional `MinEvents` to be executed when an item is created at a workstation.

* **Trigger Condition**: `onSelfItemCrafted` events are specifically designed to fire when:
    * The player is interacting with a workstation (e.g., crafting menu open in a forge, workbench, chemistry station).
    * An item is successfully crafted and moved into the output slot or player's inventory.
* **Requirement Support**: `triggered_effect`s using `onSelfItemCrafted` now fully respect `<requirement>` tags. This
  means you can set conditions based on player buffs, attributes, world state, etc., to control whether a specific
  effect triggers.

### Example: Conditional Crafting Effects ("Sphere Testing")

This example demonstrates how various `triggered_effect`s can be attached to a recipe, some of which are conditional
based on player buffs.

```xml

<effect_group name="Sphere Testing">
    <triggered_effect trigger="onSelfItemCrafted" action="AddAdditionalOutput, SCore" item="resourceYuccaFibers"
                      count="2"/>

    <triggered_effect trigger="onSelfItemCrafted" action="AddAdditionalOutput, SCore" item="ammoRocketHE" count="2">
        <requirement name="HasBuff" buff="god"/>
    </triggered_effect>

    <triggered_effect trigger="onSelfItemCrafted" action="PlaySound" sound="player#painsm">
        <requirement name="!HasBuff" buff="god"/>
    </triggered_effect>

    <triggered_effect trigger="onSelfItemCrafted" action="AddBuff" buff="buffDrugEyeKandy"/>
</effect_group>
```

**Explanation of the "Sphere Testing" Example:**

* The first `AddAdditionalOutput` will always give 2 `resourceYuccaFibers` upon crafting.
* The second `AddAdditionalOutput` for `ammoRocketHE` is **conditional**: It will *only* trigger if the crafting player
  currently possesses the `god` buff.
* The `PlaySound` effect is also **conditional**: It will *only* trigger if the crafting player *does NOT* have the
  `god` buff (`!HasBuff`).
* The `AddBuff` effect will always apply the `buffDrugEyeKandy` buff to the player upon crafting.

-----

## Important Notes & Limitations

* **`AddAdditionalOutput, SCore` Specific Usage**: The `AddAdditionalOutput` `MinEvent` action is exclusively tied to
  the recipe `onSelfItemCrafted` hook. It will not function or produce any effect if used in other contexts (e.g., on
  block triggers, entity triggers, item actions outside of recipe crafting).
* **`onSelfItemCrafted` Context**: Remember that `onSelfItemCrafted` triggers are specifically for actions taken *when
  crafting completes* within a workstation UI. They are not for general item usage or world interactions.
* **Requirements Power**: The ability to add `<requirement>` tags provides immense flexibility for creating dynamic
  crafting outcomes based on player status, time of day, location, or other game conditions.