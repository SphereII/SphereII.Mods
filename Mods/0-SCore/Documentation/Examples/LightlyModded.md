## "A Round World" Modlet: Utilizing Core SCore Features

The "A Round World" modlet showcases how to integrate various SCore features to enhance gameplay without necessarily
introducing entirely new core mechanics. It focuses on player convenience, UI improvements, and minor behavioral
adjustments.

### 1\. Advanced Zombie Features

SCore allows for subtle adjustments to zombie behavior.

* **Random Walk Types**: This feature, enabled in `Config/blocks.xml`, makes zombies use more varied walk animations,
  contributing to a less predictable and potentially more organic feel for zombie movement.
    * **How to Use**: Set the `RandomWalk` property to `true` within the `AdvancedZombieFeatures` class.
    * **Example (`blocks.xml`)**:
      ```xml
      <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedZombieFeatures']/property[@name='RandomWalk']/@value">true</set>
      ```

### 2\. Advanced Crafting and Container Interaction

SCore introduces features that streamline crafting and block interaction with containers.

* **Craft From Container (`ReadFromContainers`)**: This allows players to craft items using ingredients stored in nearby
  containers, eliminating the need to move items to the player's inventory. This is configured for general recipes (
  `AdvancedRecipes`) and specifically for block upgrade/repair actions (`BlockUpgradeRepair`).
    * **How to Use**: Set the `ReadFromContainers` property to `true` in the relevant feature classes.
    * **Example (`blocks.xml`)**:
      ```xml
      <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedRecipes']/property[@name='ReadFromContainers']/@value">true</set>
      <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='BlockUpgradeRepair']/property[@name='ReadFromContainers']/@value">true</set>
      ```
* **Block Upgrade/Repair on Nearby Enemies (`BlockOnNearbyEnemies`)**: This feature prevents players from performing
  block upgrade or repair actions if there are enemies in close proximity, adding a layer of challenge and realism to
  base defense.
    * **How to Use**: Set the `BlockOnNearbyEnemies` property to `true` within the `BlockUpgradeRepair` class.
    * **Example (`blocks.xml`)**:
      ```xml
      <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='BlockUpgradeRepair']/property[@name='BlockOnNearbyEnemies']/@value">true</set>
      ```

### 3\. Advanced UI Features

SCore offers enhancements to the game's user interface.

* **Show Target Health Bar (`ShowTargetHealthBar`)**: This enables a visible health bar for the currently targeted
  entity, providing players with immediate feedback on enemy health.
    * **How to Use**: Set the `ShowTargetHealthBar` property to `true` within the `AdvancedUI` class.
    * **Example (`blocks.xml`)**:
      ```xml
      <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedUI']/property[@name='ShowTargetHealthBar']/@value">true</set>
      ```

### 4\. Advanced Prefab Features

SCore can modify the behavior of in-game prefabs.

* **Disable Flickering Lights (`DisableFlickeringLights`)**: This feature can turn off the flickering effect often seen
  in lights within game prefabs, potentially improving visual consistency or reducing visual distractions.
    * **How to Use**: Set the `DisableFlickeringLights` property to `true` within the `AdvancedPrefabFeatures` class.
    * **Example (`blocks.xml`)**:
      ```xml
      <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedPrefabFeatures']/property[@name='DisableFlickeringLights']/@value">true</set>
      ```

### 5\. Advanced Player Features

SCore also provides features that directly benefit the player.

* **Auto Redeem Challenges (`AutoRedeemChallenges`)**: This automates the process of claiming rewards for completed
  challenges, improving player convenience by reducing manual interaction with the challenge system.
    * **How to Use**: Set the `AutoRedeemChallenges` property to `true` within the `AdvancedPlayerFeatures` class.
    * **Example (`blocks.xml`)**:
      ```xml
      <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedPlayerFeatures']/property[@name='AutoRedeemChallenges']/@value">true</set>
      ```
* **Shared Reading (`SharedReading`)**: This feature allows all players within a party to receive the benefit of a read
  book, promoting collaborative gameplay.
    * **How to Use**: Set the `SharedReading` property to `true` within the `AdvancedPlayerFeatures` class.
    * **Example (`blocks.xml`)**:
      ```xml
      <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedPlayerFeatures']/property[@name='SharedReading']/@value">true</set>
      ```

### 6\. Custom Blocks and Functionality: The Drop Box

The modlet introduces a new functional block that leverages SCore for its unique behavior.

* **`cntSphereDropBox` Block**: This block extends a vanilla crate and implements SCore's `DropBoxContainer` class.
    * **`DropBoxContainer, SCore` Class**: This SCore class provides the "auto-sort" functionality, allowing items
      placed inside to be automatically sorted into nearby matching containers.
    * **How to Use**: Define a new block and set its `Class` property to `DropBoxContainer, SCore`. The `DescriptionKey`
      should point to a localized description of its function.
    * **Example (`blocks.xml`)**:
      ```xml
      <block name="cntSphereDropBox">
          <property name="Extends" value="cntWoodWritableCrate"/>
          <property name="Class" value="DropBoxContainer, SCore"/>
          <property name="LootList" value="playerWoodWritableStorage"/>
          <property name="DescriptionKey" value="cntSphereDropBoxDesc"/>
          <property name="DropBox" value="true" />
      </block>
      ```
    * **Crafting (`recipes.xml`)**: The `cntSphereDropBox` can be crafted using `resourceWood`, `resourceNail`,
      `resourceMechanicalParts`, and `resourceElectricParts`.
        * **Example (`recipes.xml`)**:
          ```xml
          <recipe name="cntSphereDropBox" count="1">
              <ingredient name="resourceWood" count="10"/>
              <ingredient name="resourceNail" count="5"/>
              <ingredient name="resourceMechanicalParts" count="10"/>
              <ingredient name="resourceElectricParts" count="10"/>
          </recipe>
          ```
    * [cite\_start]**Localization (`Localization.txt`)**: Provides the display name "Drop Box" [cite: 1] [cite\_start]
      and its description: "A box that will auto-sort items placed inside of it into nearby matching
      containers." [cite: 1]
        * **Example (`Localization.txt`)**:
          ```
          [cite_start]cntSphereDropBox,"Drop Box" [cite: 1]
          [cite_start]cntSphereDropBoxDesc,"A box that will auto-sort items placed inside of it into nearby matching containers." [cite: 1]
          ```

### 7\. Other Item Additions

[cite\_start]The modlet also includes definitions for new "cold" versions of existing beverages (
`drinkJarCoffeeCold` [cite: 1][cite\_start], `drinkJarBlackStrapCoffeeCold` [cite: 3][cite\_start],
`drinkJarRedTeaCold` [cite: 4][cite\_start], `drinkJarGoldenRodTeaCold` [cite: 5]). While these do not directly use
SCore features based on the provided XML, they expand the available item content within the game.

* **Example (`Localization.txt`)**:
  ```
  [cite_start]drinkJarCoffeeCold,"Cold Coffee." [cite: 1]
  [cite_start]drinkJarCoffeeColdDesc,"Cold Coffee isn't as effective as hot coffee, but it'll do in a pinch." [cite: 2]
  ```

-----