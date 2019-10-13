using UnityEngine;

public class XUiC_DialogHireInformationSDX : XUiC_DialogRespondentName
{
    public override void OnOpen()
    {
        EntityPlayer player = base.xui.playerUI.entityPlayer;

        int entityID = 0;
        if(player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if(entityID == 0)
            return;

        EntityAliveSDX myEntity = player.world.GetEntity(entityID) as EntityAliveSDX;
        if(myEntity != null)
        {
            if(EntityUtilities.isTame(entityID, base.xui.playerUI.entityPlayer))
                return;
        }
        base.OnOpen();
        base.RefreshBindings(false);
    }
  
}