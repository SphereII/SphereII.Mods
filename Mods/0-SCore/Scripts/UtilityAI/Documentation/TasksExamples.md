The `UAITaskApproachSpotSDX` is a Utility AI task designed to direct an entity towards a specific designated spot, typically an "investigate position". It serves as a replacement for the vanilla `EAIApproachSpot` task, offering enhanced functionality and integration with SCore features.

## Functionality

This task primarily manages the entity's movement towards its `InvestigatePosition`. Upon starting, it ensures the entity has an investigate position, handles sleeping states, sets the entity to a crouching state, and disables block breaking during movement. In its update loop, it continuously checks for closed doors (and potentially opens them via `SCoreUtils.CheckForClosedDoor`), detects if the path is blocked, and determines if the destination has been reached. If the path is blocked or the destination is reached, the task clears the investigate position and stops.

### Properties

You can configure `UAITaskApproachSpotSDX` within your `utilityai.xml` file. As this task inherits from `UAITaskBase`, it will use common task properties, but its specific behavior is primarily determined by its internal logic.

* **`class`**: `ApproachSpotSDX, SCore` - Specifies that this is an SCore custom task.

## XML Example

Here's a conceptual XML example of how to define `UAITaskApproachSpotSDX` within an `<action>` in your `utilityai.xml`:

```xml
<action name="ApproachInvestigateSpot" weight="1">
    <task class="ApproachSpotSDX, SCore"/>
    <consideration class="HasInvestigatePosition, SCore" />
    <consideration class="TargetDistanceSDX, SCore" max_distance="50" invert="true" />
</action>
```

**Explanation**: This action, when chosen by the AI, will command the entity to move towards its set `InvestigatePosition`. The considerations would ensure it only does so if an investigation position exists and the entity is not already at it.

-----

The `UAITaskMoveToAttackTargetSDX` is a Utility AI task that directs an entity to move towards its current attack target and then engage it in combat.

## Functionality

This task first commands the entity to move towards its designated attack target. The core attack logic is executed within the `Stop` method, which is triggered when the entity reaches or is sufficiently close to its target. Upon stopping, the task checks if the target is within the range of the entity's current holding item (weapon) and if the entity has line of sight to the target. If these conditions are met, the entity will perform an attack; otherwise, it will ensure the target is set as its attack target with a specified timeout.

### Parameters

You can configure `UAITaskMoveToAttackTargetSDX` within your `utilityai.xml` file using the following parameters:

* **`class`**: `MoveToAttackTargetSDX, SCore` - Specifies that this is an SCore custom UAI task.
* **`action_index`**: `int` - Specifies the index of the `ItemAction` (usually a weapon's attack action) to be used for the attack. Defaults to `0`.
* **`target_timeout`**: `int` - Sets how long (in game ticks) the target remains the attack target. Defaults to `20` ticks.

## XML Example

Here's a conceptual XML example of how to define `UAITaskMoveToAttackTargetSDX` within an `<action>` in your `utilityai.xml`:

```xml
<action name="MoveAndAttackEnemy" weight="2">
    <task class="MoveToAttackTargetSDX, SCore" action_index="0" target_timeout="60"/>
    <consideration class="TargetDistance" min="1" max="25" />
    <consideration class="TargetIsAlive" />
</action>
```

**Explanation**: This action commands the entity to move towards its target. Once within range, it will attempt to attack using its primary attack action (`action_index="0"`) and maintain the target for 60 ticks. The considerations ensure it only moves and attacks if the target is alive and within a certain distance.

-----

The `UAITaskAttackTargetEntitySDX` is a Utility AI task that commands an entity to attack its current target [cite: uploaded:UAITaskAttackTargetEntitySDX.cs]. It extends the vanilla `UAITaskAttackTargetEntity` to provide more granular control over attack behaviors, including specific attack actions, animation triggers, and handling of various target types (entities and blocks).

## Functionality

This task manages the entity's attack sequence. Upon starting, it ensures the entity is not crouching, sets its look position to the target (head or center of block), and rotates the entity to face the target. It then continuously updates its state, checking for conditions like target death, line of sight, and facing angle before initiating an attack. It also incorporates a "buff throttle" to prevent attacks during certain buff states (e.g., reloading) [cite: uploaded:UAITaskAttackTargetEntitySDX.cs].

The task supports different attack actions based on `action_index`:

* `action_index="0"`: Performs a standard attack.
* `action_index="1"`: Performs a "use" action (e.g., for ranged weapons that use a "use" action to fire). It also triggers a "WeaponFire" animation event for ranged actions.
* Other `action_index` values: If the entity is an `EntityAliveSDX`, it will attempt to execute a custom action at that index [cite: uploaded:UAITaskAttackTargetEntitySDX.cs].

### Parameters

You can configure `UAITaskAttackTargetEntitySDX` within your `utilityai.xml` file using the following parameters [cite: uploaded:UAITaskAttackTargetEntitySDX.cs]:

* **`class`**: `AttackTargetEntitySDX, SCore` - Specifies that this is an SCore custom UAI task.
* **`action_index`**: `int` - Specifies the index of the `ItemAction` (from the entity's holding item) to be used for the attack. Defaults to `0` (attack action) [cite: uploaded:UAITaskAttackTargetEntitySDX.cs].
* **`buff_throttle`**: `string` - The name of a buff that, if active on the entity, will prevent it from attacking. Defaults to `buffReload2` [cite: uploaded:UAITaskAttackTargetEntitySDX.cs].
* **`target_timeout`**: `int` - Sets how long (in game ticks) the target remains the attack target. Defaults to `20` ticks. This is used when setting the revenge target [cite: uploaded:UAITaskAttackTargetEntitySDX.cs].

## XML Example

Here's a conceptual XML example of how to define `UAITaskAttackTargetEntitySDX` within an `<action>` in your `utilityai.xml` [cite: uploaded:utilityai.xml]:

```xml
<action name="RangedAttackEnhanced" weight="3">
    <task class="AttackTargetEntitySDX, SCore" action_index="1" buff_throttle="buffReloading" target_timeout="100"/>
    <consideration class="TargetVisible"/>
    <consideration class="TargetType" type="EntityNPC, EntityZombie"/>
    <consideration class="TargetFactionStanding" flip_y="true" max="127"/>
    <consideration class="TargetDistance" min="1" max="25"/>
</action>
```

**Explanation**: This action commands the entity to perform a ranged attack (`action_index="1"`) on its target. It will not attack if the `buffReloading` is active. The target will remain the attack target for 100 ticks. The considerations ensure the target is visible, of the correct type, hostile, and within range.

-----

