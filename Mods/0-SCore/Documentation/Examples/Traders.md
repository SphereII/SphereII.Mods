This documentation outlines how modders can implement specific features related to traders and block behavior within the
7 Days to Die modding framework, particularly in the context of the Score mod, now incorporating the provided snippet
for `blocks.xml`.

### 1\. Disable Trader Protection and Advanced Prefab Features

The Score mod introduces a comprehensive set of properties under an `AdvancedPrefabFeatures` class, which can be used to
control various aspects of trader areas, including disabling protection walls and allowing building. The user has
indicated these features would be configured within `blocks.xml`.

**Implementation:**
To configure these features, you would include the `AdvancedPrefabFeatures` class and its properties within your
`blocks.xml` file:

```xml

<property class="AdvancedPrefabFeatures">
    <property name="Logging" value="false"/>
    <property name="DisableTraderProtection" value="false"/>
    <property name="AllowBuildingInTraderArea" value="false"/>

    <property name="DisableWallVolume" value="false"/>

    <property name="DisableFlickeringLights" value="false"/>

    <property name="PrefabName_NoBuilding" value=""/>
    <property name="PrefabTag_NoBuilding" value="NoBuild"/>
</property>
```

* **`Logging`**: When set to `true`, this likely enables logging for the `AdvancedPrefabFeatures` class, useful for
  debugging.
* **`DisableTraderProtection`**: Setting this to `true` disables the standard trader protection in their prefabs.
* **`AllowBuildingInTraderArea`**: To enable building within a trader's protected area, this property must be set to
  `true`. This property also requires `DisableTraderProtection` to be enabled.
* **`DisableWallVolume`**: Setting this to `true` specifically removes the invisible wall volumes typically found behind
  traders.
* **`DisableFlickeringLights`**: When set to `true`, this property can prevent lights within the trader area from
  flickering.
* **`PrefabName_NoBuilding`**: This property allows modders to specify a comma-delimited list of prefab names. If a
  prefab's name matches one in this list, it will have a "no building" flag applied.
* **`PrefabTag_NoBuilding`**: This property takes a comma-delimited list of XML tags. If a prefab's XML includes any of
  these tags (e.g., `"NoBuild"`), a "no building" flag will be applied to that prefab.

It's important to note that changes to trader protection and building permissions within POIs, especially via prefab
features, are often determined when a game save is created and may not apply to existing saves.

You can modify the trader-related settings within the provided `blocks.xml` file by using XPath to target the specific properties under the `ConfigFeatureBlock` and its `AdvancedPrefabFeatures` class.

Here are XPath examples for changing these trader-related flags:

### Disabling/Enabling Trader Protection

To change the `DisableTraderProtection` flag:

```xml
<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedPrefabFeatures']/property[@name='DisableTraderProtection']/@value">true</set>
```

* Setting the `value` to `true` disables the default trader protection in their prefabs.
* Setting the `value` to `false` enables it.

### Allowing/Disallowing Building in Trader Areas

To change the `AllowBuildingInTraderArea` flag:

```xml
<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedPrefabFeatures']/property[@name='AllowBuildingInTraderArea']/@value">true</set>
```

* Setting the `value` to `true` allows building within trader areas. Note that this typically requires `DisableTraderProtection` to also be set to `true`.
* Setting the `value` to `false` disallows building.

### Disabling/Enabling Invisible Wall Volume

While not explicitly set in the provided `blocks.xml` content for `ConfigFeatureBlock`, based on prior information, the `DisableWallVolume` flag is part of the `AdvancedPrefabFeatures` class. If it were present in your `blocks.xml` under this section (or if you append it), you could change it like this:

```xml
<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedPrefabFeatures']/property[@name='DisableWallVolume']/@value">true</set>
```

* Setting the `value` to `true` disables the invisible wall volume behind traders.
* Setting the `value` to `false` enables it.

### Disabling/Enabling Flickering Lights in Trader Areas

To change the `DisableFlickeringLights` flag:

```xml
<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedPrefabFeatures']/property[@name='DisableFlickeringLights']/@value">true</set>
```

* Setting the `value` to `true` disables flickering lights in trader areas.
* Setting the `value` to `false` enables them.

### PrefabName_NoBuilding

PrefabName_NoBuilding: This property allows modders to specify a comma-delimited list of prefab names. If a prefab's name matches one in this list, it will have a "no building" flag applied.

```xml
<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedPrefabFeatures']/property[@name='PrefabName_NoBuilding']/@value">twd_woodbury,twd_prison,twd_hospital,twd_alexandria,twd_cdc,twd_hershels,twd_hilltop</set>
```

### PrefabTag_NoBuilding

PrefabTag_NoBuilding: This property takes a comma-delimited list of XML tags. If a prefab's XML includes any of these tags (e.g., "NoBuild"), a "no building" flag will be applied to that prefab.

```xml
<set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedPrefabFeatures']/property[@name='PrefabTag_NoBuilding']/@value">NoBuild,CustomTag</set>
```

These XPath examples provide direct ways to modify these settings within your `blocks.xml` file.

### 2\. Set Alternate Currency for a Trader

The Score mod allows modders to change a particular trader's currency by using the `alt_currency` attribute within the
`trader_info` definition. This means a trader can accept or primarily deal in a currency other than the default casino
tokens.

**Implementation:**
This attribute is typically found in the `traders.xml` file, which defines individual trader properties and their stock.
When a player interacts with a trader configured with an `alt_currency`, the backpack's currency display will update to
show the alternate currency.

Here's an example of how to set `oldCash` as an alternate currency for a trader, as shown in the mod's changelog:

```xml

<trader_info id="8" reset_interval="3" open_time="4:05" close_time="21:50" alt_currency="oldCash">
```

* **`alt_currency`**: Specify the `name` of the item that should serve as the alternate currency (e.g., `"oldCash"`).