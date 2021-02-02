using System.Xml;

// If targeting the player, you can add target="self" or just omit the target property:
// 	<requirement name="RequirementSameFactionSDX, Mods" faction="animalsCows" />
// If targeting another entity (such as one the player has just killed), add target="other":
// 	<requirement name="RequirementSameFactionSDX, Mods" faction="animalsCows" target="other" />

public class RequirementSameFactionSDX : TargetedCompareRequirementBase
{
    public string strFaction = "";

    public override bool ParamsValid(MinEventParams _params)
    {
        if (!base.ParamsValid(_params))
            return false;
        
        Faction targetFaction = FactionManager.Instance.GetFaction(this.target.factionId);
        if (targetFaction.Name == strFaction)
            return true;

        return false;
    }

    public override bool ParseXmlAttribute(XmlAttribute _attribute)
    {
        if (_attribute.Name == "faction")
        {
            strFaction = _attribute.Value.ToString();
            return true;
        }
        return base.ParseXmlAttribute(_attribute);
    }
}
