using UnityEngine;

public class XUiC_DialogHireInformationSDX : XUiC_DialogRespondentName
{
    public override void OnOpen()
    {
        Entity respondent = base.xui.Dialog.Respondent;
        if (respondent != null)
        {
            EntityAliveSDX myEntity = respondent.world.GetEntity(respondent.entityId) as EntityAliveSDX;
            if (myEntity)
            {
                if (myEntity.isTame(base.xui.playerUI.entityPlayer))
                {
                    return;
                }
            }
        }
        base.OnOpen();
        base.RefreshBindings(false);

    }
  
}