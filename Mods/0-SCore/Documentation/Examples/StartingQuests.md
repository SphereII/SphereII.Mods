This documentation outlines a system for giving players a quest and then teleporting them to a starting area with a
controlled delay in the game 7 Days to Die. The system uses a combination of an item definition and a temporary buff,
all configured in XML.

Quest Initiation and Delayed Teleportation System
This setup ensures that when a player interacts with a specific item, they receive a quest and are subsequently
teleported to the quest's starting location after a short delay. This delay is crucial to prevent in-game events from
firing prematurely before the quest is fully registered and acknowledged by the player.

-----

## Quest Initiation and Teleportation System

This system leverages an item to trigger a quest and a temporary buff to manage a delayed teleportation to the quest's
starting location.

### Item Configuration: `startLocation_RickClass`

```xml

<item name="startLocation_RickClass">
    <property name="Extends" value="questMaster"/>
    <property name="CreativeMode" value="None"/>
    <property class="Action0">
        <property name="QuestGiven" value="q_goToPOI"/>
    </property>
</item>
```

This XML snippet defines an **item** named `startLocation_RickClass` that serves as the entry point for the quest.

* **`<property name="Extends" value="questMaster"/>`**: This indicates that the item inherits properties from a
  `questMaster` base, suggesting it's designed specifically for quest-related functions.
* **`<property name="CreativeMode" value="None"/>`**: This property likely controls its availability in creative or
  debugging modes within the game.
* **`<property class="Action0"><property name="QuestGiven" value="q_goToPOI"/>`**: When this item is interacted with, it
  immediately **gives the player the quest identified by `q_goToPOI`**.

### Quest Configuration: `q_goToPOI`

```xml

<quest id="q_goToPOI">
    <property name="group_name_key" value="q_goToPOI"/>
    <property name="name_key" value="q_goToPOI1"/>
    <property name="subtitle_key" value="q_goToPOI1_subtitle"/>
    <property name="description_key" value="q_goToPOI1_description"/>
    <property name="icon" value="ui_game_symbol_book"/>
    <property name="category_key" value="quest"/>
    <property name="offer_key" value="q_goToPOI1_offer"/>
    <property name="difficulty" value="veryeasy"/>
    <property name="shareable" value="false"/>
    <property name="add_to_tier_complete" value="false"/>
    <property name="repeatable" value="false"/>
    <action type="TrackQuest"/>
    <action type="GiveBuffSDX, SCore">
        <property name="value" value="questBuff_teleport"/>
        <property name="on_complete" value="false"/>
        <property name="phase" value="1"/>
    </action>
    <objective type="GotoPOISDX, SCore" value="200-8000" phase="1">
        <property name="completion_distance" value="50"/>
        <property name="PrefabName" value="cabin_03"/>
    </objective>
    <reward type="Exp" value="500"/>
</quest>
```

This XML snippet defines an **quest** named `q_goToPOI`. This is used to handle the flow of the logic. It's a standard
quest with two
objectives.

* **`<action type="GiveBuffSDX, SCore">`**: This is a custom action that **applies a buff** to the player.
    * **`<property name="value" value="questBuff_teleport"/>`**: Specifies the name of the buff to be applied, which is
      `questBuff_teleport`.
    * **`<property name="on_complete" value="false"/>`**: This likely means the buff is applied immediately and doesn't
      wait for a previous action to complete.
    * **`<property name="phase" value="1"/>`**: This indicates the action is part of the first phase of the item's
      functionality.
* **`<objective type="GotoPOISDX, SCore" value="200-8000" phase="2">`**: This defines the primary objective of the
  `q_goToPOI` quest.
    * **`type="GotoPOISDX, SCore"`**: This specifies a custom objective type for navigating to a Point Of Interest (
      POI).
    * **`value="200-8000"`**: This likely defines a range for the POI's distance from the player.
    * **`phase="2"`**: This objective becomes active in the second phase of the quest.
    * **`<property name="completion_distance" value="50"/>`**: The player needs to be within 50 units of the POI to
      complete this objective.
    * **`<property name="PrefabName" value="cabin_03"/>`**: This specifies that the target POI is a prefab named
      `cabin_03`.

### Buff Configuration: `questBuff_teleport`

```xml

<buff name="questBuff_teleport" hiden="true">
    <stack_type value="ignore"/>
    <duration value="2"/>
    <effect_group>
        <triggered_effect trigger="onSelfBuffRemove" action="TeleportToQuest, SCore" quest="q_gotopoi"/>
    </effect_group>
</buff>
```

This XML snippet defines a **temporary buff** named `questBuff_teleport` that handles the delayed teleportation.

* **`<buff name="questBuff_teleport" hiden="true" >`**: Defines the buff. `hiden="true"` means it won't be visible in
  the player's buff list.
* **`<stack_type value="ignore"/>`**: This prevents multiple instances of the buff from stacking.
* **`<duration value="2"/>`**: The buff will last for **2 seconds**.
* **`<effect_group>`**: Contains the effects triggered by the buff.
    * **`<triggered_effect trigger="onSelfBuffRemove" action="TeleportToQuest, SCore" quest="q_gotopoi" />`**: This is
      the core of the delayed teleportation. When the `questBuff_teleport` buff expires (after 2 seconds), it triggers
      the `TeleportToQuest` action.
        * **`action="TeleportToQuest, SCore"`**: This is a custom action that teleports the player.
        * **`quest="q_gotopoi"`**: This parameter tells the `TeleportToQuest` action to teleport the player to the
          starting area associated with the `q_goToPOI` quest.

### System Flow Explained

1. **Quest Acceptance**: When the player interacts with the `startLocation_RickClass` item, the `q_goToPOI` quest is
   immediately given, and a pop-up window appears for the player to accept it.
2. **Buff Application**: Once the player clicks "OK" to accept the quest, the `GiveBuffSDX` action is triggered,
   applying the `questBuff_teleport` to the player.
3. **Delayed Teleportation**: The `questBuff_teleport` remains active for 2 seconds. During this brief period, the
   player can finish interacting with the quest acceptance window.
4. **Teleport Trigger**: After 2 seconds, the `questBuff_teleport` expires. This triggers the `onSelfBuffRemove` effect,
   which in turn executes the `TeleportToQuest` action, moving the player to the designated starting area for the
   `q_goToPOI` quest (e.g., `cabin_03`).

This design provides a crucial delay between quest acceptance and teleportation, preventing issues where triggered
events might fire before the quest is fully registered or accepted by the player.
