Here is the documentation for the Harmony patches found in the `Documentation/Harmony/EntityMoveHelper/` folder:

## EntityMoveHelper Harmony Patches

These Harmony patches are designed to modify and enhance the movement behaviors of entities within the game,
particularly affecting how they move, stop, jump, and interact with blocks during movement.

### 1. `UpdateMoveHelper.cs`

* **Patch Target**: `EntityMoveHelper.UpdateMoveHelper`
* **Purpose**: This patch modifies the core `UpdateMoveHelper` method, which is responsible for controlling an entity's
  movement logic every frame. This prevents the NPCs from jumping, walking back to the same place, and jumping again.

### 2. `Stop.cs`

* **Patch Target**: `EntityMoveHelper.Stop`
* **Purpose**: This patch customizes what happens when an `EntityMoveHelper` attempts to stop an entity's movement. It
  prevents NPCs from sliding forward after being stopped.

### 4. `DigStart.cs`

* **Patch Target**: `EntityMoveHelper.DigStart`
* **Purpose**: This patch targets the `DigStart` method, which is invoked when an entity begins a digging action (e.g.,
  breaking blocks to create a path). This prevents NPCs from digging into the ground to get you.