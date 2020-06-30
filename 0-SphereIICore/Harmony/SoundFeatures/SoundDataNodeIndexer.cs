using HarmonyLib;
using System.Xml;

// This class populates a static variable that will help us link Sound Data with Buff / Quests.
[HarmonyPatch(typeof(SoundsFromXml))]
[HarmonyPatch("Parse")]
public class SphereII_SoundsFromXML
{
    static bool Prefix(XmlNode node)
    {
        SoundDataNodeClassSDX.AddSoundData(node);
        return true;
    }
}



