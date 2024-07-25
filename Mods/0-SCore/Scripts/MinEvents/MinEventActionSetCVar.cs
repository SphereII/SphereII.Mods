//        <triggered_effect trigger = "onSelfBuffUpdate" action="SetCVar, SCore" target="selfAOE" range="4" />

using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public class MinEventActionSetCVar : MinEventActionTargetedBase
{
    string cvar = "Leader";
    public override void Execute(MinEventParams _params)
    {
        for (var j = 0; j < targets.Count; j++)
        {
            // If they already have a cvar of this, do you want to reset it? 
            //if (targets[j].Buffs.HasCustomVar(cvar)) continue;
            targets[j].Buffs.SetCustomVar(cvar, _params.Self.entityId);
        }
        _params.Self.Buffs.SetCustomVar("EntityID", _params.Self.entityId);
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        bool flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            string name = _attribute.Name.LocalName;
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