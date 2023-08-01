using UnityEngine;

public class DialogActionPickUpNPC : BaseDialogAction
{
    public override void PerformAction(EntityPlayer player)
    {
        var playerUI = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        var myEntity = playerUI.xui.Dialog.Respondent as EntityAliveSDX;
        if (myEntity == null) return;

        if (!string.IsNullOrEmpty(ID))
        {
            if (myEntity.lootContainer.items.Length > 0)
            {
                GameManager.ShowTooltip(player as EntityPlayerLocal, "npcHasItems", string.Empty, "ui_denied", null);
                return;
            }
        }
        var itemValue = myEntity.GetItemValue();
        var itemStack = new ItemStack(itemValue, 1);
        if (player.inventory.CanTakeItem(itemStack) || player.bag.CanTakeItem(itemStack))
        {
            EntityUtilities.CollectEntityServer(myEntity.entityId, player.entityId);
        }
        else
        {
            GameManager.ShowTooltip(player as EntityPlayerLocal, Localization.Get("xuiInventoryFullForPickup"), string.Empty, "ui_denied", null);
        }
    }
    
  
}