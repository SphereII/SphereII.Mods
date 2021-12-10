public class DialogRequirementHasCVarSDX : BaseDialogRequirement
{
    private static readonly string AdvFeatureClass = "AdvancedDialogDebugging";

    public string strOperator = "eq";
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        // If the character doesn't have a cvar, add it.
        if (!player.Buffs.HasCustomVar(ID))
            player.Buffs.SetCustomVar(ID, 0);

        if (player.Buffs.HasCustomVar(ID))
        {
            // If value is not specified, accepted it.
            if (string.IsNullOrEmpty(Value))
                return true;

            float.TryParse(Value, out var flValue);
            float flPlayerValue = player.Buffs.GetCustomVar(ID);
            AdvLogging.DisplayLog(AdvFeatureClass, GetType() + " HasCvar: " + ID + " Value: " + flValue + " Player Value: " + flPlayerValue + " Operator: " + strOperator);


            switch (strOperator.ToLower())
            {
                case "lt":
                    {
                        if (flPlayerValue < flValue)
                            return true;
                        break;
                    }
                case "lte":
                    {
                        if (flPlayerValue <= flValue)
                            return true;
                        break;
                    }
                case "gt":
                    {
                        if (flPlayerValue > flValue)
                            return true;
                        break;
                    }
                case "gte":
                    {
                        if (flPlayerValue >= flValue)
                            return true;
                        break;
                    }
                case "neq":
                    {
                        if (flValue != flPlayerValue)
                            return true;
                        break;
                    }
                default:
                    {
                        if (flValue == flPlayerValue)
                            return true;
                        break;
                    }
            }

            AdvLogging.DisplayLog(AdvFeatureClass, GetType() + " Has CVar: " + ID + "  Value: " + flValue + " Player Value: " + flPlayerValue + " Operator: " + strOperator + " :: No Result");

        }
        else if (strOperator.ToLower() == "not")
            return true;

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


