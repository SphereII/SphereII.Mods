Here is the documentation for the Harmony patches found in the `Documentation/Harmony/Animation/` folder:

## Animation Harmony Patches

These Harmony patches enhance and correct various animation-related behaviors for entities in the game, focusing on
avatar controllers, look-at functionality, and debugging.

### 1. `ModelBaseInitCommon.cs`

* **Patch Target**: `EModelBase.InitCommon`
* **Purpose**: This patch addresses a specific issue on dedicated servers. When an entity uses an external animation
  class (like `MecanimSDX`), the server may incorrectly assign a dummy avatar controller. This patch ensures that the
  correct `AvatarController` is assigned to the game object, preventing animation issues and reducing log warnings
  related to mismatched avatars.

### 2. `EModelBaseLookAtUpdate.cs`

* **Patch Target**: `EModelBase.LookAtUpdate`
* **Purpose**: This patch aims to fix an animation glitch often seen with NPCs, referred to as "head twist," which
  occurs when their head rotation doesn't correctly follow a target. For `EntityAliveSDX` entities (excluding those with
  the `UMA2` tag), it enhances their "look-at" behavior. Instead of always looking at the player, the entity will
  prioritize looking at its attack or revenge target if one exists and is visible. The patch ensures a smoother blending
  of the head rotation for a more natural appearance.

### 3. `AvatarControllerLateUpdate.cs`

* **Patch Target**: `AvatarZombieController.LateUpdate`
* **Purpose**: This patch includes debugging functionality related to entity movement. If the "EntitySpeedCheck" feature
  is enabled under `AdvancedTroubleshootingFeatures` in `blocks.xml`, this patch will log the zombie's forward and
  strafe speeds. Additionally, it helps to prevent animation glitches by clamping very small `strafe` speed values to
  zero and handles cases where the entity is flying.

### 4. `AvatarController.cs` (Multiple Patches)

This file contains several patches targeting the generic `AvatarController` and `AvatarZombieController` classes:

* **`AvatarControllerSetTrigger`**
    * **Patch Target**: `AvatarController.TriggerEvent(string)`
    * **Purpose**: This patch provides more flexibility for custom animators. When an animation trigger event occurs, it
      updates a `RandomIndex` integer (between 0 and 10) in the animator. It also applies a random integer value (0-10)
      to the specific `_property` that was triggered, allowing for varied animation responses. Debug logging for this is
      controlled by the "AnimatorMapper" feature.
* **`AvatarControllerSetCrouching`**
    * **Patch Target**: `AvatarController.SetCrouching`
    * **Purpose**: This patch ensures that the "IsCrouching" boolean parameter in the entity's animator is correctly set
      and synchronized, both locally and for remote players. This prevents visual discrepancies in the crouching
      animation.
* **`AvatarControllerUpdate`**
    * **Patch Target**: `AvatarZombieController.Update`
    * **Purpose**: For `EntityAliveSDX` and `EntityEnemySDX` types, this patch refines movement animation. If the
      `speedForward` or `speedStrafe` values are negligibly small (less than 0.01), they are reset to zero. This also
      ensures the "isMoving" animation boolean is set to `false` when the entity is essentially stationary, preventing
      subtle animation glitches.

### 5. `AnimatorMapper.cs`

* **Patch Target**: `AvatarAnimalController.LateUpdate`
* **Purpose**: This patch is a debugging tool. If the "EntitySpeedCheck" feature is enabled under
  `AdvancedTroubleshootingFeatures` in `blocks.xml`, this patch will aggressively log the forward and strafe speeds of
  `AvatarAnimalController` entities (animals). This is useful for troubleshooting animal animation and movement issues.

### 6. `AnimalAvatarController.cs`

* **Patch Target**: `AvatarAnimalController.assignBodyParts`
* **Purpose**: This patch addresses a common modding challenge where custom animals may have different bone naming
  conventions than the vanilla game. It attempts to intelligently locate and assign standard body parts (like "Hips", "
  Head", "LeftUpLeg", etc.) to the `AvatarAnimalController`. This helps ensure that custom animal models animate
  correctly even if their internal rig naming differs from expectations.