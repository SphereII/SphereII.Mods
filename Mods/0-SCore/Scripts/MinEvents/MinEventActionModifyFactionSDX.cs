using System.Xml;
using System.Xml.Linq;

public class MinEventActionModifyFactionSDX : MinEventActionRemoveBuff
{
    private string Faction = "";

    private float value;
    // changes the self's faction.
    //  <triggered_effect trigger="onSelfBuffStart" action="ModifyFactionSDX, SCore" target="self" faction="bandits" value="10" /> //  Give 10 points to the relationship with bandits
    //  <triggered_effect trigger="onSelfPrimaryActionEnd" action="ModifyFactionSDX, SCore" target="self" faction="undead" value="-10" /> //Take away 10 points to the relationship with undead

    public override void Execute(MinEventParams _params)
    {
        for (var i = 0; i < targets.Count; i++)
        {
            var entity = targets[i];
            if (entity != null)
            {
                // Search for the faction
                var newFaction = FactionManager.Instance.GetFactionByName(Faction);
                if (newFaction != null) newFaction.ModifyRelationship(entity.factionId, value);
            }
        }
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            var name = _attribute.Name.LocalName;
            if (name != null)
            {
                if (name == "faction")
                {
                    Faction = _attribute.Value;
                    return true;
                }

                if (name == "value")
                {
                    value = StringParsers.ParseFloat(_attribute.Value);
                    return true;
                }
            }
        }

        return flag;
    }
}