using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

public static class SoundDataNodeClassSDX
{
    private static readonly string AdvFeatureClass = "AdvancedSoundFeatures";

    // Use this value to hold a reference for each SoundData group that can contain special buffs or quests.
    public static Dictionary<string, SoundDataSDX> SoundDataSDXInfo = new Dictionary<string, SoundDataSDX>();

    public static void AddSoundData(XElement node)
    {
        var newSoundData = new SoundDataSDX();
        //var SoundGroupName = node.Attributes[0].Value;
        var SoundGroupName = node.Attributes().First<XAttribute>().Value;
        foreach (var xElement in node.Elements())
        {
            string localName = xElement.Name.LocalName;
            if (localName.EqualsCaseInsensitive("Buff"))
            {
                var text = xElement.GetAttribute("Buff");
                AdvLogging.DisplayLog(AdvFeatureClass, SoundGroupName + " Adding Trigger Buff: " + text);

                newSoundData.Buff = text;
            }

            if (localName.EqualsCaseInsensitive("Quest"))
            {
                var text = xElement.GetAttribute("Quest");
                AdvLogging.DisplayLog(AdvFeatureClass, SoundGroupName + " Adding Trigger Quest: " + text);
                newSoundData.Quest = text;
            }
        }

        // If it has a buff or a quest, add it to the monitored sound.
        if (newSoundData.Buff != null || newSoundData.Quest != null)
            if (!SoundDataSDXInfo.ContainsKey(SoundGroupName))
                SoundDataSDXInfo.Add(SoundGroupName, newSoundData);
    }

    public struct SoundDataSDX
    {
        public string Buff;
        public string Quest;
    }
}