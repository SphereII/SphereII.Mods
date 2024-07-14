public class DialogActionCVarSDX : DialogActionAddBuff
{
    private static readonly string AdvFeatureClass = "AdvancedDialogDebugging";

    public virtual float UpdateCVar(float CurrentValue)
    {
        return CurrentValue++;
    }
    public override void PerformAction(EntityPlayer player)
    {
        if (string.IsNullOrEmpty(Value))
            Value = "1";
        
        var strDisplay = "AddCVar: " + ID + " Value: " + Value;
        if (!player.Buffs.HasCustomVar(ID))
        {
            AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Adding Custom CVAr");
            (player as EntityPlayerLocal).Buffs.AddCustomVar(ID, 0);
        }

        var currentValue = player.Buffs.GetCustomVar(ID);
        currentValue = UpdateCVar(currentValue);
        player.Buffs.SetCustomVar(ID, currentValue);
        AdvLogging.DisplayLog(AdvFeatureClass, strDisplay + " Setting to " + currentValue);
    }
}

public class DialogActionReduceCVar : DialogActionAddCVarSDX
{
    public override float UpdateCVar(float CurrentValue)
    {
        return CurrentValue--;
    }
}

public class DialogActionAddCVarSDX : DialogActionCVarSDX
{
    public override float UpdateCVar(float CurrentValue)
    {
        int.TryParse(Value, out var flValue);
        return flValue;
    }
}