This documentation will guide modders on utilizing two custom block classes for 7 Days to Die: `BlockSpawnCube2SDX` and
its specialized derivative, `BlockSpawnCubeRepeater`. These classes enable blocks to dynamically spawn entities into the
game world, with varying control over spawn limits and repetition.

-----

# Custom Spawner Blocks: `BlockSpawnCube2SDX` & `BlockSpawnCubeRepeater`

These block classes provide powerful capabilities for creating dynamic spawn points in your 7 Days to Die mod. They are
designed to allow blocks to spawn entities (like zombies or custom creatures) at a specified rate and location.

## 1\. `BlockSpawnCube2SDX` (Base Spawner)

This is the foundational class for entity spawning. By default, it acts as a **single-use or limited-use spawner**. It
will attempt to spawn an entity at each tick and will destroy itself once it has reached its `MaxSpawned` limit or if it
encounters a configuration error that prevents spawning.

### Key XML Properties for `BlockSpawnCube2SDX`

The behavior of `BlockSpawnCube2SDX` is largely controlled by properties defined in its XML block definition.

| Property Name   | Type   | Default | Description                                                                                                                                                                                                       |
|:----------------|:-------|:--------|:------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Class`         | string | N/A     | **Must be set to `SpawnCube2SDX, SCore`** to use this C\# class.                                                                                                                                                  |
| `EntityGroup`   | string | ""      | The name of an [entity group](https://www.google.com/search?q=https://7daystodie.fandom.com/wiki/Entity_Groups.xml) (e.g., `ZombiesAll`, `AnimalsForest`) from which entities will be randomly selected to spawn. |
| `SpawnRadius`   | int    | 0       | If greater than 0, the spawner will attempt to find a random valid spawn point within this spherical radius around the block's position. If 0, it spawns directly on the block.                                   |
| `SpawnArea`     | int    | 15      | Used in conjunction with `SpawnRadius`. Defines the bounding box (SpawnArea x SpawnArea x SpawnArea) within which `FindRandomSpawnPointNearPosition` searches.                                                    |
| `NumberToSpawn` | int    | 1       | (Currently not fully implemented in the provided code to spawn multiple per tick, typically spawns 1). Intended to define how many entities this block *attempts* to spawn per tick.                              |
| `MaxSpawned`    | int    | 1       | The **total number of entities** this block will spawn before it destroys itself. The block tracks this internally via its `meta` value. If not specified, it defaults to 1.                                      |
| `TickRate`      | float  | 10      | The delay in game ticks (1 tick = 1 second real-time at default game speed) between each spawn attempt. If not specified, it defaults to 10 ticks (10 seconds).                                                   |
| `Config`        | string | ""      | An optional string used to pass additional configuration values, parsed by `PathingCubeParser`. See "Advanced Config Properties" below for details.                                                               |

### `BlockSpawnCube2SDX` Behavior Overview

* **Initialization**: Reads all properties from the XML definition.
* **On Block Added**: Schedules its first `UpdateTick` very quickly (1 game tick).
* **`UpdateTick` Logic (Server-Side Only)**:
    1. Checks if the total number of spawned entities (`_blockValue.meta`) has reached `MaxSpawned`.
    2. If `MaxSpawned` **is reached**, the block calls `DestroySelf()` and stops ticking (`returns false`).
    3. If `MaxSpawned` **is NOT reached**, it proceeds to:
        * Determine which entity to spawn based on `EntityGroup` or `Config` (see `ec` or `eg`).
        * Find a valid spawn position (either directly on the block or within `SpawnRadius`).
        * Create and spawn the entity in the world.
        * Apply any sign data (`ApplySignData()`) to the spawned entity (e.g., buffs, orders).
        * Increments the block's `meta` value to track the number of entities spawned.
        * Schedules the **next `UpdateTick`** according to its `TickRate`.
        * Returns `true` to keep the block active and ticking.
* **`DestroySelf()`**: A `virtual` method that either damages the block to destroy it (default) or schedules a very long
  tick if `keep` is set in `Config`, effectively preserving it.

### `BlockSpawnCube2SDX` XML Example: `DeamonPortal`

```xml

<block name="DeamonPortal">
    <property name="Extends" value="SpawnCube"/>
    <property name="CreativeMode" value="Player"/>
    <property name="CustomIcon" value="NecronStatues"/>
    <property name="Class" value="SpawnCube2SDX, SCore"/>
    <property name="Model" value="@:Entities/Vehicles/TraderVehicles/traderMountainBikeStaticPrefab.prefab"/>
    <property name="ModelOffset" value="0,0,0"/>
    <property name="MaxDamage" value="250"/>

    <property name="EntityGroup" value="ZombiesAll"/>
    <property name="SpawnRadius" value="5"/>
    <property name="SpawnArea" value="15"/>
    <property name="NumberToSpawn" value="2"/>
</block>
```

**Explanation for `DeamonPortal`:**

* This block uses the `BlockSpawnCube2SDX` class (`Class="SpawnCube2SDX, SCore"`).
* It extends `SpawnCube`, suggesting it's built upon a base "invisible" or "utility" block.
* It will spawn entities from the `ZombiesAll` group.
* It will look for a spawn point within 5 units of the block.
* `NumberToSpawn` is set to 2, but without further code modification, it will still only spawn 1 entity per tick (due to
  the current `TrySpawnEntity` implementation only creating one entity at a time).
* Crucially, `MaxSpawned` is **not defined**, so it defaults to **1**. This means the `DeamonPortal` will spawn **one
  zombie**, and then **destroy itself** after that single spawn.
* `TickRate` is **not defined**, so it defaults to **10 ticks** (10 seconds) between the initial placement and the
  first (and only) spawn attempt.

-----

## 2\. `BlockSpawnCubeRepeater` (Repeating Spawner)

This class **inherits from `BlockSpawnCube2SDX`** and overrides its `UpdateTick` method to provide a different flow
control. Instead of destroying itself after the first successful spawn (if `MaxSpawned` is 1), it is designed to *
*continuously repeat its spawn attempts** until its `MaxSpawned` limit is reached.

### Key Differences in Behavior (`UpdateTick`):

* `BlockSpawnCubeRepeater` leverages the shared `TrySpawnEntity` logic from `BlockSpawnCube2SDX` to perform the actual
  spawning.
* Its `UpdateTick` method primarily focuses on the **looping mechanism**:
    1. It checks if `_blockValue.meta` has reached `MaxSpawned` **at the very beginning** of its server-side logic.
    2. If `MaxSpawned` **is reached**, it calls `DestroySelf()` and `returns false` to stop ticking.
    3. If `MaxSpawned` **is NOT reached**, it will **always schedule another tick** for `GetTickRate()` *regardless* of
       whether `TrySpawnEntity` succeeded in spawning an entity during the current tick. This ensures it keeps trying
       until the total `MaxSpawned` count is hit.

This makes `BlockSpawnCubeRepeater` ideal for creating blocks that spawn waves of enemies or a set number of entities
over time.

### `BlockSpawnCubeRepeater` XML Example: `DeamonPortal2`

```xml

<block name="DeamonPortal2">
    <property name="Extends" value="DeamonPortal"/>
    <property name="Class" value="SpawnCubeRepeater, SCore"/>
    <property name="Model" value="@:Entities/Vehicles/TraderVehicles/traderMountainBikeStaticPrefab.prefab"/>
    <property name="ModelOffset" value="0,0,0"/>
    <property name="MaxDamage" value="250"/>

    <property name="EntityGroup" value="ZombiesAll"/>
    <property name="SpawnRadius" value="5"/>
    <property name="SpawnArea" value="15"/>
    <property name="NumberToSpawn" value="2"/>
    <property name="MaxSpawned" value="10"/>

    <property name="TickRate" value="10"/>
</block>
```

**Explanation for `DeamonPortal2`:**

* This block uses the `BlockSpawnCubeRepeater` class (`Class="SpawnCubeRepeater, SCore"`).
* It inherits properties like `EntityGroup`, `SpawnRadius`, `SpawnArea`, `NumberToSpawn` from `DeamonPortal` (which
  itself is based on `SpawnCube2SDX`).
* `MaxSpawned` is explicitly set to **10**. This means the block will attempt to spawn entities **10 times** (or until
  10 entities are successfully spawned and recorded by `meta`).
* `TickRate` is explicitly set to **10**. This means there will be a **10-tick (10-second) delay** between each spawn
  attempt.
* The `DeamonPortal2` will therefore repeatedly attempt to spawn zombies every 10 seconds until a total of 10 zombies
  have been successfully spawned, at which point the block will destroy itself.

-----

## 3\. Advanced Config Properties (via `Config` XML property or Sign Text)

Both `BlockSpawnCube2SDX` and `BlockSpawnCubeRepeater` can parse additional settings from their `Config` XML property (
or, if the block is placed in-game, from a sign placed on it). This allows for dynamic overrides of block properties or
to assign custom behaviors to spawned entities.

The `Config` property is read as a comma-separated list of `key=value` pairs. The `PathingCubeParser.GetValue()` method
is used to extract these values.

Example `Config` property in XML:
`<property name="Config" value="task=guard,buff=buffStrongerZombies,pc=123.45,leader=true,ec=4567,eg=MyCustomGroup,keep=true"/>`

| Key      | Purpose                                                                                                                                       | Expected Value                                                                      |
|:---------|:----------------------------------------------------------------------------------------------------------------------------------------------|:------------------------------------------------------------------------------------|
| `task`   | Assigns a predefined behavior buff to the spawned entity.                                                                                     | `stay`, `wander`, `guard`, `follow` (adds `buffOrderStay`, `buffOrderWander`, etc.) |
| `buff`   | Assigns one or more custom buffs to the spawned entity.                                                                                       | Comma-separated list of buff names (e.g., `buffImmunity,buffSpeedBoost`)            |
| `pc`     | Sets a custom `PathingCode` variable on the entity's buffs, typically for advanced AI pathing.                                                | Float value (e.g., `123.45`)                                                        |
| `leader` | If "true" and an `OwnerID` is set (e.g., from the player who placed the block), attempts to set the player as the entity's leader.            | `true`, `false`                                                                     |
| `ec`     | **Overrides `EntityGroup`**. Spawns a specific entity class ID instead of selecting from a group.                                             | Integer entity class ID (e.g., `110` for a zombie dog)                              |
| `eg`     | **Overrides `EntityGroup`**. Spawns from a specific entity group. Useful if you want `EntityGroup` in block XML but override via `Config`.    | Entity group name (e.g., `ZombieBosses`)                                            |
| `keep`   | Prevents the block from destroying itself even after `MaxSpawned` is reached. Instead, it schedules a very long tick effectively freezing it. | Any non-empty string value (e.g., `true`, `1`)                                      |

-----

## 4\. Usage Notes for Modders

* **Server-Side Only**: All spawning logic handled by these blocks runs exclusively on the dedicated server or the host
  in a peer-to-peer game. Clients will see entities spawn as their world state is synchronized.
* **XML `Class` Attribute**: Always ensure your block definition includes
  `<property name="Class" value="YourClassName, SCore"/>` to link the XML block to its C\# implementation.
* **`_blockValue.meta`**: This hidden integer value is used by these classes to track the number of entities spawned. Do
  not interfere with it if you want the `MaxSpawned` logic to function correctly.
* **Debugging**: If entities are not spawning, check your `EntityGroup` or `ec` values carefully. Enable debug logging
  in your game or IDE to catch any warnings or errors from `BlockSpawnCube2SDX` or `BlockSpawnCubeRepeater` regarding
  invalid configurations (e.g., missing entity groups).
* **Performance**: Be mindful of very low `TickRate` values combined with high `MaxSpawned` or large `NumberToSpawn` on
  many blocks, as this can impact server performance.