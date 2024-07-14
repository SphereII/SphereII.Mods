using System.Xml;
using System.Xml.Linq;
using UnityEngine;

//        <triggered_effect trigger = "onSelfBuffUpdate" action="AddBuffByFactionSDX, SCore" target="selfAOE" range="4" buff="buffAnimalFertility"  />
//        <triggered_effect trigger = "onSelfBuffUpdate" action="AddBuffByFactionSDX, SCore" target="selfAOE" range="4" mustmatch="true" buff="buffAnimalFertility"  />
public class MinEventActionAddBuffByFactionSDX : MinEventActionBuffModifierBase
{
    private bool MustMatch;

    public override void Execute(MinEventParams _params)
    {
        for (var i = 0; i < buffNames.Length; i++)
            if (BuffManager.GetBuff(buffNames[i]) != null)
                for (var j = 0; j < targets.Count; j++)
                {
                    Debug.Log(" Target: " + targets[j].EntityName + " Faction: " + targets[j].factionId);
                    Debug.Log(" Self: " + _params.Self.EntityName + " Faction: " + _params.Self.factionId);

                    // Check to make sure that the faction is the same
                    if (MustMatch)
                    {
                        if (targets[j].factionId == _params.Self.factionId)
                            targets[j].Buffs.AddBuff(buffNames[i], _params.Self.entityId, !_params.Self.isEntityRemote);
                    }
                    else
                    {
                        if (targets[j].factionId != _params.Self.factionId)
                            targets[j].Buffs.AddBuff(buffNames[i], _params.Self.entityId, !_params.Self.isEntityRemote);
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
                if (name == "mustmatch")
                {
                    if (_attribute.Value == "true")
                        MustMatch = true;
                    else
                        MustMatch = false;

                    return true;
                }
        }

        return flag;
    }
}