using UnityEngine;

public class DialogActionSwapWeapon : BaseDialogAction
{
    public override void PerformAction(EntityPlayer player)
    {
        var playerUI = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        var myEntity = playerUI.xui.Dialog.Respondent as IEntityAliveSDX;
        if (myEntity == null)
        {
            Debug.Log("Respondent is not IEntityAliveSDX");
            return;
        }

        var entityAlive = myEntity as EntityAlive;
        if (entityAlive == null) return;
        var item = ItemClass.GetItem(ID);
        entityAlive.Buffs.SetCustomVar("CurrentWeaponID", item.GetItemId());
        myEntity.UpdateWeapon(ID);
        EntityUtilities.UpdateHandItem(entityAlive.entityId, ID);
    }
    
 
}