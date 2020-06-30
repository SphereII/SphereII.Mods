using UnityEngine;
public class DialogRequirementHasPlayerLevelSDX : BaseDialogRequirement
{
    private static string AdvFeatureClass = "AdvancedDialogDebugging";

    public string strOperator = "eq";
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        AdvLogging.DisplayLog(AdvFeatureClass, "Player: " + player.EntityName);
        foreach (var temp in player.Buffs.CVars)
        {
            AdvLogging.DisplayLog(AdvFeatureClass, "\tLevel: " + temp.Key + " Value: " + temp.Value);
        }

        if (player.Progression.Level <= Progression.MaxLevel)
        {
            // If value is not specified, accepted it.
            if (string.IsNullOrEmpty(Value))
                return true;

            float flValue = 0f;
            float.TryParse(Value, out flValue);
            float flPlayerValue = player.Buffs.GetCustomVar(ID);
            AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " HasPlayerLevel: " + " Value: " + flValue + " Player Value: " + flPlayerValue + " Operator: " + strOperator);


            if (strOperator.ToLower() == "lt")
            {
                AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " HasPlayerLevel: Checking for LT: " + flPlayerValue + " < " + flValue);
                if (flPlayerValue < flValue)
                    return true;
                AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " HasPlayerLevel: Failed for LT: " + flPlayerValue + " < " + flValue);
            }
            else if (strOperator.ToLower() == "lte")
            {
                AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " HasPlayerLevel: Checking for LTE: " + flPlayerValue + " <= " + flValue);
                if (flPlayerValue <= flValue)
                    return true;
                AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " HasPlayerLevel: Failed for GT: " + flPlayerValue + " > " + flValue);
            }
            else if (strOperator.ToLower() == "gt")
            {
                AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " HasPlayerLevel: Checking for GT: " + flPlayerValue + " > " + flValue);
                if (flPlayerValue > flValue)
                    return true;
                AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " HasPlayerLevel: Failed for GT: " + flPlayerValue + " > " + flValue);
            }
            else if (strOperator.ToLower() == "gte")
            {
                AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " HasPlayerLevel: Checking for GTE: " + flPlayerValue + " >= " + flValue);
                if (flPlayerValue >= flValue)
                    return true;
                AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " HasPlayerLevel: Failed for GTE: " + flPlayerValue + " >= " + flValue);
            }
     
            else
            {
                AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " HasPlayerLevel: Checking for equality: " + flPlayerValue + " == " + flValue);
                if (flValue == flPlayerValue)
                    return true;
                AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " HasPlayerLevel: Failed for equality: " + flPlayerValue + " == " + flValue);
            }

            AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " Is Level: " + "  Value: " + flValue + " Player Value: " + flPlayerValue + " Operator: " + strOperator + " :: No Result");

        }
        else if (strOperator.ToLower() == "not")
            return true;

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


