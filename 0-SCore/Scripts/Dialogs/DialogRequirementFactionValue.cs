public class DialogRequirementFactionValue : BaseDialogRequirement
{
    private static readonly string AdvFeatureClass = "AdvancedDialogDebugging";
    // Show Dialog if player faction is less than 400
    //  <requirement type="FactionValue, SCore" requirementtype="Hide" value="400" operator="lt" /> 

    // strOperator is set in 0-SphereIICore/Harmony/DialogFromXML.cs
    public string strOperator = "eq";
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        AdvLogging.DisplayLog(AdvFeatureClass, $"Player: {player.EntityName}");
        var entityId = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityId = (int)player.Buffs.GetCustomVar("CurrentNPC");

        var myEntity = player.world.GetEntity(entityId) as EntityAlive;
        if (myEntity == null)
            return false;


        // If value is not specified, accepted it.
        if (string.IsNullOrEmpty(Value))
            return true;

        float.TryParse(Value, out var flValue);
        var flPlayerValue = FactionManager.Instance.GetRelationshipValue(myEntity, player);
        AdvLogging.DisplayLog(AdvFeatureClass, $"{GetType()} FactionValue: {ID} Value: {flValue} Player Value: {flPlayerValue} Operator: {strOperator}");

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
                    if (flPlayerValue != flValue)
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

        AdvLogging.DisplayLog(AdvFeatureClass, GetType() + "FactionValue: " + ID + "  Value: " + flValue + " Player Value: " + flPlayerValue + " Operator: " + strOperator + " :: No Result");
        AdvLogging.DisplayLog(AdvFeatureClass, "FactionValue:: false");
        return false;
    }

}


