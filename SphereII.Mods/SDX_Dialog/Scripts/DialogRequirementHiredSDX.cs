using UnityEngine;
public class DialogRequirementHiredSDX : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player)
    {
        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        
        // The respondent is an EntityNPC, and we don't have that. Check for the patch scripted otherEntitySDX.
        Entity respondent = uiforPlayer.xui.Dialog.Respondent;
         if (respondent != null)
        {
            EntityAliveSDX myEntity = respondent.world.GetEntity(respondent.entityId) as EntityAliveSDX;
            if (myEntity != null)
            {
                bool isTame = false;
                if ( base.Value.EqualsCaseInsensitive("not"))
                    isTame=  !myEntity.isTame(player);
                else
                    isTame = myEntity.isTame(player);
                return isTame;
            }
         }
        return false;
    }



}


