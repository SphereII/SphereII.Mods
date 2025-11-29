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
        //var itemValue = myEntity.GetItemValue();
        var itemValue = EntitySyncUtils.GetNPCItemValue(myEntity);
        var itemStack = new ItemStack(itemValue, 1);
        if (player.inventory.CanTakeItem(itemStack) || player.bag.CanTakeItem(itemStack))
        {
            // Close UI to prevent packet spam/errors
          //  playerUI.windowManager.CloseAllOpenWindows(null, false);
          //            playerUI.xui.Dialog.Respondent = null;
            //myEntity.Collect(player.entityId);
            EntitySyncUtils.Collect(myEntity, player.entityId); 
            
        }
        else
        {
            GameManager.ShowTooltip(player as EntityPlayerLocal, Localization.Get("xuiInventoryFullForPickup"), string.Empty, "ui_denied", null);
        }
    }
    
  
}