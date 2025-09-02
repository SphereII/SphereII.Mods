This feature, called **EnabledPoweredWorkstations**, lets you configure workstations to use a nearby power source
instead of a local fuel window. It's particularly useful for creating a more realistic or challenging game environment
where players can't simply fuel up a station from their inventory.

Any Powered block, such as Generator, SolarPanel, or similar BlockPowerSource, will work.

Note: This does not provide a "load" on the power source itself, or cause it to consume additional power.
---

### How to Use

1. **Enable the Feature**: First, you must enable the feature in the **Configuration Block** by setting the
   `EnablePoweredWorkstations` property to "true."
```xml
<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedWorkstationFeatures']/property[@name='EnablePoweredWorkstations']/@value">true</set>
```
2. **Apply to Workstations**: After enabling the feature, you can then apply the `RequirePower` property to specific
   workstations, like the campfire. This property tells the game that the workstation needs a power source to function.
```xml
<append xpath="//block[@name='campfire']">
    <property name="RequirePower" value="true" />
</append>
```
3. **Optional: Change the UI**: To make the change clear to players, you can also modify the workstation's user
   interface. This is done by changing the **`windowFuel`** to **`windowFuelPoweredSDX`**, which removes the fuel slots
   and shows "Power" instead of "Fuel," as defined by the **`xuiNeedPower`** localization entry.
```xml
<configs>
    <set xpath="//window_group[@name='workstation_campfire']/window[@name='windowFuel']/@name">windowFuelPoweredSDX</set>
</configs>
```