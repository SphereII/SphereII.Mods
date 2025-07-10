The Signs feature introduces enhanced and customizable sign blocks into the game, allowing for more dynamic displays,
including the potential for video content.

## Configuration in `blocks.xml`

The Signs feature is enabled via a property within the `AdvancedPlayerFeatures` class in your `blocks.xml` file:

```xml

<property class="AdvancedPlayerFeatures">
    <property name="ExtendedSigns" value="true"/>
</property>
```

* **`ExtendedSigns`**
    * **Value**: `true`
    * **Description**: A boolean flag that enables the extended signs functionality. When set to `true`, custom sign
      blocks can leverage advanced features such as displaying dynamic content or videos.

## Functionality

The Signs feature enhances traditional sign functionality by enabling the parsing and display of content directly from
web URL addresses. This capability supports direct links to various media formats, including PNG, JPG, GIF, GIFV, and
MP4 files.