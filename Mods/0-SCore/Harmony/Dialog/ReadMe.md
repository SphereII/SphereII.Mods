Here is the regenerated documentation for the Harmony patches found in the `Documentation/Harmony/Dialog/` folder, with
explicit notes on XML examples:

## Dialog-Related Harmony Patches

These Harmony patches enhance and extend the game's dialogue system, allowing for more dynamic content, flexible
loading, and extended functionality. For many of these patches, the changes are implemented at the code level and do not
have direct, specific XML configuration examples provided in the uploaded files.

### 1\. `DialogStatement.cs`

* **Patch Target**: `DialogStatement.Parse`
* **Purpose**:  This class includes a Harmony patches to allow loading up extra custom dialog elements, such as operator properties
```xml
    <action type="AddCVar, Mods" id="quest_Samara_Diary" value="1" operator="set" />
```

### 2\. `DialogFromXML.cs`

* **Patch Target**: `DialogFromXml.GetDialog`
* **Purpose**: This patch is necessary to allow actions to be added to a statement. The game already supports actions to statements,
  * but the GetRespones() doesn't create a full response for the [ Next ] lines. It just returns a pre-generated " [Next]"
  * to pass the NextStatementID. Instead, we'll loop around and grab the full reference to it.

### 3\. `DialogExtends.cs`

* **Patch Target**: `Dialog.ReadXml`
* **Purpose**: This patch introduces an "extends" functionality to dialogue definitions in XML. Similar to how blocks or
  items can inherit properties from a base definition, this allows one dialogue to inherit statements, responses, or
  other properties from another existing dialogue. This promotes modularity and reduces redundancy in dialogue creation.
  ```xml
  <dialog id="MyExtendedDialog" extends="BaseTraderDialog">
      </dialog>
  ```
  **Explanation**: `MyExtendedDialog` would inherit all properties and content from `BaseTraderDialog`, and you could
  then add new elements or modify inherited ones.

### 4\. `DialogClose.cs`

* **Patch Target**: `XUiC_DialogWindowGroup.OnClose`
* **Purpose**: This patch modifies the behavior that occurs when a dialogue window (an `XUiC_DialogWindowGroup`) is
  closed. It resets the NPC weapon.