The `HeadshotOnly.cs` patch implements the "Headshot Only" feature, which makes zombies vulnerable exclusively to
headshots.

* **Patch Target**: (Implicit, related to zombie damage application or death conditions)
* **Purpose**: To ensure that the only way to kill zombies is through headshots.

## Configuration

The "Headshot Only" feature is controlled by a property within the `AdvancedZombieFeatures` class in your `blocks.xml`
file:

```xml

<property class="AdvancedZombieFeatures">
    <property name="HeadshotOnly" value="false"/>
</property>
```

**Explanation**:

* **`HeadshotOnly`**: `false` - Setting this property's `value` to `true` will activate the "Headshot Only" feature,
  making zombies only killable via headshots.

---

The `InertEntity.cs` patch extends the `EntityAlive.IsAlive()` method, allowing certain custom entities to exist in a "
non-living" or inert state, even if the game's default logic considers them dead.

```xml

<entityclass name="myZombie">
    <property name="EntityActiveWhen" value="night"/>
    <property name="EntityActiveWhen" value="day"/>
</entityclass>
```

**Explanation**:

* **`EntityActiveWhen`**: `night/day` - Setting this property's will allow this entity to be alive during that time. If
  set to night, it will be inactive during the day.

---

## RandomDeathSpawn

This feature enables entities to spawn other entities upon their death. This allows modders to create dynamic death
events, such as a zombie exploding into smaller creatures, or a boss spawning minions when defeated.

* **Purpose**: To allow entities to spawn additional entities upon their death, diversifying death animations and
  gameplay consequences.

### XML Example

You can define the `SpawnOnDeath` property on an entity's `entity_classes.xml` definition to specify what entities it
spawns upon death (conceptual example):

```xml

<entity_classes>
    <entity_class name="zombieExploder">
        <property name="Extends" value="zombieFatCop"/>
        <property name="SpawnOnDeath" value="mySpawnGroup"/>

        <property name="SpawnOnDeathLeaveBody" value="false"/>

        <property name="SpawnOnDeathAltSpawn" value="false"/>
    </entity_class>
</entity_classes>
```

**Explanation**:

* **`<property name="SpawnOnDeath" value="mySpawnGroup" />`**: This property specifies the `entitygroup` or entity name
  from which new entities will spawn upon this entity's death.
* **`<property name="SpawnOnDeathLeaveBody" value="false"/>`**: If set to `true`, the original entity's body will remain
  after spawning new entities. Otherwise, it will be removed.
* **`<property name="SpawnOnDeathAltSpawn" value="false" />`**: If set to `true`, this enables the possibility of
  copying CVars and buffs from the dying entity to the newly spawned entities (refer to the notes for more details).

### Notes:

* If the entity has a CVar named `NoSpawnOnDeath`, then no entities will spawn upon its death.
* If the entity has a buff that starts with the name `spawnOverride` (e.g., `spawnOverrideZombiesAll`), this
  `spawnOverride` would
  be removed, and the `ZombiesAll` group (or whatever follows `spawnOverride`) would be used to spawn from,
  short-circuiting other spawn logic.
* If the entity has a buff that starts with the name `spawn2ndLife`, this will short-circuit the spawn process, skipping
  any additional spawns after this "second life" trigger.
* If the specified spawn group is `SpawnNothing`, then no entities will spawn.
* If the entity has the CVar `RandomSize` and the property `SpawnCopyScale`, then the size and scale values from the
  dying entity will be copied to the newly spawned entity, adjusting its size.
* If the entity has the property `SpawnOnDeathBuffFilter` or `SpawnOnDeathCVarFilter`, then the buffs and CVars,
  respectively, will be selectively copied over to the newly spawned entities.

---

## SCore RandomSize

This feature introduces a Harmony patch that allows entities to spawn with a random size, adding visual variety to the
game world.

* **Purpose**: To enable entities to spawn in with a randomized scale, enhancing visual diversity.

### Configuration

You can specify a custom range for random sizes by adding a property to the `entity_classes.xml` file. If this property
is not defined, the system will use a default size range.

* **Default Size Range**: `{ 0.7f, 0.8f, 0.9f, 0.9f, 1.0f, 1.0f, 1.0f, 1.1f, 1.2f }`

The rolled value is a **multiplier on the entity class's own `SizeScale`**, not a replacement for it. A `zombieBoe`
(`SizeScale` 1.08) that rolls 1.1 ends up at 1.188. A roll of 1.0 is left alone entirely, so the class keeps exactly the
size vanilla gave it.

### Which entities are randomized

By default any `EntityZombie`, plus any entity class carrying a `RandomSizes` list, gets a random size. The
per-entity-class `RandomSize` property overrides that in **both** directions:

```xml
<!-- opt a non-zombie in -->
<property name="RandomSize" value="true"/>

<!-- opt a zombie out, even one with a RandomSizes list -->
<property name="RandomSize" value="false"/>
```

When `RandomSize` is present its value wins outright; when it is absent the zombie/`RandomSizes` default applies.

### Multiplayer

The size is rolled in `EntityAlive.Init` and applied in `EntityAlive.PostInit`, which is the last thing
`EntityFactory.CreateEntityOperation.CompleteEntity` does. Applying it there means `EntityAlive.OverrideSize` is
populated before the entity can be handed to anyone, which is what makes the size reach clients: the server snapshots
`OverrideSize` into an `EntityCreationData` when it builds a spawn packet, and the client's own spawn path replays
`SetScale(SizeScale)` then `SetScale(overrideSize)` to land on the same number. Clients never roll or apply a size
themselves — remote entities are skipped.

An entity that arrives already sized (a chunk reload, or a prefab copy) keeps the size it came in with rather than
being re-rolled, because `Entity.SetScale` multiplies the physics extents on every call and scaling twice would
compound them.

### XML Usage Example

You can enforce a specific size range for an entity class using the `RandomSizes` property:

```xml

<property name="RandomSizes" value="1.2,1.2,1.4"/>
```

**Explanation**:

* **`RandomSizes`**: This property defines a comma-separated list of float values representing the possible sizes an
  entity can spawn with. The system will randomly select one of these values when the entity is spawned. For example,
  `1.2,1.2,1.4` means the entity could spawn at 1.2x, 1.2x, or 1.4x its base size.

* A `RandomSizes` list is enough on its own to opt an entity class in — see *Which entities are randomized* above. The
  `AdvancedZombieFeatures/RandomSize` feature flag in the ConfigurationBlock still has to be enabled for any of this to
  run at all.

---

Here's the documentation for the SCore Random Walk Type feature:

## SCore Random Walk Type

This feature, implemented via a Harmony patch (associated with a class that seems to be referred to as
`SCoreTransmogrifier` in your context), allows entities to spawn with a random walk type, adding visual variety to their
movement.

* **Purpose**: To enable entities to exhibit a randomized walk type upon spawning.

### Configuration

You can specify a custom range of walk types for an entity class by adding a property to its `entity_classes.xml`
definition. If this property is not defined, the system will use a default range of walk types.

* **Default Walk Type Range**: `{ 1, 2, 2, 3, 5, 6, 7, 7 }`

These are the upright walk types vanilla actually ships. Walk type 4 is not used by any entity class, 8 is
`cWalkTypeCrouch` (the crouch swap in `CrouchHeightFixedUpdate` special-cases it) and 15 is `cWalkTypeBandit`, so none of
them are in the default pool. Entities that spawn as a crawl (walk type 20 and up — `cWalkTypeCrawler` 21,
`cWalkTypeSpider` 22) are skipped entirely and keep the walk type they spawned with.

### XML Usage Example

To enforce a specific range of walk types for an entity class, use the `RandomWalkTypes` property:

```xml

<property name="RandomWalkTypes" value="2,3,21,5,6,7,22"/>
```

**Explanation**:

* **`RandomWalkTypes`**: This property defines a comma-separated list of integer values, each representing a possible
  walk type the entity can exhibit upon spawning. The system will randomly select one of these values. For example,
  `2,3,21,5,6,7,22` means the entity could use walk types 2, 3, 21, 5, 6,7 or 22.
* Naming a crawl type here does work — an entity rolled onto 21 is turned into a crawler by `EntityHuman.InitCommon`,
  and 22 reaches the animator through the normal walk type path. The skip only applies to entities that *already*
  spawned as a crawl.
* An empty or malformed `RandomWalkTypes` leaves the entity on its configured `WalkType`.

---

## AIDirectorChunkEventComponentScout Patch Documentation

The AIDirectorChunkEventComponentScout Harmony patch customizes the behavior of the AI Director's chunk event system,
specifically controlling the spawning of scout zombies.

* **Patch Target**: AIDirectorChunkEventComponent.CheckToSpawn
* **Purpose**: To introduce a configurable chance for scout zombies to spawn based on various in-game conditions, offering
more control over zombie behavior and difficulty.

###  Configuration
The behavior of this patch is controlled by the ScoutSpawnChance property within the AdvancedZombieFeatures class in
your configuration file (e.g., blocks.xml or Config/features.xml).

```xml
<property class="AdvancedZombieFeatures">
    <property name="ScoutSpawnChance" value="0.05"/> <!-- Example: 5% chance -->
</property>
```

**Configuration Key**: AdvancedZombieFeatures/ScoutSpawnChance
Type: Float

**Explanation**: 
This property defines the probability (as a float between 0.0 and 1.0) that a scout zombie will be spawned when all other conditions are met.

**Special Configuration Values**:

-1.0 (ConfigDisableCustomSpawn):
If ScoutSpawnChance is set to -1.0, this custom patch will be completely disabled. The original game's
AIDirectorChunkEventComponent.CheckToSpawn method will execute without any interference from this patch.

0.2 (ConfigAllowOriginalChance):
If ScoutSpawnChance is set to 0.2, this custom patch will also defer to the original method. This is the vanilla setting for 2.1.

---