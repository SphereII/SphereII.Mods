The system you've described is an **Item and Workstation Degradation System**. It enables tools and workstations to degrade with use during the crafting process, eventually becoming unusable until repaired or replaced.
---

### **Core Functionality**

The system works by introducing a degradation mechanic for both crafting tools and the workstations themselves. Instead of a single use-based durability system, this feature makes tools a consumable resource that requires upkeep to continue crafting. It also applies the same logic to the workstation blocks, allowing them to degrade over time as well.

This is achieved through several key components:

* **Degradation Logic**: The `ItemDegradationHelpers.cs` file contains a suite of helper methods that manage the core degradation logic. This includes checking if an item or block can degrade, calculating its degradation based on properties, and determining if it has reached a "broken" state. The system supports both **`DegradationPerUse`** and **`DegradationMaxUse`** properties, allowing modders to define how quickly an item degrades.
* **Item Degradation**: Several patches ensure that tools degrade as they are used in a workstation. For example, the `TileEntityWorkstationHandleRecipeQueue` patch handles degradation for workstations when the crafting window is closed, while `XUiCRecipeStackOutputStackPatches` handles it when the window is open. The `ItemActionHandleItemBreak` patch extends this to handheld tools, ensuring their modifications also degrade with use.
* **Workstation Degradation**: Workstations themselves can now degrade. The `TileEntityWorkstationUpdateTick` patch calls a helper method, `CheckBlockForDegradation`, which triggers the `onSelfItemDegrade` effect on the workstation's block. This allows modders to apply damage or other effects to the workstation over time.
* **UI Integration**: The system provides clear UI feedback to the player. The `XUiCRequiredItemStackGetBindingValue` patch modifies the tooltip of a degraded item to show **( Broken )**. Additionally, `ItemClassGetIconTint` applies a red tint to the icon of a degraded item to provide a visual cue that it is broken.

---

### **Preventing Use of Broken Tools**

The system prevents players from using degraded tools for crafting in several ways:

* **Fuel Grid Check**: The `XUiCWorkstationFuelGridTurnOn` patch checks if a tool required for a recipe is degraded before allowing the workstation to turn on.
* **Workstation Queue Check**: The `TileEntityWorkstationHandleRecipeQueue` patch stops the crafting queue if a required tool becomes degraded while crafting is in progress with the window closed.
* **Tool Grid Requirement**: The `XUiCWorkstationToolGridHasRequirement` patch checks the tool grid and blocks crafting if a required tool is degraded, even if the recipe itself has no explicit requirement for a specific tool type.

This system creates a more engaging and challenging gameplay loop, forcing players to manage their tools and workstations to maintain crafting capabilities.