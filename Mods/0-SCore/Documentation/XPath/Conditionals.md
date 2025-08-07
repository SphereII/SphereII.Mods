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
* **`bool xpath('')`**
    * Returns `true` or `false` depending on whether the specified xpath is evaluated successfully.
    * Note: This is broken in the 2.1 release but should be working in newer versions.
    * Also Note: This is limited to the existing file. You cannot add a condition based on the existing of a block from the entityclasses.xml.

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

<conditional>
<if cond="has_entitlement('MarauderCosmetic')">
</if>
</conditional>

<conditional>
<!-- Checks if the SCore's ConfigFeatureBlock is available -->
<if cond="xpath('/blocks/block[@name=&quot;ConfigFeatureBlock&quot;]') != null">
</if>
</conditional>
```
