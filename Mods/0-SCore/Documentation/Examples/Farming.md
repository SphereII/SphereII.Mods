The "Blooms Family Farming modlet" leverages SCore's advanced crop management and NPC AI features to introduce a
detailed farming experience, including intelligent farming NPCs and irrigation systems. This documentation explains how
these features are configured and how modders can use them, drawing examples from your provided files.

-----

## Blooms Family Farming Modlet: Leveraging SCore's Crop Management

The Blooms Family Farming modlet showcases SCore's capabilities for intricate farming mechanics, allowing for dynamic
crop growth, irrigation systems, and the introduction of AI-driven farmers.

### 1\. Core Crop Management System

SCore's Crop Management features are configured primarily within `Config/blocks.xml` in the `ConfigFeatureBlock`.
Modders can enable and fine-tune global settings for all crops.

* **Enabling Crop Management**:
    * `CropEnable`: Set to `true` to activate SCore's extended crop management system.
    * **Example (`Config/blocks.xml`)**:
      ```xml
      <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='CropManagement']/property[@name='CropEnable']/@value">true</set>
      ```
* **Growth and Water Requirements**:
    * `CheckInterval`: Defines the time (in seconds) between growth checks for crops. A lower value means faster growth.
    * `RequirePipesForSprinklers`: If `true`, sprinklers will require connection to a water pipe system to function.
    * **`PlantGrowingSDX, SCore`**: By setting the `Class` of `cropsGrowingMaster` to `PlantGrowingSDX, SCore`, all
      crops inherit SCore's advanced growing logic. This enables properties like `RequireWater`, `WaterRange`, and
      `PlantGrowing.Wilt`.
        * `RequireWater`: If `true`, the crop needs water to grow.
        * `WaterRange`: Specifies how far away a crop can be from a water source (or pipe, if
          `RequirePipesForSprinklers` is true) to receive water.
        * `PlantGrowing.Wilt`: The block a crop downgrades to if it wilts (e.g., due to lack of water).
    * **Example (`Config/blocks.xml`)**:
      ```xml
      <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='CropManagement']/property[@name='CheckInterval']/@value">60</set>
      <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='CropManagement']/property[@name='RequirePipesForSprinklers']/@value">true</set>
      <set xpath="/blocks/block[@name='cropsGrowingMaster']/property[@name='Class']/@value">PlantGrowingSDX, SCore</set>
      <append xpath="/blocks/block[@name='cropsGrowingMaster']">
          <property name="RequireWater" value="true" />
          <property name="WaterRange" value="5" />
          <property name="PlantGrowing.Wilt" value="treeDeadPineLeaf"/>
      </append>
      ```
* **Irrigation System Components**: SCore introduces new block classes to build an irrigation network.
    * **`WaterPipeSDX, SCore`**: Applied to `metalPipe` blocks, flagging them as able to carry water.
    * **`FarmPlotSDX, SCore`**: Applied to `farmPlot` blocks, enabling them to interact with the irrigation system.
    * **`CropControlPanel, SCore`**: A specialized control panel for the irrigation system.
        * **Example (`Config/blocks.xml`)**:
          ```xml
          <block name="cropControlPanelBase01">
              <property name="Extends" value="controlPanelBase01"/>
              <property name="Class" value="CropControlPanel, SCore" />
              <property name="ControlPanelName" value="debugcontrolpanel" />
          </block>
          ```
        * [cite\_start]**Localization (`Localization.txt`)**: Provides localized strings for the control panel's debug
          settings, e.g., "Debug Control Panel for Plant Irrigation System" [cite: 1][cite\_start], "Enable Debug
          Settings" [cite: 1][cite\_start], "Turning on water" [cite: 2][cite\_start], "Start RoboPlanter"[cite: 2].
    * **`WaterSourceSDX, SCore` (Sprinklers)**: Blocks like `gupSprinklerBlock` and `gupSprinklerBlockLarge` act as
      sprinklers. They use the `WaterSourceSDX, SCore` class and have a `WaterRange` property to define their area of
      effect for watering crops.
        * **Example (`Config/blocks.xml`)**:
          ```xml
          <block name="gupSprinklerBlock">
              <property name="Class" value="WaterSourceSDX, SCore" />
              <property name="WaterRange" value="5" />
          </block>
          <block name="gupSprinklerBlockLarge">
              <property name="Class" value="WaterSourceSDX, SCore" />
              <property name="WaterRange" value="25" />
          </block>
          ```

### 2\. NPC Farming AI

The modlet introduces "Farmer Frankie" as an NPC capable of farming tasks, powered by SCore's advanced AI packages and
tasks.

* **Farmer NPC Definition (`Config/npc.xml`, `Config/entityclasses.xml`, `Config/entityclasses2.xml`)**:
    * [cite\_start]`NPCFrankieFarmer` (`Config/npc.xml`): Defines the NPC's core information, linking to
      `trader_id="200"` and `dialog_id="FrankieFarmerDialog"`, and `quest_list="frankie_quests"`. [cite: 1]
    * `MyFarmer` (`Config/entityclasses.xml`, `Config/entityclasses2.xml`): This `entity_class` extends
      `npcAdvancedEmptyHandTemplate` and is tagged as `npc`. [cite\_start]Crucially, it includes `NPCFarming` in its
      `AIPackages`[cite: 1]. The `entityclasses.xml` provides a conditional loading based on `0-XNPCCore` mod, while
      `entityclasses2.xml` provides a direct append.
        * **Example (`Config/entityclasses.xml`)**:
          ```xml
          <entity_class name="MyFarmer" extends="npcAdvancedEmptyHandTemplate">
              <property name="UserSpawnType" value="Menu"/>
              <property name="Tags" value="entity,male,npc,melee,cp,DRMid,notrample"/>
              <property name="Names" value="Farmer Frankie"/>
              <property name="NPCID" value="NPCFrankieFarmer"/>
              <property name="AIPackages" value="NPCModCore,NPCModNPCHired, NPCModNPCMeleeBasic,NPCModNPCRangedBasic,NPCFarming"/>
          </entity_class>
          ```
* **Farming AI Package (`Config/utilityai.xml`)**:
    * The `NPCFarming` `ai_package` defines specific actions for farming NPCs.
    * **`Harvest` Action**: This action utilizes the `Farming, SCore` task. This is where SCore's custom AI task for
      farming is implemented.
        * `run="false"`: Indicates it's not a continuous running task.
        * `buff="IsGathering"`: A buff applied while gathering.
        * [cite\_start]`cooldownBuff="buffFarmerCoolDown"`: Uses a cooldown buff (`buffFarmerCoolDown` from
          `Config/buffs.xml`) to prevent constant harvesting. [cite: 1]
        * **Considerations**: This action includes considerations that determine when the NPC will harvest:
            * `SelfNotHasBuff, SCore`: Ensures the NPC is not currently under the `buffFarmerCoolDown` effect.
            * `IsNearFarm, SCore`: Checks if the NPC is within a specified distance of a farm (`distance="50"`).
            * `EnemyNotNear, SCore`: Prevents farming if enemies are too close (`distance="15"`).
    * **Example (`Config/utilityai.xml`)**:
      ```xml
      <ai_package name="NPCFarming" >
          <action name="Harvest" weight="2" entity_filter="IsSelf" >
              <task class="Farming, SCore" run="false" buff="IsGathering" cooldownBuff="buffFarmerCoolDown"/>
              <consideration class="SelfNotHasBuff, SCore" buffs="buffFarmerCoolDown"/>
              <consideration class="IsNearFarm, SCore" distance="50" />
              <consideration class="EnemyNotNear, SCore" distance="15"/>
          </action>
      </ai_package>
      ```
* **General NPC AI (`NPCFarmingGeneral`)**: Also in `Config/utilityai.xml`, this package defines general behaviors for
  farming NPCs using SCore tasks:
    * `IdleSDX, SCore`: Custom idle behavior.
    * `WanderSDX, SCore`: Custom wandering behavior.
    * `TerritorialSDX, SCore`: Custom territorial behavior.
    * `Guard, SCore`: Custom guard behavior, used in `Stay` and `Guard` actions.
    * **Considerations**: Many considerations, like `NotHasHomePosition, SCore`, `HasHomePosition, SCore`,
      `NotHasOrder, SCore`, `HasOrder, SCore`, `EnemyNotNear, SCore`, and `SelfHasCVar, SCore`, allow for complex
      decision-making based on NPC orders, home positions, and nearby threats.
* **NPC Dialogue and Quests (`Config/dialogs.xml`, `Config/quests.xml`, `Localization.txt`)**:
    * [cite\_start]`FrankieFarmerDialog` (`Config/dialogs.xml`): Defines the conversation tree for Farmer Frankie,
      including general greetings and specific lines about rain and farming concerns. [cite: 5, 7, 8, 9, 10, 11, 12, 13]
    * `frankie_quests` (`Config/quests.xml`): A list of quests associated with Farmer Frankie. [cite\_start]It includes
      `farmer_harvest_goldenrod`. [cite: 1]
    * `farmer_harvest_goldenrod` (`Config/quests.xml`): A fetch quest for Goldenrod Plants. [cite\_start]This is a
      standard quest type but demonstrates how quests can be integrated with the NPC. [cite: 1]
        * [cite\_start]**Localization (`Localization.txt`)**: Provides the quest title "Goldenrod
          Harvest" [cite: 2][cite\_start], subtitle "Farming Task" [cite: 2][cite\_start], and detailed descriptions for
          the quest offer and objective. [cite: 3, 4]
* **NPC Trading (`Config/traders.xml`)**:
    * [cite\_start]`trader_info id="200"` (`Config/traders.xml`): Defines Farmer Frankie's trade inventory, including
      farming tools (shovels, farm plots) and various seeds and food items. [cite: 1]

### 3\. Localization for Farming Features

All new texts related to farming, the control panel, and NPC dialogue are localized in `Config/Localization.txt`. This
ensures a customizable and translatable experience.

* **Examples (`Localization.txt`)**:
    * [cite\_start]`cropControlPanelBase01,"Debug Control Panel for Plant Irrigation System"` [cite: 1]
    * [cite\_start]`debugcontrol_turnon,"Turning on debug settings"` [cite: 2]
    * [cite\_start]`farmer_quest_goldenrod_title,Goldenrod Harvest` [cite: 2]
    * [cite\_start]
      `Frankie_start,"[Frankie looks up from his work, wary] Well now... ain't seen many new faces 'round here lately. What do you want?"` [cite: 4]

By combining these SCore features, the "Blooms Family Farming modlet" provides a rich and interactive farming
experience, giving modders powerful tools to expand gameplay.