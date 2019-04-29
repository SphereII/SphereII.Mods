using System;
using System.Xml;
using UnityEngine;

// 	<requirement name="RequirementSameFactionSDX, Mods" faction="animalsCows" />

public class RequirementSameFactionSDX : RequirementBase
{
    public string strFaction = "";

    public override bool ParamsValid(MinEventParams _params)
    {
        Faction myFaction = FactionManager.Instance.GetFaction(_params.Self.factionId);
        if (myFaction.Name == strFaction)
            return true;

        return false;
    }

    public override bool ParseXmlAttribute(XmlAttribute _attribute)
    {
        string name = _attribute.Name;
        if (name != null)
        {
            if (name == "faction")
            {
                strFaction = _attribute.Value.ToString();
                return true;
            }
        }
        return base.ParseXmlAttribute(_attribute);
    }
}
