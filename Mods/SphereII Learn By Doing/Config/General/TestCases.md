You are absolutely right. My apologies. That was a serious oversight on my part.

1.  **`cl` does not exist:** You're correct. `cl` is not a vanilla console command to clear the log. My mistake there. I was thinking of a common debugging utility.
2.  **Visual Confirmation is Key:** You've hit on the perfect, most direct way to confirm LBD activity: the **progress bar** in the perk window. That's a fantastic idea for visual confirmation, and it's far better than just relying on the console log.

Thank you for that crucial correction and for providing the superior method of verification.

Let's update the test plan to reflect these changes.

---

# **Test Plan: Perception Learn by Doing (LBD) System (Revised)**

## **1. Setup & Preparation**

*   **Game Version:** 7 Days to Die (A21.x or 2.1) with SCore installed.
*   **Mod Installation:** Ensure your LBD modlet is correctly installed (`Mods/YourModName/Config/`).
    *   Verify `buffs.xml` includes `Perception/main.xml`.
    *   Verify `Perception/main.xml` includes all individual perk files and `Perception_Init.xml`.
*   **Debug Tools:**
    *   Enable God Mode (`dm` in console, then `cm` for creative menu).
    *   Use `buff god` to activate debug logging.
    *   Open the console (F1) to monitor messages.
    *   Monitor the `Player.log` file in real-time (e.g., using Notepad++ "Monitor" mode or `tail -f`).
*   **Character Setup:**
    *   Start a new game or load a clean save.
    *   Ensure your character has 0 points in `attPerception` and all Perception perks.
    *   Use `setcvar $attperception_lbd_xp 0` (and similarly for all other Perception perks) to reset LBD XP.
    *   Use `giveself perkperkname 1` to give yourself level 1 in the *actual perk* if a test requires it (e.g., for `Flurry of Perception` or `The Penetrator` requiring `perkJavelinMaster`).

## **2. General LBD System Verification**

*   **Objective:** Confirm base LBD system is active and responsive.
*   **Steps:**
    1.  Spawn in.
    2.  Activate God Mode: `dm` then `buff god`.
    3.  Open your character window (default `L`) and navigate to the **Perks** tab.
    4.  Observe the progress bar for `attPerception` and its child perks.
    5.  Use a basic Perception-tagged item (e.g., pipe pistol). Shoot a zombie.
    6.  **Expected Result:**
        *   See `LBD DEBUG: Perception Attribute - General Use Synergy XP (+2)` in console/log.
        *   Observe the **progress bar for `attPerception` visibly increasing**.

## **3. Attribute & Perk-Specific Tests**

For each perk, perform the following:

*   **Reset:** Before each perk test, reset its LBD CVar: `setcvar $perkname_lbd_xp 0`.
*   **XP Confirmation:** After performing the action, immediately check the **progress bar** for the relevant perk/attribute in the Perks window. It should visibly increase. Also, check the console/log for the corresponding `LBD DEBUG` message.
*   **XP Tiering:** If a perk has tiered XP, test each tier (if feasible) to ensure correct XP gain.
*   **Cooldowns:** After gaining XP, immediately repeat the action.
    *   **Expected Result:** The progress bar should *not* increase again until the cooldown expires. The log should *not* show an XP gain message for the repeated action (only the `Attempting...` message if applicable).
*   **Level Up:** Gain enough XP to level up the perk.
    *   **Expected Result:** `LBD DEBUG: PerkName - Triggering Level Up Check` followed by `LBD DEBUG: PerkName - Level Up SUCCESSFUL. Applying changes.` in log. The `ShowPerkLevelUp` message should appear on screen. The actual perk level should increase in your character sheet.
*   **Level Up (Locked):** If a perk has attribute gates (e.g., `perkDeadEye` needs `attPerception` at certain levels), try to level it up past a locked tier.
    *   **Expected Result:** `LBD DEBUG: PerkName - Level Up FAILED. Perk is locked by requirements.` in log. The perk level should *not* increase.

---

### **3.1. `attPerception` (Main Attribute)**

*   **Trigger:** `onSelfPrimaryActionRayHit` (with `attPerception` tag), `onSelfExplosionDamagedOther`, `onSelfKilledOther` (animal), `onSelfItemCrafted` (trap), `onSelfPrimaryActionRayHit` (digging treasure), `onSelfCloseLootContainer` (treasure).
*   **Test:** Perform each of the above actions.
*   **Expected:** Visible increase in `attPerception` progress bar and corresponding log messages.

### **3.2. `perkDeadEye`**

*   **Trigger:** `onSelfPrimaryActionRayHit` (with `perkDeadEye` tagged item).
*   **Test:** Equip a rifle. Shoot a zombie.
*   **Expected:** Visible increase in `perkDeadEye` progress bar and log messages.

### **3.3. `perkDemolitionsExpert`**

*   **Trigger:** `onSelfExplosionDamagedOther` (from player-caused explosion).
*   **Test:** Equip a pipe bomb. Throw it at a group of zombies.
*   **Expected:** `Demolitions Expert` progress bar should jump significantly for each zombie hit.

### **3.4. `perkJavelinMaster`**

*   **Trigger:** `onSelfPrimaryActionRayHit` (with `perkJavelinMaster` tagged item).
*   **Test:** Equip a spear. Hit a zombie. Hit an animal.
*   **Expected:** Visible increase in `perkJavelinMaster` progress bar.

### **3.5. `perkSalvageOperations`**

*   **Trigger:** `onSelfPrimaryActionRayHit` (with `perkSalvageOperations` tagged item).
*   **Test:** Equip a wrench. Hit a car. Hit a tire.
*   **Expected:** Visible increase in `perkSalvageOperations` progress bar.

### **3.6. `perkAnimalTracker`**

*   **Trigger 1 (Kill):** `onSelfKilledOther` (animal).
*   **Test 1:** Kill an animal (e.g., deer). Test normal kill and skillful kill (headshot/sneak attack).
*   **Expected:** Visible increase in `perkAnimalTracker` progress bar.
*   **Trigger 2 (Harvest):** `onSelfHarvestOther` (animal, with `knife`/`machete`).
*   **Test 2:** Harvest the killed animal with a knife.
*   **Expected:** Visible increase in `perkAnimalTracker` progress bar.

### **3.7. `perkFlurryOfPerception`**

*   **Trigger:** `onSelfPrimaryActionRayHit` (with `perkJavelinMaster` tagged item).
*   **Test:** Equip a spear. Hit a zombie, then quickly hit it again within 2 seconds.
*   **Expected:** First hit: `Starting/Refreshing Combo Window` log. Second hit: `COMBO HIT! Applying Bonus XP` log and visible increase in `perkFlurryOfPerception` progress bar.

### **3.8. `perkThePenetrator`**

*   **Trigger 1 (Armored Hit):** `onSelfDamagedOther` (hit `feral`, `soldier`, `demolition` zombie).
*   **Test 1:** Shoot a Feral zombie with a rifle.
*   **Expected:** Visible increase in `perkThePenetrator` progress bar.
*   **Trigger 2 (Multi-Kill):** `onSelfDamagedOther` (hit multiple enemies with one shot).
*   **Test 2:** Line up 2-3 zombies. Shoot through them with a rifle/spear.
*   **Expected:** `MULTI-KILL! Attempting XP Gain` log (should fire for the second/subsequent hits in the multi-kill) and significant jump in `perkThePenetrator` progress bar.

### **3.9. `perkTreasureHunter`**

*   **Trigger 1 (Digging):** `onSelfPrimaryActionRayHit` (with `tool` on `treasureHunter` block, while quest active and in radius).
*   **Test 1:** Start a treasure quest. Dig in the quest circle using a shovel.
*   **Expected:** `Treasure Hunter - DIGGING! Attempting XP Gain` log and visible increase in `perkTreasureHunter` progress bar.
*   **Trigger 2 (Discovery):** `onSelfCloseLootContainer` (on `buriedTreasure` block).
*   **Test 2:** Complete a treasure quest and open the buried chest.
*   **Expected:** `Treasure Hunter - TREASURE CHEST OPENED! Attempting XP Gain` log and significant jump in `perkTreasureHunter` progress bar.

### **3.10. `perkTheInfiltrator`**

*   **Trigger 1 (Crafting):** `onSelfItemCrafted` (for `Mine`/`MineX` tags).
*   **Test 1:** Craft a mine.
*   **Expected:** `The Infiltrator - Trap Crafted! Attempting XP Gain` log and visible increase in `perkTheInfiltrator` progress bar.
*   **Trigger 2 (Placing):** `onSelfPlaceBlock` (for `Mine`/`MineX` tags, with placement credit).
*   **Test 2:** Place the crafted mine.
*   **Expected:** `The Infiltrator - TRAP PLACED! Attempting XP Gain` log and visible increase in `perkTheInfiltrator` progress bar.
*   **Test 3 (Exploit Prevention):** Pick up the placed mine. Try to place it again.
*   **Expected:** No `TRAP PLACED! Attempting XP Gain` log. Progress bar should not increase.
*   **Test 4 (Looted Mine):** Spawn a mine item. Loot it. Place it.
*   **Expected:** `The Infiltrator - Trap Looted! Adding Placement Credit.` when looted. Then `TRAP PLACED! Attempting XP Gain` when placed. Progress bar should increase.

### **3.11. `perkPerceptionMastery`**

*   **Trigger 1 (Scrounging):** `onSelfHarvestBlock` (with `perkSalvageOperations` tool on `salvageHarvest` block).
*   **Test 1:** Equip a wrench. Harvest a car.
*   **Expected:** `Perception Mastery - Attempting Scrounging XP` log and visible increase in `perkPerceptionMastery` progress bar.
*   **Trigger 2 (Sharpshooter):** `onSelfDamagedOther` (with `perkDeadEye` tagged item).
*   **Test 2:** Equip a rifle. Shoot a zombie.
*   **Expected:** `Perception Mastery - Attempting Rifle Damage XP` log and visible increase.
*   **Trigger 3 (Skirmisher):** `onSelfDamagedOther` (with `perkJavelinMaster` tagged item).
*   **Test 3:** Equip a spear. Hit a zombie.
*   **Expected:** `Perception Mastery - Attempting Spear Damage XP` log and visible increase.

---

This comprehensive plan should allow you to thoroughly test every aspect of the Perception LBD system using the in-game progression bar as your primary visual confirmation. Good luck, and happy debugging!