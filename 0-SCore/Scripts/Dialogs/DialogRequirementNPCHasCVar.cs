public class DialogRequirementNPCHasCVarSDX : BaseDialogRequirement
{
    private static readonly string AdvFeatureClass = "AdvancedDialogDebugging";

    public string strOperator = "eq";
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        
        // If the character doesn't have a cvar, add it.
        if (!talkingTo.Buffs.HasCustomVar(ID))
            talkingTo.Buffs.SetCustomVar(ID, 0);

        if (talkingTo.Buffs.HasCustomVar(ID))
        {
            // If value is not specified, accepted it.
            if (string.IsNullOrEmpty(Value))
                return true;

            float.TryParse(Value, out var flValue);
            var npcValue = talkingTo.Buffs.GetCustomVar(ID);
            switch (strOperator.ToLower())
            {
                case "lt":
                    {
                        if (npcValue < flValue)
                            return true;
                        break;
                    }
                case "lte":
                    {
                        if (npcValue <= flValue)
                            return true;
                        break;
                    }
                case "gt":
                    {
                        if (npcValue > flValue)
                            return true;
                        break;
                    }
                case "gte":
                    {
                        if (npcValue >= flValue)
                            return true;
                        break;
                    }
                case "neq":
                    {
                        if (flValue != npcValue)
                            return true;
                        break;
                    }
                default:
                    {
                        if (flValue == npcValue)
                            return true;
                        break;
                    }
            }

        }
        else if (strOperator.ToLower() == "not")
            return true;

        // If the Cvar does not exist, but does have a value to be checked, pass the condition.It just may not be set yet.
        if (string.IsNullOrEmpty(Value))
        {
            return true;
        }
        return false;
    }

}


