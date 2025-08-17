The Challenges feature in SCore extends the game's existing challenge system, allowing modders to create new and custom
challenge objectives for players. This provides a flexible framework for designing diverse gameplay goals and rewarding
players upon their completion.

## How to Create a New SCore Challenge

To create a new custom challenge within the SCore framework, follow these steps:

1. **Add New Enum Entry**: Define a new entry for your challenge objective in the relevant Enum (likely
   `ChallengeTypeSCoreEnums.cs`).
2. **Update Harmony Patch**: Modify the Harmony Patch related to `ChallengeReadObjective` to ensure it correctly parses
   and recognizes your new enum entry.
3. **Add Challenge Objective Script**: Create a new C\# script file for your specific challenge objective within the
   `Scripts/ Challenge Objective` directory (e.g., inheriting from `ChallengeObjectiveSCoreBase.cs`).
4. **Implement Trigger (Optional)**: If your challenge requires a specific in-game action to trigger its progress, you
   may need to add a delegate, event, and corresponding Harmony patch. This can sometimes be included directly within
   the challenge objective script itself (e.g., as seen in `ChallengeObjectiveEnterPOI.cs`).

## Example Syntax

Challenges are defined in XML files (e.g., `challenges.xml`). Here's an example of the syntax for a custom SCore
challenge:

```xml

<challenge name="enterPOI" title_key="EnterPOI" icon="ui_game_symbol_wood" group="ScoreTest"
           short_description_key="challengeGathererWoodShort" description_key="challengeGathererWoodDesc"
           reward_text_key="challenge_reward_1000xp" reward_event="challenge_reward_1000">
    <objective type="EnterPOI, SCore" prefab="abandoned_house_02" count="10"/>
</challenge>
```

In this example:

* The `objective` tag specifies the custom challenge type (`type="EnterPOI, SCore"`) and its parameters (
  `prefab="abandoned_house_02"`, `count="10"`).

##

Using SCore, <requirements from the Buff system are supposed, using a patch on CheckBaseRequirements. This allows xml
requirements to be applied to challenges.

	<challenge name="gatherResources" title_key="challengeGatherResources" 
         icon="ui_game_symbol_challenge_basics_gather_resources" 
         group="Basics" hint="challengeGatherResourcesHint1"
		 short_description_key="challengeGatherResourcesShort" 
         description_key="challengeGatherResourcesDesc">

         <!-- Must be in God mod, and holding a knife -->
		<requirement name="HasBuff" buff="god"/>
	    <requirement name="HoldingItemHasTags" tags="perkDeepCuts"/>

		<objective type="Gather" item="resourceYuccaFibers" count="10"/>
		<objective type="Gather" item="resourceWood" count="10"/>
		<objective type="Gather" item="resourceRockSmall" count="5"/>
	</challenge>

## Types of SCore Challenge Objectives

The Challenges feature includes various pre-defined custom objective types that modders can utilize:

* **`ChallengeObjectiveBigFire`**: Complete a challenge related to large fires.
* **`ChallengeObjectiveBlockDestroyed`**: Destroy a specific number or type of blocks.
* **`ChallengeObjectiveBlockDestroyedByFire`**: Destroy blocks specifically by fire.
* **`ChallengeObjectiveBlockUpgrade`**: Upgrade a specific block or number of blocks.
* **`ChallengeObjectiveCompleteQuestStealth`**: Complete a quest while maintaining stealth.
* **`ChallengeObjectiveCraftWithIngredient`**: Craft items using a specific ingredient.
* **`ChallengeObjectiveCraftWithTags`**: Craft items that have specific tags.
* **`ChallengeObjectiveCVar`**: Achieve a specific value for a player CVar (Console Variable).
* **`ChallengeObjectiveDecapitation`**: Achieve a certain number of decapitations.
* **`ChallengeObjectiveEnterPOI`**: Enter a specific Point of Interest (POI).
* **`ChallengeObjectiveExtinguishFire`**: Extinguish a certain number of fires.
* **`ChallengeObjectiveGatherTags`**: Gather items with specific tags.
* **`ChallengeObjectiveHarvest`**: Harvest a specific resource or quantity.
* **`ChallengeObjectiveHireNPC`**: Hire a specific NPC or number of NPCs.
* **`ChallengeObjectiveKillWithItem`**: Kill entities using a specific item.
* **`ChallengeObjectivePlaceBlockByTag`**: Place blocks that have a specific tag.
* **`ChallengeObjectiveStartFire`**: Start a certain number of fires.
* **`ChallengeObjectiveStealthKillStreak`**: Achieve a streak of stealth kills.
* **`ChallengeObjectiveWearTags`**: Wear armor or clothing items with specific tags.

New Challenges have been created using a V2 suffix that supports buff requirement style.

* **`ChallengeObjectiveCVarV2`** : Checks if the cvar is set to a specific value.
* **`ChallengeObjectiveHarvestV2`**: Checks for blocks or items being harvested.
* **`ChallengeObjectiveKillV2`**: Checks kill conditions.

These custom objectives allow for highly diverse and engaging challenges beyond the vanilla game's offerings.

Here are XML examples for each of the custom challenge objectives available in the SCore Challenges feature:

### 1\. ChallengeObjectiveBigFire

This objective encourages players to create large fires

```xml

<objective type="BigFire, SCore" count="20"/>
```

**Explanation**: Completes when the number of active fire blocks in the world reaches 20 or more.

### 2\. ChallengeObjectiveBlockDestroyed

This objective tracks the destruction of specific blocks by name, material, within biomes, or specific POIs.

```xml

<objective type="BlockDestroyed, SCore" count="20" block="myblock" biome="burn_forest" poi="traderJen"/>
<objective type="BlockDestroyed, SCore" count="20" material="myMaterial" biome="pine_forest" poi_tags="wilderness"/>
```

**Explanation**: Counts when the player destroys 20 blocks. Can be filtered by `block` name, `material` ID, `biome`,
`poi` name, or `poi_tags`.

### 3\. ChallengeObjectiveBlockDestroyedByFire

This objective tracks the number of blocks destroyed specifically by fire.

```xml

<objective type="BlockDestroyedByFire, SCore" count="20"/>
```

**Explanation**: Completes when 20 blocks are destroyed by fire.

### 4\. ChallengeObjectiveBlockUpgrade

This objective tracks the upgrading of blocks based on various criteria.

```xml

<objective type="BlockUpgradeSCore, SCore" block="frameShapes:VariantHelper" count="10" held="meleeToolRepairT0StoneAxe"
           needed_resource="resourceWood" needed_resource_count="8"/>
<objective type="BlockUpgradeSCore, SCore" block_tags="wood" count="10" biome="burnt_forest"/>
```

**Explanation**: Counts when the player upgrades blocks. Can be filtered by specific `block` name, `block_tags`,
`biome`, `held` item, `needed_resource`, and `needed_resource_count`.

### 5\. ChallengeObjectiveCompleteQuestStealth

This objective encourages completing a quest with consecutive stealth kills.

```xml

<objective type="CompleteQuestStealth, SCore" count="2"/>
<objective type="CompleteQuestStealth, SCore" count="1000"/>
```

**Explanation**: Tracks consecutive stealth kills during a quest. If the `count` is set very high, it can act as an
objective to complete the entire quest in stealth (until all sleepers are cleared).

### 6\. ChallengeObjectiveCraftWithIngredient

This objective tracks crafting items that use a specific ingredient or ingredient tag.

```xml

<objective type="CraftWithIngredient, SCore" count="2" ingredient="resourceLegendaryParts"/>
<objective type="CraftWithIngredient, SCore" count="2" item_tags="tag"/>
```

**Explanation**: Counts when items are crafted using the specified `ingredient` name or `item_tags`.

### 7\. ChallengeObjectiveCraftWithTags

This objective tracks crafting items that have a specific tag.

```xml

<objective type="CraftWithTags, SCore" count="2" item_tags="tag01"/>
```

**Explanation**: Counts when items with the specified `item_tags` are crafted.

### 8\. ChallengeObjectiveCVar

This objective tracks changes to a specified player CVar (Console Variable).

```xml

<objective type="CVar, SCore" cvar="myCVar" count="20" description_key="onCraft"/>
```

**Explanation**: Completes when the CVar named `myCVar` reaches a value of 20.

### 9\. ChallengeObjectiveDecapitation

This objective tracks the number of decapitations performed.

```xml

<objective type="Decapitation, SCore" count="2"/>
```

**Explanation**: Counts when the player performs a specified number of decapitations on entities.

### 10\. ChallengeObjectiveEnterPOI

This objective tracks entering specific Points of Interest (POIs).

```xml

<objective type="EnterPOI, SCore" prefab="abandoned_house_02" count="10"/>
```

**Explanation**: Completes when the player enters the specified `prefab` POI a certain number of times. Can also be
filtered by `tags` on the POI.

### 11\. ChallengeObjectiveExtinguishFire

This objective tracks the number of fires extinguished by the player.

```xml

<objective type="ExtinguishFire, SCore" count="20"/>
```

**Explanation**: Counts when the player extinguishes 20 fires.

### 12\. ChallengeObjectiveGatherTags

This objective tracks gathering items that have specific tags.

```xml

<objective type="GatherTags, SCore" item_tags="junk" count="10"/>
```

**Explanation**: Counts items with the specified `item_tags` in the player's inventory (backpack and toolbelt). Can also
be filtered by `loot_list`.

### 13\. ChallengeObjectiveHarvest

This objective tracks harvesting actions, filtered by item, block, or biome.

```xml

<objective type="Harvest, SCore" count="20" item="resourceWood"/>
<objective type="Harvest, SCore" count="20" item="resourceWood" biome="burnt_forest"/>
<objective type="Harvest, SCore" count="20" item="resourceWood" held_tags="meleeItem"/>
<objective type="Harvest, SCore" count="20" item_tags="woodtag"/>
<objective type="Harvest, SCore" count="20" block_tags="blocktag"/>
```

**Explanation**: Counts when the player harvests items. Can be filtered by harvested `item` name, `item_tags`,
`block_tags`, `biome`, or `held_tags` (item held during harvest).

### 14\. ChallengeObjectiveHireNPC

This objective tracks the hiring of NPCs.

```xml

<objective type="HireNPC, SCore" count="20"/>
```

**Explanation**: Completes when the player hires 20 NPCs.

### 15\. ChallengeObjectiveKillWithItem

This objective tracks killing entities with a specific item or item tag.

```xml

<objective type="KillWithItem, SCore" count="2" item="gunHandgunT1P1stol"/>
<objective type="KillWithItem, SCore" count="2" item_tags="handgunSkill"/>
<objective type="KillWithItem, SCore" count="2" item="gunHandgunT1Pistol" entity_tags="zombie"
           target_name_key="xuiZombies"/>
```

**Explanation**: Counts entity kills. Can be filtered by `item` name, `item_tags`, `entity_tags` (of killed entity),
`target_name_key` (for localization), `biome`, `killer_has_bufftag`, `killed_has_bufftag`, and `is_twitch_spawn`. Also
supports `stealth` kills.

### 16\. ChallengeObjectivePlaceBlockByTag

This objective tracks placing blocks with a specific tag.

```xml

<objective type="PlaceBlockByTag, SCore" count="2" block_tags="myTag"/>
```

**Explanation**: Counts when the player places blocks that have the specified `block_tags`.

### 17\. ChallengeObjectiveStartFire

This objective tracks the number of fires started by the player.

```xml

<objective type="StartFire, SCore" count="20"/>
```

**Explanation**: Completes when the player starts 20 fires.

### 18\. ChallengeObjectiveStealthKillStreak

This objective tracks consecutive stealth kills.

```xml

<objective type="StealthKillStreak, SCore" count="2" cvar="longestStreakCVar"/>
```

**Explanation**: Counts consecutive stealth kills. If a non-stealth kill occurs, the streak resets. A `cvar` can be
specified to store the longest streak.

### 19\. ChallengeObjectiveWearTags

This objective tracks wearing items that have specific tags or mods.

```xml

<objective type="WearTags,SCore" item_tags="armorHead"/>
<objective type="WearTags,SCore" item_mod="modGunBarrelExtender"/>
<objective type="WearTags,SCore" installable_tags="turretRanged"/>
<objective type="WearTags,SCore" modifier_tags="barrelAttachment"/>
```

**Explanation**: Counts when the player wears items (including modifications and cosmetic mods) that match specified
`item_tags`, `item_mod` names, `installable_tags`, or `modifier_tags`.

*** V2 Challenges
V2 Challenges support buff style requirements.

### CVarV2

This objective tracks a CVar and provides a custom description and display name.

```xml

<objective type="CVarV2,SCore" cvar="player_m_desert" count="5000" cvar_override="xuiCVar" description_key="xuiTravel"/>
```

**Explanation**: Counts when the specified `cvar` reaches or exceeds the specified `count`. The `cvar_override` is used
to provide a custom display name for the `cvar`, and the `description_key` is a localization key that overrides the
default description text.

### HarvestV2 With Requirements

This objective tracks various harvesting tasks using requirements.

This is an example of a **HarvestV2** objective used within a **Challenge** that has multiple requirements. The
challenge is only active when both requirements are met.

```xml

<challenge name="Harvesting02" title_key="Harvesting" icon="ui_game_symbol_wood">
    <requirement name="RequirementBlockHasHarvestTags, SCore" tags="allHarvest,oreWoodHarvest"/>
    <requirement name="HoldingItemHasTags" tags="miningTool,shovel"/>
    <objective type="HarvestV2, SCore" count="200"/>
</challenge>
```

**Explanation**: This challenge counts each item harvested (`count="200"`) only when the player is harvesting a block
that has the `allHarvest` or `oreWoodHarvest` tags and is holding an item with the `miningTool` or `shovel` tags.

### ObjectiveKillV2 With Requirements

This objective tracks killing any type of enemy.

```xml
<challenge name="Kill01" title_key="KillWithDeepCuts" icon="ui_game_symbol_wood" >
   <requirement name="HoldingItemHasTags" tags="perkDeadEye"/>
   <requirement name="HitLocation" body_parts="Head" />
   <objective type="KillV2, SCore" count="200" />
</challenge>
```

**Explanation**: Counts when the player kills any entity. The objective is completed when the total number of kills reaches the specified **count**.