using UnityEngine;

public class DialogActionAnimatorSet : BaseDialogAction
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

        // if (myEntity.emodel.avatarController.TryGetInt(ID, out var value))
        // {
        //     Debug.Log($"Current ID and Value {ID} : {value}");
        // }
        if (StringParsers.TryParseFloat(Value, out var result))
        {
            myEntity.emodel.avatarController.UpdateInt(ID, (int)result);    
        }
        // if (myEntity.emodel.avatarController.TryGetInt(ID, out var value2))
        // {
        //     Debug.Log($"New ID and Value {ID} : {value2}");
        // }
        
    }
    
 
}