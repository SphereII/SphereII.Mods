## XML / XPath Modding Enhancements (A22)



-----

### Include Statements

The game now supports the `<include>` syntax in XML, allowing you to break down large files into smaller, more
manageable ones or to load different XML files based on conditions.

**Example:**

```xml

<configs>
    <include filename="myEntityclasses.xml"/>
</configs>
```

**Key Points:**

* **Usage:** You can use `<include>` statements in any of the game's XML files within your `Mods` folder.
* **`filename` attribute:** Specifies a path *relative* to the parent XML file (e.g., `Config/entityclasses.xml`).
* **Relative Paths:** Can reference files within the same folder or in sub-folders, but **not** from different modlets.
* **Naming:** No naming restrictions for included XML files.
* **Structure:** Each included XML file must adhere to the format and structure of the parent file, including having a
  top-level node.
* **XPath Capabilities:** Included XML files have the same XPath capabilities as the parent file.
* **Case Sensitivity:** Ensure the case of your referenced file names matches, as file references will fail on Linux and
  Mac with case mismatches. (e.g., `<include filename="Bosses.xml" />` would fail if the actual filename is
  `bosses.xml`).
* **Type Matching:** Included XML files can only be of the same type as the file they are added to. For example,
  `entityclasses.xml` can only include files that also patch `entityclasses.xml`; you cannot include a file that patches
  `blocks.xml` from `entityclasses.xml`.
* **Conditionals:** You can wrap `<include>` statements with conditionals.

**Include Examples:**

* **Example 1:** `MyMod/Config/entityclasses.xml`
  ```xml
  <include filename="Conditionals/ConditionalExamples.xml" />
  ```
* **Example 2:** `MyMod/Config/entityclasses.xml`
  ```xml
  <include filename="EntityChanges/ferals.xml" />
  <include filename="EntityChanges/bosses.xml" />
  ```
* **Example 3:** `MyMod/Config/entityclasses.xml`
  ```xml
  <include filename="bosses.xml" />
  ```

These examples illustrate how you can use the `<include>` syntax to reference and incorporate additional XML files,
promoting better organization and modularity in your modding projects.