### FireEvent Tracker Debugging Command

This powerful debugging tool allows you to monitor all `FireEvents` being executed by a specific entity in real-time. It
is designed for troubleshooting and to help modders identify which events are firing and when.

#### How to Use

The tracker is controlled via console commands. Follow these steps to enable it:

1. **Find the Entity ID**: First, you need the unique ID of the entity you wish to monitor. You can get this by using
   the `le` (list entities) command in the console.

2. **Enable the Tracker**: Once you have the ID, run the following `setcvar` command, replacing `<entityId>` with your
   target's ID:

   ```
   setcvar $fireeventtracker <entityId>
   ```

3. **Monitor the Log**: After the command is executed, a Harmony patch is activated that logs every `FireEvent` from the
   specified entity. You can view this output in the game's log file.

#### How to Disable

To turn the tracker off, run the following command in the console:

```
setcvar $fireeventtracker 0
```

#### Important Notes

* **Persistence**: This setting will persist even after you restart the game. You must manually disable it once you are
  finished with your troubleshooting.

-----

### New Custom Triggers for Learn by Doing (SCore)

To support more immersive "Learn by Doing" progression systems, new custom triggers have been added. These hooks allow
effects to be triggered by a wider variety of specific player actions.

Below is a list of the available triggers, their functions, and examples of their use.

-----

#### **onSelfLockpickSuccess**

Fires when the player successfully picks a lock. This trigger is also compatible with the "Locks" modlet.

* **Example:**
  ```xml
  <triggered_effect trigger="onSelfLockpickSuccess" action="LogMessage" message="Lock Pick successful"/>
  ```

-----

#### **onSelfItemBought**

Fires when the player purchases an item from a trader or vending machine.

* **Example:**
  ```xml
  <triggered_effect trigger="onSelfItemBought" action="LogMessage" message="Bought Something"/>
  ```

-----

#### **onSelfItemSold**

Fires when the player sells an item to a trader or vending machine.

* **Example:**
  ```xml
  <triggered_effect trigger="onSelfItemSold" action="LogMessage" message="Sold Something"/>
  ```

-----

#### **onSelfQuestComplete**

Fires when the player turns in a quest and it is marked as complete.

* **Example:**
  ```xml
  <triggered_effect trigger="onSelfQuestComplete" action="LogMessage" message="Quest Complete"/>
  ```

-----

#### **onSelfItemCrafted**

Fires each time an item finishes its crafting process in the queue. For items crafted in a workstation, this event will
fire for each completed item when the player next opens that workstation's interface.

* **Example:**
  ```xml
  <triggered_effect trigger="onSelfItemCrafted" action="LogMessage" message="Item Was Crafted"/>
  ```

-----

#### **onRecipeCrafted**

This trigger is specifically for use with workstations. It fires while the workstation's interface is open and is
primarily used to pass the station's `CraftArea` name to requirements like `RequirementRecipeCraftArea`.

* **Example:**
  ```xml
  <triggered_effect trigger="onRecipeCrafted" action="LogMessage" message="Recipe Was Crafted"/>
  ```

-----

#### **onSelfItemRepaired**

Fires when the player successfully repairs an item.

* **Example:**
  ```xml
  <triggered_effect trigger="onSelfItemRepaired" action="LogMessage" message="Item Was Repaired"/>
  ```
  
## The following Requirements were written to go along with these triggers.

### `RequirementRecipeCraftArea`

Checks if a recipe is crafted at one of the specified workstations.

```xml

<requirement name="RequirementRecipeCraftArea, SCore" craft_area="forge,workstation"/>
```

**Explanation**: Requires the recipe to be crafted in either the "forge" or the "workstation". The `craft_area`
parameter accepts a comma-separated list of valid workstation names.

-----

### `RequirementRecipeHasLongCraftTime`

Checks if a recipe's base crafting time meets a certain condition.

```xml

<requirement name="RequirementRecipeHasLongCraftTime, SCore" operation="GTE" value="60"/>
```

**Explanation**: Requires the recipe's crafting time to be Greater Than or Equal To 60 seconds. The `operation` can be
`LT`, `LTE`, `GT`, `GTE`, or `E`.

-----

### `RequirementRecipeHasTags`

Checks if a recipe has at least one of the specified tags.

```xml

<requirement name="RequirementRecipeHasTags, SCore" tags="perkGreaseMonkey,tool"/>
```

**Explanation**: Requires the recipe to have either the "perkGreaseMonkey" or the "tool" tag. The `tags` parameter
accepts a comma-separated list. By default, this requirement passes if **any** tag matches.
