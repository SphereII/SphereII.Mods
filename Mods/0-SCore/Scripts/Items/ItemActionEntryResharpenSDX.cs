using Audio;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemActionEntryResharpenSDX : BaseItemActionEntry {
    private readonly string lblNeedMaterials;
    private StateTypes state;

    public ItemActionEntryResharpenSDX(XUiController controller) : base(controller, "Resharpen",
        "ui_game_symbol_wrench", GamepadShortCut.DPadLeft) {
        lblNeedMaterials = Localization.Get("xuiRepairMissingMats");
        controller.xui.PlayerInventory.OnBackpackItemsChanged += PlayerInventory_OnBackpackItemsChanged;
        controller.xui.PlayerInventory.OnToolbeltItemsChanged += PlayerInventory_OnToolbeltItemsChanged;
    }

    private void PlayerInventory_OnToolbeltItemsChanged() {
        RefreshEnabled();
        if (ParentItem != null) ParentItem.MarkDirty();
    }

    private void PlayerInventory_OnBackpackItemsChanged() {
        RefreshEnabled();
        if (ParentItem != null) ParentItem.MarkDirty();
    }

    public override void OnDisabledActivate() {
        var stateTypes = state;
        if (stateTypes != StateTypes.NotEnoughMaterials) return;
        GameManager.ShowTooltip(ItemController.xui.playerUI.entityPlayer, lblNeedMaterials);
        var forId = ItemClass.GetForId(((XUiC_ItemStack)ItemController).ItemStack.itemValue.type);
        if (!forId.Properties.Contains("SharpenItem")) return;
        var strSharpenItem = string.Empty;
        forId.Properties.ParseString("SharpenItem", ref strSharpenItem);
        if (string.IsNullOrEmpty(strSharpenItem)) return;
        var itemClass = ItemClass.GetItemClass(strSharpenItem);
        if (itemClass == null) return;
        var @is = new ItemStack(new ItemValue(itemClass.Id), 0);
        ItemController.xui.playerUI.entityPlayer.AddUIHarvestingItem(@is, true);
    }

    public override void RefreshEnabled() {
        base.RefreshEnabled();
        state = StateTypes.Normal;
        var xui = ItemController.xui;
        if (((XUiC_ItemStack)ItemController).ItemStack.IsEmpty() || ((XUiC_ItemStack)ItemController).StackLock) return;
        var forId = ItemClass.GetForId(((XUiC_ItemStack)ItemController).ItemStack.itemValue.type);
        Enabled = state == StateTypes.Normal;
        if (!Enabled)
        {
            IconName = "ui_game_symbol_book";
            return;
        }

        var itemValue = ((XUiC_ItemStack)ItemController).ItemStack.itemValue;
        if (forId.RepairTools == null || forId.RepairTools.Length <= 0) return;
        var itemClass = ItemClass.GetItemClass(forId.RepairTools[0].Value);
        if (itemClass == null) return;
        var b = Convert.ToInt32(Math.Ceiling(Mathf.CeilToInt(itemValue.UseTimes) /
                                             (float)itemClass.RepairAmount.Value));
        xui.PlayerInventory.GetItemCount(new ItemValue(itemClass.Id));
        if (Mathf.Min(xui.PlayerInventory.GetItemCount(new ItemValue(itemClass.Id)), b) * itemClass.RepairAmount.Value >
            0) return;
        state = StateTypes.NotEnoughMaterials;
        Enabled = state == StateTypes.Normal;
    }

    public static void WarnQueueFull(XUiController ItemController) {
        var text = "No room in queue!";
        if (Localization.Exists("wrnQueueFull")) text = Localization.Get("wrnQueueFull");
        GameManager.ShowTooltip(ItemController.xui.playerUI.entityPlayer, text);
        Manager.PlayInsidePlayerHead("ui_denied");
    }

    public override void OnActivated() {
        var xui = ItemController.xui;
        var playerInventory = xui.PlayerInventory;
        var itemStack = ((XUiC_ItemStack)ItemController).ItemStack.Clone();
        var forId = ItemClass.GetForId(itemStack.itemValue.type);
        var childByType = xui.FindWindowGroupByName("crafting").GetChildByType<XUiC_CraftingWindowGroup>();

        if (itemStack.itemValue.HasQuality)
        {
            if (itemStack.itemValue.PercentUsesLeft < 0.30)
            {
                GameManager.ShowTooltip(ItemController.xui.playerUI.entityPlayer, Localization.Get("TooDamagedToSharpen"));
                return;
            }

            if (itemStack.itemValue.PercentUsesLeft > 0.8)
            {
                GameManager.ShowTooltip(ItemController.xui.playerUI.entityPlayer, Localization.Get("NotDamagedEnoughToSharpen"));
                return;
            }
        }

        if (!forId.Properties.Contains("SharpenItem")) return;

        var strSharpenItem = string.Empty;
        forId.Properties.ParseString("SharpenItem", ref strSharpenItem);
        if (string.IsNullOrEmpty(strSharpenItem))
            return;

        var itemClass = ItemClass.GetItemClass(strSharpenItem);
        if (itemClass == null)
            return;

        var repairItem = new ItemValue(itemClass.Id);
        var count = playerInventory.GetItemCount(repairItem);
        if (count < 1)
        {
            var text = "Not enough " + strSharpenItem + " to fix this: " + count + " / 1";
            GameManager.ShowTooltip(ItemController.xui.playerUI.entityPlayer, text);
            return;
        }

        // Randomize a value to reduce the use times, but always leave a bit so it's never 100%
        var random = GameRandomManager.Instance.CreateGameRandom();
        var flRandom = random.RandomRange((int)(itemStack.itemValue.MaxUseTimes * 0.2), (int)itemStack.itemValue.UseTimes);
        itemStack.itemValue.UseTimes -= flRandom;
        playerInventory.RemoveItem(new ItemStack(repairItem,1));
        
        ((XUiC_ItemStack)base.ItemController).ItemStack = ((itemStack.count <= 0) ? ItemStack.Empty.Clone() : itemStack.Clone());
        ((XUiC_ItemStack)base.ItemController).WindowGroup.Controller.SetAllChildrenDirty(false);

    }

    private enum StateTypes {
        Normal,
        NotEnoughMaterials
    }
}