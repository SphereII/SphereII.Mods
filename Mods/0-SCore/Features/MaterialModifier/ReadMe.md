The Material Modifier feature provides powerful capabilities to dynamically change the visual appearance of entities by
altering their materials. This can be used for various effects, such as adapting a creature's look to its environment or
changing an entity's appearance based on specific in-game events.

## 1\. Alternate Materials (`AltMats` Property)

The `AltMats` property allows you to define a list of alternative materials for an entity. This is typically used to
enable variations in an entity's appearance, which can be dynamically applied by the game based on context (e.g., biome,
temperature, or other conditions, though the exact trigger isn't specified in this snippet).

* **XML Usage Example:**

  ```xml
  <append xpath="entity_classes/entity_class[@name='zombieArlene']">
      <property name="AltMats" value="#@modfolder:Resources/ww_zeds_1.unity3d?HD_Arlene_COLD,#@modfolder:Resources/ww_zeds_1.unity3d?HD_Arlene_FROZEN,#@modfolder:Resources/ww_zeds_1.unity3d?HD_Arlene_SNOW HIGH,#@modfolder:Resources/ww_zeds_1.unity3d?HD_Arlene_SNOW LOW"/>
  </append>

  <append xpath="entity_classes/entity_class[@name='zombieArleneFeral']">
      <property name="AltMats" value="#@modfolder:Resources/ww_zeds_1.unity3d?HD_Arlene_COLD,#@modfolder:Resources/ww_zeds_1.unity3d?HD_Arlene_FROZEN,#@modfolder:Resources/ww_zeds_1.unity3d?HD_Arlene_SNOW HIGH,#@modfolder:Resources/ww_zeds_1.unity3d?HD_Arlene_SNOW LOW"/>
  </append>
  ```

* **Property Details:**

    * **`AltMats`**: The `value` is a comma-separated list of material paths. Each path typically points to a Unity3D
      asset containing the material and the specific material name within that asset (e.g.,
      `#@modfolder:Resources/assetBundleName.unity3d?MaterialName`).

## 2\. Replace Material (`MinEventActionReplaceMaterial`)

The `MinEventActionReplaceMaterial` allows you to change specific materials on an entity in response to a triggered
event. This is useful for visual transformations, such as a zombie's appearance changing when it becomes radiated, or
for applying dynamic visual effects.

* **Functionality**: This `MinEventAction` is typically used within an `<effect_group>` and triggered by a game event (
  e.g., `onSelfFirstSpawn`). When triggered, it replaces a specified `target_material_name` on the entity with a new
  `replace_material`.

* **XML Usage Example:**

  ```xml
  <append xpath="entity_classes/entity_class[@name='zombieArleneRadiated']">
      <effect_group name="ReplaceMaterial">
          <triggered_effect trigger="onSelfFirstSpawn" action="ReplaceMaterial, SCore" target_material_name="HD_Arlene_Radiated" replace_material="#@modfolder:Resources/ww_zeds_1.unity3d?HD_Arlene_Rad"/>
          <triggered_effect trigger="onSelfFirstSpawn" action="ReplaceMaterial, SCore" target_material_name="HD_Arlene_Radiated2" replace_material="#@modfolder:Resources/ww_zeds_1.unity3d?HD_Arlene_Rad"/>
          <triggered_effect trigger="onSelfFirstSpawn" action="ReplaceMaterial, SCore" target_material_name="HD_Arlene_Radiated3" replace_material="#@modfolder:Resources/ww_zeds_1.unity3d?HD_Arlene_Rad"/>
      </effect_group>
  </append>
  ```

* **Properties for `MinEventActionReplaceMaterial`**:

    * **`trigger`**: (e.g., `onSelfFirstSpawn`) Specifies the event that causes the material replacement to occur.
    * **`action`**: `ReplaceMaterial, SCore` - Specifies the action to perform.
    * **`target_material_name` (string)**: The name of the material on the entity that will be replaced.
    * **`replace_material` (string)**: The path to the new material asset, including the Unity3D bundle and material
      name (e.g., `#@modfolder:Resources/assetBundleName.unity3d?NewMaterialName`).