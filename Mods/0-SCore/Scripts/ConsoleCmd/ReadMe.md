Here's a summary of the custom console commands (ConsoleCmd) available in this mod:

These custom console commands extend the game's existing console functionality, providing modders and players with
direct control over various new features, debugging tools, and administrative actions.

* **`ConsoleCmdUtilityAI.cs`**: Provides console commands for debugging and manipulating the Utility AI system, likely
  allowing for inspection or modification of AI states and behaviors.
* **`ConsoleCmdUnitTestSCore.cs`**: Offers console commands related to unit testing for SCore features, useful for
  development and debugging.
* **`ConsoleCmdReloadSCore.cs`**: Enables a console command to reload SCore's configurations or features without
  restarting the game, streamlining the modding workflow.
* **`ConsoleCmdReloadDialog.cs`**: Provides a console command to reload dialogue XML files, useful for iterating on
  dialogue content without a full game restart.
* **`ConsoleCmdlock.cs`**: Introduces console commands for interacting with custom lockpicking features, likely for
  debugging or administrating locks.
* **`ConsoleCmdAdjustMarkup.cs`**: Offers a console command to adjust markup settings, potentially for visual debugging
  or altering UI display.
* **`ConsoleCmdAdjustCVar.cs`**: Provides a versatile console command to directly adjust custom CVars (Console
  Variables), allowing for dynamic modification of game states or player attributes.
* **`ConsoleCmdActionFireClear.cs`**: Implements a console command to clear active fires from the game world, useful for
  administrative purposes or resetting fire-related tests.
* **`ConsoleCmdActionDelaySDX.cs`**: Offers a console command to introduce delays in actions, potentially for debugging
  or testing timed events.
* **`ConsoleCmdWeaponSway.cs`**: Provides a console command to control or adjust the Weapon Camera Sway feature,
  allowing players to fine-tune the visual sway effects of weapons and camera in first-person view.

To execute these custom console commands, open the in-game console by pressing `F1`. Then, type the command followed by any required arguments, and press `Enter`.
Here's how to execute each of the custom console commands, along with their descriptions and examples. To use these commands, open the in-game console by pressing `F1`, type the command, and press `Enter`.

### 1\. `utilityai` / `uai`

* **Description**: Reloads the Utility AI.
* **Execution Example**:
  ```
  utilityai
  ```
  or
  ```
  uai
  ```

### 2\. `unittest`

* **Description**: Runs a test script.
* **Execution Example**:
  ```
  unittest <ScriptClassName> <sourceEntityID/Name> <targetEntityID/Name>
  ```
  (e.g., `unittest CanDamageChecks npcHarleyBat zombieBoe`)

### 3\. `ReloadSCore`

* **Description**: Reloads the specified XML file, which could have significant game-changing results.
* **Execution Example**:
  ```
  ReloadSCore <xml_file_name>
  ```
  (e.g., `ReloadSCore blocks.xml` or `ReloadSCore buffs.xml`)

### 4\. `dialog` / `dialogs`

* **Description**: Reloads the dialogue XML files.
* **Execution Example**:
  ```
  dialog
  ```
  or
  ```
  dialogs
  ```

### 5\. `lock`

* **Description**: Sets `BreakTime` and `MaxGive` CVars on the primary player for lock testing.
* **Execution Example**:
  ```
  lock <break_time_float> <max_give_float>
  ```
  (e.g., `lock 1.2 8` to set a break time of 1.2 and max give of 8)

### 6\. `adjustmarkup`

* **Description**: Adjusts the trader's buy markup value.
* **Execution Example**:
  ```
  adjustmarkup <markup_float_value>
  ```
  (e.g., `adjustmarkup 3.0` to set the trader markup to 3.0)

### 7\. `setcvar`

* **Description**: Sets a CVar on the primary player.
* **Execution Example**:
  ```
  setcvar <cvarName> <cvarValue>
  ```
  (e.g., `setcvar myPlayerXP 5000` to set `myPlayerXP` CVar to 5000; setting `cvarValue` to 0 removes the CVar)

### 8\. `fireclear`

* **Description**: Clears the Fire cache, effectively resetting the state of all fires in the world.
* **Execution Example**:
  ```
  fireclear
  ```

### 9\. `actiondelay` / `ad`

* **Description**: Changes the time before a new action in the Utility AI is chosen. Default is 0.2 seconds.
* **Execution Example**:
  ```
  actiondelay <delay_time_seconds>
  ```
  (e.g., `actiondelay 0.5` to set action delay to 0.5 seconds)

### 10\. `weaponsway`

* **Description**: Toggles or sets the Weapon Sway visual effect on the primary player.
* **Execution Example**:
  ```
  weaponsway <true/false>
  ```
  (e.g., `weaponsway true` to enable sway, `weaponsway false` to disable it)