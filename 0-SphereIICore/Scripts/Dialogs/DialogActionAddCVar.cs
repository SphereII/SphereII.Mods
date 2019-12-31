using UnityEngine;
public class DialogActionAddCVar : DialogActionAddBuff
{
    private static string AdvFeatureClass = "AdvancedDialogDebugging";

    public string strOperator = "add";

    public override void PerformAction(EntityPlayer player)
    {
        if (string.IsNullOrEmpty(Value))
            Value = "1";

        int flValue = 1;
        int.TryParse(Value, out flValue);

        string strDisplay = "AddCVar: " + ID + " Value: " + flValue + " Operator: " + strOperator;
        if (!player.Buffs.HasCustomVar(ID))
        {
            AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Adding Custom CVAr");
            (player as EntityPlayerLocal).Buffs.AddCustomVar(ID, 0);
        }

        float CurrentValue = player.Buffs.GetCustomVar(ID);
        if (strOperator.ToLower() == "add")
            CurrentValue += flValue;
        else if (strOperator.ToLower() == "sub")
            CurrentValue -= flValue;
        else  /// default is set
            CurrentValue = flValue;

        player.Buffs.SetCustomVar(ID, CurrentValue, true);
        AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Setting to " + CurrentValue);
        return;
    }
}
