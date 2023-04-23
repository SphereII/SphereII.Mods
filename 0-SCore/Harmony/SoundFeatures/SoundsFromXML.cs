using HarmonyLib;
using System.Xml;
using UnityEngine;

// This class populates a static variable that will help us link Sound Data with Buff / Quests.
namespace Harmony.SoundFeatures
{
    [HarmonyPatch(typeof(SoundsFromXml))]
    [HarmonyPatch("Parse")]
    public class SoundsFromXMLPrefix
    {
        private static bool Prefix(ref XmlNode node)
        {
            SoundDataNodeClassSDX.AddSoundData(node);
            return true;
        }
    }
}