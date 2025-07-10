The `NoVehicleTake.cs` patch implements the "Vehicle No Take" feature, which prevents players from manually picking up
vehicles after they have been placed or spawned. This can enforce a more realistic or challenging gameplay experience by
requiring players to use specific tools or methods for vehicle relocation.

## Configuration

The "Vehicle No Take" feature is controlled by a property within the `AdvancedPlayerFeatures` class in your `blocks.xml`
file:

```xml

<property class="AdvancedPlayerFeatures">
    <property name="VehicleNoTake" value="false"/>
</property>
```

**Explanation**:

* **`VehicleNoTake`**: `false` - Setting this property's `value` to `true` will enable the "Vehicle No Take" feature,
  preventing players from picking up vehicles.

If the `VehicleNoTake` setting is active, vehicles can still be picked up under specific conditions:

* If the vehicle entity has the `takeable` tag, it will be allowed for pickup, overriding the `VehicleNoTake` setting.
* If the player has a CVar formatted as `<vehicleName>_pickup` (where `<vehicleName>` is the vehicle's `name` attribute), that specific vehicle can be picked up.
* If the player possesses the `PickUpAllVehicles` CVar, they will be able to pick up any vehicle, regardless of other settings.