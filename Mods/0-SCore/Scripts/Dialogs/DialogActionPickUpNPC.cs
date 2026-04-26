using UnityEngine;

public class DialogActionPickUpNPC : BaseDialogAction
{
    public override void PerformAction(EntityPlayer player)
    {
        var playerUI = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        var myEntity = playerUI.xui.Dialog.Respondent as EntityAlive;
        if (myEntity == null || myEntity is not IEntityAliveSDX) return;

        if (HasNpcInInventory(myEntity))
        {
            GameManager.ShowTooltip(player as EntityPlayerLocal, Localization.Get("npcContainsNPC"), string.Empty, "ui_denied", null);
            return;
        }

        if (!string.IsNullOrEmpty(ID))
        {
            if (myEntity.lootContainer?.items.Length > 0)
            {
                GameManager.ShowTooltip(player as EntityPlayerLocal, Localization.Get("npcHasItems"), string.Empty, "ui_denied", null);
                return;
            }
        }

        var itemValue = EntitySyncUtils.GetNPCItemValue(myEntity);
        var itemStack = new ItemStack(itemValue, 1);
        if (player.inventory.CanTakeItem(itemStack) || player.bag.CanTakeItem(itemStack))
        {
            EntitySyncUtils.Collect(myEntity.entityId, player.entityId);
        }
        else
        {
            GameManager.ShowTooltip(player as EntityPlayerLocal, Localization.Get("xuiInventoryFullForPickup"), string.Empty, "ui_denied", null);
        }
    }

    private static bool HasNpcInInventory(EntityAlive entity)
    {
        foreach (var stack in entity.inventory.GetSlots())
        {
            if (!stack.IsEmpty() && stack.itemValue.HasMetadata("EntityClassId"))
                return true;
        }

        if (entity is EntityTrader && HarvestManager.Has(entity.entityId))
        {
            var hc = HarvestManager.GetOrCreate(entity.entityId);
            foreach (var stack in hc.items)
            {
                if (!stack.IsEmpty() && stack.itemValue.HasMetadata("EntityClassId"))
                    return true;
            }
        }
        else if (entity.lootContainer?.items != null)
        {
            foreach (var stack in entity.lootContainer.items)
            {
                if (!stack.IsEmpty() && stack.itemValue.HasMetadata("EntityClassId"))
                    return true;
            }
        }

        return false;
    }
}