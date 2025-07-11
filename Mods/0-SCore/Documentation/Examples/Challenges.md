The newly provided XML files introduce a comprehensive challenge system, heavily utilizing SCore's custom objective
types. This allows modders to create a wide variety of in-game challenges that track player actions beyond the vanilla
game's capabilities.

Here's a guide for modders on how to define and use these SCore-powered challenges:

-----

## Challenge System: A Modder's Guide to SCore Challenges

### 1\. Challenge File Inclusion

The main `challenges.xml` file acts as a conditional loader for the specific challenge definition files. This ensures
that the challenges are only loaded if a compatible version of the SCore mod is present, preventing errors.

* **Conditional Loading**: The `<if cond="mod_version('0-SCore_sphereii') &gt;= version(1,0,81,1048)">` tag checks the
  SCore mod version. If the condition is met, it includes `challenges-01.xml`, `challenges-02.xml`, and
  `challenges-03.xml`.
    * **Example (`challenges.xml`)**:
      ```xml
      <configs>
          <conditional>
              <if cond="mod_version('0-SCore_sphereii') &gt;= version(1,0,81,1048)">
                  <include filename="challenges-01.xml"/>
                  <include filename="challenges-02.xml"/>
                  <include filename="challenges-03.xml"/>
              </if>
          </conditional>
      </configs>
      ```

### 2\. Challenge Structure: Categories and Groups

Challenges are organized hierarchically using categories and groups. This helps in structuring the in-game challenge
menu and makes navigation easier for players.

* **`challenge_category`**: A broad grouping for challenges, often representing major themes or aspects of gameplay.
  They have a `name`, `title` (localized or direct), and an `icon`.
    * **Example (`challenges-01.xml`)**:
      ```xml
      <challenge_category name="AbilityCategory" title="Ability Challenges" icon="ui_game_symbol_challenge_category2" />
      ```
* **`challenge_group`**: A sub-group within a category, further organizing challenges. They have a `category` (linking
  to a `challenge_category`), `name`, `title_key` (for localization), `reward_text_key`, and `reward_event`.
    * **Example (`challenges-01.xml`)**:
      ```xml
      <challenge_group category="AbilityCategory" name="PerceptionGroup" title_key="perceptionChallenges_key" reward_text_key="No Rewards" reward_event="NoEvent"  />
      ```
    * You can see categories like "AbilityCategory", "SCore01Category", and "NPCCategory", with groups such as "
      PerceptionGroup", "SCore01Group", and "NPCGroup".

### 3\. Defining Individual Challenges

Each specific challenge is defined using the `<challenge>` element.

* **`challenge` Attributes**:

    * `name`: Unique identifier for the challenge.
    * `title_key`: Localization key for the challenge's title.
    * `icon`: UI symbol displayed next to the challenge.
    * `group`: Links the challenge to a specific `challenge_group`.
    * `short_description_key`: Localization key for a brief summary.
    * `description_key`: Localization key for a detailed description.
    * `reward_text_key`: Localization key for the reward text (e.g., "challenge\_reward\_1000xp").
    * `reward_event`: Game event triggered upon completion (e.g., "challenge\_reward\_1000").
    * `prerequisite_hint`, `hint`: Localization keys for hints or prerequisites.

* **`objective` Element**: The core of a challenge, defining what the player must do to complete it. The `type`
  attribute is where SCore's custom objectives come into play. Most objectives also have a `count` attribute defining
  the target quantity.

### 4\. SCore Custom Objective Types: How to Use Them

SCore introduces powerful custom objective types, denoted by `, SCore` after the objective name. These go beyond vanilla
objectives to track complex player actions.

#### 4.1. Combat-Related Objectives

These objectives track player kills under various conditions.

* **`KillWithItem, SCore`**: Tracks kills made with a specific `item` or an `item_tag`. Can also track `stealth` kills.
    * **Parameters**:
        * `count`: Number of kills required.
        * `item`: Specific item name (e.g., `gunRifleT0PipeRifle`).
        * `item_tag`: Tag applied to the item (e.g., `melee`, `ranged`, `bow`, `knife`, `machete`, `spearSkill`, `gun`).
        * `stealth`: `true` to require stealth kills.
    * **Examples (`challenges-01.xml`)**:
      ```xml
      <objective type="KillWithItem, SCore" count="10" item="gunRifleT0PipeRifle"  />
      <objective type="KillWithItem, SCore" count="10" item_tag="melee" stealth="true"  />
      <objective type="KillWithItem, SCore" count="10" item_tag="bow"  />
      ```
* **`StealthKillStreak, SCore`**: Tracks consecutive stealth kills using items with a specific `item_tag`.
    * **Parameters**:
        * `count`: Length of the stealth kill streak.
        * `item_tag`: Tag applied to the item (e.g., `weapon`, `bow`, `knife,machete`, `spearSkill`).
    * **Examples (`challenges-01.xml`)**:
      ```xml
      <challenge name="bonusChallenge03" title_key="killzombieswithstealthStreak" icon="ui_game_symbol_quest" group="BonusChallengesGroup">
          <objective type="StealthKillStreak, SCore" count="10" item_tag="weapon" />
      </challenge>
      ```
* **`Decapitation, SCore`**: Tracks decapitations using items with a specific `item_tag`.
    * **Parameters**:
        * `count`: Number of decapitations required.
        * `item_tag`: Tag applied to the item (e.g., `knife,machete`, `gun`, `bow`).
    * **Examples (`challenges-02.xml`)**:
      ```xml
      <challenge name="Decap01" title_key="decap_title" icon="ui_game_symbol_quest" group="SCore01Group">
          <objective type="Decapitation, SCore" count="10" item_tag="knife,machete"/>
      </challenge>
      ```

#### 4.2. Crafting and Inventory Objectives

These objectives track player actions related to crafting and item management.

* **`CraftWithIngredient, SCore`**: Tracks crafting items using a specific `ingredient`.
    * **Parameters**:
        * `count`: Number of times an item must be crafted using the specified ingredient.
        * `ingredient`: Name of the specific ingredient (e.g., `resourceLegendaryParts`, `resourceWood`).
    * **Examples (`challenges-02.xml`)**:
      ```xml
      <challenge name="CraftWith01" title_key="craftWith01_title" icon="ui_game_symbol_quest" group="SCore01Group">
          <objective type="CraftWithIngredient, SCore" count="2" ingredient="resourceLegendaryParts"/>
      </challenge>
      ```
* **`GatherTags, SCore`**: Tracks gathering items that have a specific `item_tags`.
    * **Parameters**:
        * `count`: Number of items to gather.
        * `item_tags`: Tag applied to the item (e.g., `junk`).
    * **Examples (`challenges-02.xml`)**:
      ```xml
      <challenge name="Gather01" title_key="gather01_title" icon="ui_game_symbol_quest" group="SCore04Group">
          <objective type="GatherTags, SCore" item_tags="junk" count="10"/>
      </challenge>
      ```
    * **Localization (`Localization.txt`)**:
      ```
      [cite_start]gather01_title,"Gather Junk" [cite: 6]
      [cite_start]gather01_desc,"Gather any item that is tagged as junk. This includes resourceScrapPolymers" [cite: 6]
      ```
* **`WearTags, SCore`**: Tracks wearing items that have a specific `item_tags`.
    * **Parameters**:
        * `item_tags`: Tag applied to the item (e.g., `armorHead`, `armorChest`).
    * **Examples (`challenges-02.xml`)**:
      ```xml
      <challenge name="Wear01" title_key="wear01_title" icon="ui_game_symbol_challenge_basics_wear_clothing" group="SCore04Group">
          <objective type="WearTags,SCore" item_tags="armorHead"/>
          <objective type="WearTags,SCore" item_tags="armorChest"/>
      </challenge>
      ```
    * **Localization (`Localization.txt`)**:
      ```
      [cite_start]wear01_title,"Wear Head Armour" [cite: 6]
      ```

#### 4.3. Fire-Related Objectives

These objectives specifically interact with the FireV2 system.

* **`BlockDestroyedByFire, SCore`**: Tracks blocks that are destroyed by fire.
    * **Parameters**:
        * `count`: Number of blocks to be destroyed by fire.
    * **Examples (`challenges-02.xml`)**:
      ```xml
      <challenge name="BurnWithFire01" title_key="BurnWithFire01" icon="ui_game_symbol_quest" group="SCore02Group">
          <objective type="BlockDestroyedByFire, SCore" count="20"/>
      </challenge>
      ```
* **`StartFire, SCore`**: Tracks starting fires.
    * **Parameters**:
        * `count`: Number of fires to start.
    * **Examples (`challenges-02.xml`)**:
      ```xml
      <challenge name="BurnWithFire02" title_key="BurnWithFire02" icon="ui_game_symbol_quest" group="SCore02Group">
          <objective type="StartFire, SCore" count="1"/>
      </challenge>
      ```
* **`BigFire, SCore`**: Tracks "big fires," likely referring to fires reaching a certain size or intensity.
    * **Parameters**:
        * `count`: Number of "big fires" to achieve.
    * **Examples (`challenges-02.xml`)**:
      ```xml
      <challenge name="BurnWithFire03" title_key="onBigFire" icon="ui_game_symbol_quest" group="SCore02Group">
          <objective type="BigFire, SCore" count="20"/>
      </challenge>
      ```
* **`ExtinguishFire, SCore`**: Tracks extinguishing fires.
    * **Parameters**:
        * `count`: Number of fires to extinguish.
    * **Examples (`challenges-02.xml`)**:
      ```xml
      <challenge name="ExtinguishFire01" title_key="onExtinguish" icon="ui_game_symbol_quest" group="SCore02Group">
          <objective type="ExtinguishFire, SCore" count="20"/>
      </challenge>
      ```

#### 4.4. Block Interaction Objectives

These objectives track interactions with blocks, including harvesting and upgrading.

* **`Harvest, SCore`**: Tracks harvesting resources. Can be specified by `item`, `item_tags`, `held_tags` (tool used),
  `block_tag`, and `biome`.
    * **Parameters**:
        * `count`: Quantity of items to harvest.
        * `item`: Specific item harvested (e.g., `resourceWood`).
        * `item_tags`: Tags of the item harvested.
        * `held_tags`: Tags of the tool held during harvest (e.g., `axe`).
        * `block_tag`: Tag of the block being harvested (e.g., `challenge_pallet`, `wood`).
        * `biome`: Biome where the harvest must occur (e.g., `burnt_forest`).
    * **Examples (`challenges-02.xml`)**:
      ```xml
      <challenge name="Harvest01" title_key="onHarvest" icon="ui_game_symbol_quest" group="SCore03Group">
          <objective type="Harvest, SCore" count="20" item="resourceWood" held_tags="axe" biome="burnt_forest"/>
          <objective type="Harvest, SCore" count="20" item="resourceWood" held_tags="axe" block_tag="challenge_pallet"/>
      </challenge>
      ```
* **`BlockUpgradeSCore,SCore`**: Tracks block upgrades. Can be specified by `block`, `block_tags`, `held` tool,
  `needed_resource`, `needed_resource_count`, and `biome`.
    * **Parameters**:
        * `count`: Number of blocks to upgrade.
        * `block`: Specific block name (e.g., `frameShapes:VariantHelper`).
        * `block_tags`: Tags of the block (e.g., `wood`).
        * `held`: Tool held during upgrade (e.g., `meleeToolRepairT0StoneAxe`).
        * `needed_resource`: Resource required for the upgrade (e.g., `resourceWood`).
        * `needed_resource_count`: Quantity of the resource needed.
        * `biome`: Biome where the upgrade must occur (e.g., `burnt_forest`).
    * **Examples (`challenges-02.xml`)**:
      ```xml
      <challenge name="scoreUpgradeTest" title_key="challengeUpgradeBlocks" group="SCore03Group">
          <objective type="BlockUpgradeSCore,SCore" block="frameShapes:VariantHelper" count="10" held="meleeToolRepairT0StoneAxe" needed_resource="resourceWood" needed_resource_count="8"/>
      </challenge>
      <challenge name="scoreUpgradeTest_biome" title_key="challengeUpgradeBlocks" group="SCore03Group">
          <objective type="BlockUpgradeSCore,SCore" block_tags="wood" count="10" biome="burnt_forest"/>
      </challenge>
      ```

#### 4.5. NPC Interaction Objectives

These objectives track player interactions with Non-Player Characters (NPCs).

* **`HireNPC, SCore`**: Tracks hiring NPCs. Can be specified by `target_name`, `entity_tags`, `item` given, `item_tags`
  given, or `item_material` given.
    * **Parameters**:
        * `count`: Number of NPCs to hire.
        * `target_name`: Specific NPC name (e.g., `npcNurseKnife`).
        * `entity_tags`: Tags of the NPC entity (e.g., `female`, `male`).
        * `item`: Specific item given to hire (e.g., `resourceCropGoldenrodPlant`).
        * `item_tags`: Tags of the item given (e.g., `brass`).
        * `item_material`: Material of the item given (e.g., `Mwood`).
        * `biome`: Biome where the NPC must be hired (e.g., `burnt_forest`).
    * **Examples (`challenges-03.xml`)**:
      ```xml
      <challenge name="NPC01" title_key="NPC_title" icon="ui_game_symbol_quest" group="NPCGroup">
          <objective type="HireNPC, SCore" count="10" />
      </challenge>
      <challenge name="NPC04" title_key="NPC_title" icon="ui_game_symbol_quest" group="NPCGroup">
          <objective type="HireNPC, SCore" count="5" entity_tags="female"/>
      </challenge>
      <challenge name="NPC06" title_key="NPC_title" icon="ui_game_symbol_quest" group="NPCGroup">
          <objective type="HireNPC, SCore" count="5" entity_tags="male" biome="burnt_forest"/>
      </challenge>
      <challenge name="NPC07" title_key="NPC_title" icon="ui_game_symbol_quest" group="NPCGroup">
          <objective type="HireNPC, SCore" count="5" entity_tags="female" item="resourceCropGoldenrodPlant"/>
      </challenge>
      <challenge name="NPC09" title_key="NPC_title" icon="ui_game_symbol_quest" group="NPCGroup">
          <objective type="HireNPC, SCore" count="5" entity_tags="male" item_material="Mwood"/>
      </challenge>
      ```

### 5\. Localization

All `_key` attributes (e.g., `title_key`, `short_description_key`, `description_key`) in challenges refer to entries in
your `Localization.txt` file. This allows for easy translation and clear in-game text for your challenges.

* **Example (`Localization.txt`)**:
  ```
  Key,English
  [cite_start]SCore04challenges_key,"SCore Challenges" [cite: 6]
  [cite_start]gather01_title,"Gather Junk" [cite: 6]
  [cite_start]gather01_short,"Gather Junk" [cite: 6]
  [cite_start]gather01_desc,"Gather any item that is tagged as junk. This includes resourceScrapPolymers" [cite: 6]
  [cite_start]wear01_title,"Wear Head Armour" [cite: 6]
  [cite_start]wear01_short,"Wear Head Armour" [cite: 6]
  [cite_start]wear01_desc,"Wear any head armour with the tag of armorHead" [cite: 6]
  ```

By understanding and utilizing these SCore-specific objective types and the challenge XML structure, modders can create
a rich and engaging questing experience for their players.