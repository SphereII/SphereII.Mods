//        <triggered_effect trigger = "onSelfBuffUpdate" action="DamageBlock, SCore" />

using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public class MinEventActionDamageBlock : MinEventActionTargetedBase
{
    public override void Execute(MinEventParams _params)
    {
        var blockValue = _params.BlockValue;
        var position = new Vector3i(_params.Position);
        var ticks = _params.StartPosition.x;
        var perUse = _params.StartPosition.y;
        blockValue.Block.DamageBlock(GameManager.Instance.World, 0, position, blockValue, (int)perUse * (int)ticks, -1);
    }
}