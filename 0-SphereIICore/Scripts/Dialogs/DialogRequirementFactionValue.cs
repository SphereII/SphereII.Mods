using UnityEngine;
public class DialogRequirementFactionValue : BaseDialogRequirement
{
    private static string AdvFeatureClass = "AdvancedDialogDebugging";
    // Show Dialog if player faction is less than 400
        //  <requirement type="FactionValue, Mods" requirementtype="Hide" value="400" operator="lt" /> 

        // strOperator is set in 0-SphereIICore/Harmony/DialogFromXML.cs
    public string strOperator = "eq";
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        AdvLogging.DisplayLog(AdvFeatureClass, "Player: " + player.EntityName);
        int entityID = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        EntityAlive myEntity = player.world.GetEntity(entityID) as EntityAlive;
        if (myEntity == null)
            return false;


        // If value is not specified, accepted it.
        if (string.IsNullOrEmpty(Value))
            return true;

        float flValue = 0f;
        float.TryParse(Value, out flValue);
        float flPlayerValue = FactionManager.Instance.GetRelationshipValue( myEntity, player );
        AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " FactionValue: " + ID + " Value: " + flValue + " Player Value: " + flPlayerValue + " Operator: " + strOperator);


        if (strOperator.ToLower() == "lt")
        {
            AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " FactionValue: Checking for LT: " + flPlayerValue + " < " + flValue);
            if (flPlayerValue < flValue)
                return true;
            AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " FactionValue: Failed for LT: " + flPlayerValue + " < " + flValue);
        }
        else if (strOperator.ToLower() == "lte")
        {
            AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " FactionValue: Checking for LTE: " + flPlayerValue + " <= " + flValue);
            if (flPlayerValue <= flValue)
                return true;
            AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " FactionValue: Failed for GT: " + flPlayerValue + " > " + flValue);
        }
        else if (strOperator.ToLower() == "gt")
        {
            AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " FactionValue: Checking for GT: " + flPlayerValue + " > " + flValue);
            if (flPlayerValue > flValue)
                return true;
            AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " FactionValue: Failed for GT: " + flPlayerValue + " > " + flValue);
        }
        else if (strOperator.ToLower() == "gte")
        {
            AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " FactionValue: Checking for GTE: " + flPlayerValue + " >= " + flValue);
            if (flPlayerValue >= flValue)
                return true;
            AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " FactionValue: Failed for GTE: " + flPlayerValue + " >= " + flValue);
        }

        else
        {
            AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " FactionValue: Checking for equality: " + flPlayerValue + " == " + flValue);
            if (flValue == flPlayerValue)
                return true;
            AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + " FactionValue: Failed for equality: " + flPlayerValue + " == " + flValue);
        }

        AdvLogging.DisplayLog(AdvFeatureClass, this.GetType() + "FactionValue: " + ID + "  Value: " + flValue + " Player Value: " + flPlayerValue + " Operator: " + strOperator + " :: No Result");
        
    
        AdvLogging.DisplayLog(AdvFeatureClass, "FactionValue:: false");
        return false;
    }

}


