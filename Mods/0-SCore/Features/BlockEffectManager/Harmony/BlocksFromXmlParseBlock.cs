using System.Xml.Linq;
using HarmonyLib;

[HarmonyPatch(typeof(BlocksFromXml))]
[HarmonyPatch(nameof(BlocksFromXml.ParseBlock))]
public class BlocksFromXmlParseBlock
{
    public static bool Prefix(XElement elementBlock)
    {
        var attribute = elementBlock.GetAttribute(XNames.name);
        BlockEffectsManager.ReadBlock(attribute, elementBlock);
        return true;
    }
    
}
