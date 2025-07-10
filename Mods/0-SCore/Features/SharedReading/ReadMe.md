The Shared Reading feature allows players within the same party to share the benefits of reading books or schematics.
This means that if one player in a party reads a book, other party members can also gain the associated benefits, such
as learning recipes or gaining skill points.

## Configuration in `blocks.xml`

The Shared Reading feature is controlled by a property within the `AdvancedPlayerFeatures` class in your `blocks.xml`
file:

```xml

<property class="AdvancedPlayerFeatures">
    <property name="SharedReading" value="false"/>
</property>
```

* **`SharedReading`**
    * **Value**: `false`
    * **Description**: A boolean flag that enables or disables the Shared Reading feature. When set to `true`, all
      players within a party will receive the benefits when one member reads a book or schematic.

## Functionality

When the `SharedReading` property is enabled, the system works by intercepting the "on read" events of items (like books
and schematics). It then uses a network package (`NetPackageMinEventShared.cs`) to communicate this reading event to
other players in the party, ensuring they also receive the associated effects or knowledge. This eliminates the need for
every player in a group to find and read the same book individually.