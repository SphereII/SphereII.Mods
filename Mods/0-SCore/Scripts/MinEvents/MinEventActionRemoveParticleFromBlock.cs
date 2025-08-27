//        <triggered_effect trigger = "onSelfBuffUpdate" action="RemoveParticleFromBlock, SCore" />

using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public class MinEventActionRemoveParticleFromBlock : MinEventActionTargetedBase
{
    public override void Execute(MinEventParams _params)
    {
        

        var position = new Vector3i(_params.Position);
        Debug.Log($"Removing Particles To Block Position: {position} {_params.BlockValue.Block.GetBlockName()} ");
        BlockUtilitiesSDX.removeParticles(position);
    }
}