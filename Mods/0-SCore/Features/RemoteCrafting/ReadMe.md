The Remote Crafting feature introduces advanced inventory management capabilities, allowing players to utilize resources
from nearby containers for repairing and crafting, and streamlining item transfer with a specialized "drop box" system.

## 1. Core Configuration in `blocks.xml`

The behavior of Remote Crafting, Repair, and Drop Box features are controlled by properties defined within the
`ConfigFeatureBlock` in your `blocks.xml` file.

### Advanced Recipes Configuration (`<property class="AdvancedRecipes">`)

These settings primarily affect crafting from containers and general remote container interactions:

* **`ReadFromContainers`**
    * **Value**: `false`
    * **Description**: Enables or disables the ability to craft using ingredients from nearby containers.
* **`BlockOnNearbyEnemies`**
    * **Value**: `false`
    * **Description**: If `true`, crafting from containers (and other `AdvancedRecipes` features) is blocked if enemies
      are detected within a certain range (defined by `DistanceEnemy` in `BlockUpgradeRepair` or a default 20f from
      `RemoteCraftingUtils.cs`).
* **`Distance`**
    * **Value**: `10`
    * **Description**: Defines the radius (in blocks) within which the system searches for containers to draw materials
      from for crafting.
* **`LandClaimContainersOnly`**
    * **Value**: `false`
    * **Description**: If `true`, the system will only search for containers located within the player's land claim for
      remote crafting.
* **`LandClaimPlayerOnly`**
    * **Value**: `false`
    * **Description**: If `true`, remote crafting will only function when the player themselves is standing within their
      own land claim.
* **`Debug`**
    * **Value**: `false`
    * **Description**: When `true`, enables debug logging related to remote crafting, showing the name of opened
      container/workstation in the log.
* **`BroadcastManage`**
    * **Value**: `false`
    * **Description**: If `true`, allows disabling remote crafting on specific named workstations.
* **`disablereceiver`**
    * **Value**: (empty)
    * **Description**: A comma-separated list of workstation names (case-sensitive) where remote crafting should be
      disabled.
* **`disablesender`**
    * **Value**: (empty)
    * **Description**: A comma-separated list of container names (case-sensitive) from which remote crafting should be
      disabled.
* **`Invertdisable`**
    * **Value**: `false`
    * **Description**: If `true`, inverts the logic of `disablesender`, enabling sending *only* from workstations listed
      in `disablesender`.
* **`nottoWorkstation`**
    * **Value**: (empty)
    * **Description**: Defines specific containers that *cannot* send items to certain workstations. Uses a format like
      `workstation:container1,container2;workstation2:container3`.
* **`bindtoWorkstation`**
    * **Value**: (empty)
    * **Description**: Binds specific containers to send items *only* to certain workstations. Uses a format like
      `workstation:container1,container2;workstation2:container3`.
* **`enforcebindtoWorkstation`**
    * **Value**: `false`
    * **Description**: If `true`, disables remote crafting if the workstation is not explicitly bound to a container via
      `bindtoWorkstation`.

### Block Upgrade Repair Configuration (`<property class="BlockUpgradeRepair">`)

These settings specifically affect the ability to repair blocks using resources from containers:

* **`BlockOnNearbyEnemies`**
    * **Value**: `false`
    * **Description**: If `true`, repairing blocks is prevented if enemies are detected nearby.
* **`DistanceEnemy`**
    * **Value**: `30`
    * **Description**: The distance (in blocks) used to check for nearby enemies when `BlockOnNearbyEnemies` is enabled
      for repair.
* **`ReadFromContainers`**
    * **Value**: `false`
    * **Description**: Enables or disables the ability to repair blocks using resources stored in nearby containers.
* **`Distance`**
    * **Value**: `40`
    * **Description**: Defines the radius (in blocks) within which the system searches for containers to draw repair
      materials from.

## 2. Feature: Repair from Containers

This feature allows players to repair blocks using resources not only from their personal inventory but also from nearby
connected containers.

* **Functionality**: When enabled via `ReadFromContainers` under `BlockUpgradeRepair`, the game will automatically check
  containers within the specified `Distance` for required repair materials. This streamlines the repair process, as
  players don't need to manually transfer materials to their inventory. The `BlockOnNearbyEnemies` setting can prevent
  repairs if hostile entities are too close.

## 3. Feature: Crafting from Containers

This feature extends the crafting system to allow players to use ingredients directly from nearby containers when
crafting items at a workstation.

* **Functionality**: With `ReadFromContainers` enabled under `AdvancedRecipes`, crafting stations will recognize
  materials available in nearby containers within the configured `Distance`. This means players do not need to manually
  move all ingredients into their personal inventory before crafting. The crafting UI will reflect the total available
  quantity, combining items from the player's inventory and accessible containers. The system also handles the
  consumption of these items from the containers.

## 4. Feature: Drop Box Functionality

The Drop Box feature provides a specialized container that automatically transfers its contents to other nearby
accessible containers.

* **Functionality**: A `BlockDropBoxContainer` is a unique loot container that, upon being closed or on a regular
  `UpdateTick` interval, attempts to move all its items into other containers within its configured `Distance`. This is
  useful for quickly offloading loot into a main storage system without manual sorting. The distance for item transfer
  is determined by the `Distance` property in `AdvancedRecipes`.

```xml
<configs>
    <append xpath="/blocks">
        <block name="cntSphereDropBoxTest">
            <property name="Extends" value="cntWoodWritableCrate"/>
            <property name="Class" value="DropBoxContainer, SCore"/>
            <property name="LootList" value="playerWoodWritableStorage"/>

            <property name="UpdateTick" value="200"/>
            <property name="DropBox" value="true"/>

        </block>

        <block name="cntSphereDropBoxTest2">
            <property name="Extends" value="cntWoodWritableCrate"/>
            <property name="LootList" value="playerWoodWritableStorage"/>

            <property name="DropBox" value="true"/>
        </block>
    </append>
</configs>
```

### Explanation of Properties:

* **`<block name="cntSphereDropBoxTest">`**

    * **`Extends`**: `cntWoodWritableCrate` - Specifies that this block inherits properties from the `cntWoodWritableCrate` block.
    * **`Class`**: `DropBoxContainer, SCore` - Assigns the custom `DropBoxContainer` class from the `SCore` module to this block. This class enables the automatic item distribution functionality.
    * **`LootList`**: `playerWoodWritableStorage` - Defines which loot list this container uses for its inventory.
    * **`UpdateTick`**: `200` - Sets the interval (in game ticks) at which the drop box will attempt to distribute its items to nearby containers. The default is 100 ticks.
    * **`DropBox`**: `true` - This property explicitly marks the block as a drop box. Setting it to `true` will optionally trigger item distribution when a user closes the box, in addition to the `UpdateTick`.

* **`<block name="cntSphereDropBoxTest2">`**

    * This is a simpler example, inheriting from `cntWoodWritableCrate` and simply setting `DropBox` to `true`. Without the `Class="DropBoxContainer, SCore"` property, it would rely on other game mechanics (like the `DropBoxToContainers.cs` Harmony patch) to enable its drop box functionality upon closing the container, but it would not have the `UpdateTick` based automatic distribution.

## 5. Broadcast Button

The `XUiC_BroadcastButton` is a UI element designed to control whether a specific loot container participates in the
remote crafting network.

* **Functionality**: This button, when present in a container's UI, allows players to toggle whether that container's
  inventory is included in the `Broadcastmanager`'s network. Containers registered with the `Broadcastmanager` are then
  discoverable by remote crafting and repair functions, adhering to other configuration settings like `disablesender`
  and `bindtoWorkstation`.