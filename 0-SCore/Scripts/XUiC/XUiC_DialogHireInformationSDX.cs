public class XUiC_DialogHireInformationSDX : XUiC_DialogRespondentName
{
    public override void OnOpen()
    {
        EntityPlayer player = xui.playerUI.entityPlayer;

        var entityID = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if (entityID == 0)
            return;

        var myEntity = player.world.GetEntity(entityID) as EntityAliveSDX;
        if (myEntity != null)
            if (EntityUtilities.isTame(entityID, xui.playerUI.entityPlayer))
                return;
        base.OnOpen();
        RefreshBindings();
    }
}