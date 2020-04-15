using System.Xml;
using UnityEngine;
public class MinEventActionModifyFactionSDX : MinEventActionRemoveBuff
{
    string Faction = "";
    float value = 0f;
    // changes the self's faction.
    //  <triggered_effect trigger="onSelfBuffStart" action="ModifyFactionSDX, Mods" target="self" faction="bandits" value="10" /> //  Give 10 points to the relationship with bandits
    //  <triggered_effect trigger="onSelfPrimaryActionEnd" action="ModifyFactionSDX, Mods" target="self" faction="undead" value="-10" /> //Take away 10 points to the relationship with undead

    public override void Execute(MinEventParams _params)
    {
        for (int i = 0; i < this.targets.Count; i++)
        {
            EntityAlive entity = this.targets[i] as EntityAlive;
            if (entity != null)
            {
                // Search for the faction
                Faction newFaction = FactionManager.Instance.GetFactionByName(Faction);
                if (newFaction != null)
                {
                    newFaction.ModifyRelationship(entity.factionId, value);
                }
            }

        }
    }

    public override bool ParseXmlAttribute(XmlAttribute _attribute)
    {
        bool flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            string name = _attribute.Name;
            if (name != null)
            {
                if (name == "faction")
                {
                    Faction = _attribute.Value;
                    return true;
                }
                if (name == "value")
                {
                    value = StringParsers.ParseFloat( _attribute.Value );
                    return true;
                }

            }
        }
        return flag;
    }
}
