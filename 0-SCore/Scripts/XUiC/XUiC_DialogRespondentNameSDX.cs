public class XUiC_DialogRespondentNameSDX : XUiC_DialogRespondentName
{
    public override void OnOpen()
    {
        EntityPlayer player = xui.playerUI.entityPlayer;
        if (player != null && player.Buffs.HasCustomVar("CurrentNPC"))
        {
            int entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");
            if (entityID > 0)
            {
                EntityAlive npc = player.world.GetEntity(entityID) as EntityAlive;
                if (npc)
                    npc.emodel.avatarController.SetBool("IsBusy", true);
            }

        }
        base.OnOpen();
    }

    public override void OnClose()
    {
        EntityPlayer player = xui.playerUI.entityPlayer;
        if (player != null && player.Buffs.HasCustomVar("CurrentNPC"))
        {
            int entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");
            if (entityID > 0)
            {
                EntityAlive npc = player.world.GetEntity(entityID) as EntityAlive;
                if (npc)
                    npc.emodel.avatarController.SetBool("IsBusy", false);
            }

        }

        base.OnClose();
    }

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
            if (fieldName == "respondentname")
            {
                var myEntity = player.world.GetEntity(entityID) as EntityAlive;
                if (myEntity)
                {
                    if (EntityUtilities.GetHireCost(entityID) <= 0)
                        value = myEntity.EntityName;
                    //else
                    //    value = myEntity.EntityName;// + " ( Hire for " + myEntity.GetHireCost() + " " + myEntity.GetHireCurrency().ItemClass.Name + " )";
                    return true;
                }

                value = !(xui.Dialog.Respondent != null) ? string.Empty : Localization.Get(xui.Dialog.Respondent.EntityName);
                return true;
            }

        return false;
    }
}