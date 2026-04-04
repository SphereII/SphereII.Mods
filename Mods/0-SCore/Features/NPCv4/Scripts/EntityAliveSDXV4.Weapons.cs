using System;
using UnityEngine;

public partial class EntityAliveSDXV4
{
    // =========================================================================
    // Weapon management
    // =========================================================================

    public void RefreshWeapon()
    {
        var item = ItemClass.GetItem(_currentWeapon);
        UpdateWeapon(item);
    }

    public void UpdateWeapon(string itemName = "")
    {
        if (string.IsNullOrEmpty(itemName))
            itemName = _currentWeapon;

        var item = ItemClass.GetItem(itemName);
        UpdateWeapon(item);
        EntityUtilities.UpdateHandItem(entityId, itemName);
    }

    /// <summary>
    /// Changes the NPC's held item and refreshes the animator's right-hand transform.
    /// Must be called after <see cref="SetupStartingItems"/> so <c>_defaultWeapon</c> is set.
    /// </summary>
    public void UpdateWeapon(ItemValue item, bool force = false)
    {
        if (item == null) return;
        if (item.GetItemId() < 0) return;
        _currentWeapon = item.ItemClass.GetItemName();

        if (!FindWeapon(_currentWeapon))
        {
            if (string.IsNullOrEmpty(_defaultWeapon)) return;
            item = ItemClass.GetItem(_defaultWeapon);
        }

        if (item.GetItemId() == inventory.holdingItemItemValue.GetItemId() && !force) return;

        _currentWeapon = item.ItemClass.GetItemName();
        Buffs.SetCustomVar("CurrentWeaponID", item.GetItemId());
        inventory.SetItem(0, item, 1);

        foreach (var action in item.ItemClass.Actions)
        {
            if (action is ItemActionRanged ranged)
                ranged.AutoFire = new DataItem<bool>(true);
        }

        // Refresh the animator after the weapon switch so the right-hand transform is correct.
        var entityClassType = EntityClass.list[entityClass];
        emodel.avatarController.SwitchModelAndView(entityClassType.mesh.name, emodel.IsFPV, IsMale);

        if (emodel.avatarController is AvatarZombieController zombieController)
            zombieController.rightHandT = zombieController.FindTransform(GetRightHandTransformName());

        // Item update must happen after SwitchModelAndView so the weapon attaches to the updated hand.
        inventory.OnUpdate();
        inventory.ForceHoldingItemUpdate();
    }

    /// <summary>
    /// Returns true if <paramref name="weapon"/> is available to this NPC (loot container,
    /// enter-game items, or current hand item).
    /// </summary>
    public bool FindWeapon(string weapon)
    {
        var currentWeapon = ItemClass.GetItem(weapon);
        if (currentWeapon == null) return false;
        if (!currentWeapon.ItemClass.Properties.Contains("CompatibleWeapon")) return false;
        var playerWeapon = currentWeapon.ItemClass.Properties.GetStringValue("CompatibleWeapon");
        if (string.IsNullOrEmpty(playerWeapon)) return false;
        var playerWeaponItem = ItemClass.GetItem(playerWeapon);
        if (playerWeaponItem == null) return false;

        if (lootContainer != null && lootContainer.HasItem(playerWeaponItem)) return true;

        for (int i = 0; i < itemsOnEnterGame.Count; i++)
        {
            if (itemsOnEnterGame[i].itemValue.ItemClass.GetItemName()
                    .Equals(weapon, StringComparison.InvariantCultureIgnoreCase))
                return true;
        }

        return GetHandItem().ItemClass.GetItemName()
            .Equals(weapon, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Harmony redirects <c>AvatarAnimator.GetRightHandTransformName()</c> to this method so
    /// that per-weapon hand-joint overrides work correctly on NPC entities.
    /// </summary>
    public new string GetRightHandTransformName()
    {
        var currentItemHand = inventory.holdingItem;
        if (currentItemHand.Properties.Contains(EntityClass.PropRightHandJointName))
            currentItemHand.Properties.ParseString(EntityClass.PropRightHandJointName, ref rightHandTransformName);
        else
        {
            rightHandTransformName = "Gunjoint";
            EntityClass.list[entityClass].Properties
                .ParseString(EntityClass.PropRightHandJointName, ref rightHandTransformName);
        }
        return rightHandTransformName;
    }

    // =========================================================================
    // Inventory setup
    // =========================================================================

    public override void SetupStartingItems()
    {
        for (int i = 0; i < itemsOnEnterGame.Count; i++)
        {
            var itemStack = itemsOnEnterGame[i];
            var forId     = ItemClass.GetForId(itemStack.itemValue.type);
            if (forId.HasQuality)
                itemStack.itemValue = new ItemValue(itemStack.itemValue.type, 1, 6, false, null, 1f);
            else
                itemStack.count = forId.Stacknumber.Value;

            if (i == 0) _defaultWeapon = forId.GetItemName();
            inventory.SetItem(i, itemStack);
        }
    }

    private void AddToInventory()
    {
        // Gate on a cvar so the initial inventory is only populated once per entity.
        if (Buffs.GetCustomVar("InitialInventory") > 0) return;
        Buffs.SetCustomVar("InitialInventory", 1);

        var ec = EntityClass.list[entityClass];
        if (!ec.Properties.Values.ContainsKey("StartingItems")) return;

        foreach (var item in ec.Properties.Values["StartingItems"].Split(','))
        {
            var itemStack = ItemStack.FromString(item.Trim());
            if (itemStack.itemValue.IsEmpty()) continue;
            var forId = ItemClass.GetForId(itemStack.itemValue.type);
            if (forId.HasQuality)
                itemStack.itemValue = new ItemValue(itemStack.itemValue.type, 1, 6, false, null, 1f);
            lootContainer.AddItem(itemStack);
        }
    }

    // =========================================================================
    // Quests
    // =========================================================================

    public void GiveQuest(string strQuest)
    {
        var questIdLower = strQuest.ToLower();
        foreach (var quest in questJournal.quests)
            if (quest.ID == questIdLower) return;   // no duplicates

        var newQuest = QuestClass.CreateQuest(strQuest);
        if (newQuest == null) return;

        newQuest.SharedOwnerID = entityId;
        newQuest.QuestGiverID  = -1;
        questJournal.AddQuest(newQuest);
    }

    // =========================================================================
    // Trader ID toggling — temporarily zeroed during damage so NPCInfo
    // properties are evaluated correctly, then restored afterwards.
    // =========================================================================

    public void ToggleTraderID(bool restore)
    {
        if (NPCInfo == null) return;
        NPCInfo.TraderID = restore ? _defaultTraderID : 0;
    }

    // =========================================================================
    // Mission (hide/show NPC)
    // =========================================================================

    /// <summary>
    /// Hides the NPC and marks it as on a mission (<c>send = true</c>), or
    /// restores its visibility when the mission ends (<c>send = false</c>).
    /// </summary>
    public void SendOnMission(bool send)
    {
        if (send)
        {
            var enemy = GetRevengeTarget();
            if (enemy != null)
            {
                SetAttackTarget(null, 0);
                enemy.SetAttackTarget(null, 0);
                enemy.SetRevengeTarget(null);
                enemy.DoRagdoll(new DamageResponse());
                SetRevengeTarget(null);
            }

            isIgnoredByAI = true;
            transform.localScale = Vector3.zero;
            emodel.SetVisible(false, false);
            enabled = false;
            Buffs.AddCustomVar("onMission", 1f);

            if (NavObject != null) NavObject.IsActive = false;
            DebugNameInfo = "";
            emodel.enabled = false;
            SetupDebugNameHUD(false);
        }
        else
        {
            transform.localScale = _scale;
            emodel.SetVisible(true, true);
            enabled = true;
            Buffs.RemoveCustomVar("onMission");
            if (NavObject != null) NavObject.IsActive = true;
            isIgnoredByAI = false;
            emodel.enabled = true;
        }
    }
}
