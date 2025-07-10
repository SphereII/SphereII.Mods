Here's a revised summary of the custom Actions and Requirements, which extend the game's dialogue system for more dynamic and conditional conversations.

### Actions

These actions are executed when a dialogue option is chosen, allowing for various effects to occur within the game.

* **ActionAddItemRebirth**: Adds an item to the player's inventory, often used in conjunction with entity rebirth mechanics.
* **ActionAddItemSDX**: Adds a specified item to the player's inventory.
* **ActionAddCVar**: Adds or sets a CVar (Console Variable) on a specified entity (e.g., player or NPC).
* **ActionAddCVarSelf**: Adds or sets a CVar specifically on the entity executing the dialogue action.
* **ActionAddBuffSDX**: Applies a specified buff to an entity.
* **ActionAnimatorSet**: Sets parameters (like booleans, integers, floats) on an entity's animator.
* **ActionDespawnEntityRebirth**: Despawns an entity, potentially triggering a rebirth mechanic.
* **ActionDisplayInfo**: Displays a message or information to the player.
* **ActionExecuteCommandSDX**: Executes a console command from within the dialogue system.
* **ActionGetItemSDX**: Allows the player to receive an item as part of the dialogue action.
* **ActionGiveToNPC**: Transfers items from the player to the NPC engaging in dialogue.
* **ActionGiveQuestSDX**: Awards a specific quest to the player.
* **ActionOpenDialogSDX**: Opens another specified dialogue, allowing for branching or sequential conversations.
* **ActionPickUpNPC**: Allows the player to "pick up" an NPC, effectively adding them to inventory or making them a follower.
* **ActionPlaySoundSDX**: Plays a specified sound effect.
* **ActionRemoveBuffNPCSDX**: Removes a specified buff from an NPC.
* **ActionRemoveBuffSDX**: Removes a specified buff from the player or other entity.
* **ActionRewardSkillPoint**: Rewards the player with a skill point.
* **ActionShowToolTipSDX**: Displays a tooltip message to the player.
* **ActionSwapWeapon**: Forces an entity to swap their currently held weapon.
* **ActionTeleport**: Teleports an entity to a specified location.

### Requirements

These requirements define conditions that must be met for a dialogue option or statement to be visible or selectable.

* **RequirementBuffSDX**: Checks if an entity has a specific buff.
* **RequirementCVar**: Checks if an entity has a CVar (Console Variable) and optionally if it matches a specific value or range.
* **RequirementCompletedQuestSDX**: Checks if the player has completed a specific quest.
* **RequirementEnemyNearby**: Checks if an enemy entity is nearby.
* **RequirementFaction**: Checks the faction of an entity.
* **RequirementFactionValue**: Checks the numerical relationship value between two factions.
* **RequirementFailedQuestSDX**: Checks if the player has failed a specific quest.
* **RequirementHasItemSDX**: Checks if an entity has a specific item in their inventory.
* **RequirementHasPackage**: Checks if an entity has a specific AI package active.
* **RequirementHasPlayerLevelSDX**: Checks if the player has reached a specific level.
* **RequirementHasQuestSDX**: Checks if the player currently has a specific quest active.
* **RequirementHasTag**: Checks if an entity has a specific tag.
* **RequirementHasTask**: Checks if an entity has a specific AI task active.
* **RequirementHiredSDX**: Checks if an entity is currently hired by the player.
* **RequirementIsSleeper**: Checks if an entity is currently in a "sleeper" state.
* **RequirementIsTrader**: Checks if the entity is a trader.
* **RequirementLeader**: Checks if the entity is the leader of a group.
* **RequirementNPCHasCVar**: Checks if an NPC has a specific CVar.
* **RequirementNPCHasItemSDX**: Checks if an NPC has a specific item in their inventory.
* **RequirementNotHaveItemSDX**: Checks if an entity *does not* have a specific item in their inventory.
* **RequirementRandomRoll**: Performs a random roll to determine if the requirement is met.



### Actions

#### 1\. `ActionAddItemRebirth`

Adds an item to the player's inventory, often used with entity rebirth mechanics.

```xml
<action type="AddItemRebirth, SCore" item="resourceScrapIron" count="10" />
```

**Explanation**: Adds 10 `resourceScrapIron` to the player's inventory.

#### 2\. `ActionAddItemSDX`

Adds a specified item to the player's inventory.

```xml
<action type="AddItemSDX, SCore" item="resourcePaper" count="5" />
```

**Explanation**: Adds 5 `resourcePaper` to the player's inventory.

#### 3\. `ActionAddCVar`

Adds or sets a CVar on a specified entity.

```xml
<action type="AddCVar, SCore" target="self" cvar="myCustomCVar" value="1" />
```

**Explanation**: Sets `myCustomCVar` to a value of `1` on the entity executing the action.

#### 4\. `ActionAddCVarSelf`

Adds or sets a CVar specifically on the entity executing the dialogue action.

```xml
<action type="AddCVarSelf, SCore" cvar="playerQuestProgress" value="10" />
```

**Explanation**: Sets `playerQuestProgress` to `10` on the player.

#### 5\. `ActionAddBuffSDX`

Applies a specified buff to an entity.

```xml
<action type="AddBuffSDX, SCore" target="self" buff="buffStaminaRegen"/>
```

**Explanation**: Applies the `buffStaminaRegen` to the entity executing the action.

#### 6\. `ActionAnimatorSet`

Sets parameters on an entity's animator.

```xml
<action type="AnimatorSet, SCore" target="self" parameter="IsTalking" value="true"/>
```

**Explanation**: Sets the boolean animator parameter "IsTalking" to `true` on the entity.

#### 7\. `ActionDespawnEntityRebirth`

Despawns an entity, potentially triggering a rebirth mechanic.

```xml
<action type="DespawnEntityRebirth, SCore" target="self"/>
```

**Explanation**: Despawns the entity executing the action.

#### 8\. `ActionDisplayInfo`

Displays a message or information to the player.

```xml
<action type="DisplayInfo, SCore" text="msgMyCustomInfo" sound="ui_button_click"/>
```

**Explanation**: Displays a message from the localization key `msgMyCustomInfo` and plays `ui_button_click` sound.

#### 9\. `ActionExecuteCommandSDX`

Executes a console command from within the dialogue system.

```xml
<action type="ExecuteCommandSDX, SCore" command="giveself xp 1000"/>
```

**Explanation**: Executes the console command `giveself xp 1000`.

#### 10\. `ActionGetItemSDX`

Allows the player to receive an item as part of the dialogue action.

```xml
<action type="GetItemSDX, SCore" item="resourcePaper" count="10"/>
```

**Explanation**: Gives 10 `resourcePaper` to the player.

#### 11\. `ActionGiveToNPC`

Transfers items from the player to the NPC engaging in dialogue.

```xml
<action type="GiveToNPC, SCore" item="casinoCoin" count="100" />
```

**Explanation**: Takes 100 `casinoCoin` from the player and gives it to the NPC.

#### 12\. `ActionGiveQuestSDX`

Awards a specific quest to the player.

```xml
<action type="GiveQuestSDX, SCore" id="quest_newcomer"/>
```

**Explanation**: Gives the player the quest with ID `quest_newcomer`.

#### 13\. `ActionOpenDialogSDX`

Opens another specified dialogue.

```xml
<action type="OpenDialogSDX, SCore" id="dialog_trader_special"/>
```

**Explanation**: Opens the dialogue with ID `dialog_trader_special`.

#### 14\. `ActionPickUpNPC`

Allows the player to "pick up" an NPC.

```xml
<action type="PickUpNPC, SCore" target="self"/>
```

**Explanation**: The NPC executing this action will be "picked up" (e.g., added to player inventory or become a follower).

#### 15\. `ActionPlaySoundSDX`

Plays a specified sound effect.

```xml
<action type="PlaySoundSDX, SCore" sound="impact_metal_light"/>
```

**Explanation**: Plays the `impact_metal_light` sound.

#### 16\. `ActionRemoveBuffNPCSDX`

Removes a specified buff from an NPC.

```xml
<action type="RemoveBuffNPCSDX, SCore" buff="buffStunned"/>
```

**Explanation**: Removes the `buffStunned` from the NPC.

#### 17\. `ActionRemoveBuffSDX`

Removes a specified buff from the player or other entity.

```xml
<action type="RemoveBuffSDX, SCore" target="self" buff="buffBleeding"/>
```

**Explanation**: Removes the `buffBleeding` from the entity executing the action.

#### 18\. `ActionRewardSkillPoint`

Rewards the player with a skill point.

```xml
<action type="RewardSkillPoint, SCore" amount="1"/>
```

**Explanation**: Rewards the player with 1 skill point.

#### 19\. `ActionShowToolTipSDX`

Displays a tooltip message to the player.

```xml
<action type="ShowToolTipSDX, SCore" text="msgCustomToolTip"/>
```

**Explanation**: Displays a tooltip with text from the localization key `msgCustomToolTip`.

#### 20\. `ActionSwapWeapon`

Forces an entity to swap their currently held weapon.

```xml
<action type="SwapWeapon, SCore" target="self"/>
```

**Explanation**: Forces the entity to switch their current weapon.

#### 21\. `ActionTeleport`

Teleports an entity to a specified location.

```xml
<action type="Teleport, SCore" target="self" x="100" y="70" z="200"/>
```

**Explanation**: Teleports the entity to coordinates (100, 70, 200).



### Requirements

#### 1\. `RequirementBuffSDX`

Checks if an entity has a specific buff.

```xml
<requirement type="BuffSDX, SCore" target="self" buff="buffStaminaFull" />
```

**Explanation**: Requires the entity to have the `buffStaminaFull`.

#### 2\. `RequirementCVar`

Checks if an entity has a CVar (Console Variable) and optionally if it matches a specific value or range.

```xml
<requirement type="CVar, SCore" target="self" cvar="playerScore" value="100" operator="GTE" />
```

**Explanation**: Requires `playerScore` on the target to be Greater Than or Equal to 100. `operator` can be `LT`, `LTE`, `GT`, `GTE`, `EQ`, `NEQ`.

#### 3\. `RequirementCompletedQuestSDX`

Checks if the player has completed a specific quest.

```xml
<requirement type="CompletedQuestSDX, SCore" id="quest_trader_first_quest" />
```

**Explanation**: Requires the player to have completed the quest with ID `quest_trader_first_quest`.

#### 4\. `RequirementEnemyNearby`

Checks if an enemy entity is nearby.

```xml
<requirement type="EnemyNearby, SCore" target="self" range="15" />
```

**Explanation**: Requires an enemy entity to be within 15 blocks of the entity.

#### 5\. `RequirementFaction`

Checks the faction of an entity.

```xml
<requirement type="Faction, SCore" target="self" faction="undead" />
```

**Explanation**: Requires the entity to belong to the "undead" faction.

#### 6\. `RequirementFactionValue`

Checks the numerical relationship value between two factions.

```xml
<requirement type="FactionValue, SCore" target_faction="traders" other_faction="bandits" value="0" operator="LTE" />
```

**Explanation**: Requires the relationship value between "traders" and "bandits" factions to be Less Than or Equal to 0. `operator` can be `LT`, `LTE`, `GT`, `GTE`, `EQ`, `NEQ`.

#### 7\. `RequirementFailedQuestSDX`

Checks if the player has failed a specific quest.

```xml
<requirement type="FailedQuestSDX, SCore" id="quest_rescue_survivor" />
```

**Explanation**: Requires the player to have failed the quest with ID `quest_rescue_survivor`.

#### 8\. `RequirementHasItemSDX`

Checks if an entity has a specific item in their inventory.

```xml
<requirement type="HasItemSDX, SCore" target="self" item="gun9mmPistol" count="1" />
```

**Explanation**: Requires the entity to have at least 1 `gun9mmPistol` in their inventory.

#### 9\. `RequirementHasPackage`

Checks if an entity has a specific AI package active.

```xml
<requirement type="HasPackage, SCore" target="self" package="Patrol"/>
```

**Explanation**: Requires the entity to have the "Patrol" AI package active.

#### 10\. `RequirementHasPlayerLevelSDX`

Checks if the player has reached a specific level.

```xml
<requirement type="HasPlayerLevelSDX, SCore" value="50" operator="GTE" />
```

**Explanation**: Requires the player's level to be Greater Than or Equal to 50. `operator` can be `LT`, `LTE`, `GT`, `GTE`, `EQ`, `NEQ`.

#### 11\. `RequirementHasQuestSDX`

Checks if the player currently has a specific quest active.

```xml
<requirement type="HasQuestSDX, SCore" id="quest_fetch_medicine" />
```

**Explanation**: Requires the player to currently have the quest with ID `quest_fetch_medicine` active.

#### 12\. `RequirementHasTag`

Checks if an entity has a specific tag.

```xml
<requirement type="HasTag, SCore" target="self" tag="robot" />
```

**Explanation**: Requires the entity to have the "robot" tag.

#### 13\. `RequirementHasTask`

Checks if an entity has a specific AI task active.

```xml
<requirement type="HasTask, SCore" target="self" task="Guard" />
```

**Explanation**: Requires the entity to have the "Guard" AI task active.

#### 14\. `RequirementHiredSDX`

Checks if an entity is currently hired by the player.

```xml
<requirement type="HiredSDX, SCore" target="self" />
```

**Explanation**: Requires the entity to be currently hired by the player.

#### 15\. `RequirementIsSleeper`

Checks if an entity is currently in a "sleeper" state.

```xml
<requirement type="IsSleeper, SCore" target="self" />
```

**Explanation**: Requires the entity to be in a "sleeper" state.

#### 16\. `RequirementIsTrader`

Checks if the entity is a trader.

```xml
<requirement type="IsTrader, SCore" target="self" />
```

**Explanation**: Requires the entity to be a trader.

#### 17\. `RequirementLeader`

Checks if the entity is the leader of a group.

```xml
<requirement type="Leader, SCore" target="self" />
```

**Explanation**: Requires the entity to be the leader of a group.

#### 18\. `RequirementNPCHasCVar`

Checks if an NPC has a specific CVar.

```xml
<requirement type="NPCHasCVar, SCore" target="self" cvar="npcSkillLevel" value="5" operator="GTE" />
```

**Explanation**: Requires the NPC to have `npcSkillLevel` CVar Greater Than or Equal to 5. `operator` can be `LT`, `LTE`, `GT`, `GTE`, `EQ`, `NEQ`.

#### 19\. `RequirementNPCHasItemSDX`

Checks if an NPC has a specific item in their inventory.

```xml
<requirement type="NPCHasItemSDX, SCore" target="self" item="meleeToolAxeT1StoneAxe" count="1" />
```

**Explanation**: Requires the NPC to have at least 1 `meleeToolAxeT1StoneAxe`.

#### 20\. `RequirementNotHaveItemSDX`

Checks if an entity *does not* have a specific item in their inventory.

```xml
<requirement type="NotHaveItemSDX, SCore" target="self" item="foodCanChili" />
```

**Explanation**: Requires the entity to *not* have `foodCanChili` in their inventory.

#### 21\. `RequirementRandomRoll`

Performs a random roll to determine if the requirement is met.

```xml
<requirement type="RandomRoll, SCore" value="0.5" />
```

**Explanation**: A 50% chance for the requirement to be met. `value` is a float from 0.0 to 1.0.