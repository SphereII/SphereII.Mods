//        <triggered_effect trigger = "onSelfBuffUpdate" action="SetCVar, SCore" target="selfAOE" range="4" />

using System.Xml;
using UnityEngine;

public class MinEventActionSetCVar : MinEventActionTargetedBase
{
    string cvar = "Leader";
    string myentityname;
    public override void Execute(MinEventParams _params)
    {
        GameManager.Instance.World.GetRandomSpawnPositionMinMaxToPosition(_params.Self.position, 1, 4, 1, false, out var transformPos);
        if (transformPos == Vector3.zero)
            transformPos = _params.Self.position + Vector3.back;

        for (var j = 0; j < targets.Count; j++)
        {
            // If they already have a cvar of this, do you want to reset it? 
            //if (targets[j].Buffs.HasCustomVar(cvar)) continue;

            targets[j].Buffs.SetCustomVar(cvar, _params.Self.entityId);
            targets[j].position = transformPos;
            _params.Self.Buffs.SetCustomVar($"myFamiliar", targets[j].entityId);
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
                if (name == "cvar")
                {
                    cvar = _attribute.Value;
                    return true;
                }
           
            }
        }
        return flag;
    }
}