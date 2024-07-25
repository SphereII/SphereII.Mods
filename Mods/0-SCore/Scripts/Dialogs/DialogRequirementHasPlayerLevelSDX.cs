public class DialogRequirementHasPlayerLevelSDX : BaseDialogRequirement
{
    private static readonly string AdvFeatureClass = "AdvancedDialogDebugging";

    public string strOperator = "eq";

    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        if (player.Progression.Level <= Progression.MaxLevel)
        {
            // If value is not specified, accepted it.
            if (string.IsNullOrEmpty(Value))
                return true;

            float.TryParse(Value, out var flValue);
            float flPlayerValue = player.Buffs.GetCustomVar(ID);
            AdvLogging.DisplayLog(AdvFeatureClass, GetType() + " HasPlayerLevel: " + " Value: " + flValue + " Player Value: " + flPlayerValue + " Operator: " + strOperator);


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

            AdvLogging.DisplayLog(AdvFeatureClass, GetType() + " Is Level: " + "  Value: " + flValue + " Player Value: " + flPlayerValue + " Operator: " + strOperator + " :: No Result");
        }

        // If the Cvar does not exist, but does have a value to be checked, pass the condition.It just may not be set yet.
        if (string.IsNullOrEmpty(Value))
        {
            AdvLogging.DisplayLog(AdvFeatureClass, "HasCVar: " + " Player does not have this cvar. Returning true.");
            return true;
        }

        AdvLogging.DisplayLog(AdvFeatureClass, "Has CVar:: false");
        return false;
    }
}