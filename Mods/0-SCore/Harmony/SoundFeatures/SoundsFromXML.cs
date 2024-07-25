using HarmonyLib;
using System.Xml;
using System.Xml.Linq;

// This class populates a static variable that will help us link Sound Data with Buff / Quests.
namespace Harmony.SoundFeatures
{
    [HarmonyPatch(typeof(SoundsFromXml))]
    [HarmonyPatch("Parse")]
    public class SoundsFromXML
    {
        private static bool Prefix(XElement node)
        {
            SoundDataNodeClassSDX.AddSoundData(node);
            return true;
        }
    }
}