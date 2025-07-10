## TileEntityAlwaysActive Patch

The `TileEntityAlwaysActive` patch modifies the behavior of tile entities, enabling them to remain active even when they
might normally be inactive (e.g., when the player is far away, or the chunk is not fully loaded). This is crucial for
mechanisms that need to function continuously, regardless of player proximity.

* **Purpose**: To ensure that certain tile entities remain continuously active within the game world, allowing their
  associated logic and functions to execute without interruption due to player distance or chunk loading status.

```xml

<block name="MyBlock">
    <property name="AlwaysActive" value="true"/>
</block>
```

**Explanation**:

* **`AlwaysActive`**: `true` - Enables or disables if the TileEntity should always be considered 'Active'

---

The "Powered Workstations" feature introduces the ability for crafting stations and other workstations to require power
to function, adding a layer of infrastructure management to crafting and base building.

## Functionality

When enabled, this feature modifies how certain workstations operate, requiring them to be connected to a power source
to be usable. This adds depth to base design, as players must consider power generation and distribution when setting up
their crafting hubs. This works on the concept of 'Wireless' power. There is no need for a line to connect to one tile entity or another. 

* **Purpose**: To enable workstations to be powered, meaning they will only function when supplied with electricity.

## Configuration

The "Enable Powered Workstations" feature is controlled by a property within the `AdvancedWorkstationFeatures` class in
your `blocks.xml` file:

```xml

<property class="AdvancedWorkstationFeatures">
    <property name="Logging" value="false"/>
    <property name="EnablePoweredWorkstations" value="false"/>
</property>
```

**Explanation**:

* **`EnablePoweredWorkstations`**: `false` - Setting this property's `value` to `true` will activate the Powered
  Workstations feature, requiring compatible workstations to be powered to operate.
* **`Logging`**: `false` - Enables or disables verbose logging specifically for advanced workstation features.

For the Tile Entities: 

```xml

<block name="MyBlock">
    <property name="RequirePower" value="true"/>
</block>
```

**Explanation**:

* **`RequirePower`**: `true` - This workstation will consider it has enough fuel if there is a powered tile entity nearby, such as a generator.
