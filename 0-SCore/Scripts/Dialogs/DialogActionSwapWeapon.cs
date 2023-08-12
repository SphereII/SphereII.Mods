using UnityEngine;

public class DialogActionSwapWeapon : BaseDialogAction
{
    public override void PerformAction(EntityPlayer player)
    {
        var playerUI = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        var myEntity = playerUI.xui.Dialog.Respondent as EntityAliveSDX;
        if (myEntity == null)
        {
            Debug.Log("Respondent is not EntityAliveSDX");
            return;
        }

        var item = ItemClass.GetItem(ID);
        myEntity.Buffs.SetCustomVar("CurrentWeaponID", item.GetItemId());
        myEntity.UpdateWeapon(item);
        EntityUtilities.UpdateHandItem(myEntity.entityId);
    }
    
 
}