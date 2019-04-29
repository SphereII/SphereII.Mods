public class XUiC_DialogRespondentNameSDX : XUiC_DialogRespondentName
{
    public override bool GetBindingValue(ref string value, BindingItem binding)
    {
        string fieldName = binding.FieldName;
        if (fieldName != null)
        {
            if (fieldName == "respondentname")
            {
                if (base.xui.Dialog.Respondent != null)
                {
                    EntityAliveSDX myEntity = base.xui.playerUI.entityPlayer.world.GetEntity(base.xui.Dialog.Respondent.entityId) as EntityAliveSDX;
                    if (myEntity)
                    {
                        if (myEntity.GetHireCost() <= 0)
                            value = myEntity.EntityName;
                        else
                            value = myEntity.EntityName;// + " ( Hire for " + myEntity.GetHireCost() + " " + myEntity.GetHireCurrency().ItemClass.Name + " )";
                        return true;
                    }
                }
                value = ((!(base.xui.Dialog.Respondent != null)) ? string.Empty : Localization.Get(base.xui.Dialog.Respondent.EntityName, string.Empty));
                return true;
            }
        }
        return false;
    }
}