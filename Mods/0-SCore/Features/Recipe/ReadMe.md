This documentation outlines the new features introduced within the `Recipe` folder, enhancing the crafting system with
capabilities for additional outputs from recipes.

## 1\. Additional Recipe Output (via `MinEventActionAddAdditionalOutput`)

This feature allows recipes to define supplementary items that are produced alongside the primary crafted item. These
additional outputs can be conditional, providing greater flexibility in recipe design.

* **Functionality**: When a recipe is crafted, the `onSelfItemCrafted` trigger is invoked. Any associated requirements
  are then processed, and if the conditions (e.g., a `<requirement>` within the `triggered_effect`) are met, the
  specified additional items will be added to the player's inventory or the workstation's output. The system also
  includes checks for full inventory or workstation output, halting crafting if there is not enough space in the output slots and providing a
  tooltip notification.

  **Note on Triggering MinEvents**: While many `MinEvents` triggered by `onSelfItemCrafted` typically require the player
  to be actively looking into their backpack or the workstation's interface for the effects to fire, the
  `AddAdditionalOutput` action does **not** have this limitation. It will deliver the additional items regardless of
  whether the player is viewing their inventory or the workstation output at the exact moment of crafting completion.

* AddAdditionalOutput also only works as a MinEvent in onSelfItemCrafted. It has no Execute() code, so it does nothing.
  This is only used to store data.

* **XML Usage**: You can integrate this feature into your `recipes.xml` (or equivalent item configuration files) within
  an `<effect_group>` using the `onSelfItemCrafted` trigger.

  **Example:**

  ```xml
  <recipe name="ammo45ACPCase" count="30" craft_time="5" craft_area="MillingMachine" tags="workbenchCrafting,PerkHOHMachineGuns">
      <ingredient name="resourceBrassIngot" count="5"/>

      <effect_group name="Additional Output">
          <triggered_effect trigger="onSelfItemCrafted" action="AddAdditionalOutput, SCore" item="resourceYuccaFibers" count="2"/>
          <triggered_effect trigger="onSelfItemCrafted" action="AddAdditionalOutput, SCore" item="resourceDuctTape" count="1"/>
      </effect_group>
      
      <effect_group name="Conditional Output Example">

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

* **Properties for `MinEventActionAddAdditionalOutput`**:

    * **`item` (string)**: Specifies the `name` of the item to be added as additional output.
    * **`count` (integer)**: Defines the quantity of the `item` to be added.