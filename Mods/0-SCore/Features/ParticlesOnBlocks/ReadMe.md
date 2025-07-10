The Particles On Blocks feature allows modders to attach and control particle effects directly on blocks within the game
world, enabling dynamic visual feedback for various block states and events.

## Configuration for Particles On Blocks

Particle effects can be configured directly within a block's definition in `blocks.xml` (or similar XML files) by adding
a `<property class="Particles">` section. This allows for particles to be triggered upon block spawn, when damaged, and
can even be biome-specific.

**XML Example:**

```xml

<block name="blah">
    <property class="Particles">
        <property name="OnSpawn" value="unitybundle,unitybundle2"/>

        <property name="OnSpawn_pine_forest" value="unitybundle,unitybundle2"/>
        <property name="OnSpawnProb" value="0.2"/>

        <property name="DamagedParticle" value="unitybundle,unitybundle2"/>
        <property name="DamagedParticle_snow" value="unitybundle,unitybundle2"/>
        <property name="OnDamagedProb" value="0.2"/>
    </property>
</block>
```

### Property Descriptions:

* **`<property name="OnSpawn" value="unitybundle,unitybundle2"/>`**

    * **Description**: Specifies one or more particle effects to be spawned when the block is initially placed or loaded
      into the world. The `value` is a comma-separated list of particle effect paths. Each path typically points to a
      Unity3D asset containing the particle system (e.g.,
      `#@modfolder:Resources/MyParticleBundle.unity3d?ParticleEffectName`).

* **`<property name="OnSpawn_biome" value="unitybundle,unitybundle2"/>`**

    * **Description**: (e.g., `OnSpawn_pine_forest`) Allows you to define specific particle effects to be spawned on
      block creation, but *only* if the block is within the specified biome. This overrides the general `OnSpawn`
      property if a matching biome is found.

* **`<property name="OnSpawnProb" value="0.2"/>`**

    * **Description**: A float value (0.0 to 1.0) representing the probability that the `OnSpawn` (or biome-specific
      `OnSpawn_biome`) particle effect will be triggered when the block spawns. `0.2` means a 20% chance.

* **`<property name="DamagedParticle" value="unitybundle,unitybundle2"/>`**

    * **Description**: Specifies one or more particle effects to be played when the block receives damage. The `value`
      is a comma-separated list of particle effect paths.

* **`<property name="DamagedParticle_biome" value="unitybundle,unitybundle2"/>`**

    * **Description**: (e.g., `DamagedParticle_snow`) Allows you to define specific particle effects to be played when
      the block is damaged, but *only* if the block is within the specified biome. This overrides the general
      `DamagedParticle` property if a matching biome is found.

* **`<property name="OnDamagedProb" value="0.2"/>`**

    * **Description**: A float value (0.0 to 1.0) representing the probability that the `DamagedParticle` (or
      biome-specific `DamagedParticle_biome`) effect will be triggered when the block takes damage. `0.2` means a 20%
      chance.