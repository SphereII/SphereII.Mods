using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public class MinEventActionChangeFactionSDX : MinEventActionRemoveBuff
{
    private string Faction = "";
    // changes the self's faction.
    //  <triggered_effect trigger="onSelfBuffStart" action="ChangeFactionSDX, SCore" target="self" value="bandits" /> //  change faction to bandits
    //  <triggered_effect trigger="onSelfBuffStart" action="ChangeFactionSDX, SCore" target="self" value="undead" /> //change faction to undead
    //  <triggered_effect trigger="onSelfBuffStart" action="ChangeFactionSDX, SCore" target="self" value="original" /> //change faction to the original value

    public override void Execute(MinEventParams _params)
    {
        for (var i = 0; i < targets.Count; i++)
        {
            var entity = targets[i];
            if (entity != null)
            {
                // If the faction name is original, try to find the original faction of the entity, stored via cvar.
                if (Faction == "original")
                {
                    // If there's already a factionoriginal cvar, retrive the faction name to be re-assigned.
                    if (entity.Buffs.HasCustomVar("FactionOriginal"))
                    {
                        var FactionID = (byte)entity.Buffs.GetCustomVar("FactionOriginal");
                        var Temp = FactionManager.Instance.GetFaction(FactionID);
                        if (Temp != null)
                            Faction = Temp.Name;
                    }
                    else
                    {
                        if (FactionManager.Instance.GetFactionByName(entity.EntityName).ID == 0)
                        {
                            entity.factionId = FactionManager.Instance.CreateFaction(entity.EntityName).ID;
                            entity.factionRank = byte.MaxValue;
                        }
                    }
                }

                // Search for the faction
                var newFaction = FactionManager.Instance.GetFactionByName(Faction);
                if (newFaction != null)
                {
                    // If there's no original cvar, store it here. That's because it doesn't seem to persist over restarts, but cvars will
                    if (!entity.Buffs.HasCustomVar("FactionOriginal"))
                        entity.Buffs.SetCustomVar("FactionOriginal", entity.factionId);

                    // Set the new faction ID as a cvar for saving.
                    entity.Buffs.SetCustomVar("FactionNew", newFaction.ID);

                    Debug.Log("Changing " + entity.EntityName + " faction from " + entity.factionId + "  to " + newFaction.ID);

                    entity.factionId = newFaction.ID;
                    Debug.Log("\nNew Faction: " + entity.factionId);
                }
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
                if (name == "value")
                {
                    Faction = _attribute.Value;
                    return true;
                }
        }

        return flag;
    }
}