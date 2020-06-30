using UnityEngine;
public class XUiC_DialogRespondentNameSDX : XUiC_DialogRespondentName
{
    public override bool GetBindingValue(ref string value, BindingItem binding)
    {
        EntityPlayer player = base.xui.playerUI.entityPlayer;
        if(player == null)
            return false;

        int entityID = 0;
        if(player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if(entityID == 0)
            return false;

        string fieldName = binding.FieldName;
        if(fieldName != null)
        {
            if(fieldName == "respondentname")
            {
                EntityAlive myEntity = player.world.GetEntity(entityID) as EntityAlive;
                if(myEntity)
                {
                    if(EntityUtilities.GetHireCost( entityID) <= 0)
                        value = myEntity.EntityName;
                    //else
                    //    value = myEntity.EntityName;// + " ( Hire for " + myEntity.GetHireCost() + " " + myEntity.GetHireCurrency().ItemClass.Name + " )";
                    return true;
                }
                value = ((!(base.xui.Dialog.Respondent != null)) ? string.Empty : Localization.Get(base.xui.Dialog.Respondent.EntityName));
                return true;
            }
        }
        return false;
    }
}