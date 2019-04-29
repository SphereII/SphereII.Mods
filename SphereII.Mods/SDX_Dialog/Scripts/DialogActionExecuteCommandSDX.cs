using UnityEngine;
public class DialogActionExecuteCommandSDX : DialogActionAddBuff
{
    public override BaseDialogAction.ActionTypes ActionType
    {
        get
        {
            return BaseDialogAction.ActionTypes.AddBuff;
        }
    }

    public override void PerformAction(EntityPlayer player)
    {
        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        // The respondent is an EntityNPC, and we don't have that. Check for the patch scripted otherEntitySDX.
        Entity respondent = uiforPlayer.xui.Dialog.Respondent;
        if (respondent != null)
        {
            EntityAliveSDX myEntity = player.world.GetEntity(respondent.entityId) as EntityAliveSDX;
            if ( myEntity != null )
            {
                myEntity.ExecuteCMD(base.ID, player);
            }
        }
    }

    private string name = string.Empty;
}
