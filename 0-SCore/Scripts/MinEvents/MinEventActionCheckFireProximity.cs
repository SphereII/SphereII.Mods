using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
public class MinEventActionCheckFireProximity : MinEventActionRemoveBuff
{

    //  		<triggered_effect trigger="onSelfBuffUpdate" action="CheckFireProximity, SCore" range="5"  />

    public override void Execute(MinEventParams _params)
    {
        if (FireManager.Instance.Enabled == false) return;

        var position = new Vector3i(_params.Self.position);
        var count = FireManager.Instance.CloseFires(position, (int)maxRange);
        _params.Self.Buffs.SetCustomVar("_closeFires", (float)count);
    }
}