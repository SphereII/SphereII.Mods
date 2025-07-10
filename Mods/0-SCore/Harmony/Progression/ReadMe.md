The `Progression.cs` patch implements the "Zero XP" feature, allowing modders to completely disable experience gain from
all sources within the game. This can be used to create alternative progression systems or specific challenge scenarios
where XP is not a factor.

## Functionality

This feature is implemented through a Harmony patch that modifies the game's experience point (XP) award system. When
the `ZeroXP` setting is enabled, any attempt to gain XP (e.g., from killing zombies, completing quests, or crafting)
will be prevented, effectively halting all experience-based progression.

* **Patch Target**: (Implicit, methods related to `Progression.AddExperience` or similar XP gain functions)
* **Purpose**: To disable gaining Experience from any source.

## Configuration

The "Zero XP" feature is controlled by a property within the `AdvancedProgression` class in your `blocks.xml` file:

```xml

<property class="AdvancedProgression">
    <property name="Logging" value="false"/>
    <property name="ZeroXP" value="false"/>
</property>
```

**Explanation**:

* **`ZeroXP`**: `false` - Setting this property's `value` to `true` will activate the "Zero XP" feature, preventing
  players from gaining any experience points.
* **`Logging`**: `false` - Enables or disables verbose logging specifically for progression features.