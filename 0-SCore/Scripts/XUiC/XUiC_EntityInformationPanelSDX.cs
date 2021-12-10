internal class XUiC_EntityInformationPanelSDX : XUiController
{
    public override bool GetBindingValue(ref string value, string binding)
    {
        EntityPlayer player = xui.playerUI.entityPlayer;
        if (player == null)
            return false;

        var entityID = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if (entityID == 0)
            return false;

        var fieldName = "";

        fieldName = binding;
        if (fieldName != null)
            if (fieldName == "statement")
            {
                var myEntity = player.world.GetEntity(entityID) as EntityAliveSDX;
                if (myEntity) value = EntityUtilities.DisplayEntityStats(entityID);
                return false;
            }

        return false;
    }
}