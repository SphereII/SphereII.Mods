## Block-Related Harmony Patches

These Harmony patches enhance, fix, and extend various functionalities related to blocks in the game, covering
placement, collision, animation, and error handling. For most patches, the changes are implemented at the code level and
do not have direct XML configuration examples.

### 1\. `TerrainAlignmentUtilsPatch.cs`

* **Patch Target**: `TerrainAlignmentUtils.GetAlignedTransform`
* **Purpose**: This patch likely modifies the game's default terrain alignment logic. It aims to improve how blocks or
  objects align themselves with the ground, potentially resolving issues with rotation or placement on uneven or complex
  terrain.
* **XML Example**: This is a code-level patch that modifies game behavior and does not have a direct XML configuration
  example in the provided files.

### 2\. `Particles.cs`

* **Patch Target**: `Block.OnUpdateTick`
* **Purpose**: This patch is a key component of the "Particles On Blocks" feature. It hooks into the `OnUpdateTick`
  method of blocks to manage and display particle effects. It enables custom particles to be shown on blocks over time,
  based on specific conditions.
* **XML Example**: Particles on blocks are configured directly within a block's definition.
  ```xml
  <block name="blah" >
      <property class ="Particles" >
         <property name="OnSpawn" value="unitybundle,unitybundle2"/>

         <property name="OnSpawn_pine_forest" value="unitybundle,unitybundle2"/>
         <property name="OnSpawnProb" value="0.2"/>

         <property name="DamagedParticle" value="unitybundle,unitybundle2"/>
         <property name="DamagedParticle_snow" value="unitybundle,unitybundle2"/>
         <property name="OnDamagedProb" value="0.2"/>
      </property>
  </block>
  ```
  **Explanation**: This example shows how to add a `Particles` class property to any block. `OnSpawn` triggers particles
  when the block is created, `DamagedParticle` on damage. Biome-specific variants (`OnSpawn_pine_forest`,
  `DamagedParticle_snow`) and probabilities (`OnSpawnProb`, `OnDamagedProb`) can also be set.

### 3\. `OnEntityCollidedWithBlock.cs`

* **Patch Target**: `Block.OnEntityCollidedWithBlock`
* **Purpose**: This patch extends or modifies the game's default behavior when an entity collides with a block. This
  allows modders to implement custom effects, apply damage, trigger events, or define unique interactions that occur
  upon collision between an entity and a block.
* **XML Example**: This is a code-level patch that extends collision behavior and does not have a direct XML
  configuration example in the provided files. Its effects would be triggered by existing game events.

### 4\. `OnBlockAdded.cs`

* **Patch Target**: `Block.OnBlockAdded`
* **Purpose**: This patch modifies the game's logic when a new block is placed or loaded into the world. It specifically
  checks for a custom property (`SCoreCustomBlock`) on the block. If found, it attempts to instantiate associated custom
  logic, enabling dynamic behavior or specialized functionality for custom blocks as soon as they are added.
* **XML Example**: This patch enables custom block behaviors defined in C\# and does not have a direct XML configuration
  example for the patch itself in the provided files. Custom blocks would define their `Class` attribute to use
  `SCoreCustomBlock`.

### 5\. `GroundAlignPatch.cs`

* **Patch Target**: `BlockShape.SetRotationBasedOnGround`
* **Purpose**: This patch focuses on how block shapes orient themselves relative to the ground. It likely provides fixes
  or enhancements to the rotation logic, ensuring that blocks are correctly aligned when placed on various surfaces,
  improving visual consistency and potentially placement mechanics.
* **XML Example**: This is a code-level patch that modifies alignment behavior and does not have a direct XML
  configuration example in the provided files.

### 6\. `ElectricityMultiDimFix.cs`

* **Patch Target**: `TileEntityPowered.GetPowerItemCount`
* **Purpose**: This patch addresses potential issues related to electricity within the game, particularly concerning
  multi-dimensional blocks or complex power setups. It modifies how the count of power items is retrieved, aiming to
  ensure accurate power calculations and distribution for powered tile entities.
* **XML Example**: This is a code-level patch that fixes electricity calculation and does not have a direct XML
  configuration example in the provided files.

### 7\. `ChunkPoolBlockEntityTransform.cs`

* **Patch Target**: `Chunk.PoolBlockEntityTransform`
* **Purpose**: This patch incorporates debugging and error handling for block entity transforms. If the
  `EnablePoolBlockEntityTransformCheck` and `LogPoolBlockEntityTransformCheck` features are enabled under the
  `ErrorHandling` class in `blocks.xml`, this patch will check for null or invalid block entity transforms and log
  warnings, helping to identify and troubleshoot issues with block rendering or functionality.
* **XML Example**: While this patch doesn't have its own direct XML, its behavior is controlled by settings in the
  `ErrorHandling` class within `blocks.xml`:
  ```xml
  <property class="ErrorHandling">
      <property name="EnablePoolBlockEntityTransformCheck" value="false"/>
      <property name="LogPoolBlockEntityTransformCheck" value="false"/>
  </property>
  ```
  **Explanation**: Setting `EnablePoolBlockEntityTransformCheck` to `true` enables the check for null transforms.
  Setting `LogPoolBlockEntityTransformCheck` to `true` will make it log warnings.

### 8\. `BlockWorkarounds.cs`

* **Patch Target**: `Block.canPlaceBlockAt` (multiple overloads)
* **Purpose**: This file contains various workarounds aimed at fixing specific bugs or undesirable behaviors related to
  block placement. One notable workaround addresses a null reference bug that could occur with `BlockRotationData` when
  attempting to place certain blocks, improving the stability of the placement system.
* **XML Example**: This is a code-level patch that provides bug fixes for block placement and does not have a direct XML
  configuration example in the provided files.

### 9\. `BlockEntityDataGetRenderers.cs`

* **Patch Target**: `BlockEntityData.GetRenderers`
* **Purpose**: This patch specifically targets a common null reference bug that can occur when the game attempts to
  retrieve renderers associated with block entity data. By patching this method, it ensures that renderer retrieval is
  more robust, preventing crashes and improving the overall stability of block rendering.
* **XML Example**: While this patch doesn't have its own direct XML, its behavior is controlled by a setting in the
  `ErrorHandling` class within `blocks.xml`:
  ```xml
  <property class="ErrorHandling">
      <property name="BlockEntityDataGetRenderers" value="false"/>
  </property>
  ```
  **Explanation**: Setting `BlockEntityDataGetRenderers` to `true` activates this protection against null reference
  errors during renderer retrieval.

### 10\. `BlockCanPlaceBlockAt.cs`

* **Patch Target**: `Block.canPlaceBlockAt` (different overloads)
* **Purpose**: This patch works in conjunction with features like "Anti-Nerd Pole" (`AdvancedPlayerFeatures` in
  `blocks.xml`). It modifies the conditions under which a block can be placed at a specific location, particularly
  preventing scenarios like "nerd-poling" where players jump and place blocks directly underneath themselves for rapid
  vertical ascent.
* **XML Example**: This patch contributes to a player feature configured in `blocks.xml` under `AdvancedPlayerFeatures`:
  ```xml
  <property class="AdvancedPlayerFeatures">
      <property name="AntiNerdPole" value="false"/>
  </property>
  ```
  **Explanation**: Setting `AntiNerdPole` to `true` activates the prevention of "nerd-poling," which this patch helps
  enforce.