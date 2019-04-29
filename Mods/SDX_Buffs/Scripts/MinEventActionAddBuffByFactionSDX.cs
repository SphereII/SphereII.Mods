using System;
using System.Xml;
using UnityEngine;

//        <triggered_effect trigger = "onSelfBuffUpdate" action="AddBuffByFactionSDX, Mods" target="selfAOE" range="4" buff="buffAnimalFertility"  />
//        <triggered_effect trigger = "onSelfBuffUpdate" action="AddBuffByFactionSDX, Mods" target="selfAOE" range="4" mustmatch="true" buff="buffAnimalFertility"  />
public class MinEventActionAddBuffByFactionSDX : MinEventActionBuffModifierBase
{

    bool MustMatch = false;

    public override void Execute(MinEventParams _params)
    {
        for (int i = 0; i < this.buffNames.Length; i++)
        {
            if (BuffManager.GetBuff(this.buffNames[i]) != null)
            {
                for (int j = 0; j < this.targets.Count; j++)
                {
                    Debug.Log(" Target: " + targets[j].EntityName + " Faction: " + targets[j].factionId);
                    Debug.Log(" Self: " + _params.Self.EntityName + " Faction: " + _params.Self.factionId);

                    // Check to make sure that the faction is the same
                    if (MustMatch)
                    {
                        if (this.targets[j].factionId == _params.Self.factionId)
                            this.targets[j].Buffs.AddBuff(this.buffNames[i], _params.Self.entityId, !_params.Self.isEntityRemote);
                    }
                    else
                    {
                        if (this.targets[j].factionId != _params.Self.factionId)
                            this.targets[j].Buffs.AddBuff(this.buffNames[i], _params.Self.entityId, !_params.Self.isEntityRemote);
                    }
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
                if (name == "mustmatch")
                {
                    if (_attribute.Value == "true")
                        MustMatch = true;
                    else
                        MustMatch = false;

                    return true;
                }
            }
        }
        return flag;
    }
}


