public class DialogRequirementHasCVarSDX : BaseDialogRequirement, IDialogOperator
{
    private static readonly string AdvFeatureClass = "AdvancedDialogDebugging";

    public string Operator { get; set; } = "eq";

    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var invert = Operator.ToLower() == "not";

        // If the character doesn't have a cvar, add it.
        if (!player.Buffs.HasCustomVar(ID))
            player.Buffs.SetCustomVar(ID, 0);

        if (player.Buffs.HasCustomVar(ID))
        {
            float flPlayerValue = player.Buffs.GetCustomVar(ID);

            // If value is not specified, accepted it, unless the cvar isn't supposed to exist.
            if (string.IsNullOrEmpty(Value))
            {
                var passes = invert ? flPlayerValue == 0 : true;
                AdvLogging.DisplayLog(AdvFeatureClass, $"{GetType()} HasCvar: {ID} Operator: {Operator} Value is empty, returning {passes}");
                return passes;
            }

            float.TryParse(Value, out var flValue);
            AdvLogging.DisplayLog(AdvFeatureClass, GetType() + " HasCvar: " + ID + " Value: " + flValue + " Player Value: " + flPlayerValue + " Operator: " + Operator);


            switch (Operator.ToLower())
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
                case "not":
                    {
                        if (flPlayerValue == 0)
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

            AdvLogging.DisplayLog(AdvFeatureClass, GetType() + " Has CVar: " + ID + "  Value: " + flValue + " Player Value: " + flPlayerValue + " Operator: " + Operator + " :: No Result");

        }
        else if (invert)
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


