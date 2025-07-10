The `AdvancedSoundFeatures` enhance the game's audio system, introduces a unique functionality to trigger game effects, such as giving a buff
or a quest, based on specific sounds being played within the game. This enables dynamic environmental storytelling or
interactive elements where an audio cue can directly influence gameplay by applying buffs or initiating quests.

## Configuration in `blocks.xml`

```xml

<property class="AdvancedSoundFeatures">
    <property name="Logging" value="false"/>
</property>
```

**Explanation**:

* **`Logging`**: `false` - Enables or disables verbose logging for the sound features, useful for debugging.

```xml
    <append xpath="/Sounds/SoundDataNode[@name='zombiefemalescoutalert']">
        <buff value="buffCursed" />
        <Quest value="myQuest" />
    </append>
```

**Explanation**:

* **`buff/Quest`**: - Provides the specified buff or quest to any entity that hears the sound.
