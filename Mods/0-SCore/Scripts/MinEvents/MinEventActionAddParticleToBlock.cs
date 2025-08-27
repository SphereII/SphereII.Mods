//        <triggered_effect trigger = "onSelfBuffUpdate" action="AddParticleToBlock, SCore" />

using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public class MinEventActionAddParticleToBlock : MinEventActionTargetedBase
{
    public string particleName;
    public bool centered;
    public override void Execute(MinEventParams _params)
    {
        var blockValue = _params.BlockValue;
        var position = new Vector3i(_params.Position);
        Debug.Log($"Adding Particles To Block Position: {position} {blockValue.Block.GetBlockName()} : Particle {particleName}");
        if (centered)
        {
            BlockUtilitiesSDX.addParticlesCentered(particleName, position);
            return;
        }
        BlockUtilitiesSDX.addParticles(particleName, position);

    }
    
    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var flag = base.ParseXmlAttribute(_attribute);
        if (flag) return true;
        var name = _attribute.Name.LocalName;

        switch (name)
        {
            case "particle":
                particleName = _attribute.Value;
                return true;
            case "centered":
                centered = StringParsers.ParseBool(_attribute.Value);
                return true;
    
        }

        return false;
    }


}