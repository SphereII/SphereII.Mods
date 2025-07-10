Here are the corrected conceptual XML examples for each of the custom UAI Considerations, illustrating how they might be
used within a `<consideration>` tag in your `utilityai.xml` file.

**Note**: These are conceptual examples based on the C\# script's parsing logic and typical `7 Days to Die` modding
patterns. The exact context within a full `utilityai.xml` (e.g., as part of a `<task>` or `<action>`) is omitted for
brevity. For considerations that implicitly refer to "self" or "target" in their name (e.g., `SelfHealthSDX`,
`TargetDistanceSDX`), the AI system automatically determines the entity being evaluated.

### UAI Considerations

#### 1\. `UAIConsiderationCanHear`

Evaluates if the entity can hear its target, typically used for stealth or detection.

```xml

<consideration class="CanHear, SCore"/>
```

**Explanation**: Gives a score based on whether the entity can hear its target.

#### 2\. `UAIConsiderationCanSeeTarget`

Evaluates if the entity has a line of sight to its target.

```xml

<consideration class="CanSeeTarget, SCore"/>
```

**Explanation**: Gives a score based on whether the entity can see its target.

#### 3\. `UAIConsiderationCooldown`

Evaluates if a cooldown period for a specific action or state has elapsed.

```xml

<consideration class="Cooldown, SCore" cvar="myAbilityCooldown" cooldown_time="5"/>
```

**Explanation**: Returns a high score if `myAbilityCooldown` CVar has been off cooldown for at least 5 seconds.

#### 4\. `UAIConsiderationEnemyNear`

Evaluates if an enemy entity is within a specified proximity.

```xml

<consideration class="EnemyNear, SCore" range="15"/>
```

**Explanation**: Gives a score based on whether an enemy is within 15 units.

#### 5\. `UAIConsiderationFailOnDistanceToLeader`

Causes an action to fail if the entity is too far from its leader.

```xml

<consideration class="FailOnDistanceToLeader, SCore" max_distance="50"/>
```

**Explanation**: Causes the action to return a very low score (effectively failing) if the entity is more than 50 units
from its leader.

#### 6\. `UAIConsiderationHasHomePositioncs`

Evaluates if the entity has a defined home position.

```xml

<consideration class="HasHomePosition, SCore"/>
```

**Explanation**: Gives a score based on whether the entity has a home position set.

#### 7\. `UAIConsiderationHasInvestigatePosition`

Evaluates if the entity has a position marked for investigation.

```xml

<consideration class="HasInvestigatePosition, SCore"/>
```

**Explanation**: Gives a score based on whether the entity has an investigation position.

#### 8\. `UAIConsiderationHasOrder`

Evaluates if the entity has a specific order (e.g., guard, follow) issued to it.

```xml

<consideration class="HasOrder, SCore" order_type="Guard"/>
```

**Explanation**: Gives a score if the entity has the "Guard" order.

#### 9\. `UAIConsiderationHasPath`

Evaluates if the entity currently has a valid path to its destination.

```xml

<consideration class="HasPath, SCore"/>
```

**Explanation**: Gives a score if the entity currently possesses a valid path.

#### 10\. `UAIConsiderationHasPathingCode`

Evaluates if the entity has a specific `PathingCode` CVar set.

```xml

<consideration class="HasPathingCode, SCore" cvar_value="5" operator="EQ"/>
```

**Explanation**: Gives a score if the entity's `PathingCode` CVar is exactly 5.

#### 11\. `UAIConsiderationHasTarget`

Evaluates if the entity currently has any target assigned to it.

```xml

<consideration class="HasTarget, SCore"/>
```

**Explanation**: Gives a score if the entity has any active target.

#### 12\. `UAIConsiderationIsAlerted`

Evaluates if the entity is currently in an alerted state.

```xml

<consideration class="IsAlerted, SCore"/>
```

**Explanation**: Gives a score if the entity is currently alerted.

#### 13\. `UAIConsiderationIsBusy`

Evaluates if the entity is currently performing a task or is otherwise occupied.

```xml

<consideration class="IsBusy, SCore"/>
```

**Explanation**: Gives a score if the entity is currently busy.

#### 14\. `UAIConsiderationIsInside`

Evaluates if the entity is currently located indoors.

```xml

<consideration class="IsInside, SCore"/>
```

**Explanation**: Gives a score if the entity is currently indoors.

#### 15\. `UAIConsiderationIsNearFarm`

Evaluates if the entity is near a farm plot.

```xml

<consideration class="IsNearFarm, SCore" range="10"/>
```

**Explanation**: Gives a score if the entity is within 10 units of a farm plot.

#### 16\. `UAIConsiderationIsNearPlant`

Evaluates if the entity is near a plant (potentially for farming AI).

```xml

<consideration class="IsNearPlant, SCore" range="5"/>
```

**Explanation**: Gives a score if the entity is within 5 units of any plant.

#### 17\. `UAIConsiderationIsSleeping`

Evaluates if the entity is currently in a sleeping state.

```xml

<consideration class="IsSleeping, SCore"/>
```

**Explanation**: Gives a score if the entity is currently sleeping.

#### 18\. `UAIConsiderationIsTargeted`

Evaluates if the entity is currently being targeted by another entity.

```xml

<consideration class="IsTargeted, SCore"/>
```

**Explanation**: Gives a score if the entity is currently being targeted.

#### 19\. `UAIConsiderationMaxTask`

Evaluates if a specific task has reached its maximum execution count or duration.

```xml

<consideration class="MaxTask, SCore" task_name="HealSelf" max_count="3"/>
```

**Explanation**: Gives a score based on whether the "HealSelf" task has been executed more than 3 times.

#### 20\. `UAIConsiderationPathBlocked`

Evaluates if the entity's current path is blocked by an obstacle.

```xml

<consideration class="PathBlocked, SCore"/>
```

**Explanation**: Gives a score if the entity's current path is blocked.

#### 21\. `UAIConsiderationPathTarget`

Evaluates if the entity currently has a valid path to its target.

```xml

<consideration class="PathTarget, SCore"/>
```

**Explanation**: Gives a score if the entity has a valid path to its target.

#### 22\. `UAIConsiderationRandom`

Introduces a random chance for the consideration to return a specific score.

```xml

<consideration class="Random, SCore" chance="0.5"/>
```

**Explanation**: Has a 50% chance of returning a high score, adding unpredictability.

#### 23\. `UAIConsiderationSelfAttackTarget`

Evaluates properties or states related to the entity's own current attack target.

```xml

<consideration class="SelfAttackTarget, SCore" target_alive="false"/>
```

**Explanation**: Gives a score if the entity's attack target is not alive.

#### 24\. `UAIConsiderationSelfHasBuff`

Evaluates if the entity itself has a specific buff active.

```xml

<consideration class="SelfHasBuff, SCore" buff="buffBleeding"/>
```

**Explanation**: Gives a score if the entity has `buffBleeding`.

#### 25\. `UAIConsiderationSelfHasCVar`

Evaluates if the entity itself has a specific CVar set, and potentially its value.

```xml

<consideration class="SelfHasCVar, SCore" cvar="myEntityFlag" value="1" operator="EQ"/>
```

**Explanation**: Gives a score if `myEntityFlag` CVar on the entity is 1.

#### 26\. `UAIConsiderationSelfHasItem`

Evaluates if the entity itself has a specific item in its inventory.

```xml

<consideration class="SelfHasItem, SCore" item="meleeToolAxeT0StoneAxe" count="1"/>
```

**Explanation**: Gives a score if the entity has at least 1 `meleeToolAxeT0StoneAxe`.

#### 27\. `UAIConsiderationSelfHealthSDX`

Evaluates the entity's own health, often relative to maximum health.

```xml

<consideration class="SelfHealthSDX, SCore" health_threshold="0.5" operator="LTE"/>
```

**Explanation**: Gives a score if the entity's health is Less Than or Equal to 50%.

#### 28\. `UAIConsiderationTargetDistanceSDX`

Evaluates the distance to the entity's target.

```xml

<consideration class="TargetDistanceSDX, SCore" max_distance="20"/>
```

**Explanation**: Gives a score if the target is within 20 units.

#### 29\. `UAIConsiderationTargetFactionStandingSDX`

Evaluates the entity's relationship value with the target's faction.

```xml

<consideration class="TargetFactionStandingSDX, SCore" faction="traders" value="0" operator="LT"/>
```

**Explanation**: Gives a score if the target's faction standing with "traders" is Less Than 0.

#### 30\. `UAIConsiderationTargetHasBuff`

Evaluates if the target entity has a specific buff active.

```xml

<consideration class="TargetHasBuff, SCore" buff="buffStunned"/>
```

**Explanation**: Gives a score if the target has `buffStunned`.

#### 31\. `UAIConsiderationTargetHasCVar`

Evaluates if the target entity has a specific CVar set, and potentially its value.

```xml

<consideration class="TargetHasCVar, SCore" cvar="isFriendlyNPC" value="1" operator="EQ"/>
```

**Explanation**: Gives a score if `isFriendlyNPC` CVar on the target is 1.

#### 32\. `UAIConsiderationTargetHasTags`

Evaluates if the target entity has specific tags.

```xml

<consideration class="TargetHasTags, SCore" tags="animal,hostile"/>
```

**Explanation**: Gives a score if the target has "animal" or "hostile" tags.

#### 33\. `UAIConsiderationTargetHealthSDX`

Evaluates the health of the target entity, often relative to its maximum health.

```xml

<consideration class="TargetHealthSDX, SCore" health_threshold="0.2" operator="LT"/>
```

**Explanation**: Gives a score if the target's health is Less Than 20%.

#### 34\. `UAIConsiderationTargetIsAlive`

Evaluates if the target entity is currently alive.

```xml

<consideration class="TargetIsAlive, SCore"/>
```

**Explanation**: Gives a score if the target entity is alive.

#### 35\. `UAIConsiderationTargetLeader`

Evaluates properties or states related to the target being a leader.

```xml

<consideration class="TargetLeader, SCore"/>
```

**Explanation**: Gives a score if the target is a leader.

#### 36\. `UAIConsiderationTargetSameClass`

Evaluates if the target entity belongs to the same class as the evaluating entity.

```xml

<consideration class="TargetSameClass, SCore"/>
```

**Explanation**: Gives a score if the target's entity class is the same as the evaluating entity's.

#### 37\. `UAIConsiderationTargetTileEntity`

Evaluates properties or states related to a target being a TileEntity.

```xml

<consideration class="TargetTileEntity, SCore" tile_entity_type="Loot"/>
```

**Explanation**: Gives a score if the target is a TileEntity of type "Loot".

#### 38\. `UAIConsiderationTargetWeaponRange`

Evaluates if the target is within the entity's weapon range.

```xml

<consideration class="TargetWeaponRange, SCore"/>
```

**Explanation**: Gives a score if the target is within the entity's weapon range.

#### 39\. `UAIConsiderationTrapped`

Evaluates if the entity is currently trapped.

```xml

<consideration class="Trapped, SCore"/>
```

**Explanation**: Gives a score if the entity is currently trapped.