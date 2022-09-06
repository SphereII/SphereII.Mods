using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
public class MinEventActionCheckFireProximity : MinEventActionRemoveBuff
{
    string cvar = "_closeFires";
    //  		<triggered_effect trigger="onSelfBuffUpdate" action="CheckFireProximity, SCore" range="5" cvar="_closeFires" />

    public override void Execute(MinEventParams _params)
    {
        if (FireManager.Instance == null) return;
        if (FireManager.Instance.Enabled == false) return;

        var position = new Vector3i(_params.Self.position);
        var count = FireManager.Instance.CloseFires(position, (int)maxRange);
        _params.Self.Buffs.SetCustomVar(cvar, (float)count);
    }

    public override bool ParseXmlAttribute(XmlAttribute _attribute)
    {
        var flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            var name = _attribute.Name;
            if (name != null)
                if (name == "cvar")
                {
                    cvar = _attribute.Value;
                    return true;
                }
        }

        return flag;
    }
}