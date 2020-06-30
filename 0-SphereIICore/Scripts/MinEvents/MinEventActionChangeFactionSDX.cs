using System.Xml;
using UnityEngine;
public class MinEventActionChangeFactionSDX : MinEventActionRemoveBuff
{
    string Faction = "";
    // changes the self's faction.
    //  <triggered_effect trigger="onSelfBuffStart" action="ChangeFactionSDX, Mods" target="self" value="bandits" /> //  change faction to bandits
    //  <triggered_effect trigger="onSelfBuffStart" action="ChangeFactionSDX, Mods" target="self" value="undead" /> //change faction to undead
    //  <triggered_effect trigger="onSelfBuffStart" action="ChangeFactionSDX, Mods" target="self" value="original" /> //change faction to the original value

    public override void Execute(MinEventParams _params)
    {
        for (int i = 0; i < this.targets.Count; i++)
        {
            EntityAlive entity = this.targets[i] as EntityAlive;
            if (entity != null)
            {
                // If the faction name is original, try to find the original faction of the entity, stored via cvar.
                if (Faction == "original" )
                {
                    // If there's already a factionoriginal cvar, retrive the faction name to be re-assigned.
                    if (entity.Buffs.HasCustomVar("FactionOriginal"))
                    {
                        var FactionID = (byte)entity.Buffs.GetCustomVar("FactionOriginal");
                        Faction Temp = FactionManager.Instance.GetFaction(FactionID);
                        if (Temp != null)
                            Faction = Temp.Name;

                    }
                    else
                    {
                        if (FactionManager.Instance.GetFactionByName( entity.EntityName).ID == 0)
                        {
                            entity.factionId = FactionManager.Instance.CreateFaction(entity.EntityName, true, "").ID;
                            entity.factionRank = byte.MaxValue;
                        }
                    }

                }

                // Search for the faction
                Faction newFaction = FactionManager.Instance.GetFactionByName(Faction);
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

    public override bool ParseXmlAttribute(XmlAttribute _attribute)
    {
        bool flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            string name = _attribute.Name;
            if (name != null)
            {
                if (name == "value" )
                {
                    Faction = _attribute.Value;
                    return true;
                }
            }
        }
        return flag;
    }
}
