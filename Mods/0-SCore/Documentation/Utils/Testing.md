
### **FireEventTracker Debug Patch**

#### **1. Overview**

The FireEventTracker is a real-time debugging tool designed to monitor and display all game events (`MinEvent`) that are triggered on a specific living entity (`EntityAlive`). This feature is essential for troubleshooting custom perks, items, buffs, or other game logic that relies on the event system.

#### **2. Commands**

The tracker is controlled via console commands.

* **Activate Tracking**

    * **Command:** `setcvar $fireeventtracker <entityId>`
    * **Description:** Begins tracking and printing events for the entity with the specified ID. Replace `<entityId>` with the numerical ID of the target entity.

* **Deactivate Tracking**

    * **Command:** `setcvar $fireeventtracker 0`
    * **Description:** Immediately stops all event tracking.

#### **3. Usage Workflow**

Follow these steps to use the event tracker:

1.  **Identify Target:** In the game, find the entity you wish to monitor.
2.  **Find Entity ID:** Open the console (F1) and type `le` to list all active entities. Find your target in the list and note its ID number.
3.  **Activate Tracker:** In the console, enter the activation command with the target's ID.
    * *Example:* To track an entity with ID `172`, you would type:
      ```
      setcvar $fireeventtracker 172
      ```
4.  **Monitor Output:** Observe the console. All events firing on entity `172` will now be printed as they occur.
5.  **Deactivate Tracker:** When you are finished debugging, enter the deactivation command in the console:
    ```
    setcvar $fireeventtracker 0
    ```

#### **4. Notes**

* Only one entity can be tracked at a time. Activating the tracker on a new entity will stop tracking on the previous one.
* The target entity must be loaded and active in the world for the tracker to function correctly.