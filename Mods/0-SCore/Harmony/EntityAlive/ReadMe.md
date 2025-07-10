## EntityAlive Harmony Patches

These Harmony patches target various aspects of `EntityAlive` (and its derived classes like `EntityTrader` and custom
`SDX` entities) to enhance, fix, or extend their behaviors, animations, and interactions within the game.

### 1\. `ReplicateSpeed.cs`

* **Patch Target**: `EntityAlive.ReplicateSpeed`
* **Purpose**: This patch is crucial for ensuring smooth and accurate replication of an entity's speed across the
  network, particularly for custom entities utilizing custom EAI (Enemy AI) movement. It addresses potential
  desynchronization issues where a remote client might not accurately perceive the speed of an entity controlled by the
  server.

### 2\. `EntityTraderRektMute.cs`

* **Patch Target**: `EntityTrader.RektMute`
* **Purpose**: This patch alters the `PlayVoiceSetEntry` method within `EntityTrader` objects. It offers the capability
  to mute all traders, or specifically mute a trader Rekt voice lines, by utilizing the `MuteTraderAll` and
  `MuteTraderRekt` CVars respectively.

### 3\. `EntityFactory.cs`

* **Patch Target**: `EntityFactory.CreateEntity`
* **Purpose**: Provides a match for the SCore's Entity Types in a GetType() check, skipping the warning message about
  slow look ups.

### 4\. `EntityAliveUpdateBlockStatusEffect.cs`

* **Patch Target**: `EntityAlive.UpdateBlockStatusEffect`
* **Purpose**: This patch extends how entities react to their environment by checking for custom status effects (
  `SCoreBlockEffects`) based on the blocks they are standing on or in contact with.
* If the entity has this cvar set `UpdateBlockStatusEffect`, then the UpdateBlockStatusEffect will not be called, giving
  immunity to them for taking damage based ont he blocks they are standing on.

### 5\. `EntityAlivePatches.cs`

### 6\. `EntityAliveJiggleAdjustment.cs`

* **Patch Target**: `EntityAlive.StartJiggle`
* **Purpose**:
  If an entity possesses the "AlwaysJiggle" property, its jiggle animation will execute continuously, irrespective of
  its proximity to the player. Similarly, if an entity has the "NeverScaleAI" property, its AI behavior will maintain a
  consistent scale, neither scaling up nor down, regardless of its distance from the player.

### 7\. `AddScriptToTransform.cs`

* **Patch Target**: `Transform.AddScriptToTransform` (likely an SCore-defined extension method)
* **Purpose**: This patch automatically adds the `AnimationEventBridge` component to all newly spawned entities, a
  component essential for NPC functionalities. Additionally, it identifies all transforms named `CollisionCallForward`
  and attaches the `RootTransformRefEntity` component to their respective game objects. Finally, if the
  `ComponentMapper` setting within the Configuration Block is enabled, the patch will print the entire transform
  hierarchy to the log for debugging purposes.