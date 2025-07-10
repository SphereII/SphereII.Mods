The Block Clock feature introduces custom clock blocks into the game, allowing for in-game displays of time. This
provides players with a visual reference for the current in-game time without needing to open the menu.

## 1\. `BlockClockScript.cs`

* **Purpose**: This script defines a custom block that functions as an in-game clock. It likely updates its display to
  reflect the current game time. It will automatically attach the ClockScript.
*
* **XML Example**: To use this clock in-game, you would define a block in your `blocks.xml` and assign this script as
  its class.
  ```xml
  <block name="myClockBlock">
      <property name="Extends" value="myBaseBlock"/>
      <property name="Class" value="ClockDMT, SCore"/>
      </block>
  ```

---

This feature introduces a specialized block that functions as an "always active" TileEntity, enabling it to continuously
distribute a buff within its Area of Effect (AoE). This allows modders to create dynamic environmental hazards or
beneficial zones that are persistently active in the game world, regardless of player proximity.

## Functionality

When placed, this block leverages underlying systems to remain active at all times, similar to the functionality
provided by the `TileEntityAlwaysActive` patch. This constant activity allows its associated AoE component (likely a
`TileEntityAoE`) to continuously apply a specified buff to entities within its configured radius. This could be used for
effects such as a healing aura, a debuff field, or a damage zone.

## XML Example

To utilize this block, you would define it in your `blocks.xml` file, referencing a custom block class (e.g.,
`BlockDecoAoE, SCore`) that incorporates this always-active buff distribution logic. The properties for the AoE effect (
like the buff to apply, its range, and application interval) would be defined directly on the block.

```xml

<block name="myAlwaysActiveBuffZone">
    <property name="Extends" value="myBaseDecorativeBlock"/>
    <property name="Class" value="DecoAoE, SCore"/>
    <property name="ActiveRadiusEffects" value="buffCampfireAOE(3)"/>
</block>
```

---

The `BlockTakeAndReplace.cs` script defines a custom block that can be picked up by the player after a delay, and
optionally replaced or harvested. This feature integrates with the broader "Advanced Pick Up And Place" system, allowing
for flexible block manipulation and custom gathering mechanics.

## Functionality

When a player activates a block configured with `BlockTakeAndReplace`, a timer is initiated. After the delay, the block
is "taken," meaning it's either removed and potentially replaced with another block, or its resources are harvested. The
delay and required tools can be customized, influencing the player's ability to quickly pick up and process these
blocks.

### Block Properties

You can configure the behavior of a `BlockTakeAndReplace` by setting the following properties in its `blocks.xml`
definition:

* **`TakeDelay`**: `float` - The time, in seconds, before the block is taken after activation. Default is `6.0` seconds.
  Can be reduced by holding specific tools.
* **`HoldingItem`**: `string` - A comma-separated list of item names. If the player is holding one of these items, the
  `TakeDelay` may be reduced.
* **`PickUpBlock`**: `string` - The name of the block (or item) that is added to the player's inventory when this block
  is successfully "taken".
* **`ValidMaterials`**: `string` - A comma-separated list of material IDs. If specified, the block can only be picked up
  if its material matches one of these.
* **`CheckToolForMaterial`**: `bool` - If `true`, the system will check if the player is holding a tool specified by
  `TakeWithTool` (or `HoldingItem`) to validate picking up the block based on `ValidMaterials`.
* **`TakeWithTool`**: `string` - Overrides `HoldingItem`. If set, only the specified tool(s) (comma-separated item
  names) can be used to interact with this block. This value can be further overridden by a global setting in
  `AdvancedPickUpAndPlace`.
* **`HarvestOnPickUp`**: `bool` - If `true`, when the block is taken, it triggers its standard harvest drop event (as
  defined in `materials.xml` or the block's `itemsToDrop`), rather than giving a `PickUpBlock`. The block is also fully
  damaged.

### Interaction Logic

* **Activation (`OnBlockActivated`)**: When a player activates the block, it first checks if the `ValidMaterialCheck`
  passes. If so, a UI timer is displayed, initiating the `TakeItemWithTimer` process.
* **Timer and Tool Effects (`TakeItemWithTimer`)**: The `fTakeDelay` timer is displayed to the player. If the player is
  holding an item specified by `HoldingItem` (or `TakeWithTool`), the delay can be reduced (e.g., by half for a
  crowbar/hammer, and further by the item's quality). The holding item also degrades with use.
* **Taking the Item (`TakeTarget`)**: Once the timer completes, this method executes.
    * If `HarvestOnPickUp` is `true`, the block's harvest drop is triggered, and the block is destroyed.
    * Otherwise, an `ItemStack` for the block itself (or the `PickUpBlock` item if specified) is added to the player's
      inventory. If inventory is full, it's dropped. The block is then fully damaged and removed.
* **Material and Tool Validation (`ValidMaterialCheck`)**: This method determines if the block can be picked up. If
  `Legacy` mode is `false`, it checks if the block's material is in `ValidMaterials`. If `CheckToolForMaterial` is
  `true`, it also verifies if the player is holding a `TakeWithTool` (or `HoldingItem`) or an item with a tag matching
  the block's material ID.

## Relation to Advanced Pick Up And Place Configuration

The `BlockTakeAndReplace` script also reads global settings from the `AdvancedPickUpAndPlace` class in your
`blocks.xml` (specifically in its `LateInit` method):

```xml

<property class="AdvancedPickUpAndPlace">
    <property name="Logging" value="false"/>
    <property name="Legacy" value="true"/>
    <property name="TakeWithTool" value="meleeToolRepairT1ClawHammer"/>
</property>
```

* **`Legacy`**: `true` - This global setting influences some internal checks. If `true`, it enables a "legacy" or
  simpler method of handling item checks (e.g., for wood boards).
* **`TakeWithTool` (Global)**: `meleeToolRepairT1ClawHammer` - This global property from `AdvancedPickUpAndPlace` can
  override the `TakeWithTool` property defined directly on the block. It specifies a default tool that can be used for
  taking blocks under this system.

## XML Example

Here's a comprehensive conceptual example of how to define a block using `BlockTakeAndReplace` in your `blocks.xml`:

```xml

<block name="myCustomTakeableBoard">
    <property name="Extends" value="woodPlank"/>
    <property name="Class" value="BlockTakeAndReplace, SCore"/>

    <property name="TakeDelay" value="8.0"/>
    <property name="HoldingItem" value="meleeToolRepairT1ClawHammer,meleeToolWrench"/>
    <property name="PickUpBlock" value="woodPlank"/>
    <property name="ValidMaterials" value="Mwood,Mwood_weak"/>
    <property name="CheckToolForMaterial" value="true"/>
    <property name="HarvestOnPickUp" value="false"/>
</block>

<block name="myHarvestableBush">
<property name="Extends" value="plantGrowingAloeVera"/>
<property name="Class" value="BlockTakeAndReplace, SCore"/>
<property name="TakeDelay" value="1.0"/>
<property name="HarvestOnPickUp" value="true"/>
<property name="HoldingItem" value="meleeToolShovelT0StoneShovel"/>
</block>
```

---

The `BlockTriggeredSDX.cs` script defines a highly versatile custom block that can be activated by various means,
control animations, interact with player CVars and buffs, and even function as a container. It is designed to be
`AlwaysActive` as a `TileEntity`, ensuring its logic runs continuously.

## Functionality

This block acts as an interactive element in the game world, capable of dynamically responding to player presence,
activating upon being looked at or interacted with, and controlling associated animators. It can also apply buffs to
players based on activation conditions.

### Block Properties

You can configure the behavior of a `BlockTriggeredSDX` by setting the following properties in its `blocks.xml`
definition:

* **`Class`**: `TriggeredSDX, SCore` - Assigns this script as the block's class, enabling all its functionalities.
* **`AlwaysActive`**: `true` - (From comments) This property, when set to `true`, ensures the block acts as a
  `TileEntity` that is always active, allowing its logic to run continuously regardless of player proximity.
* **`ActivationDistance`**: `int` - (From comments) Specifies how far out the `TileEntity` will re-scan to detect
  players for activation.
* **`RandomIndex`**: `int` - An integer value used to set a "RandomIndex" parameter in the block's animator. This can be
  used for randomizing animations.
* **`ActivateOnLook`**: `bool` - If `true`, the block will trigger its activation logic (including `ActivationBuffs` and
  animator updates) when the player looks at it.
* **`IsContainer`**: `bool` - If `true`, the block can also function as a storage device, allowing players to open its
  loot container.
* **`CopyCVarToAnimator`**: `string` - A semicolon-separated list of CVar names. The block will read the values of these
  CVars from the activating `EntityAlive` (usually the player) and set corresponding `float` parameters in its animator.
* **`ActivationBuffs`**: `string` - A semicolon-separated list of buffs or CVar conditions. If the activating entity
  meets these conditions (e.g., has a specific buff, or a cvar matches a value), the buff will be applied.

### Core Logic & Functionality

* **Activation Mechanisms**: The block can be activated either by player interaction (`OnBlockActivated`) or simply by
  being looked at (`ActivateOnLook`). Both methods trigger `TriggerActivationBuffs` and `UpdateAnimator`.
* **Animator Control (`UpdateAnimator`, `ActivateBlock`)**:
    * `CopyCVarToAnimator`: Reads specified CVars from the player and updates corresponding float parameters in the
      block's animator, enabling dynamic animation based on player stats.
    * `ActivateBlock`: Controls the "On" boolean and "TriggerOn" trigger parameters in the block's animator. It also
      sets a "RandomIndex" integer, allowing for randomized animation states when activated.
* **Buff-based Triggers (`TriggerActivationBuffs`)**: Based on the `ActivationBuffs` property, the block can apply buffs
  to the activating entity. This property can include direct buff names or CVar conditions (e.g.,
  `buffCursed,cvar(4),myOtherCvar`), allowing for conditional buff application.
* **Container Behavior (`IsContainer`)**: If `IsContainer` is `true`, the block will behave as a standard loot container
  upon activation, allowing the player to access its inventory.

## XML Example

Here's a comprehensive conceptual example of how to define a block using `BlockTriggeredSDX` in your `blocks.xml`:

```xml

<block name="myInteractiveTriggerBlock">
    <property name="Extends" value="cntWoodCrate"/>
    <property name="Class" value="TriggeredSDX, SCore"/>
    <property name="AlwaysActive" value="true"/>
    <property name="ActivationDistance" value="5"/>
    <property name="RandomIndex" value="10"/>
    <property name="ActivateOnLook" value="true"/>
    <property name="IsContainer" value="true"/>
    <property name="CopyCVarToAnimator" value="playerLevelCVar;playerStaminaCVar"/>

    <property name="ActivationBuffs" value="buffCursed,myCvar(4),anotherCvar"/>

</block>
```

---

The `BlockSpawnCube2SDX.cs` script defines an advanced entity spawner block that can automatically spawn entities from
specified groups or classes, apply initial settings to them, and then manage its own lifespan. It offers extensive
customization for controlling spawns, including quantity, radius, and initial behaviors of spawned entities.

## Functionality

When placed in the world, the `BlockSpawnCube2SDX` schedules an update tick. During this tick, it attempts to spawn
entities based on its configuration. It can spawn entities from an entity group or a specific entity class, apply
initial buffs or AI tasks to them, and then either destroy itself or go dormant after reaching its maximum spawn count.

### Block Properties

You can configure the behavior of a `BlockSpawnCube2SDX` by setting the following properties in its `blocks.xml`
definition:

* **`Class`**: `BlockSpawnCube2SDX, SCore` - Assigns this script as the block's class.
* **`TickRate`**: `float` - The rate (in game ticks) at which the block attempts to perform its spawn logic. Default is
  `10`.
* **`MaxSpawned`**: `int` - The maximum number of entities this block can spawn. Once this limit is reached, the block
  will destroy itself (unless configured to `keep`). Default is `1`.
* **`NumberToSpawn`**: `int` - The number of entities to attempt to spawn per successful tick (though the `UpdateTick`
  code spawns one at a time before incrementing meta). Default is `1`.
* **`SpawnRadius`**: `int` - If greater than 0, entities will spawn within a random radius around the block's position.
* **`SpawnArea`**: `int` - Used with `SpawnRadius`. Defines the size of the area (in blocks) to search for a random
  spawn point near the block. Default is `15`.
* **`EntityGroup`**: `string` - The name of the `entitygroup` from which entities will be randomly selected and spawned.
  This is used if `Config` does not specify `eg`.
* **`Config`**: `string` - A string that can contain various key-value pairs parsed by `PathingCubeParser` (similar to
  sign text data). This provides granular control over spawning and initial entity properties.

### Configuration via `Config` Property (`_signText`)

The `Config` property allows for highly detailed configuration using a format that can be parsed for specific
sub-properties:

* **`ec` (Entity Class)**: `string` - Specifies a direct entity class name to spawn (e.g., `ec=zombieMale`). Overrides
  `eg` and `EntityGroup`.
* **`eg` (Entity Group)**: `string` - Specifies an `entitygroup` name to spawn from (e.g., `eg=ZombiesAll`). Overrides
  `EntityGroup`.
* **`task`**: `string` - Sets an initial AI task for the spawned entity. Possible values include `stay`, `wander`,
  `guard`, `follow`. This applies buffs like `buffOrderStay`, `buffOrderWander`, etc.
* **`buff`**: `string` - A comma-separated list of buffs to apply to the newly spawned entity (e.g.,
  `buff=buffBurningMolotov,buffImmunity`).
* **`pc` (Pathing Code)**: `float` - Sets a `PathingCode` CVar on the spawned entity, which can be used by custom AI.
* **`leader`**: `string` - If present (e.g., `leader=true`), and the block has an `OwnerID` (from a
  `TileEntityPoweredTrigger`), the spawned entity will be set as a follower of that owner.
* **`keep`**: `string` - If present (e.g., `keep=true`), the block will not destroy itself after reaching `MaxSpawned`
  entities. Instead, it will schedule a very long update tick (making it effectively dormant).

### Spawning Logic

1. **`OnBlockAdded`**: When the `BlockSpawnCube2SDX` is placed, it immediately schedules its first `UpdateTick`.
2. **`UpdateTick`**: This is the core spawning method, running periodically on the server:

* **Spawn Limit Check**: It checks if the `MaxSpawned` limit has been reached (`_blockValue.meta` tracks spawned count).
  If so, it calls `DestroySelf`.
* **Entity Selection**: It determines which entity to spawn, prioritizing `ec` from `Config`, then `eg` from `Config`,
  and finally the `EntityGroup` property.
* **Spawn Point**: An entity is spawned at or near the block's position. If `SpawnRadius` is greater than 0, it attempts
  to find a random spawn point within `SpawnArea` around the block.
* **Rotation Matching**: The spawned entity's rotation is matched to the block's rotation.
* **`ApplySignData`**: This crucial step applies initial buffs, AI tasks (`stay`, `wander`, `guard`, `follow`),
  `PathingCode` CVar, and sets the entity's leader/owner based on the `Config` property.
* **Counter Update**: The block's `meta` value is incremented to track the spawned count, and the block is updated on
  the server.
* **Self-Destruction**: After processing, `DestroySelf` is called, either removing the block or making it dormant.

### Block Destruction (`DestroySelf`)

The block's destruction or dormancy is controlled by the `keep` sub-property in the `Config` string:

* If `keep` is not specified or `false`, the block is immediately damaged for its full health and removed.
* If `keep` is `true`, the block is not destroyed but schedules a very long `UpdateTick` (10000UL ticks), effectively
  making it dormant and preventing further spawns.

## XML Example

Here's a comprehensive conceptual example of how to define a `BlockSpawnCube2SDX` in your `blocks.xml`:

```xml

<block name="myCustomSpawnCube">
    <property name="Extends" value="cubeConcrete"/>
    <property name="Class" value="BlockSpawnCube2SDX, SCore"/>

    <property name="TickRate" value="5"/>
    <property name="MaxSpawned" value="3"/>
    <property name="NumberToSpawn" value="1"/>
    <property name="SpawnRadius" value="2"/>
    <property name="SpawnArea" value="10"/>
    <property name="EntityGroup" value="ZombiesAll"/>
    <property name="Config" value="ec=zombieFatHawaiian;task=guard;buff=buffImmunity;pc=10;keep=true"/>
</block>
```