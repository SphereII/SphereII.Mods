using UnityEngine;
public class DialogRequirementHasCVarSDX : BaseDialogRequirement
{
    private static string AdvFeatureClass = "AdvancedDialogDebugging";

    public string strOperator = "eq";
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        AdvLogging.DisplayLog(AdvFeatureClass, "Player: " + player.EntityName);
        foreach (var temp in player.Buffs.CVars)
        {
            AdvLogging.DisplayLog(AdvFeatureClass, "\tCVar: " + temp.Key + " Value: " + temp.Value);
        }

        // If the character doesn't have a cvar, add it.
        if (!player.Buffs.HasCustomVar(ID))
            player.Buffs.SetCustomVar(ID, 0);

        if (player.Buffs.HasCustomVar(ID))
        {
            // If value is not specified, accepted it.
            if (string.IsNullOrEmpty(Value))
                return true;

            float flValue = 0f;
            float.TryParse(Value, out flValue);
            float flPlayerValue = player.Buffs.GetCustomVar(ID);
            AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " HasCvar: " + ID + " Value: " + flValue + " Player Value: " + flPlayerValue + " Operator: " + strOperator);


            if (strOperator.ToLower() == "lt")
            {
                AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " HasCvar: Checking for LT: " + flPlayerValue + " < " + flValue);
                if (flPlayerValue < flValue)
                    return true;
                AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " HasCvar: Failed for LT: " + flPlayerValue + " < " + flValue);
            }
            else if (strOperator.ToLower() == "lte")
            {
                AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " HasCvar: Checking for LTE: " + flPlayerValue + " <= " + flValue);
                if (flPlayerValue <= flValue)
                    return true;
                AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " HasCvar: Failed for GT: " + flPlayerValue + " > " + flValue);
            }
            else if (strOperator.ToLower() == "gt")
            {
                AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " HasCvar: Checking for GT: " + flPlayerValue + " > " + flValue);
                if (flPlayerValue > flValue)
                    return true;
                AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " HasCvar: Failed for GT: " + flPlayerValue + " > " + flValue);
            }
            else if (strOperator.ToLower() == "gte")
            {
                AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " HasCvar: Checking for GTE: " + flPlayerValue + " >= " + flValue);
                if (flPlayerValue >= flValue)
                    return true;
                AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " HasCvar: Failed for GTE: " + flPlayerValue + " >= " + flValue);
            }
     
            else
            {
                AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " HasCvar: Checking for equality: " + flPlayerValue + " == " + flValue);
                if (flValue == flPlayerValue)
                    return true;
                AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " HasCvar: Failed for equality: " + flPlayerValue + " == " + flValue);
            }

            AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " Has CVar: " + ID + "  Value: " + flValue + " Player Value: " + flPlayerValue + " Operator: " + strOperator + " :: No Result");

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


