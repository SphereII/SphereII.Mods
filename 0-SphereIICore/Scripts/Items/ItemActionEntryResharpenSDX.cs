using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ItemActionEntryResharpenSDX : BaseItemActionEntry
{
    private enum StateTypes
    {
        Normal,
        NotEnoughMaterials
    }

    private string lblNeedMaterials;
    private ItemActionEntryResharpenSDX.StateTypes state;

    public ItemActionEntryResharpenSDX(XUiController controller) : base(controller, "Resharpen", "ui_game_symbol_wrench", BaseItemActionEntry.GamepadShortCut.DPadLeft, "crafting/craft_click_craft", "ui/ui_denied")
    {
        this.lblNeedMaterials = Localization.Get("xuiRepairMissingMats");
        controller.xui.PlayerInventory.OnBackpackItemsChanged += this.PlayerInventory_OnBackpackItemsChanged;
        controller.xui.PlayerInventory.OnToolbeltItemsChanged += this.PlayerInventory_OnToolbeltItemsChanged;
    }

    private void PlayerInventory_OnToolbeltItemsChanged()
    {
        this.RefreshEnabled();
        if (base.ParentItem != null)
        {
            base.ParentItem.MarkDirty();
        }
    }

    private void PlayerInventory_OnBackpackItemsChanged()
    {
        this.RefreshEnabled();
        if (base.ParentItem != null)
        {
            base.ParentItem.MarkDirty();
        }
    }

    public override void OnDisabledActivate()
    {
        ItemActionEntryResharpenSDX.StateTypes stateTypes = this.state;
        if (stateTypes != ItemActionEntryResharpenSDX.StateTypes.NotEnoughMaterials)
        {
            return;
        }
        GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, this.lblNeedMaterials);
        ItemClass forId = ItemClass.GetForId(((XUiC_ItemStack)base.ItemController).ItemStack.itemValue.type);
        if (forId.Properties.Contains("SharpenItem"))
        {
            String strSharpenItem = String.Empty;
            forId.Properties.ParseString("SharpenItem", ref strSharpenItem);
            if (String.IsNullOrEmpty(strSharpenItem))
                return;
            ItemClass itemClass = ItemClass.GetItemClass(strSharpenItem, false);
            if (itemClass != null)
            {
                ItemStack @is = new ItemStack(new ItemValue(itemClass.Id, false), 0);
                base.ItemController.xui.playerUI.entityPlayer.AddUIHarvestingItem(@is, true);
            }
        }
    }

    public override void RefreshEnabled()
    {
        base.RefreshEnabled();
        this.state = ItemActionEntryResharpenSDX.StateTypes.Normal;
        XUi xui = base.ItemController.xui;
        if (((XUiC_ItemStack)base.ItemController).ItemStack.IsEmpty() || ((XUiC_ItemStack)base.ItemController).StackLock)
        {
            return;
        }
        ItemClass forId = ItemClass.GetForId(((XUiC_ItemStack)base.ItemController).ItemStack.itemValue.type);
        base.Enabled = (this.state == ItemActionEntryResharpenSDX.StateTypes.Normal);
        if (!base.Enabled)
        {
            base.IconName = "ui_game_symbol_book";
            return;
        }
        ItemValue itemValue = ((XUiC_ItemStack)base.ItemController).ItemStack.itemValue;
        if (forId.RepairTools != null && forId.RepairTools.Length > 0)
        {
            ItemClass itemClass = ItemClass.GetItemClass(forId.RepairTools[0].Value, false);
            if (itemClass != null)
            {
                int b = Convert.ToInt32(Math.Ceiling((double)((float)Mathf.CeilToInt(itemValue.UseTimes) / (float)itemClass.RepairAmount.Value)));
                xui.PlayerInventory.GetItemCount(new ItemValue(itemClass.Id, false));
                if (Mathf.Min(xui.PlayerInventory.GetItemCount(new ItemValue(itemClass.Id, false)), b) * itemClass.RepairAmount.Value <= 0)
                {
                    this.state = ItemActionEntryResharpenSDX.StateTypes.NotEnoughMaterials;
                    base.Enabled = (this.state == ItemActionEntryResharpenSDX.StateTypes.Normal);
                }
            }
        }
    }

    public static void WarnQueueFull(XUiController ItemController)
    {
        string text = "No room in queue!";
        if (Localization.Exists("wrnQueueFull"))
        {
            text = Localization.Get("wrnQueueFull");
        }
        GameManager.ShowTooltip(ItemController.xui.playerUI.entityPlayer, text);
        Audio.Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
    }

    public override void OnActivated()
    {
        XUi xui = base.ItemController.xui;
        XUiM_PlayerInventory playerInventory = xui.PlayerInventory;
        ItemValue itemValue = ((XUiC_ItemStack)base.ItemController).ItemStack.itemValue;
        ItemClass forId = ItemClass.GetForId(itemValue.type);
        XUiC_CraftingWindowGroup childByType = xui.FindWindowGroupByName("crafting").GetChildByType<XUiC_CraftingWindowGroup>();

        if (itemValue.HasQuality)
        {
            if (itemValue.PercentUsesLeft < 0.30 )
            {
                String text = "This item is too worn out to be resharpened.";
                GameManager.ShowTooltip(ItemController.xui.playerUI.entityPlayer, text);
                return;
            }
            if (itemValue.PercentUsesLeft > 0.8)
            {
                String text = "This item is still in pretty good shape.";
                GameManager.ShowTooltip(ItemController.xui.playerUI.entityPlayer, text);
                return;

            }
        }
        if (forId.Properties.Contains("SharpenItem"))
        {
            String strSharpenItem = String.Empty;
            forId.Properties.ParseString("SharpenItem", ref strSharpenItem);
            if (String.IsNullOrEmpty(strSharpenItem))
                return;

            Recipe recipe = new Recipe();
            recipe.count = 1;
            ItemClass itemClass = ItemClass.GetItemClass(strSharpenItem, false);
            if (itemClass == null)
                return;

            
            int Count = playerInventory.GetItemCount(new ItemValue(itemClass.Id, false));
            if (Count < 1)
            {
                String text = "Not enough " + strSharpenItem + " to craft this: " + Count + " / 1";
                GameManager.ShowTooltip(ItemController.xui.playerUI.entityPlayer, text);
                return;
            }
            if (childByType != null)
            {
                recipe.ingredients.Add(new ItemStack(new ItemValue(itemClass.Id, false), 1));
                recipe.itemValueType = itemValue.type;
                recipe.craftingTime = 1;
                recipe.craftExpGain = 1;
            }
           // ItemClass.GetForId(recipe.itemValueType);
            GameRandom random = GameRandomManager.Instance.CreateGameRandom();
            float flRandom = random.RandomRange((int)itemValue.UseTimes, ((float)itemValue.MaxUseTimes / 1.20f));
            if (!childByType.AddRepairItemToQueue(recipe.craftingTime, itemValue.Clone(), (int)flRandom))
            {
                WarnQueueFull(ItemController);
                return;
            }
            ((XUiC_ItemStack)ItemController).ItemStack = ItemStack.Empty.Clone();
            playerInventory.RemoveItems(recipe.ingredients, 1);
        }
    }
}
