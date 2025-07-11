This documentation details the "SphereII Take and Replace" modlet, which leverages SCore's `TakeAndReplace` class to
allow players to pick up certain blocks and automatically replace them with another, often a simpler, block. This
feature is commonly applied to environmental elements like boarded windows and shipping crates, providing a streamlined
way to "dismantle" them for resources.

-----

## SphereII Take and Replace Modlet: Leveraging SCore's TakeAndReplace Feature

The `TakeAndReplace, SCore` class in SCore provides modders with powerful control over how players can interact with and
acquire blocks from the world. It enables a "pickup and replace" mechanic, transforming complex blocks into simpler
forms or resources upon interaction.

### 1\. Core `TakeAndReplace` Class Properties

The `TakeAndReplace, SCore` class is applied to a block or shape definition and is configured using several properties.

* **`Class`**: Specifies that this block uses the SCore `TakeAndReplace` logic.
    * **How to Use**: Set `Class="TakeAndReplace, SCore"` on the block or shape definition.
    * **Example (`blocks.xml`, `shapes.xml`)**:
      ```xml
      <property name="Class" value="TakeAndReplace, SCore"/>
      ```
* **`CanPickup`**: Enables the block to be picked up by the player.
    * **How to Use**: Set `CanPickup="true"`.
    * **Example (`blocks.xml`)**:
      ```xml
      <property name="CanPickup" value="true" />
      ```
* **`TakeDelay`**: The time (in seconds) it takes for the player to pick up the block.
    * **How to Use**: Set `TakeDelay` to a numerical value.
    * **Example (`blocks.xml`)**:
      ```xml
      <property name="TakeDelay" value="15" />
      ```
* **`PickUpBlock`**: The block (or item) that is placed into the player's inventory when the original block is picked
  up.
    * **How to Use**: Set `PickUpBlock` to the name of the desired block or item.
    * **Example (`blocks.xml`)**:
      ```xml
      <property name="PickUpBlock" value="resourceWood"/>
      ```
* **`ValidMaterials` (Optional)**: If specified, the block can only be picked up if its material matches one in this
  comma-separated list.
    * **How to Use**: Add `<property name="ValidMaterials" value="Mwood_weak,Mwood_regular"/>`.
    * **Example (Provided Text for `windowBoarded`)**:
      ```xml
      <property name="ValidMaterials" value="Mwood_weak,Mwood_regular"/>
      ```
* **`TakeWithTool` (Optional)**: If specified, the "Take" prompt for the block will only appear if the player is holding
  one of the items listed in this comma-separated value. This happens after `ValidMaterials` check.
    * **How to Use**: Add
      `<property name="TakeWithTool" value="meleeToolRepairT0StoneAxe,meleeToolRepairTazaStoneAxe"/>`.
    * **Example (Provided Text for `windowBoarded`)**:
      ```xml
      <property name="TakeWithTool" value="meleeToolRepairT0StoneAxe,meleeToolRepairTazaStoneAxe"/>
      ```
* **`CheckToolForMaterial` (Optional)**: If `true`, the system will check for a tag on the currently held tool. This tag
  must be the material ID of the block being picked up.
    * **How to Use**: Add `<property name="CheckToolForMaterial" value="true"/>`. Ensure the tool item's `Tags` property
      includes the material ID (e.g., `Mwood_weak`, `Mwood_regular`).
    * **Example (Provided Text for `windowBoarded`)**:
      ```xml
      <property name="CheckToolForMaterial" value="true" />
      ```

### 2\. Tool Behavior and Sounds

The `TakeAndReplace` system also modifies tool interactions and sounds.

* **`Take Sound`**: The sound played when a block is picked up is now more appropriate for the material being taken (
  e.g., wooden sound for wood materials, steel sound for steel materials).
* **`silenttake` Tag**: If the item the player is holding has the tag `silenttake`, no sound will be played when a block
  is taken, regardless of `TakeWithTool` or `HoldingItem` properties.
    * **How to Use**: Add `<property name="Tags" value="silenttake"/>` to an item's definition in `items.xml`.

### 3\. Application in `blocks.xml` and `shapes.xml`

The modlet applies the `TakeAndReplace, SCore` class to various blocks and shapes, primarily shipping crates and window
barricades/debris, enabling them to be picked up and replaced, usually yielding `resourceWood`. This functionality is
conditional on `mod_loaded('0-SCore_sphereii')`.

* **Shipping Crates (`blocks.xml`)**: Various types of shipping crates (e.g., `cntShippingCrateLabEquipment`,
  `cntShippingCrateShamway`, `cntShippingCrateCarParts`) are configured to be picked up.
    * **Example (`blocks.xml`)**:
      ```xml
      <conditional>
          <if cond="mod_loaded('0-SCore_sphereii')">
              <append xpath="/blocks/block[@name ='cntShippingCrateLabEquipment']" >
                  <property name="Class" value="TakeAndReplace, SCore"/>
                  <property name="CanPickup" value="true" />
                  <property name="TakeDelay" value="15" />
                  <property name="PickUpBlock" value="resourceWood"/>
              </append>
              </if>
      </conditional>
      ```
* **Window Barricades (`blocks.xml`)**: Various window barricade blocks (e.g., `shutters1Plate`, `plywoodOsbCTRPlate`,
  `woodBarricadeCTRPlate`) are also configured for `TakeAndReplace`.
    * **Example (`blocks.xml`)**:
      ```xml
      <append xpath="/blocks/block[@name ='shutters1Plate']" >
          <property name="Class" value="TakeAndReplace, SCore"/>
          <property name="CanPickup" value="true" />
          <property name="TakeDelay" value="15" />
          <property name="PickUpBlock" value="resourceWood"/>
      </append>
      ```
* **Window Shapes and Debris (`shapes.xml`)**: Several window shapes (e.g., `windowBoarded`, `window01Empty`,
  `windowplug01`) and wood debris shapes are set up for `TakeAndReplace`, yielding `woodShapes:VariantHelper`.
    * **Example (`shapes.xml`)**:
      ```xml
      <conditional>
          <if cond="mod_loaded('0-SCore_sphereii')">
              <append xpath="/shapes/shape[@name='windowBoarded']">
                  <property name="Class" value="TakeAndReplace, SCore"/>
                  <property name="CanPickup" value="true"/>
                  <property name="TakeDelay" value="8"/>
                  <property name="PickUpBlock" value="woodShapes:VariantHelper"/>
                  </append>
              </if>
      </conditional>
      ```

### 4\. Related Item: The Crow Bar

The modlet introduces a "Crow Bar" item, which is designed to interact with this system.

* **`CrowBar` Item (`Localization.txt`)**: The Crow Bar's description explicitly mentions its use for opening crates and
  boarded doors/windows quicker, leveraging a "Breaking and Entering perk for quicker times." While the XML for the Crow
  Bar itself isn't provided to show its `TakeWithTool` or other properties, its description strongly implies it's
  intended to be used with `TakeAndReplace` blocks.
    * **Example (`Localization.txt`)**:
      ```
      CrowBar,items,Tool,KgNone,"Crow Bar",,,,,
      CrowBarDesc,items,Tool,KgNone,"Open up crates and boarded doors and windows quicker with the crow bar. Uses Breaking and Entering perk for quicker times.",,,,,
      ```

This `TakeAndReplace` system provides a flexible and powerful way for modders to define interactive environmental
elements that can be efficiently "harvested" by players, often providing a streamlined experience compared to simply
destroying blocks.