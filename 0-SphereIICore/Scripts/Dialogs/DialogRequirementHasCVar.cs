using UnityEngine;
public class DialogRequirementHasCVarSDX : BaseDialogRequirement
{
    public string strOperator = "EQ";
    public override bool CheckRequirement(EntityPlayer player)
    {
        if (player.Buffs.HasCustomVar(ID))
        {
            // If value is not specified, accepted it.
            if (string.IsNullOrEmpty(Value) )
                return true;

            float flValue = 0f;
            float.TryParse(Value, out flValue);
            float flPlayerValue = player.Buffs.CVars[ID];

            if ( strOperator.ToLower() == "eq")
                if ( flValue == flPlayerValue)
                  return true;
            if (strOperator.ToLower() == "lt")
                if (flPlayerValue < flValue)
                    return true;
            if (strOperator.ToLower() == "gt")
                if (flPlayerValue > flValue)
                    return true;
            if (strOperator.ToLower() == "gte")
                if (flPlayerValue >= flValue)
                    return true;
            if (strOperator.ToLower() == "lte")
                if (flPlayerValue <= flValue)
                    return true;

        }
        return false;
    }



}


