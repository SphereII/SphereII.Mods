using UnityEngine;

public class DialogActionAnimatorSet : BaseDialogAction
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
        if (StringParsers.TryParseFloat(Value, out var result))
        {
            entityAlive.emodel.avatarController.UpdateInt(ID, (int)result);
        }
        
    }
    
 
}