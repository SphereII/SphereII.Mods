This documentation outlines how modders can implement specific features within the 7 Days to Die modding framework,
particularly focusing on how the SCore mod utilizes `SoundDataNode` to trigger buffs and quests.

### Sounds: Triggering Buffs and Quests

The SCore mod enhances the functionality of `SoundDataNode` in `sounds.xml`, allowing modders to apply buffs and even
trigger quests directly when specific sounds are played. This enables dynamic gameplay reactions to in-game audio
events.

**Implementation:**
You can append `buff` and `Quest` elements within an existing `SoundDataNode` definition in your `sounds.xml` file.

Here are examples demonstrating how to apply a buff and trigger a quest when specific animal death sounds occur:

```xml

<append xpath="/Sounds/SoundDataNode[@name='rabbitdeath']">
    <buff value="buffBadKarma"/>
</append>
<append xpath="/Sounds/SoundDataNode[@name='chickendeath']">
<buff value="buffBadKarma"/>
</append>

<append xpath="/Sounds/SoundDataNode[@name='chickendeath']">
<Quest value="myChickenQuest"/>
</append>
```

* **`<buff value="buffName" />`**: This element applies a buff with the specified `buffName` to any player or entity
  that can "hear" the sound. For instance, `buffBadKarma` is applied when a rabbit or chicken dies.
* **`<Quest value="questID" />`**: This element triggers the quest with the specified `questID` (e.g., `myChickenQuest`)
  when the sound event occurs.