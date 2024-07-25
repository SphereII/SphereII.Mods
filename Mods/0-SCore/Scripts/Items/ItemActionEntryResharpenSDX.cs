using Audio;
using System;
using UnityEngine;

public class ItemActionEntryResharpenSDX : BaseItemActionEntry
{
    private readonly string lblNeedMaterials;
    private StateTypes state;

    public ItemActionEntryResharpenSDX(XUiController controller) : base(controller, "Resharpen", "ui_game_symbol_wrench", GamepadShortCut.DPadLeft)
    {
        lblNeedMaterials = Localization.Get("xuiRepairMissingMats");
        controller.xui.PlayerInventory.OnBackpackItemsChanged += PlayerInventory_OnBackpackItemsChanged;
        controller.xui.PlayerInventory.OnToolbeltItemsChanged += PlayerInventory_OnToolbeltItemsChanged;
    }

    private void PlayerInventory_OnToolbeltItemsChanged()
    {
        RefreshEnabled();
        if (ParentItem != null) ParentItem.MarkDirty();
    }

    private void PlayerInventory_OnBackpackItemsChanged()
    {
        RefreshEnabled();
        if (ParentItem != null) ParentItem.MarkDirty();
    }

    public override void OnDisabledActivate()
    {
        var stateTypes = state;
        if (stateTypes != StateTypes.NotEnoughMaterials) return;
        GameManager.ShowTooltip(ItemController.xui.playerUI.entityPlayer, lblNeedMaterials);
        var forId = ItemClass.GetForId(((XUiC_ItemStack)ItemController).ItemStack.itemValue.type);
        if (forId.Properties.Contains("SharpenItem"))
        {
            var strSharpenItem = string.Empty;
            forId.Properties.ParseString("SharpenItem", ref strSharpenItem);
            if (string.IsNullOrEmpty(strSharpenItem))
                return;
            var itemClass = ItemClass.GetItemClass(strSharpenItem);
            if (itemClass != null)
            {
                var @is = new ItemStack(new ItemValue(itemClass.Id), 0);
                ItemController.xui.playerUI.entityPlayer.AddUIHarvestingItem(@is, true);
            }
        }
    }

    public override void RefreshEnabled()
    {
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
        if (forId.RepairTools != null && forId.RepairTools.Length > 0)
        {
            var itemClass = ItemClass.GetItemClass(forId.RepairTools[0].Value);
            if (itemClass != null)
            {
                var b = Convert.ToInt32(Math.Ceiling(Mathf.CeilToInt(itemValue.UseTimes) / (float)itemClass.RepairAmount.Value));
                xui.PlayerInventory.GetItemCount(new ItemValue(itemClass.Id));
                if (Mathf.Min(xui.PlayerInventory.GetItemCount(new ItemValue(itemClass.Id)), b) * itemClass.RepairAmount.Value <= 0)
                {
                    state = StateTypes.NotEnoughMaterials;
                    Enabled = state == StateTypes.Normal;
                }
            }
        }
    }

    public static void WarnQueueFull(XUiController ItemController)
    {
        var text = "No room in queue!";
        if (Localization.Exists("wrnQueueFull")) text = Localization.Get("wrnQueueFull");
        GameManager.ShowTooltip(ItemController.xui.playerUI.entityPlayer, text);
        Manager.PlayInsidePlayerHead("ui_denied");
    }

    public override void OnActivated()
    {
        var xui = ItemController.xui;
        var playerInventory = xui.PlayerInventory;
        var itemValue = ((XUiC_ItemStack)ItemController).ItemStack.itemValue;
        var forId = ItemClass.GetForId(itemValue.type);
        var childByType = xui.FindWindowGroupByName("crafting").GetChildByType<XUiC_CraftingWindowGroup>();

        if (itemValue.HasQuality)
        {
            if (itemValue.PercentUsesLeft < 0.30)
            {
                var text = "This item is too worn out to be resharpened.";
                GameManager.ShowTooltip(ItemController.xui.playerUI.entityPlayer, text);
                return;
            }

            if (itemValue.PercentUsesLeft > 0.8)
            {
                var text = "This item is still in pretty good shape.";
                GameManager.ShowTooltip(ItemController.xui.playerUI.entityPlayer, text);
                return;
            }
        }

        if (forId.Properties.Contains("SharpenItem"))
        {
            var strSharpenItem = string.Empty;
            forId.Properties.ParseString("SharpenItem", ref strSharpenItem);
            if (string.IsNullOrEmpty(strSharpenItem))
                return;

            var recipe = new Recipe();
            recipe.count = 1;
            var itemClass = ItemClass.GetItemClass(strSharpenItem);
            if (itemClass == null)
                return;


            var Count = playerInventory.GetItemCount(new ItemValue(itemClass.Id));
            if (Count < 1)
            {
                var text = "Not enough " + strSharpenItem + " to craft this: " + Count + " / 1";
                GameManager.ShowTooltip(ItemController.xui.playerUI.entityPlayer, text);
                return;
            }

            if (childByType != null)
            {
                recipe.ingredients.Add(new ItemStack(new ItemValue(itemClass.Id), 1));
                recipe.itemValueType = itemValue.type;
                recipe.craftingTime = 1;
                recipe.craftExpGain = 1;
            }

            // ItemClass.GetForId(recipe.itemValueType);
            var random = GameRandomManager.Instance.CreateGameRandom();
            var flRandom = random.RandomRange((int)itemValue.UseTimes, itemValue.MaxUseTimes / 1.20f);
            if (!childByType.AddRepairItemToQueue(recipe.craftingTime, itemValue.Clone(), (int)flRandom))
            {
                WarnQueueFull(ItemController);
                return;
            }

            ((XUiC_ItemStack)ItemController).ItemStack = ItemStack.Empty.Clone();
            playerInventory.RemoveItems(recipe.ingredients);
        }
    }

    private enum StateTypes
    {
        Normal,
        NotEnoughMaterials
    }
}