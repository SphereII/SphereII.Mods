using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class BlockEffectsManager
{
    public static Dictionary<string, MinEffectController> BlockEffects = new Dictionary<string, MinEffectController>();
    public static void ReadBlock(string blockName, XElement node)
    {
        var effects = MinEffectController.ParseXml(node, null, MinEffectController.SourceParentType.ItemClass);
        if (effects == null) return;
        Log.Out($"BuffEffectsManager: {blockName} Adding {effects.EffectGroups.Count} Effect Groups.");
        BlockEffects[blockName] = effects;
    }

    public static MinEffectController GetBlockEffect(string blockName)
    {
        return BlockEffects.GetValueOrDefault(blockName);
    }
}


