public class DialogActionAddCVar : DialogActionAddBuff
{
    private static readonly string AdvFeatureClass = "AdvancedDialogDebugging";

    public string strOperator = "add";

    public override void PerformAction(EntityPlayer player)
    {
        if (string.IsNullOrEmpty(Value))
            Value = "1";

        int.TryParse(Value, out var flValue);

        var strDisplay = "AddCVar: " + ID + " Value: " + flValue + " Operator: " + strOperator;
        if (!player.Buffs.HasCustomVar(ID))
        {
            AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Adding Custom CVAr");
            player.Buffs.AddCustomVar(ID, 0);
        }

        var currentValue = player.Buffs.GetCustomVar(ID);
        switch (strOperator.ToLower())
        {
            case "add":
                currentValue += flValue;
                break;
            case "sub":
                currentValue -= flValue;
                break;
            // default is set
            default:
                currentValue = flValue;
                break;
        }

        player.Buffs.SetCustomVar(ID, currentValue, true);
        AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Setting to " + currentValue);
        return;
    }
}
