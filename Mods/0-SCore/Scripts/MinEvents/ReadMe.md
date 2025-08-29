MinEventActions are versatile triggers that can be added to various game elements (like buffs, items, or entity XMLs) to
execute specific actions in response to in-game events. They provide modders with powerful tools to create dynamic and
complex interactions.

Here's an overview of the custom MinEventActions available:

* **MinEventActionAddAdditionalOutput**: Adds extra items to a player's inventory or workstation output when a crafting
  recipe is completed, often with conditional requirements.
* **MinEventActionAddBuffByFactionSDX**: Applies a buff to entities based on their faction relationship with a specified
  entity or faction.
* **MinEventActionAddBuffToPrimaryPlayer**: Applies a specific buff directly to the primary local player.
* **MinEventActionAddFireDamage**: Applies fire damage to a block, initiating or intensifying a fire.
* **MinEventActionAddFireDamageCascade**: Spreads fire to neighboring flammable blocks, often with filtering options
  based on material properties.
* **MinEventActionAddScriptToArmour**: Dynamically attaches a custom C# script to a piece of armor.
* **MinEventActionAddScriptToTransform**: Dynamically attaches a custom C# script to a game object's transform.
* **MinEventActionAdjustTransformValues**: Adjusts the position, rotation, or scale values of a game object's transform.
* **MinEventActionAnimatorFireTriggerSDX**: Fires a specific trigger on an entity's animator.
* **MinEventActionAnimatorSetFloatSDX**: Sets a float parameter on an entity's animator.
* **MinEventActionAnimatorSetIntSDX**: Sets an integer parameter on an entity's animator.
* **MinEventActionAnimatorSDX**: A versatile animator action to set various animator parameters (e.g., bools, floats,
  integers, triggers).
* **MinEventActionAttachPrefabWithAnimationsToEntity**: Attaches a prefab (which may include animations) to an entity
  dynamically.
* **MinEventActionAutoRedeemChallenges**: Automatically redeems completed challenges for the player.
* **MinEventActionChangeFactionSDX**: Changes the faction of an entity.
* **MinEventActionChangeFactionSDX2**: An alternative or extended version of `MinEventActionChangeFactionSDX` for
  changing factions.
* **MinEventActionCheckFireProximity**: Checks if an entity is near an active fire.
* **MinEventActionCheckWeapon**: Checks the currently held weapon or item for specific properties or tags.
* **MinEventActionClearOwner**: Clears the owner of a specified entity.
* **MinEventActionClearStaleHires**: Clears out old or "stale" hired NPC data, likely for maintenance.
* **MinEventActionConvertItem**: Converts one item into another.
* **MinEventActionCreateItemSDX**: Creates and adds an item to an entity's inventory.
* **MinEventActionDespawnNPC**: Despawns a specified NPC entity.
* **MinEventActionExecuteConsoleCommand**: Executes a console command.
* **MinEventActionExecuteConsoleCommandCVars**: Executes a console command, allowing the use of CVars within the command
  string.
* **MinEventActionGiveQuestSDX**: Awards a specific quest to the player.
* **MinEventActionGuardClear**: Clears a "guard" order for an NPC.
* **MinEventActionGuardHere**: Sets an NPC's guard position to the current location and facing direction of the
  executing entity.
* **MinEventActionGuardThere**: Sets an NPC's guard position to the current location and facing direction of the NPC
  itself.
* **MinEventActionHideNPCSDX**: Hides a specified NPC entity.
* **MinEventActionModifyFactionSDX**: Modifies an entity's faction relationship with another faction by a specified
  value.
* **MinEventActionModifyRelatedFactionsSDX**: Modifies an entity's relationship with multiple related factions
  simultaneously.
* **MinEventActionModifySkillSDX**: Modifies a player's skill level.
* **MinEventActionNotifyTeam**: Sends a notification to the player's team.
* **MinEventActionNotifyTeamTeleport**: Notifies the player's team about an upcoming teleportation.
* **MinEventActionOpenWindow**: Opens a specific UI window.
* **MinEventActionPlayerLevelSDX**: Modifies a player's level.
* **MinEventActionPumpQuestSDX**: Advances or "pumps" the progress of a specific quest.
* **MinEventActionRandomLootSDX**: Generates random loot from a loot list and adds it to an entity's inventory.
* **MinEventActionRecalculateEncumbrance**: Forces a recalculation of the player's encumbrance.
* **MinEventActionResetTargetsSDX**: Resets the attack or revenge targets of an entity.
* **MinEventActionSetCVar**: Sets a CVar on an entity.
* **MinEventActionSetDateToCVar**: Sets a CVar to the current in-game date.
* **MinEventActionSetFactionRelationship**: Sets the numerical relationship value between two factions.
* **MinEventActionSetOrder**: Issues an order (e.g., stay, follow, guard) to an NPC.
* **MinEventActionSetOwner**: Sets the owner of an entity.
* **MinEventActionSetParticleAttractorFromAttackTarget**: Sets a particle attractor's target to the executing entity's
  attack target.
* **MinEventActionSetParticleAttractorFromPlayer**: Sets a particle attractor's target to the player.
* **MinEventActionSetParticleAttractorFromSource**: Sets a particle attractor's target to the source of the triggered
  effect.
* **MinEventActionSetRevengeTarget**: Sets a new revenge target for an entity.
* **MinEventActionShowToolTipSDX**: Displays a tooltip message.
* **MinEventActionSkillPointSDX**: Awards a skill point to the player.
* **MinEventActionSpawnBabySDX**: Spawns a "baby" version of an entity.
* **MinEventActionSpawnEntityAtPoint**: Spawns a specified entity at a given point in the world.
* **MinEventActionSwapWeapon**: Forces an entity to swap its currently held weapon.
* **MinEventActionTeamTeleportNow**: Instantly teleports the player's entire team to a specified location.
* **MinEventActionTeleport**: Teleports an entity to a specified location.
* **MinEventActionTeleportToQuest**: Teleports the player to a quest objective location.
* **MinEventActionToggleCamera**: Toggles a camera view or state.

### MinEventActions Examples

Here is the reformatted documentation for the custom MinEventActions, maintaining the requested structure and adding a summary to each:

### MinEventActions Examples

### MinEventActionAddAdditionalOutput
Adds extra items to a player's inventory or workstation output upon crafting completion, often with conditional requirements.

```xml
<triggered_effect trigger="onSelfItemCrafted" action="AddAdditionalOutput, SCore" item="resourceScrapPolymers" count="5"/>
```

When the item containing this effect is crafted, 5 `resourceScrapPolymers` are added to the output.


### Add Buff By Faction

Tracks and manages the application of buffs to entities based on faction alignment.

In an XML configuration file, the buff might be set up like this:

```xml
<triggered_effect trigger="onSelfBuffUpdate" action="AddBuffByFactionSDX, SCore" target="selfAOE" range="4" mustmatch="true" buff="buffAnimalFertility" />
```

This configuration defines an action where a buff is added to targets in a specified range, depending on whether their faction matches or differs from the entity's faction.

### Add Script To Transform

Tracks and manages the addition of scripts to specific transforms in a game entity, based on event triggers.

In an XML configuration file, the script addition might be set up like this:

```xml
<triggered_effect trigger="onSelfEnteredGame" action="AddScriptToTransform, SCore" transform="Camera" script="GlobalSnowEffect.GlobalSnow, BetterBiomeEffects"/>
```

This configuration defines an action where a script is added to a specific transform (like the camera) when the entity (player) enters the game.

### Adjust Transform Values

Tracks and manages the adjustment of transform values such as position and rotation for specific child objects in a game entity.

XML Snippet Relation:
In an XML configuration file, the transform adjustment might be set up like this:

```xml
<triggered_effect trigger="onSelfBuffUpdate" action="AdjustTransformValues, SCore" parent_transform="AK47" local_offset="-0.05607828,0.07183618,-0.02150292" local_rotation="-3.98,-9.826,-5.901" debug="true"/>
```

This configuration defines an action where the local position and rotation of a specified transform (like "AK47") are adjusted when a buff is updated on the entity.

### MinEventActionAdjustTransformValues

Tracks and manages the adjustment of transform values such as position and rotation for specific child objects in a game entity.

XML Snippet Relation:
In an XML configuration file, the transform adjustment might be set up like this:

```xml
<triggered_effect trigger="onSelfBuffUpdate" action="AdjustTransformValues, SCore" 
    parent_transform="AK47" 
    local_offset="-0.05607828,0.07183618,-0.02150292" 
    local_rotation="-3.98,-9.826,-5.901" 
    debug="true"/>
```

This configuration defines an action where the local position and rotation of a specified transform (like "AK47") are adjusted when a buff is updated on the entity.

### MinEventActionAnimatorFireTriggerSDX

Fires a specified trigger on all animators found on the target entities.

```xml
<triggered_effect trigger="onSelfBuffStart" action="AnimatorFireTriggerSDX, SCore" trigger="triggerName"/>
```

### MinEventActionAnimatorSpeedSDX

Adjusts the speed of the animator for target entities based on the specified value.

```xml
<triggered_effect trigger="onSelfBuffStart" action="AnimatorSpeedSDX, SCore" target="self" value="1"/> <!-- normal speed -->
<triggered_effect trigger="onSelfBuffStart" action="AnimatorSpeedSDX, SCore" target="self" value="2"/> <!-- twice the speed -->
```

### MinEventActionAnimatorSetFloatSDX

Sets a float property on the animator for the target entities, with an optional reference to a custom variable (CVar).

```xml
<triggered_effect trigger="onSelfBuffStart" action="AnimatorSetFloatSDX, SCore" target="self" property="speed" value="2"/>
<triggered_effect trigger="onSelfBuffStart" action="AnimatorSetFloatSDX, SCore" target="self" property="speed" value="@customSpeed"/>
```

### MinEventActionAnimatorSetIntSDX

Sets an integer property on the animator for the target entities, with an optional reference to a custom variable (CVar).

```xml
<triggered_effect trigger="onSelfBuffStart" action="AnimatorSetIntSDX, SCore" target="self" property="actionState" value="1"/>
<triggered_effect trigger="onSelfBuffStart" action="AnimatorSetIntSDX, SCore" target="self" property="actionState" value="@customActionState"/>
```

### MinEventActionAttachPrefabWithAnimationsToEntity

Attaches a prefab to an entity and enables any animator components within the entity's hierarchy, adding a `BlockClockScript` if it's not already present.

```xml
<triggered_effect trigger="onSelfBuffStart" action="AttachPrefabWithAnimationsToEntity, SCore" target="self"/>
```

### MinEventActionAutoRedeemChallenges

Automatically redeems completed challenges for the target entity by setting the challenge state to "Redeemed" and triggering the appropriate completion events.

```xml
<triggered_effect trigger="onSelfBuffUpdate" action="AutoRedeemChallenges, SCore"/>
```

### MinEventActionChangeFactionSDX

Changes the faction of the target entity based on a specified faction name, with the option to revert to the original faction.

```xml
<triggered_effect trigger="onSelfBuffStart" action="ChangeFactionSDX, SCore" target="self" value="bandits"/> <!-- change faction to bandits -->
<triggered_effect trigger="onSelfBuffStart" action="ChangeFactionSDX, SCore" target="self" value="undead"/> <!-- change faction to undead -->
<triggered_effect trigger="onSelfBuffStart" action="ChangeFactionSDX, SCore" target="self" value="original"/> <!-- revert to original faction -->
```

### MinEventActionChangeFactionSDX2

Changes the faction of the target entity based on a specified faction name, with the option to revert to the original faction. The event clears the entity's attack and revenge targets after the faction change.

```xml
<triggered_effect trigger="onSelfBuffStart" action="ChangeFactionSDX2, SCore" target="self" value="bandits"/> <!-- change faction to bandits -->
<triggered_effect trigger="onSelfBuffStart" action="ChangeFactionSDX2, SCore" target="self" value="undead"/> <!-- change faction to undead -->
<triggered_effect trigger="onSelfBuffStart" action="ChangeFactionSDX2, SCore" target="self" value="original"/> <!-- revert to original faction -->
```

### MinEventActionCheckWeapon

Checks the current weapon of the target entity by retrieving the weapon ID from a custom variable and updating the weapon accordingly.

```xml
<triggered_effect trigger="onSelfBuffUpdate" action="CheckWeapon, SCore"/>
```

### MinEventActionClearOwner

Clears the owner of a tile entity, such as a powered trigger, when the entity damages a block.

```xml
<triggered_effect trigger="onSelfDamagedBlock" action="ClearOwner, SCore"/>
```

### MinEventActionClearStaleHires

Checks and clears any stale or dangling hires associated with the entity.

```xml
<triggered_effect trigger="onSelfBuffUpdate" action="ClearStaleHires, SCore"/>
```

### MinEventActionConvertItem

Converts the current item being used by the entity into another item when its usage reaches a specified limit. The item is downgraded after a set number of uses.

```xml
<triggered_effect trigger="onSelfPrimaryActionEnd" action="ConvertItem, SCore" downgradeItem="meleeClub" maxUsage="10"/>
```

### MinEventActionCreateItemSDX

Creates an item or spawns items from a loot group when triggered. The items can be directly added to the player's inventory or dropped in the game world if the inventory is full.

```xml
<triggered_effect trigger="onSelfBuffRemove" action="CreateItemSDX, SCore" item="drinkJarCoffee" count="2"/>

<!-- It's not actually lootgroup, but rather, loot container ID. -->
<triggered_effect trigger="onSelfBuffRemove" action="CreateItemSDX, SCore" lootgroup="2" count="1"/>

<!-- lootgroup2= is actually the loot group from loot.xml. To maintain backward compatibility, the original is left untouched. -->
<triggered_effect trigger="onSelfBuffRemove" action="CreateItemSDX, SCore" lootgroup2="LootList" count="1" />
```

### MinEventActionDespawnNPC

Despawns the NPC entity by removing it from the game world when the action is triggered.

```xml
<triggered_effect trigger="onSelfBuffUpdate" action="DespawnNPC, SCore"/>
```

### MinEventActionExecuteConsoleCommand

Executes a specified console command when the action is triggered, either locally or sent to the server depending on whether the game is in client or server mode.

```xml
<triggered_effect trigger="onSelfBuffStart" action="ExecuteConsoleCommand, SCore" command="st night"/>
```

### MinEventActionExecuteConsoleCommandCVars

Executes a console command using values from specified custom variables (CVars) when the action is triggered. The command is assembled dynamically with the CVar values replacing placeholders in the command string.

```xml
<triggered_effect trigger="onSelfBuffStart" action="ExecuteConsoleCommandCVars, SCore" command="testCommand {0} {1}" cvars="cvar1,cvar2"/>
```

### MinEventActionGiveQuestSDX

Gives a specified quest to the target entity or player when the action is triggered.

```xml
<triggered_effect trigger="onSelfBuffStart" action="GiveQuestSDX, SCore" target="self" quest="myNewQuest"/>
```

### MinEventActionGuardClear

Clears the guard position and look position for an entity that can receive orders.

```xml
<triggered_effect trigger="onSelfBuffUpdate" action="GuardClear, SCore"/>
```

### MinEventActionGuardHere

Sets the guard position for an entity to either the position of its leader or owner, or to the entity's own position if no leader or owner is found. The guard look position is also updated based on the chosen position.

```xml
<triggered_effect trigger="onSelfBuffUpdate" action="GuardHere, SCore"/>
```
### GuardThere ###

Sets the guard position to match where the entity is standing.

```xml
<triggered_effect trigger="onSelfBuffUpdate" action="GuardThere" />
```

### MinEventActionHideNPCSDX

Hides or reveals the target NPC by sending it on a mission based on the specified "hide" attribute value.

```xml
<triggered_effect trigger="onSelfBuffUpdate" action="HideNPCSDX, SCore" hide="true"/>
```

### MinEventActionModifyFactionSDX

Modifies the relationship between the target entity and a specified faction by adding or subtracting points from the relationship value.

```xml
<triggered_effect trigger="onSelfBuffStart" action="ModifyFactionSDX, SCore" target="self" faction="bandits" value="10"/> <!-- Increases relationship with bandits by 10 points -->
<triggered_effect trigger="onSelfPrimaryActionEnd" action="ModifyFactionSDX, SCore" target="self" faction="undead" value="-10"/> <!-- Decreases relationship with undead by 10 points -->
```

### ModifyRelatedFactionsSDX

This action modifies relationships between factions when a specific primary action is taken.

```xml
<triggered_effect trigger="onSelfPrimaryActionEnd" action="ModifyRelatedFactionsSDX, SCore" target="self" faction="redteam" value="10" />
```

### ModifySkillSDX

This action modifies a player's skill when a specific event occurs.
```xml
<triggered_effect trigger="onSelfPrimaryActionEnd" action="ModifySkillSDX, SCore" target="self" skill="mining" value="2" />
```

### NotifyTeamAttack

This action notifies the team of that an entity is being attacked.
```xml
<triggered_effect trigger="onSelfPrimaryActionEnd" action="NotifyTeamAttack, SCore" target="selfAoE" />
```

### NotifyTeamTeleport

This action notifies the team of an entity that is teleporting.
```xml
<triggered_effect trigger="onSelfPrimaryActionEnd" action="NotifyTeamTeleport, SCore" target="self" />
```

### MinEventActionOpenWindow

Opens a specified UI window for the player when the action is triggered.

```xml
<triggered_effect trigger="onSelfDamagedBlock" action="OpenWindow, SCore" window="SCoreCompanionsGroup"/>
```

### MinEventActionPlayerLevelSDX

Increases the player's level by one if the player has not yet reached the maximum level, and displays a level-up tooltip.

```xml
<triggered_effect trigger="onSelfBuffStart" action="PlayerLevelSDX, SCore" target="self"/>
```

### MinEventActionPumpQuestSDX

Refreshes all objectives of the quests for the target entities when triggered.

```xml
<triggered_effect trigger="onSelfBuffStart" action="PumpQuestSDX, SCore" target="self"/>
```

### MinEventActionRandomLootSDX

Generates random loot for the target entity from a specified loot group when the action is triggered. The amount of loot can be influenced by a custom variable, such as `spLootExperience`.

```xml
<triggered_effect trigger="onSelfBuffRemove" action="RandomLootSDX, SCore" lootgroup="brassResource" count="1"/>
```

### MinEventActionRecalculateEncumbrance

Recalculates the encumbrance for the player entity when the action is triggered, ensuring the player's encumbrance status is up-to-date.

```xml
<triggered_effect trigger="onSelfBuffStart" action="RecalculateEncumbrance, SCore"/>
```

### MinEventActionResetTargetsSDX

Resets the attack and revenge targets for the target entities when the action is triggered.

```xml
<triggered_effect trigger="onSelfBuffStart" action="ResetTargetsSDX, SCore"/>
```

### MinEventActionSetCVar

Sets a custom variable (CVar) for the target entities within a specified range, associating the CVar with the entity that triggered the action.

```xml
<triggered_effect trigger="onSelfBuffUpdate" action="SetCVar, SCore" target="selfAOE" range="4" cvar="Leader"/>
```

### MinEventActionSetDateToCVar

Sets the current in-game day to a custom variable (CVar) when the action is triggered.

```xml
<triggered_effect trigger="onSelfBuffStart" action="SetDateToCVar, SCore" target="self"/>
```

### MinEventActionSetFactionRelationship

Sets the target's relationship with a specified faction to a new value, adjusting how the faction perceives the target entity.

```xml
<!-- Sets the player's relationship with bandits to 400 ("Neutral"). -->
<triggered_effect trigger="onSelfBuffStart" action="SetFactionRelationshipSDX, Mods" target="self" faction="bandits" value="400"/>
```

### MinEventActionSetOrder

Sets a specific order for the target entity, such as "Stay" or "Wander", when the action is triggered.

```xml
<triggered_effect trigger="onSelfBuffUpdate" action="SetOrder, SCore" value="Stay"/>
```

### MinEventActionSetOwner

Sets the owner of a `TileEntityPoweredTrigger` to the local player when the entity damages a block.

```xml
<triggered_effect trigger="onSelfDamagedBlock" action="SetOwner, SCore"/>

```

### MinEventActionSetRevengeTarget

Sets the revenge target of the target entities to the specified entity ID. If the value is 0 or omitted, the revenge target will be cleared.

```xml
<!-- Sets the revenge target of the entity you're attacking to yourself -->
<triggered_effect trigger="onSelfAttackedOther" action="SetRevengeTarget, SCore" target="other" value="@_entityId"/>

<!-- Clears the revenge target of the entity that damaged you -->
<triggered_effect trigger="onOtherDamagedSelf" action="SetRevengeTarget, SCore" target="other" value="0"/>
```

### MinEventActionShowToolTipSDX

Displays a tooltip message to the player, optionally playing a sound and setting a title, when the action is triggered.

```xml
<triggered_effect trigger="onSelfBuffStart" action="ShowToolTipSDX, SCore" message_key="tipMessage" sound="tipSound" title_key="tipTitle"/>
```

### MinEventActionSkillPointSDX

Awards the target player entity a specified number of skill points when the action is triggered.

```xml
<triggered_effect trigger="onSelfBuffStart" action="SkillPointSDX, SCore" target="self" value="2"/> <!-- Adds 2 skill points -->

```

### MinEventActionSpawnBabySDX

A dummy class for backwards compatibility that inherits from `MinEventActionSpawnEntitySDX`.

### MinEventActionAddBuffToPrimaryPlayer

Adds a specified buff to the primary player when the action is triggered.

```xml
<triggered_effect trigger="onSelfBuffStart" action="AddBuffToPrimaryPlayer, SCore" buff="buffName"/>
```

### MinEventActionSpawnEntityAtPoint

Spawns an entity from a specified spawn group at a designated point, such as the impact point of a projectile, when the action is triggered.

```xml
<triggered_effect trigger="onProjectileImpact" action="SpawnEntityAtPoint, SCore" SpawnGroup="ZombiesBurntForest"/>
```

### MinEventActionSwapWeapon

Swaps the target entity's current weapon with a specified weapon when the action is triggered.

```xml
<triggered_effect trigger="onSelfBuffUpdate" action="SwapWeapon, SCore" item="meleeClub"/>
```

### MinEventActionTeamTeleportNow

Teleports hired NPCs in the player's team who are within a certain distance to the player's location. NPCs who are guarding or too far away are not teleported.

```xml
<triggered_effect trigger="onSelfBuffUpdate" action="NotifyTeamTeleportNow, SCore"/>
```

### MinEventActionTeleport

Teleports the player to a specified location when the action is triggered, using a defined portal.

```xml
<triggered_effect trigger="onSelfBuffStart" action="Teleport, SCore" location="Portal01"/>
```

### MinEventActionTeleportToQuest

Teleports the player to the position of a specified quest when the action is triggered.

```xml
<triggered_effect trigger="onSelfBuffStart" action="TeleportToQuest, SCore" quest="sorcery_trader_arcane"/>
```

### MinEventActionToggleCamera

Toggles the camera's enabled status based on the specified attributes when the action is triggered.

```xml
<triggered_effect trigger="onSelfBuffStart" action="ToggleCamera, SCore" cameraName="Camera" value="false"/> <!-- Disables the camera -->
<triggered_effect trigger="onSelfBuffFinish" action="ToggleCamera, SCore" cameraName="Camera" value="true"/> <!-- Enables the camera -->
```

### `MinEventActionShowPerkLevelUp, SCore` ( Under LearnByDoing Feature)

This `MinEventAction` is designed to display a custom tooltip message to a player when a specified perk levels up. It provides clear and localized feedback about perk progression.

```xml
    <triggered_effect trigger="onSelfBuffStart" action="ShowPerkLevelUp, SCore" perk="skillArchery" sound="read_skillbook_final" />
```

**Explanation**: This action takes the following attributes:

* **`name="ShowPerkLevelUp, SCore"`**: The name of the action.
* **`perk`**: (Required) A string representing the internal ID of the perk whose level-up message should be displayed (e.g., `perkPummelPete`, `skillPistols`). The value is converted to lowercase for comparison.
* **`sound`**: (Optional, inherited from `MinEventActionShowToolbeltMessage`) Specifies a sound event to play concurrently with the tooltip message (e.g., `read_skillbook_final`).

**Message Construction:**
The action constructs the message automatically using localized strings. The format is:
`"[Localized LearnByDoingLevelUp phrase] [Localized PerkName]!"`

**Example Output:**
"Skill Level Up\! You are a better Demolitions Expert\!"

**Localization Keys Used:**
The action relies on specific localization keys from your `Localization.txt` file to construct the message:

* `LearnByDoingLevelUp`: For the generic "Level Up\!" phrase (e.g., "Skill Level Up\!").
* `[PerkName]Name`: For the perk's full localized name (e.g., `perkPummelPeteName` for "Demolitions Expert").

**Example Usage:**

You would typically use this action within a `triggered_effect` that responds to progression events, such as `onProgressionLevelUp` or `onSelfBuffStart` (if a buff is used to indicate level-up):

```xml
<effect_group>
    <triggered_effect trigger="onSelfBuffStart" action="ShowPerkLevelUp, SCore" perk="skillArchery" sound="read_skillbook_final" />
</effect_group>
```

## Additional Recipe Output (via `MinEventActionAddAdditionalOutput`)

This feature allows recipes to define supplementary items that are produced alongside the primary crafted item. These
additional outputs can be conditional, providing greater flexibility in recipe design.

* **Functionality**: When a recipe is crafted, the `onSelfItemCrafted` trigger is invoked. Any associated requirements
  are then processed, and if the conditions (e.g., a `<requirement>` within the `triggered_effect`) are met, the
  specified additional items will be added to the player's inventory or the workstation's output. The system also
  includes checks for full inventory or workstation output, halting crafting if there is not enough space in the output slots and providing a
  tooltip notification.

  **Note on Triggering MinEvents**: While many `MinEvents` triggered by `onSelfItemCrafted` typically require the player
  to be actively looking into their backpack or the workstation's interface for the effects to fire, the
  `AddAdditionalOutput` action does **not** have this limitation. It will deliver the additional items regardless of
  whether the player is viewing their inventory or the workstation output at the exact moment of crafting completion.

* AddAdditionalOutput also only works as a MinEvent in onSelfItemCrafted. It has no Execute() code, so it does nothing.
  This is only used to store data.

* **XML Usage**: You can integrate this feature into your `recipes.xml` (or equivalent item configuration files) within
  an `<effect_group>` using the `onSelfItemCrafted` trigger.

  **Example:**

  ```xml
  <recipe name="ammo45ACPCase" count="30" craft_time="5" craft_area="MillingMachine" tags="workbenchCrafting,PerkHOHMachineGuns">
      <ingredient name="resourceBrassIngot" count="5"/>

      <effect_group name="Additional Output">
          <triggered_effect trigger="onSelfItemCrafted" action="AddAdditionalOutput, SCore" item="resourceYuccaFibers" count="2"/>
          <triggered_effect trigger="onSelfItemCrafted" action="AddAdditionalOutput, SCore" item="resourceDuctTape" count="1"/>
      </effect_group>
      
      <effect_group name="Conditional Output Example">

          <triggered_effect trigger="onSelfItemCrafted" action="AddAdditionalOutput, SCore" item="ammoRocketHE" count="2">
              <requirement name="HasBuff" buff="god"/> 
          </triggered_effect>

          <triggered_effect trigger="onSelfItemCrafted" action="PlaySound" sound="player#painsm">
              <requirement name="!HasBuff" buff="god"/> 
          </triggered_effect>

          <triggered_effect trigger="onSelfItemCrafted" action="AddBuff" buff="buffDrugEyeKandy"/>
      </effect_group>
  </recipe>
  ```

* **Properties for `MinEventActionAddAdditionalOutput`**:

  * **`item` (string)**: Specifies the `name` of the item to be added as additional output.
  * **`count` (integer)**: Defines the quantity of the `item` to be added.




