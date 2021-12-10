public class DialogRequirementHasCVarSDX : BaseDialogRequirement
{
    private static readonly string AdvFeatureClass = "AdvancedDialogDebugging";

   // public string strOperator = "eq";

    public virtual bool CheckValue(float playerValue, float value)
    {
        return playerValue == value;
    }
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        AdvLogging.DisplayLog(AdvFeatureClass, "Player: " + player.EntityName);
        foreach (var temp in player.Buffs.CVars) 
            AdvLogging.DisplayLog(AdvFeatureClass, "\tCVar: " + temp.Key + " Value: " + temp.Value);

        // If the character doesn't have a cvar, add it.
        if (!player.Buffs.HasCustomVar(ID))
            player.Buffs.SetCustomVar(ID, 0);

        if (player.Buffs.HasCustomVar(ID))
        {
            // If value is not specified, accepted it.
            if (string.IsNullOrEmpty(Value))
                return true;

            float.TryParse(Value, out var flValue);
            var flPlayerValue = player.Buffs.GetCustomVar(ID);
         //   AdvLogging.DisplayLog(AdvFeatureClass, GetType() + " HasCvar: " + ID + " Value: " + flValue + " Player Value: " + flPlayerValue + " Operator: " + strOperator);

            return CheckValue(flPlayerValue, flValue);
        }
    
        // If the Cvar does not exist, but does have a value to be checked, pass the condition.It just may not be set yet.
        if (string.IsNullOrEmpty(Value))
        {
            AdvLogging.DisplayLog(AdvFeatureClass, "HasCVar: " + ID + " Player does not have this cvar. Returning true.");
            return true;
        }

        AdvLogging.DisplayLog(AdvFeatureClass, "Has CVar:: false");
        return false;
    }
}

public class DialogRequirementCVarGT : DialogRequirementHasCVarSDX
{
    public override bool CheckValue(float playerValue, float value)
    {
        return playerValue > value;
    }
}

public class DialogRequirementCVarGTE : DialogRequirementHasCVarSDX
{
    public override bool CheckValue(float playerValue, float value)
    {
        return playerValue >= value;
    }
}

public class DialogRequirementCVarLT : DialogRequirementHasCVarSDX
{
    public override bool CheckValue(float playerValue, float value)
    {
        return playerValue < value;
    }
}

public class DialogRequirementCVarLTE : DialogRequirementHasCVarSDX
{
    public override bool CheckValue(float playerValue, float value)
    {
        return playerValue <= value;
    }
}

public class DialogRequirementNotCVarGTE : DialogRequirementHasCVarSDX
{
    public override bool CheckValue(float playerValue, float value)
    {
        return !base.CheckValue(playerValue, value);
    }
}

public class DialogRequirementNotCVarLT : DialogRequirementHasCVarSDX
{
    public override bool CheckValue(float playerValue, float value)
    {
        return !base.CheckValue(playerValue, value);
    }
}

public class DialogRequirementNotCVarLTE : DialogRequirementHasCVarSDX
{
    public override bool CheckValue(float playerValue, float value)
    {
        return !base.CheckValue(playerValue, value);
    }
}
public class DialogRequirementNotCVarGT : DialogRequirementHasCVarSDX
{
    public override bool CheckValue(float playerValue, float value)
    {
        return !base.CheckValue(playerValue, value);
    }
}