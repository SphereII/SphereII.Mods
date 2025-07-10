The `utilityai.xml` file defines the Utility AI (UAI) packages, which dictate the behavior of various entities, including zombies and NPCs. It uses a system of "actions" (decisions) evaluated by "considerations" to choose the most appropriate "task" to perform.

## Structure of `utilityai.xml`

The file is organized into `<ai_packages>`, each defining a distinct AI profile that can be assigned to different entity types.

* **`<ai_packages>`**: The root element containing all defined AI profiles.
* **`<ai_package name="...">`**: Defines a specific AI profile (e.g., "Zombie\_Dumb", "Human Basic", "Human Melee", "Human Ranged"). Entities assigned this package will follow its defined behaviors.
* **`<action name="..." weight="...">`**: Represents a major decision or behavioral routine within an AI package. Actions are evaluated, and the one with the highest score is chosen.
    * `name`: A descriptive name for the action.
    * `weight`: A multiplier applied to the final score of the action after all its considerations are evaluated.
* **`<task class="...">`**: Specifies the actual behavior to be executed if its parent `<action>` is chosen.
    * `class`: The C\# class defining the UAI task (e.g., `Wander`, `MoveToTarget`, `AttackTargetEntity`).
    * Optional parameters like `run="true/false"` can modify the task's behavior.
* **`<consideration class="..." [parameters] />`**: Evaluates specific conditions to contribute to an action's score. Consideration scores range from 0 to 1.
    * `class`: The C\# class defining the UAI consideration (e.g., `SelfHealth`, `TargetDistance`).
    * `invert`: `[true/false]` - If `true`, inverts the output value of the consideration.
    * `min`/`max`: Define a range for scoring (e.g., `min="10" max="20"` for `TargetDistance`).
    * `flip_y`: `[true/false]` - Often used to invert the scoring based on a range.
    * `type`: Filters targets by entity type (e.g., `type="Block"` or `type="EntityNPC, EntityZombie"`).
    * `action_index`: Used by attack tasks to specify which attack action (from `entity_classes.xml`) to use (e.g., `action_index="0"`).

## AI Packages Overview

The provided `utilityai.xml` defines several AI packages, each tailored for different entity types and combat styles:

### Zombie\_Dumb

A basic AI package for zombies, focusing on simple movement and attack behaviors.

* **Wander**: Moves the zombie randomly.
* **MoveToWaypoint**: Moves towards a set waypoint or block.
* **MoveToEnemy**: Approaches an enemy, differentiating between walking and running based on distance.
* **MeleeAttackEntity**: Performs a melee attack against an entity.
* **MeleeAttackBlock**: Performs a melee attack against a block, often when its path is blocked.

### Human Basic

A general AI package for human-like entities, including basic movement and fleeing behavior.

* **Wander**: Moves the entity randomly.
* **MoveToWaypoint**: Moves towards a set waypoint.
* **Flee**: Attempts to escape from a target when health is low or the target is too close.

### Human Melee

An AI package for human-like entities that prefer melee combat.

* **MoveToEnemy**: Approaches an enemy, differentiating between walking and running based on distance.
* **MeleeAttack**: Performs a melee attack against an entity.

### Human Ranged

An AI package for human-like entities that prefer ranged combat.

* **MoveToEnemy**: Approaches an enemy, maintaining a specific distance for ranged attacks.
* **Flee**: Attempts to escape from a target when it's too close.
* **RangedAttack**: Performs a ranged attack against a visible target within range.

## Full XML Content

```xml
<utility_ai>
	<ai_packages>
		<ai_package name="Zombie_Dumb">
			<action name="Wander" weight="1">
				<task class="Wander"/>
			</action>
			
			<action name="MoveToWaypoint" weight="2">
				<task class="MoveToTarget" run="false"/>
				
				<consideration class="TargetType" type="Block"/>
				<consideration class="TargetDistance" min="2" max="4"/>
				<consideration class="TargetVisible" flip_y="true"/>
			</action>
			
			<action name="MoveToEnemy" weight="2">
				<task class="MoveToTarget" run="false"/>
				
				<consideration class="TargetType" type="EntityNPC"/>
				<consideration class="TargetFactionStanding" flip_y="true" max="127"/>
				<consideration class="TargetDistance" min="2" max="20"/>
			</action>
			
			<action name="MoveToEnemyRun" weight="2">
				<task class="MoveToTarget" run="true"/>
				
				<consideration class="TargetType" type="EntityNPC"/>
				<consideration class="TargetFactionStanding" flip_y="true" max="127"/>
				<consideration class="TargetDistance" min="20" max="25"/>
			</action>
			
			<action name="MeleeAttackEntity" weight="3">
				<task class="AttackTargetEntity" action_index="0"/>
				
				<consideration class="TargetType" type="EntityNPC"/>
				<consideration class="TargetFactionStanding" flip_y="true" max="127"/>
				<consideration class="TargetDistance" flip_y="true" min="2" max="3"/>
			</action>
			
			<action name="MeleeAttackBlock" weight="3">
				<task class="AttackTargetBlock" action_index="0"/>
				
				<consideration class="PathBlocked"/>
				<consideration class="TargetType" type="Block"/>
				<consideration class="TargetDistance" flip_y="true" min="2" max="4"/>
			</action>
		</ai_package>
		
		<ai_package name="Human Basic"> <action name="Wander" weight="1">
				<task class="Wander"/>
			</action>
			
			<action name="MoveToWaypoint" weight="1">
				<task class="MoveToTarget" run="false"/>
				
				<consideration class="TargetDistance" min="2" max="4"/>
				<consideration class="TargetVisible" flip_y="true"/>
			</action>
			
			<action name="Flee" weight="4">
				<task class="FleeFromTarget" max_distance="10"/>
				
				<consideration class="SelfHealth" flip_y="true" min="20" max="30"/>
				<consideration class="TargetFactionStanding" flip_y="true" max="127"/>
				<consideration class="TargetDistance" flip_y="true" min="4" max="5"/>
			</action>
		</ai_package>
		
		<ai_package name="Human Melee"> <action name="MoveToEnemy" weight="2">
				<task class="MoveToTarget" run="false"/>
				
				<consideration class="TargetType" type="EntityNPC, EntityZombie"/>
				<consideration class="TargetFactionStanding" flip_y="true" max="127"/>
				<consideration class="TargetDistance" min="3" max="4"/>
			</action>
			
			<action name="MoveToEnemyRun" weight="2">
				<task class="MoveToTarget" run="true"/>
				
				<consideration class="TargetType" type="EntityNPC, EntityZombie"/>
				<consideration class="TargetFactionStanding" flip_y="true" max="127"/>
				<consideration class="TargetDistance" min="3" max="10"/>
			</action>
			
			<action name="MeleeAttack" weight="3">
				<task class="AttackTargetEntity" action_index="0"/>
				
				<consideration class="TargetType" type="EntityNPC, EntityZombie"/>
				<consideration class="TargetFactionStanding" flip_y="true" max="127"/>
				<consideration class="TargetDistance" flip_y="true" min="0" max="2"/>
			</action>
		</ai_package>
		
		<ai_package name="Human Ranged"> <action name="MoveToEnemy" weight="2" distance="6">
				<task class="MoveToTarget" run="false"/>
				
				<consideration class="TargetType" type="EntityNPC, EntityZombie"/>
				<consideration class="TargetFactionStanding" flip_y="true" max="127"/>
				<consideration class="TargetDistance" min="10" max="20"/>
			</action>
			
			<action name="Flee" weight="3">
				<task class="FleeFromTarget" max_distance="10"/>
				
				<consideration class="TargetType" type="EntityNPC, EntityZombie"/>
				<consideration class="TargetFactionStanding" flip_y="true" max="127"/>
				<consideration class="TargetDistance" flip_y="true" min="1" max="6"/>
			</action>
			
			<action name="RangedAttack" weight="3">
				<task class="AttackTargetEntity" action_index="0"/>

				<consideration class="TargetVisible"/>
				<consideration class="TargetType" type="EntityNPC, EntityZombie"/>
				<consideration class="TargetFactionStanding" flip_y="true" max="127"/>
				<consideration class="TargetDistance" min="1" max="5"/>
			</action>
		</ai_package>
	</ai_packages>
</utility_ai>
```
