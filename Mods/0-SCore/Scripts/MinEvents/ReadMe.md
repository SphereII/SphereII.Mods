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

Here are conceptual XML examples for each of the custom MinEventActions, illustrating how they might be used within a
`<triggered_effect>` tag. These effects can be attached to various game elements like buffs, item actions, or entity
definitions, depending on the `trigger` you specify.

**Note**: These are conceptual examples based on the C\# script's parsing logic and typical `7 Days to Die` modding
patterns. The exact `trigger` for each effect would depend on the context where you apply it.

### MinEventActions Examples

#### 1\. `MinEventActionAddAdditionalOutput`

Adds extra items to a player's inventory or workstation output upon crafting completion.

```xml

<triggered_effect trigger="onSelfItemCrafted" action="AddAdditionalOutput, SCore" item="resourceScrapPolymers"
                  count="5"/>
```

**Explanation**: When the item containing this effect is crafted, 5 `resourceScrapPolymers` are added to the output.

#### 2\. `MinEventActionAddBuffByFactionSDX`

Applies a buff to entities based on their faction relationship.

```xml

<triggered_effect trigger="onSelfTargetAttacked" action="AddBuffByFactionSDX, SCore" target="other" faction="player"
                  buff="buffConcussion" operator="EQ" value="0"/>
```

**Explanation**: When the entity's target is attacked, if the target's relationship with the "player" faction is exactly
0, apply `buffConcussion` to the target.

#### 3\. `MinEventActionAddBuffToPrimaryPlayer`

Applies a specific buff directly to the primary local player.

```xml

<triggered_effect trigger="onSelfPrimaryActionEnd" action="AddBuffToPrimaryPlayer, SCore" buff="buffStatHealthRegen"/>
```

**Explanation**: After a primary action, `buffStatHealthRegen` is applied to the local player.

#### 4\. `MinEventActionAddFireDamage`

Applies fire damage to a block.

```xml

<triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamage, SCore" target="positionAOE" range="3"/>
```

**Explanation**: When the entity damages a block, fire damage is applied in a 3-block radius around the hit position.

#### 5\. `MinEventActionAddFireDamageCascade`

Spreads fire to neighboring flammable blocks.

```xml

<triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamageCascade, SCore" range="4" filter="Material"/>
```

**Explanation**: When the entity damages a block, fire spreads to neighboring blocks up to 4 blocks away if they share
the same material.

#### 6\. `MinEventActionAddScriptToArmour`

Dynamically attaches a custom C\# script to a piece of armor.

```xml

<triggered_effect trigger="onSelfEquipStart" action="AddScriptToArmour, SCore"
                  script="MyCustomArmorScript, MyModAssembly"/>
```

**Explanation**: When the armor piece is equipped, `MyCustomArmorScript` is attached to it.

#### 7\. `MinEventActionAddScriptToTransform`

Dynamically attaches a custom C\# script to a game object's transform.

```xml

<triggered_effect trigger="onSelfPrimaryActionEnd" action="AddScriptToTransform, SCore"
                  script="MyCustomScript, MyModAssembly" transform="FireTransform"/>
```

**Explanation**: After a primary action, `MyCustomScript` is attached to the transform named "FireTransform".

#### 8\. `MinEventActionAdjustTransformValues`

Adjusts the position, rotation, or scale values of a game object's transform.

```xml

<triggered_effect trigger="onSelfEquipStart" action="AdjustTransformValues, SCore" target_transform="Flashlight"
                  position_offset="0,0.1,0"/>
```

**Explanation**: When equipped, adjusts the "Flashlight" transform's position.

#### 9\. `MinEventActionAnimatorFireTriggerSDX`

Fires a specific trigger on an entity's animator.

```xml

<triggered_effect trigger="onSelfBuffStart" action="AnimatorFireTriggerSDX, SCore" target="self" Animator_trigger="Stunned"/>
```

**Explanation**: When the buff starts, fires the "Stunned" trigger on the entity's animator.

#### 10\. `MinEventActionAnimatorSetFloatSDX`

Sets a float parameter on an entity's animator.

```xml

<triggered_effect trigger="onSelfBuffUpdate" action="AnimatorSetFloatSDX, SCore" target="self" parameter="RunSpeed"
                  value="1.5"/>
```

**Explanation**: On buff update, sets the "RunSpeed" float parameter to 1.5 on the entity's animator.

#### 11\. `MinEventActionAnimatorSetIntSDX`

Sets an integer parameter on an entity's animator.

```xml

<triggered_effect trigger="onSelfBuffUpdate" action="AnimatorSetIntSDX, SCore" target="self" parameter="AttackState"
                  value="2"/>
```

**Explanation**: On buff update, sets the "AttackState" integer parameter to 2 on the entity's animator.

#### 12\. `MinEventActionAnimatorSDX`

A versatile animator action to set various animator parameters.

```xml

<triggered_effect trigger="onSelfDismember" action="AnimatorSDX, SCore" target="self" parameter="IsDismembered"
                  type="Bool" value="true"/>
```

**Explanation**: On dismemberment, sets the "IsDismembered" boolean animator parameter to `true`.

#### 13\. `MinEventActionAttachPrefabWithAnimationsToEntity`

Attaches a prefab (which may include animations) to an entity dynamically.

```xml

<triggered_effect trigger="onSelfBuffStart" action="AttachPrefabWithAnimationsToEntity, SCore"
                  prefab="myCustomEffectPrefab" parent_transform="Head"/>
```

**Explanation**: When the buff starts, `myCustomEffectPrefab` is attached to the entity's "Head" transform.

#### 14\. `MinEventActionAutoRedeemChallenges`

Automatically redeems completed challenges for the player.

```xml

<triggered_effect trigger="onSelfBuffEnd" action="AutoRedeemChallenges, SCore"/>
```

**Explanation**: When the buff ends, all completed challenges are automatically redeemed for the player.

#### 15\. `MinEventActionChangeFactionSDX`

Changes the faction of an entity.

```xml

<triggered_effect trigger="onSelfDamaged" action="ChangeFactionSDX, SCore" target="self" value="bandits"/>
```

**Explanation**: When the entity is damaged, its faction is changed to "bandits".

#### 16\. `MinEventActionChangeFactionSDX2`

An alternative or extended version of `MinEventActionChangeFactionSDX` for changing factions.

```xml

<triggered_effect trigger="onSelfDamaged" action="ChangeFactionSDX2, SCore" target="self" value="undead"/>
```

**Explanation**: Similar to `ChangeFactionSDX`, changes the entity's faction to "undead" when damaged.

#### 17\. `MinEventActionCheckFireProximity`

Checks if an entity is near an active fire.

```xml

<triggered_effect trigger="onSelfBuffUpdate" action="CheckFireProximity, SCore" range="10" cvar="_closeFires"/>
```

**Explanation**: On buff update, checks for fires within 10 blocks and stores the count in `_closeFires` CVar.

#### 18\. `MinEventActionCheckWeapon`

Checks the currently held weapon or item for specific properties or tags.

```xml

<triggered_effect trigger="onSelfPrimaryActionStart" action="CheckWeapon, SCore" item_tags="knife"/>
```

**Explanation**: Before a primary action, checks if the held weapon has the "knife" tag.

#### 19\. `MinEventActionClearOwner`

Clears the owner of a specified entity.

```xml

<triggered_effect trigger="onSelfDeath" action="ClearOwner, SCore" target="self"/>
```

**Explanation**: On death, clears the ownership of the entity.

#### 20\. `MinEventActionClearStaleHires`

Clears out old or "stale" hired NPC data, likely for maintenance.

```xml

<triggered_effect trigger="onSelfRespawn" action="ClearStaleHires, SCore"/>
```

**Explanation**: On player respawn, clears any stale hired NPC data.

#### 21\. `MinEventActionConvertItem`

Converts one item into another.

```xml

<triggered_effect trigger="onSelfPrimaryActionEnd" action="ConvertItem, SCore" from_item="foodCanChili"
                  to_item="foodCanDogFood"/>
```

**Explanation**: After a primary action, converts `foodCanChili` in inventory to `foodCanDogFood`.

#### 22\. `MinEventActionCreateItemSDX`

Creates and adds an item to an entity's inventory.

```xml

<triggered_effect trigger="onSelfPrimaryActionEnd" action="CreateItemSDX, SCore" target="self" item="gun9mmPistol"
                  count="1"/>
```

**Explanation**: After a primary action, adds 1 `gun9mmPistol` to the entity's inventory.

#### 23\. `MinEventActionDespawnNPC`

Despawns a specified NPC entity.

```xml

<triggered_effect trigger="onSelfDeath" action="DespawnNPC, SCore" target="self"/>
```

**Explanation**: Despawns the NPC upon its death.

#### 24\. `MinEventActionExecuteConsoleCommand`

Executes a console command.

```xml

<triggered_effect trigger="onSelfBuffStart" action="ExecuteConsoleCommand, SCore" command="giveself xp 100"/>
```

**Explanation**: When the buff starts, executes the console command `giveself xp 100`.

#### 25\. `MinEventActionExecuteConsoleCommandCVars`

Executes a console command, allowing the use of CVars within the command string.

```xml

<triggered_effect trigger="onSelfBuffStart" action="ExecuteConsoleCommandCVars, SCore"
                  command="setcvar myQuestVar {myQuestCVar}"/>
```

**Explanation**: Executes `setcvar myQuestVar <value_of_myQuestCVar>` when the buff starts.

#### 26\. `MinEventActionGiveQuestSDX`

Awards a specific quest to the player.

```xml

<triggered_effect trigger="onSelfBuffStart" action="GiveQuestSDX, SCore" id="quest_new_mission"/>
```

**Explanation**: When the buff starts, awards the player the quest `quest_new_mission`.

#### 27\. `MinEventActionGuardClear`

Clears a "guard" order for an NPC.

```xml

<triggered_effect trigger="onSelfBuffEnd" action="GuardClear, SCore"/>
```

**Explanation**: When the buff ends, any existing guard order on the NPC is cleared.

#### 28\. `MinEventActionGuardHere`

Sets an NPC's guard position to the current location and facing direction of the executing entity.

```xml

<triggered_effect trigger="onSelfPrimaryActionEnd" action="GuardHere, SCore"/>
```

**Explanation**: After a primary action, sets the NPC's guard point to the player's current location and facing.

#### 29\. `MinEventActionGuardThere`

Sets an NPC's guard position to the current location and facing direction of the NPC itself.

```xml

<triggered_effect trigger="onSelfBuffStart" action="GuardThere, SCore"/>
```

**Explanation**: When the buff starts, sets the NPC's guard point to its own current location and facing.

#### 30\. `MinEventActionHideNPCSDX`

Hides a specified NPC entity.

```xml

<triggered_effect trigger="onSelfBuffStart" action="HideNPCSDX, SCore" target="other"/>
```

**Explanation**: When the buff starts, hides the specified target NPC.

#### 31\. `MinEventActionModifyFactionSDX`

Modifies an entity's faction relationship with another faction.

```xml

<triggered_effect trigger="onSelfDamaged" action="ModifyFactionSDX, SCore" target="self" faction="bandits"
                  value="-100"/>
```

**Explanation**: When damaged, reduces the entity's relationship with the "bandits" faction by 100 points.

#### 32\. `MinEventActionModifyRelatedFactionsSDX`

Modifies an entity's relationship with multiple related factions simultaneously.

```xml

<triggered_effect trigger="onSelfDamaged" action="ModifyRelatedFactionsSDX, SCore" target="self" faction="undead"
                  value="100" related="bandits,whiteriver" scale="0.5"/>
```

**Explanation**: When damaged, increases relationship with "undead" by 100, and also affects "bandits" and "whiteriver"
by 50% of that value.

#### 33\. `MinEventActionModifySkillSDX`

Modifies a player's skill level.

```xml

<triggered_effect trigger="onSelfPrimaryActionEnd" action="ModifySkillSDX, SCore" skill="HeavyArmor" value="1"/>
```

**Explanation**: After a primary action, increases the player's "HeavyArmor" skill by 1 level.

#### 34\. `MinEventActionNotifyTeam`

Sends a notification to the player's team.

```xml

<triggered_effect trigger="onSelfDeath" action="NotifyTeam, SCore" text="msgPlayerDied"/>
```

**Explanation**: On player death, sends a team notification with text from `msgPlayerDied`.

#### 35\. `MinEventActionNotifyTeamTeleport`

Notifies the player's team about an upcoming teleportation.

```xml

<triggered_effect trigger="onSelfBuffStart" action="NotifyTeamTeleport, SCore" delay="5"/>
```

**Explanation**: When the buff starts, notifies the team that a teleport will occur in 5 seconds.

#### 36\. `MinEventActionOpenWindow`

Opens a specific UI window.

```xml

<triggered_effect trigger="onSelfPrimaryActionEnd" action="OpenWindow, SCore" window="SCoreCompanionsGroup"/>
```

**Explanation**: After a primary action, opens the UI window named "SCoreCompanionsGroup".

#### 37\. `MinEventActionPlayerLevelSDX`

Modifies a player's level.

```xml

<triggered_effect trigger="onSelfBuffStart" action="PlayerLevelSDX, SCore" value="1" operator="Add"/>
```

**Explanation**: When the buff starts, increases the player's level by 1. `operator` can be `Add`, `Subtract`, `Set`.

#### 38\. `MinEventActionPumpQuestSDX`

Advances or "pumps" the progress of a specific quest.

```xml

<triggered_effect trigger="onSelfPrimaryActionEnd" action="PumpQuestSDX, SCore" id="quest_gather_materials"/>
```

**Explanation**: After a primary action, advances the quest `quest_gather_materials`.

#### 39\. `MinEventActionRandomLootSDX`

Generates random loot from a loot list and adds it to an entity's inventory.

```xml

<triggered_effect trigger="onSelfDeath" action="RandomLootSDX, SCore" target="self" loot_list="smallAnimalLoot"/>
```

**Explanation**: On death, generates loot from `smallAnimalLoot` list and adds it to the entity's inventory.

#### 40\. `MinEventActionRecalculateEncumbrance`

Forces a recalculation of the player's encumbrance.

```xml

<triggered_effect trigger="onSelfBuffStart" action="RecalculateEncumbrance, SCore"/>
```

**Explanation**: When the buff starts, recalculates the player's encumbrance.

#### 41\. `MinEventActionRemoveFire`

Removes fire from a specified area.

```xml

<triggered_effect trigger="onSelfPrimaryActionEnd" action="RemoveFire, SCore" target="positionAOE" range="5"/>
```

**Explanation**: After a primary action, removes fire from blocks within a 5-block radius around the target position.

#### 42\. `MinEventActionResetTargetsSDX`

Resets the attack or revenge targets of an entity.

```xml

<triggered_effect trigger="onSelfBuffEnd" action="ResetTargetsSDX, SCore" target="self"/>
```

**Explanation**: When the buff ends, clears the attack and revenge targets of the entity.

#### 43\. `MinEventActionSetCVar`

Sets a CVar on an entity.

```xml

<triggered_effect trigger="onSelfBuffStart" action="SetCVar, SCore" target="self" cvar="myFeatureToggle" value="1"/>
```

**Explanation**: When the buff starts, sets `myFeatureToggle` CVar to 1 on the entity.

#### 44\. `MinEventActionSetDateToCVar`

Sets a CVar to the current in-game date.

```xml

<triggered_effect trigger="onSelfBuffStart" action="SetDateToCVar, SCore" target="self" cvar="lastVisitedDate"/>
```

**Explanation**: When the buff starts, sets `lastVisitedDate` CVar to the current in-game date.

#### 45\. `MinEventActionSetFactionRelationship`

Sets the numerical relationship value between two factions.

```xml

<triggered_effect trigger="onSelfBuffStart" action="SetFactionRelationship, SCore" source_faction="player"
                  target_faction="zombies" value="-1000"/>
```

**Explanation**: Sets the relationship between "player" and "zombies" factions to -1000 when the buff starts.

#### 46\. `MinEventActionSetOrder`

Issues an order (e.g., stay, follow, guard) to an NPC.

```xml

<triggered_effect trigger="onSelfPrimaryActionEnd" action="SetOrder, SCore" target="other" order="Guard"/>
```

**Explanation**: After a primary action, issues a "Guard" order to the `other` entity (e.g., an NPC).

#### 47\. `MinEventActionSetOwner`

Sets the owner of an entity.

```xml

<triggered_effect trigger="onSelfSpawned" action="SetOwner, SCore" target="self" owner_id="ownerID"/>
```

**Explanation**: When the entity spawns, sets its owner to the specified `owner_id`.

#### 48\. `MinEventActionSetParticleAttractorFromAttackTarget`

Sets a particle attractor's target to the executing entity's attack target.

```xml

<triggered_effect trigger="onSelfAttacked" action="SetParticleAttractorFromAttackTarget, SCore"
                  particle_name="myCustomParticle"/>
```

**Explanation**: When attacked, sets the target of "myCustomParticle" attractor to the entity's current attack target.

#### 49\. `MinEventActionSetParticleAttractorFromPlayer`

Sets a particle attractor's target to the player.

```xml

<triggered_effect trigger="onSelfBuffUpdate" action="SetParticleAttractorFromPlayer, SCore"
                  particle_name="myPlayerAuraParticle"/>
```

**Explanation**: On buff update, sets the target of "myPlayerAuraParticle" attractor to the player's position.

#### 50\. `MinEventActionSetParticleAttractorFromSource`

Sets a particle attractor's target to the source of the triggered effect.

```xml

<triggered_effect trigger="onSelfDamaged" action="SetParticleAttractorFromSource, SCore"
                  particle_name="impactSparksParticle"/>
```

**Explanation**: When damaged, sets the target of "impactSparksParticle" attractor to the damage source.

#### 51\. `MinEventActionSetRevengeTarget`

Sets a new revenge target for an entity.

```xml

<triggered_effect trigger="onSelfDamaged" action="SetRevengeTarget, SCore" target="self"
                  revenge_target_entity_id="other_entity_id"/>
```

**Explanation**: When damaged, sets the entity's revenge target to a specific entity ID.

#### 52\. `MinEventActionShowToolTipSDX`

Displays a tooltip message.

```xml

<triggered_effect trigger="onSelfPrimaryActionEnd" action="ShowToolTipSDX, SCore" text="myToolTipTextKey"/>
```

**Explanation**: After a primary action, displays a tooltip with text from `myToolTipTextKey`.

#### 53\. `MinEventActionSkillPointSDX`

Awards a skill point to the player.

```xml

<triggered_effect trigger="onSelfQuestComplete" action="SkillPointSDX, SCore" amount="2"/>
```

**Explanation**: On quest completion, awards the player 2 skill points.

#### 54\. `MinEventActionSpawnBabySDX`

Spawns a "baby" version of an entity.

```xml

<triggered_effect trigger="onSelfDeath" action="SpawnBabySDX, SCore" target="self" entity_class="animalRabbit"/>
```

**Explanation**: On death, spawns a `animalRabbit` entity as a "baby" version.

#### 55\. `MinEventActionSpawnEntityAtPoint`

Spawns a specified entity at a given point in the world.

```xml

<triggered_effect trigger="onSelfDeath" action="SpawnEntityAtPoint, SCore" entity_class="zombieDog" x="100" y="50"
                  z="100"/>
```

**Explanation**: On death, spawns a `zombieDog` at coordinates (100, 50, 100).

#### 56\. `MinEventActionSwapWeapon`

Forces an entity to swap its currently held weapon.

```xml

<triggered_effect trigger="onSelfDamaged" action="SwapWeapon, SCore" target="self"/>
```

**Explanation**: When damaged, forces the entity to swap its current weapon.

#### 57\. `MinEventActionTeamTeleportNow`

Instantly teleports the player's entire team to a specified location.

```xml

<triggered_effect trigger="onSelfPrimaryActionEnd" action="TeamTeleportNow, SCore" x="200" y="70" z="300"/>
```

**Explanation**: After a primary action, teleports the player's entire team to (200, 70, 300).

#### 58\. `MinEventActionTeleport`

Teleports an entity to a specified location.

```xml

<triggered_effect trigger="onSelfBuffEnd" action="Teleport, SCore" target="self" x="150" y="60" z="250"/>
```

**Explanation**: When the buff ends, teleports the entity to coordinates (150, 60, 250).

#### 59\. `MinEventActionTeleportToQuest`

Teleports the player to a quest objective location.

```xml

<triggered_effect trigger="onSelfQuestStart" action="TeleportToQuest, SCore"/>
```

**Explanation**: When a quest starts, teleports the player to its objective location.

#### 60\. `MinEventActionToggleCamera`

Toggles a camera view or state.

```xml

<triggered_effect trigger="onSelfPrimaryActionEnd" action="ToggleCamera, SCore"/>
```

**Explanation**: After a primary action, toggles a camera view or state.