using UnityEngine;
class XUiC_EntityInformationPanelSDX : XUiController
{
    public override bool GetBindingValue(ref string value, BindingItem binding)
    {
        EntityPlayer player = base.xui.playerUI.entityPlayer;
        if (player == null)
            return false;

        int entityID = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if (entityID == 0)
            return false;

        string fieldName = binding.FieldName;
        if (fieldName != null)
        {
            if (fieldName == "statement")
            {
                EntityAliveSDX myEntity = player.world.GetEntity(entityID) as EntityAliveSDX;
                if (myEntity)
                {
                    value = EntityUtilities.DisplayEntityStats(entityID);
                }
                    return false;

            }
        }
        return false;
    }
}

