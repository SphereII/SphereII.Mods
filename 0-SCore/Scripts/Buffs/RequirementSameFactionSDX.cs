using System.Xml;
using System.Xml.Linq;

// If targeting the player, you can add target="self" or just omit the target property:
// 	<requirement name="RequirementSameFactionSDX, SCore" faction="animalsCows" />
// If targeting another entity (such as one the player has just killed), add target="other":
// 	<requirement name="RequirementSameFactionSDX, SCore" faction="animalsCows" target="other" />

public class RequirementSameFactionSDX : TargetedCompareRequirementBase
{
    public string strFaction = "";

    public override bool ParamsValid(MinEventParams _params)
    {
        if (!base.ParamsValid(_params))
            return false;

        var targetFaction = FactionManager.Instance.GetFaction(target.factionId);
        if (targetFaction.Name == strFaction)
            return true;

        return false;
    }

    public override bool ParseXAttribute(XAttribute _attribute)
    {
        if (_attribute.Name.LocalName == "faction")
        {
            strFaction = _attribute.Value;
            return true;
        }

        return base.ParseXAttribute(_attribute);
    }
}