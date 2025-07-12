This guide aims to demystify XPath and its significance for modding in 7 Days to Die, starting from Alpha 17. The
information here is a compilation of forum and Discord discussions and will be updated as more details about the full
implementation become available.

-----

## What is XPath?

**XPath** is a powerful mechanism for adding, changing, or removing XML lines and nodes *without directly editing the
vanilla XML files*. It allows you to apply patches to the game's XML data dynamically, making mod installation and
removal much cleaner and safer.

Partial XPath support existed in SDX since its initial release, and many SDX Modlets are already designed this way.
While this guide will use some SDX XPath examples, SDX is **not required** to use XPath in A17 and later versions of the
game.

This guide emphasizes learning by example, so you'll find explanations alongside practical XPath code snippets.

-----

## Mod Structure (A17+)

The expected mod structure for Alpha 17 and beyond is as follows:

```
Mods/
  <ModName>/
    Config/
    UIAtlases/ItemIconAtlas/ (Optional, for custom icons)
    ModInfo.xml
  <ModName2>/
    Config/
    ModInfo.xml
```

You can load multiple mods, and they will load as long as a `ModInfo.xml` file exists within the mod's root directory.
This is similar to previous alphas, with the key addition of the `Config` folder.

### The `Config` Folder

The `Config` folder is where your XPath-based XML files reside. It supports loading XML files with names that **must
match** the vanilla file names they are patching (e.g., `Config/entityclasses.xml`, `Config/gamestages.xml`,
`Config/items.xml`).

**Key points about `Config` files:**

* They contain **only the changes** you want to apply, not full copies of the vanilla files.
* You **cannot** create new files like `entityclasses2.xml` and expect them to work; all changes for `entityclasses.xml`
  must be within that specific file.
* Each mod can have its own version of these `Config` files (e.g., `Mods/MyMod/Config/items.xml` and
  `Mods/AnotherMod/Config/items.xml`).

### In-Memory Merging

During game initialization, all XPath changes are merged **in-memory only**. No actual game files are modified. This is
a significant advantage:

* You can easily remove mods without needing to re-validate game files via Steam or restore previous XML copies.
* It eliminates issues with partial mod merges that can lead to broken game configurations.

### The Power of Modlets

This XPath system enables the creation of "modlets" – smaller, self-contained mods that can:

* **Add, remove, or change small pieces of the game.**
* Be used independently or combined to create unique gameplay styles.
* Be integrated into larger "overhaul" mods without duplicating effort.

**Modlets are safe from game updates:** Unlike direct edits to `Data/Config` files, modlets residing in your `Mods`
folder are preserved during Steam updates. As long as your XPath remains valid for the new XML structure, your modlets
should continue to work without additional effort.

For example, a "No Ammo" modlet (allowing you to unload guns for bullets) could be easily dropped into your `Mods`
folder, whether you're running a vanilla game or a large overhaul like Valmod. This fosters a collaborative modding
environment where modders can focus on unique features without reinventing the wheel.

-----

## XPath Dissected: Understanding the Basics

XPath syntax can initially seem daunting, but it becomes clear with examples. We'll start with the simplest command:
`set`.

All XPath commands include an `xpath=""` parameter that specifies the exact location within the XML where you want to
make a change.

### Example: Changing Minibike Container Size

Let's say we want to change the size of the minibike container. Here's a snippet of the relevant XML:

```xml

<lootcontainers>
    <lootcontainer id="62" count="0" size="4,3" sound_open="UseActions/open_shopping_basket" open_time="0"
                   sound_close="UseActions/close_shopping_basket" loot_quality_template="baseTemplate">
    </lootcontainer>
</lootcontainers>
```

We need to precisely identify the XML node we want to modify. The `lootcontainer` with `id="62"` is our target. We want
to change its `size` attribute.

XPath uses the `/` separator to navigate through XML nodes.

1. `xpath="/lootcontainers/lootcontainer"` gets us close, but there are many `lootcontainer` nodes.
2. To target the specific container with `id="62"`, we use a condition for attributes:
   `xpath="/lootcontainers/lootcontainer[@id='62']"`. The `id` is an attribute, so we use `@id`.
3. Finally, to directly access the `size` attribute's value, we append `/@size`:
   `xpath="/lootcontainers/lootcontainer[@id='62']/@size"`.

Our full `set` XPath command looks like this:

```xml

<set xpath="/lootcontainers/lootcontainer[@id='62']/@size">7,6</set>
```

This line changes the `size` attribute of the `lootcontainer` with `id="62"` to `7,6`.

### Example: Adding a New Ammo Type

Now, let's add a new ammo type called "NoAmmo" to every gun that uses "9mmBullet." We want to modify all relevant guns,
regardless of their specific ID or name.

Here's a simplified XML snippet:

```xml

<items>
    <item id="40" name="gunPistol">
        <property class="Action0">
            <property name="Magazine_items" value="9mmBullet"/>
        </property>
    </item>
</items>
```

We need to find every `property` element that has a `class` attribute of "Action0", and then within that, find a
`property` element with a `name` attribute of "Magazine\_items" AND a `value` attribute of "9mmBullet".

The full XPath code is:

```xml

<set xpath="/items/item/property[@class='Action0']/property[@name='Magazine_items'][@value='9mmBullet']/@value">
    9mmBullet,NoAmmo
</set>
```

This command says: "For every `item` within `items`, find a `property` with `class='Action0'`. Inside that, find a
`property` where `name='Magazine_items'` **and** `value='9mmBullet'`, and then change **that `value` attribute** to
`9mmBullet,NoAmmo`."

The `[@name='Magazine_items'][@value='9mmBullet']` part is crucial. It ensures we only modify `Magazine_items`
properties that *specifically* have "9mmBullet" as their value, preventing unintended changes (e.g., to the `paintTool`'
s `Magazine_items` which has a value of "paint").

-----

## XPath Commands

Beyond `set`, several other XPath commands allow for diverse modifications.

### `set`

* **Purpose:** Changes an existing attribute or value.
* **Example:**
  ```xml
  <set xpath="/lootcontainers/lootcontainer[@id='62']/@size">7,6</set>
  ```
  This changes the `size` attribute of `lootcontainer` with `id="62"` to `7,6`.

### `append`

* **Purpose:** Adds new XML content or appends a value to an existing one.

#### Adding Additional Values (String Concatenation)

* This allows you to add to an existing value without needing to know the original value.
* **Example:**
  ```xml
  <append xpath="/items/item/property[@class='Action0']/property[@name='Magazine_items' and @value='9mmBullet']/@value">,NoAmmo</append>
  ```
  This adds `,NoAmmo` to the existing value of the `Magazine_items` attribute where it currently holds `9mmBullet`.

#### Adding Additional Nodes (Blocks of XML)

* This allows you to insert larger blocks of XML code. The appended content is added at the end of the specified XPath
  location, but still within its scope.
* **Example:** (From Xyth's Junk Items mod)
  ```xml
  <configs>
    <append xpath="/items">
      <item name="itemMaster">
        <property name="Meshfile" value="Items/Misc/sackPrefab" />
        <property name="DropMeshfile" value="Items/Misc/sack_droppedPrefab" />
        <property name="Material" value="organic" />
        <property name="HoldType" value="45" />
        <property name="Stacknumber" value="500" />
        <property name="Weight" value="5" />
        <property name="Group" value="Resources" />
        <property name="SellableToTrader" value="false" />
      </item>
      <item name="scrapSteel">
        <property name="Extends" value="itemMaster" />
        <property name="CustomIcon" value="ui_game_symbol_scrapSteel" />
        <property name="Material" value="Msteel" />
        <property name="Weight" value="1" />
        <property name="MeltTimePerUnit" value="0.5" />
        <property name="RepairAmount" value="10" />
      </item>
    </append>
  </configs>
  ```
  This snippet adds two new `item` definitions to the end of the `/items` node in `items.xml`.

### `insertAfter`

* **Purpose:** Adds new XML nodes *after* the specified XPath target.
* **Example:**
  ```xml
  <insertAfter xpath="/windows/window[@name='windowVehicleStats']/rect[@name='content']">
    <panel pos="240, 0" style="header.panel">
      <sprite style="header.icon" sprite="ui_game_symbol_add"/>
      <label style="header.name" text="COMBINE" text_key="xuiCombine"/>
    </panel>
  </insertAfter>
  ```
  This adds a new `<panel>` element in the `windowVehicleStats` window, specifically after the `rect` named `'content'`.

### `insertBefore`

* **Purpose:** Adds new XML nodes *before* the specified XPath target.
* **Example:** (Similar to `insertAfter`, but placing the content before the target.)

### `remove`

* **Purpose:** Removes an XML node.
* **Example:**
  ```xml
  <remove xpath="/xui/ruleset[@name='default']/window_group[@name='toolbelt']/window[@name='HUDLeftStatBars']" />
  ```
  This removes the `HUDLeftStatBars` window from the `toolbelt` window group.

### `setattribute`

* **Purpose:** Adds a new attribute to an existing node.
* **Example:**
  ```xml
  <setattribute xpath="/progression/attributes/attribute[@name='attPerception']" name="max_level">1000</setattribute>
  ```
  This adds a `max_level` attribute with a value of `1000` to the `attribute` node named `attPerception`.

### `removeattribute` (New in A18)

* **Purpose:** Removes an attribute from an existing node.
* **Example:**
  ```xml
  <removeattribute xpath="/progression/attributes/attribute[@name='attPerception']/@value" />
  ```
  This removes the `value` attribute from the `attribute` named `attPerception`.

-----

## XPath Advanced Conditionals

While direct matches (`@name='toolbelt'`) are common, XPath offers more flexible conditional matching for complex
scenarios.

* **`starts-with(s1, s2)`:** Matches if string `s1` begins with string `s2`.

    * **Example:**
      ```xml
      <set xpath="/entity_classes/entity_class[starts-with(@name, 'zombieTemp')]/property[@name='Class']/@value">EntityZombieSDX, Mods</set>
      ```
      This changes the `Class` property for all `entity_class` nodes whose `name` attribute starts with `'zombieTemp'`.

* **`ends-with(s1, s2)`:** Matches if string `s1` ends with string `s2`.

    * **Note:** `ends-with()` is **not currently supported** in the game's XPath version.
    * **Example (Hypothetical):**
      ```xml
      <set xpath="/entity_classes/entity_class[ends-with(@name, 'lateMale')]/property[@name='Class']/@value">EntityZombieSDX, Mods</set>
      ```

* **`contains(s1, s2)`:** Matches if string `s1` contains string `s2`.

    * **Example:**
      ```xml
      <set xpath="/entity_classes/entity_class[contains(@name, 'Template')]/property[@name='Class']/@value">EntityZombieSDX, Mods</set>
      ```
      This changes the `Class` property for all `entity_class` nodes whose `name` attribute contains `'Template'`.

* **`not`:** Reverses a condition.

    * **Example:**

      ```xml
      <set xpath="/entity_classes/entity_class[not(contains(@name, 'zombie'))]/property[@name='Class']/@value">EntityZombieSDX, Mods</set>
      ```

      This changes the `Class` property for every `entity_class` node whose `name` attribute **does not** contain
      `'zombie'`.

    * **Combined Example:**

      ```xml
      <set xpath="/items/item[starts-with(@name, 'drinkJar') and not(contains(@name, 'Empty'))]/property[@name='Stacknumber']/@value">64</set>
      ```

      This sets the `Stacknumber` to `64` for items whose name starts with `'drinkJar'` but does **not** contain
      `'Empty'`.

* **`and`:** Requires multiple conditions to be true.

    * **Example:**
      ```xml
      <set xpath="/items/item/property[@class='Action0']/property[@name='Magazine_items' and @value='9mmBullet']/@value">9mmBullet,NoAmmo</set>
      ```
      (As seen before, this requires both `name='Magazine_items'` AND `value='9mmBullet'` to be true.)

* **`or`:** Requires at least one of multiple conditions to be true.

    * **Example:**
      ```xml
      <set xpath="/items/item/property[@class='Action0']/property[@name='Magazine_items' or @value='9mmBullet']/@value">9mmBullet,NoAmmo</set>
      ```
      This would make changes if the `property`'s `name` is `Magazine_items` **OR** its `value` is `9mmBullet`.

-----

## Modlet Example: Guppycur's Blood Moon

This simple modlet demonstrates how XPath can alter gameplay.

**Guppycur's Blood Moon Modlet**

Guppycur's original mod for Alpha 16 modified `gamestages.xml` to prevent the "trickle" of single zombies during the 4th
wave of a Blood Moon horde. Instead of a full file replacement, an XPath modlet achieves this cleanly.

**Files:**

Place these files in your game folder:

* `Mods/BloodMoonTrickle/Config/gamestages.xml`
* `Mods/BloodMoonTrickle/ModInfo.xml`

**`gamestages.xml` Content:**

```xml

<configs>
    <set xpath="/gamestages/spawner[@name='BloodMoonHorde']/*/spawn[4]/@maxAlive">50</set>
</configs>
```

**Explanation of the XPath:**

* **`set`**: This command changes an existing value.
* **`/gamestages/spawner[@name='BloodMoonHorde']`**: This targets the `spawner` node within `gamestages` that has the
  attribute `name='BloodMoonHorde'`.
* **`/*/`**: This is a wildcard, matching any child element of the `spawner` node.
* **`spawn[4]`**: This is a crucial part: it targets the **fourth** `spawn` element *within* the current scope.
* **`/@maxAlive`**: This targets the `maxAlive` attribute of that fourth `spawn` element.

**Why `spawn[4]`?**

In some rare cases, a perfectly precise XPath targeting might be difficult. When a more general XPath could cause
unintended changes, you can use an **index** to specify a particular child element. In the `gamestages.xml`, the fourth
`spawn` line (e.g., `<spawn group="ZombiesNight" num="65" maxAlive="1" />`) is often the "trickle" spawn.

For example, for `gamestage="64"`:

```xml

<gamestage stage="64">
    <spawn group="feralHordeStageGS54" num="21" maxAlive="8" duration="2" interval="38"/>
    <spawn group="feralHordeStageGS59" num="21" maxAlive="8" duration="2"/>
    <spawn group="feralHordeStageGS64" num="21" maxAlive="8" duration="2"/>
    <spawn group="ZombiesNight" num="65" maxAlive="1"/>
</gamestage>
```

By using `spawn[4]`, the modlet specifically changes `maxAlive` to `50` for the `ZombiesNight` group on appropriate
gamestages. Earlier gamestages, which might not have a fourth `spawn` line, are unaffected, providing players with more
breathing room at lower game stages.

-----

## Localization Support (A18 and Beyond)

As of Alpha 18, localization support is available directly from your `Mods` folder.

In your mod's `Config` folder, create `Localization.txt` and `Localization - Quest.txt` files. These files must match
the case and spelling of the vanilla entries.

The localization files use a comma-delimited heading, similar to the vanilla version. For mods, you only need to specify
the languages you are adding. For instance, if your mod only provides English localization, you don't need to list other
languages in the heading.

If your new localization key matches a vanilla value or a value from a previously loaded mod, your mod's value will
update it, with the last loaded mod taking precedence.

**File Format:**

```
HEADER
ENTRY
```

**Example:**

```
Key,Source,Context,Changes,English
myKey,UI,Item stat,New,This is my English Key
```

**Important Note:** If you only include non-English translations (e.g., French) and leave the English field blank or
omit it, users playing in a language that doesn't have a direct translation for your key will fall back to the English
localization. If you *do* specify an English translation, players using other languages will see your English
localization unless their language is also explicitly provided.

-----

## XML / XPath Modding Enhancements (A22)

Alpha 22 introduces two significant features for advanced modding: **Conditionals** and **Include statements**.

-----

### Conditionals

Conditionals allow you to embed `if`/`else` statements directly within your XML, enabling dynamic application of changes
based on various conditions during XML loading.

**Example (blocks.xml):**

```xml

<configs>
    <conditional>
        <if cond="mod_loaded('MyDependencyModlet')">
            <set xpath="/blocks/block[@name='keystoneBlock']/property[@name='Model']/@value">
                @:Entities/LootContainers/luggageBigClosedPrefab.prefab
            </set>
        </if>
    </conditional>
</configs>
```

This example changes the model of the `keystoneBlock` only if the modlet named `MyDependencyModlet` is loaded.

**Key Points:**

* **Structure:** Every `<if />` or `<else>` statement must be inside a `<conditional>` block. `<else>` statements are
  optional.
* **"Else-If" Behavior:** If multiple `<if />` statements are at the same level within a `<conditional>` block, they are
  treated as "else-if." Only the first `if` condition that evaluates to `true` will be executed; subsequent `if`
  statements in that same conditional block will be skipped.
* **Condition Requirements:** Each `<if />` statement must have at least one `cond` attribute that evaluates to `true`
  or `false`.
* **Compatibility:** All XML files that support XPath also support conditionals.
* **Wrapping Includes:** Conditionals can wrap `<include />` statements, but `<include />` statements can only appear at
  the top-level (patch level) within conditionals.
* **Evaluation Scope (`evaluator` attribute):**
    * The `evaluator` attribute is optional and defaults to `"host"`.
    * If set to `"client"`, conditionals are evaluated independently by both the client and the host.
    * If a conditional is placed at the top level (e.g., directly under `<configs>`), the `evaluator` attribute is
      ignored, and it's always evaluated on the host. This ensures proper evaluation of `<include>` files.
    * Host-evaluated conditionals are not fully detailed in `ConfigsDump`; only their final evaluated results are
      stored.
* **Placement:** Conditionals can be placed at any level within the XML.
* **Limitations:** Conditionals cannot evaluate based on other XML properties from the game (e.g., you cannot use a
  block's property value as a condition).
* **NCalc Integration:** Conditional checks use the NCalc
  library ([https://github.com/ncalc/ncalc](https://github.com/ncalc/ncalc)).

### `cond` Attribute

The `cond` attribute within `<if />` statements is an equation that must evaluate to `true` or `false`. It can be
thought of as: "if this mod is loaded, do this," or "if this mod is at version 2.2, do this."

* **Multiple Conditions:** A `cond` can contain multiple equations separated by `and` or `or`.
* **Supported Operands:**
    * `>=`: Equal or Greater Than
    * `<=`: Equal or Less Than
    * `>`: Greater Than
    * `<`: Less Than
    * `==`: Equal To
    * `!=`: Not Equal To
    * `<>`: Not Equal To
* **Value Types:**
    * For numbers or booleans, quotes are not needed for the comparison value.
        * Example: `<if cond="mod_loaded('mymod') == true" />`
        * Example: `<if cond="gamepref('OptionsDynamicMusicEnabled') == 1" />`
    * For strings, quotes **are required** for the comparison value.
        * Example: `<if cond="serverinfo('ServerName') == 'Testing'" />` (Note the single quotes around `'Testing'`)
    * If the return type is a boolean, the operand can be omitted if you're testing for `true`.
        * `<if cond="mod_loaded('myModlet')" />` is equivalent to `<if cond="mod_loaded('myModlet') == true" />`
    * To reverse a boolean condition: `<if cond="mod_loaded('myModlet') == false" />`

### Custom NCalc Functions

Several custom functions have been added to extend NCalc for specific game-related conditions:

* **`bool mod_loaded("modlet name")`**
    * Returns `true` if the modlet (identified by its `ModInfo.xml` `name` attribute) is loaded; `false` otherwise. All
      modlets are loaded before this is evaluated.
* **`Version mod_version("modlet name")`**
    * Returns the `Version` object of the modlet if loaded. Returns `0.0` if not loaded. Can be compared using the
      `version()` function.
* **`Version game_version()`**
    * Returns the current game's `Version` object. Can be compared using the `version()` function.
* **`Version version(x, y, z)`**
    * Converts numbers into a `Version` object for comparison with `mod_version()` and `game_version()`.
    * `y` and `z` can be omitted (e.g., `version(22)` for 22.x.x, `version(22,1)` for 22.1.x). You cannot omit `y` if
      you want to evaluate `z`.
* **`int/string/bool serverinfo("server property")`**
    * Returns a property value from the server (e.g., `"GameName"`). The return type depends on the property's value.
* **`int/string/bool gamepref("gamePref")`**
    * Returns a game preference value (e.g., `"OptionsDynamicMusicEnabled"`). The return type depends on the
      preference's value.
* **`bool event("event name")`**
    * Returns `true` or `false` based on whether the specified event (defined in `events.xml`) is currently active.
      Allows for date-based conditional changes.
* **`int time_minutes()`**
    * Returns the current minute at the time of XML parsing.
* **`bool game_loaded()`**
    * Returns `true` or `false` depending on whether the game is currently loaded.
* **`bool has_entitlement('')`**
    * Returns `true` or `false` depending on whether a specific DLC (e.g., `'MarauderCosmetic'`) is active.

**Conditional Examples:**

```xml

<conditional>
    <if cond="mod_loaded('MyDependencyModlet')">
        <include filename="NPCs/entityclasses.xml"/>
    </if>
</conditional>

<conditional>
<if cond="mod_loaded('MyDependencyModlet') == false">
</if>
</conditional>

<conditional>
<if cond="mod_version('MyDependencyModlet') >= version(22,0)">
</if>
</conditional>

<conditional evaluator="client">
<if cond="gamepref('OptionsDynamicMusicEnabled')">
</if>
</conditional>

<conditional>
<if cond="(time_minutes() % 2) == 0">
</if>
</conditional>

<conditional>
<if cond="(time_minutes() <= 25) and (time_minutes() >= 20)">
</if>
</conditional>

<conditional>
<if cond="(time_minutes() <= 20)">
</if>
<else>
    <if cond="(time_minutes() <= 25)">
    </if>
    <else>
        <if cond="(time_minutes() <= 30)">
        </if>
    </else>
</else>
</conditional>

<conditional>
<if cond="(time_minutes() <= 20)">
</if>
<if cond="(time_minutes() <= 25)">
</if>
<if cond="(time_minutes() <= 30)">
</if>
<else>
</else>
</conditional>

<conditional>
<if cond="game_version() >= version(22,0)">
</if>
</conditional>

<conditional>
<if cond="mod_loaded('0-XNPCCore') and mod_loaded('0-SCore_sphereii')">
    <include filename="NPCs/entityclasses.xml"/>
</if>
</conditional>
```

-----

### Include Statements

The game now supports the `<include>` syntax in XML, allowing you to break down large files into smaller, more
manageable ones or to load different XML files based on conditions.

**Example:**

```xml

<configs>
    <include filename="myEntityclasses.xml"/>
</configs>
```

**Key Points:**

* **Usage:** You can use `<include>` statements in any of the game's XML files within your `Mods` folder.
* **`filename` attribute:** Specifies a path *relative* to the parent XML file (e.g., `Config/entityclasses.xml`).
* **Relative Paths:** Can reference files within the same folder or in sub-folders, but **not** from different modlets.
* **Naming:** No naming restrictions for included XML files.
* **Structure:** Each included XML file must adhere to the format and structure of the parent file, including having a
  top-level node.
* **XPath Capabilities:** Included XML files have the same XPath capabilities as the parent file.
* **Case Sensitivity:** Ensure the case of your referenced file names matches, as file references will fail on Linux and
  Mac with case mismatches. (e.g., `<include filename="Bosses.xml" />` would fail if the actual filename is
  `bosses.xml`).
* **Type Matching:** Included XML files can only be of the same type as the file they are added to. For example,
  `entityclasses.xml` can only include files that also patch `entityclasses.xml`; you cannot include a file that patches
  `blocks.xml` from `entityclasses.xml`.
* **Conditionals:** You can wrap `<include>` statements with conditionals.

**Include Examples:**

* **Example 1:** `MyMod/Config/entityclasses.xml`
  ```xml
  <include filename="Conditionals/ConditionalExamples.xml" />
  ```
* **Example 2:** `MyMod/Config/entityclasses.xml`
  ```xml
  <include filename="EntityChanges/ferals.xml" />
  <include filename="EntityChanges/bosses.xml" />
  ```
* **Example 3:** `MyMod/Config/entityclasses.xml`
  ```xml
  <include filename="bosses.xml" />
  ```

These examples illustrate how you can use the `<include>` syntax to reference and incorporate additional XML files,
promoting better organization and modularity in your modding projects.