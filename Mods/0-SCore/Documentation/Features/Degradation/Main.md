## **Item and Workstation Degradation System ⏳**

This system enables tools, items, and workstations to degrade with use during crafting or over time. This creates a more challenging and engaging gameplay loop by requiring players to manage their equipment's upkeep.
---

### **Core Functionality**

The system is built on a series of Harmony patches and XML configurations. It allows crafting tools and workstations to
degrade with use, eventually becoming unusable until repaired or replaced. It works by:

* **Degradation Logic**: The core degradation is managed by helper methods in the code. These methods determine if an
  item or block can degrade, calculate its degradation, and check if it has reached a "broken" state.
* **Item Degradation**: Patches ensure that tools degrade as they're used in a workstation. For example, a patch handles
  degradation when the crafting window is closed, while another handles it when the window is open. Handheld tools also
  degrade through the `ItemActionHandleItemBreak` patch.
* **Workstation Degradation**: Workstations themselves can degrade with use. A patch called
  `TileEntityWorkstationUpdateTick` triggers the `onSelfItemDegrade` effect on the workstation's block, allowing it to
  take damage or be downgraded.

***

### **XML Configuration & Hooks** 📝

The system uses XML properties and `effect_group`s to enable and customize degradation.

#### **Properties**

These properties are used to define how an item or block degrades. They can be added to an item, item modifier, or
block.

* **`DegradationMaxUse`**: Sets the maximum number of uses before an item or block breaks. This value can be a single
  number or a quality-based range. For example, `value="100,600" param1="1,6"` means an item with a quality of 1 has 100
  max uses, and a quality 6 has 600 max uses.
* **`DegradationPerUse`**: The amount of durability an item loses per use. This can also be applied to a block to define
  its degradation per tick.
* **`DegradationBreaksAfter`**: A boolean property that, if set to `true`, will destroy the item once it fully degrades.

#### **Standard Degradation Hooks**

These are pre-configured snippets that can be included in your XML to apply common degradation logic.

* **`StandardItemActiveSettings`**: Applies degradation to items that are considered "active" (e.g., held in hand). It
  uses a **`onSelfRoutineUpdate`** trigger with a **`DegradeItemValueMod, SCore`** action to cause damage over time.
* **`StandardSettings`**: A snippet that enables **`Quality`** for an item and sets default degradation properties. It
  also allows the item to be repaired with a **`resourceRepairKit`**.

#### **Specialized Hooks & Examples**

* **Tools**: Tools like the anvil can be configured to degrade. They inherit properties that allow them to take damage
  with each use.
* **Item Modifiers**: Item modifiers, like a flashlight or water purifier, can be configured to degrade while active.
  For example, a `modArmorWaterPurifier` will degrade when the player drinks murky water or performs a secondary action.
  A broken mod will also apply a **`buffStatusModBroken`** buff to the player.
* **Blocks**: Workstation blocks, like the campfire or forge, can be configured to downgrade to a "broken" state. They
  can also be upgraded back to their original state with specific items. A `triggered_effect` with a `RandomRoll`
  requirement is often used to prevent the block from degrading too quickly.
    * A broken mod will apply a buff called **`buffStatusModBroken`**. [cite_start]This buff's name is "Broken Mod" and
      its tooltip is "A mod is broken. It must be repaired"[cite: 1, 2].
    * [cite_start]The buff also has a short name that displays as "A mod is broken"[cite: 2].

### **UI Integration**

The system provides clear UI feedback to the player.

* **Tooltip**: The tooltip of a degraded item will display **`( Broken )`**.
* **Visual Tint**: The icon of a degraded item can be tinted red by default, but this color can be customized with the
  `BrokenTint` property.
* **Buff Icon**: When a mod on an item is broken, a blinking red icon appears on the player's UI, along with a buff name
  and description to indicate a broken mod.

### **Preventing Use of Broken Tools**

The system prevents players from using degraded tools for crafting. Patches check the fuel grid, workstation queue, and
tool grid to block crafting if a required tool is degraded.