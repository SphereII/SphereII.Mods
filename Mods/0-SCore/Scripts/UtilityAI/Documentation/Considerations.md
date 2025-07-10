UAI (Utility AI) Considerations are components of the Utility AI system that evaluate various factors and conditions to determine the best course of action for an entity. Each consideration returns a score, and the highest-scoring action is chosen.

Here's an overview of the custom UAI Considerations available:

* **UAIConsiderationCanHear**: Evaluates if the entity can hear its target, typically used for stealth or detection.
* **UAIConsiderationCanSeeTarget**: Evaluates if the entity has a line of sight to its target.
* **UAIConsiderationCooldown**: Evaluates if a cooldown period for a specific action or state has elapsed.
* **UAIConsiderationEnemyNear**: Evaluates if an enemy entity is within a specified proximity.
* **UAIConsiderationFailOnDistanceToLeader**: Causes an action to fail if the entity is too far from its leader.
* **UAIConsiderationHasHomePositioncs**: Evaluates if the entity has a defined home position.
* **UAIConsiderationHasInvestigatePosition**: Evaluates if the entity has a position marked for investigation.
* **UAIConsiderationHasOrder**: Evaluates if the entity has a specific order (e.g., guard, follow) issued to it.
* **UAIConsiderationHasPath**: Evaluates if the entity currently has a valid path to its destination.
* **UAIConsiderationHasPathingCode**: Evaluates if the entity has a specific `PathingCode` CVar set, often used to guide AI behavior along predefined paths.
* **UAIConsiderationHasTarget**: Evaluates if the entity currently has any target assigned to it.
* **UAIConsiderationIsAlerted**: Evaluates if the entity is currently in an alerted state.
* **UAIConsiderationIsBusy**: Evaluates if the entity is currently performing a task or is otherwise occupied.
* **UAIConsiderationIsInside**: Evaluates if the entity is currently located indoors.
* **UAIConsiderationIsNearFarm**: Evaluates if the entity is near a farm plot.
* **UAIConsiderationIsNearPlant**: Evaluates if the entity is near a plant (potentially for farming AI).
* **UAIConsiderationIsSleeping**: Evaluates if the entity is currently in a sleeping state.
* **UAIConsiderationIsTargeted**: Evaluates if the entity is currently being targeted by another entity.
* **UAIConsiderationMaxTask**: Evaluates if a specific task has reached its maximum execution count or duration.
* **UAIConsiderationPathBlocked**: Evaluates if the entity's current path is blocked by an obstacle.
* **UAIConsiderationPathTarget**: Evaluates if the entity currently has a valid path to its target.
* **UAIConsiderationRandom**: Introduces a random chance for the consideration to return a specific score, adding unpredictability to AI decisions.
* **UAIConsiderationSelfAttackTarget**: Evaluates properties or states related to the entity's own current attack target.
* **UAIConsiderationSelfHasBuff**: Evaluates if the entity itself has a specific buff active.
* **UAIConsiderationSelfHasCVar**: Evaluates if the entity itself has a specific CVar set, and potentially its value.
* **UAIConsiderationSelfHasItem**: Evaluates if the entity itself has a specific item in its inventory.
* **UAIConsiderationSelfHealthSDX**: Evaluates the entity's own health, often relative to maximum health.
* **UAIConsiderationTargetDistanceSDX**: Evaluates the distance to the entity's target.
* **UAIConsiderationTargetFactionStandingSDX**: Evaluates the entity's relationship value with the target's faction.
* **UAIConsiderationTargetHasBuff**: Evaluates if the target entity has a specific buff active.
* **UAIConsiderationTargetHasCVar**: Evaluates if the target entity has a specific CVar set, and potentially its value.
* **UAIConsiderationTargetHasTags**: Evaluates if the target entity has specific tags.
* **UAIConsiderationTargetHealthSDX**: Evaluates the health of the target entity, often relative to its maximum health.
* **UAIConsiderationTargetIsAlive**: Evaluates if the target entity is currently alive.
* **UAIConsiderationTargetLeader**: Evaluates properties or states related to the target being a leader.
* **UAIConsiderationTargetSameClass**: Evaluates if the target entity belongs to the same class as the evaluating entity.
* **UAIConsiderationTargetTileEntity**: Evaluates properties or states related to a target being a TileEntity.
* **UAIConsiderationTargetWeaponRange**: Evaluates if the target is within the entity's weapon range.
* **UAIConsiderationTrapped**: Evaluates if the entity is currently trapped.