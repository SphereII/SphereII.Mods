The NPC Features in this mod introduce a wide array of enhancements and modifications to non-player characters,
encompassing their behaviors, interactions, appearances, and underlying game mechanics. This includes new entity types,
advanced AI, dialogue systems, and various quality-of-life improvements.

-----

## NPC Features: Harmony Patches

These Harmony patches specifically modify various aspects of NPC behavior, appearance, and interaction within the game.

### 1\. `EntityTraderEModalSetLookAt.cs`

* **Patch Target**: `EntityTrader.EModelSetLookAt`
* **Purpose**: This patch is designed to control or adjust how trader NPCs look at specific targets. This could be used
  to fix unnatural head/body rotations, ensure traders correctly focus on the player during dialogue, or prevent them
  from looking at irrelevant objects.

### 2\. `EntityStatsInit.cs`

* **Patch Target**: `EntityStats.Init`
* **Purpose**: This patch provides a framework to apply custom initial properties or states to entities upon their
  initialization. It adds Stamina property to NPCs.

### 3\. `EntityNPCSpeedFix.cs`

* **Patch Target**: `EntityNPC.SetSpeed`
* **Purpose**: This patch aims to resolve issues related to NPC movement speed. It checks if the `NPCSpeedFix` feature
  is enabled (in `blocks.xml` under `AdvancedNPCFeatures`). If so, it adjusts how NPC speeds are set, ensuring they move
  correctly and consistently, addressing potential animation or pathing discrepancies.

### 4\. `EntityNPCMakeVulnerable.cs`

* **Patch Target**: `EntityNPC.IsVulnerable`
* **Purpose**: This patch allows modders to control the vulnerability of NPC traders. By default, traders are often
  invulnerable. This patch enables a setting (`MakeTraderVulnerable` in `blocks.xml` under `AdvancedNPCFeatures`) that,
  when set to `true`, makes trader NPCs susceptible to damage and death.
* **XML Example**: This patch's behavior is controlled by a setting in the `AdvancedNPCFeatures` class within
  `blocks.xml`:
  ```xml
  <property class="AdvancedNPCFeatures">
      <property name="MakeTraderVulnerable" value="false"/>
  </property>
  ```
  **Explanation**: Setting `MakeTraderVulnerable` to `true` will allow trader NPCs to take damage and be killed.

### 5\. `EntityNPCJumpHeight.cs`

* **Patch Target**: `EntityNPC.GetJumpStrength`
* **Purpose**: This patch customizes the jump height of NPC entities. It allows for the jump strength to be adjusted
  dynamically or through configuration, providing modders with control over how high NPCs can jump. This can be useful
  for balancing NPC movement or designing custom parkour-capable NPCs.
* **XML Example**:
  ```xml
  <entityclass name="MyNPC">
      <property name="JumpHeight" value="10"/>
  </entityclass>
  ```

### 6\. `EntityAliveStartingEquipment.cs`

* **Patch Target**: `EntityAlive.SetCurrentWeapon`, `EntityAlive.Equipment.SetItem`
* **Purpose**: This patch addresses how entities are equipped with starting items, particularly for custom entities or
  NPCs with specific equipment needs. It modifies how entities' current weapons are set and how items are placed in
  their equipment slots, ensuring custom starting gear is correctly applied.

### 7\. `EntityAlivePatcherAwardKill.cs`

* **Patch Target**: `EntityAlive.AwardKill`
* **Purpose**: This patch extends the `AwardKill` method, which is invoked when an entity is killed and rewards (e.g.,
  XP) are distributed. It gives Kill credit to the NPC.

### 8\. `EntityAlivePatcher.cs`

* **Patch Target**: `EntityAlive.OnEntityDeath`, `EntityAlive.OnUpdateLive`
* **Purpose**: This file contains general patches for `EntityAlive` entities. It allows for custom behaviors or triggers
  on entity death and during their live updates. This serves as a versatile hook for modders to inject custom logic into
  the lifecycle of any living entity.

### 9\. `EntityAliveGroundDetection.cs`

* **Patch Target**: `EntityAlive.IsOnGround`
* **Purpose**: This patch modifies the `IsOnGround` method, which determines if an `EntityAlive` is currently touching
  the ground. This could be used to refine ground detection for custom entities, adjust behavior for specific terrain
  types, or fix discrepancies in how "on ground" status is determined.

### 10\. `DynamicPrefabGetTraderArea.cs`

* **Patch Target**: `DynamicPrefab.GetTraderArea`
* **Purpose**: This patch will prevent a crash when the quest system is checking to see if the POI is near a trader.
  NPCs are quasi-traders, but not for this check.

### 11\. `DynamicMusicPlayerTracking.cs`

* **Patch Target**: `DynamicMusicPlayer.Update`
* **Purpose**: Sometimes NPCs will be picked up when the DynamicMusic is scanning for EntityTraders, and it'll
  pick an invalid trader (aka, a non-trader NPC.). This patch checks for it.

### 12\. `AllowStashButtonsForNPCs.cs`

The `AllowStashButtonsForNPCs` feature introduces the functionality for NPCs to have and utilize "stash buttons" within
their user interface, similar to how players might interact with their inventory or containers.

## Functionality

This feature is implemented through a Harmony patch on relevant UI or entity interaction methods. Its purpose is to
enable UI elements (referred to as "stash buttons") that allow for various interactions with NPCs, such as transferring
items to/from them, managing their inventory, or other stash-like functions. This enhances the player's ability to
manage and interact with NPCs, especially companions or traders, by providing a more direct UI method for item handling.

* **Patch Target**: (Implicit, related to UI or NPC inventory interaction)
* **Purpose**: To make "stash buttons" (likely for inventory management or quick transfers) visible and functional when
  interacting with NPCs.

