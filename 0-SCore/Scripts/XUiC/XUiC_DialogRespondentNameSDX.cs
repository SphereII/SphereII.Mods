public class XUiC_DialogRespondentNameSDX : XUiC_DialogRespondentName
{
    //public override void OnOpen()
    //{
    //    EntityPlayer player = xui.playerUI.entityPlayer;
    //    if (player != null && player.Buffs.HasCustomVar("CurrentNPC"))
    //    {
    //        int entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");
    //        if (entityID > 0)
    //        {
    //            EntityAlive npc = player.world.GetEntity(entityID) as EntityAlive;
    //            if (npc)
    //                npc.emodel.avatarController.SetBool("IsBusy", true);
    //        }

    //    }
    //    base.OnOpen();
    //}

    //public override void OnClose()
    //{
    //    EntityPlayer player = xui.playerUI.entityPlayer;
    //    if (player != null && player.Buffs.HasCustomVar("CurrentNPC"))
    //    {
    //        int entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");
    //        if (entityID > 0)
    //        {
    //            EntityAlive npc = player.world.GetEntity(entityID) as EntityAlive;
    //            if (npc)
    //                npc.emodel.avatarController.SetBool("IsBusy", false);
    //        }

    //    }

    //    base.OnClose();
    //}

    public override bool GetBindingValue(ref string value, string binding)
    {
        if (binding == null) return base.GetBindingValue(ref value, binding);

        var fieldName = "";

        fieldName = binding;
        if (fieldName != null)
        {
            if (fieldName == "respondentname")
            {
                value = "";
                EntityPlayer player = xui.playerUI.entityPlayer;
                if (player == null)
                    return true;

                var entityID = 0;
                if (player.Buffs.HasCustomVar("CurrentNPC"))
                    entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

                if (entityID == 0)
                    return true;

                var myEntity = player.world.GetEntity(entityID) as EntityAlive;
                if (myEntity)
                {
                    value = myEntity.EntityName;
                    return true;
                }

                value = "Unknown";
                return true;
            }
        }
        return true;
    }
}