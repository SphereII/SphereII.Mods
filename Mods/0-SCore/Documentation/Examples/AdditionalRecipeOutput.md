## **Additional Recipe Outputs 🎁**

This document explains the new feature in the 0-SCore mod that allows recipes to produce additional output items using
`effect_group`s. This functionality is implemented through two separate Harmony patches that target different crafting
scenarios.

-----

### **How It Works** 📝

The additional output feature is supported by two key patches:

* **`TileEntityWorkstationHandleRecipeQueue`**: This patch handles crafting actions that occur while the workstation
  window is **closed**.
* **`XUiCRecipeStackOutputStackPatches`**: This patch handles crafting actions that occur while the workstation or
  backpack crafting window is **open**.

Together, these patches ensure that recipes with defined `effect_group`s for additional outputs will function correctly,
regardless of whether the crafting window is visible or not.

When a craft is completed, these patches read the recipe's `effect_group`s to determine if any additional items should
be created. The new items are then added to the appropriate inventory: the workstation's output grid or the player's
backpack.

### **Using Additional Outputs** ⚙️

To use this feature, you must add an `effect_group` to your recipe definition in the XML. The `effect_group` must
contain a `triggered_effect` with the `action` attribute set to `AddAdditionalOutput, SCore`.

#### **Example XML Snippet**

Below is an example of a recipe that produces additional outputs. This example demonstrates how to add a
`triggered_effect` to a recipe.

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
</recipe>
```

* **`triggered_effect`**: The core element for defining additional output.
* **`trigger="onSelfItemCrafted"`**: This trigger ensures the effect only runs after the primary recipe item has been
  successfully crafted.
* **`action="AddAdditionalOutput, SCore"`**: This is the specific action that tells the system to generate an additional
  item.
* **`item`**: Specifies the item to be created as additional output.
* **`count`**: Specifies the number of items to add.

You can also use `<requirement>` tags within the `triggered_effect` to make the additional output conditional. For
example, in the provided snippet, two `ammoRocketHE` items are only added if the player has the `god` buff. This allows
for complex and context-sensitive crafting outcomes.