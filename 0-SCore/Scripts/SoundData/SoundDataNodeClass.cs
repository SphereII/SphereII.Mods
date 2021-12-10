using System.Collections.Generic;
using System.Xml;

public static class SoundDataNodeClassSDX
{
    private static readonly string AdvFeatureClass = "AdvancedSoundFeatures";

    // Use this value to hold a reference for each SoundData group that can contain special buffs or quests.
    public static Dictionary<string, SoundDataSDX> SoundDataSDXInfo = new Dictionary<string, SoundDataSDX>();

    public static void AddSoundData(XmlNode node)
    {
        var newSoundData = new SoundDataSDX();
        var SoundGroupName = node.Attributes[0].Value;
        foreach (var obj in node.ChildNodes)
        {
            var xmlNode = (XmlNode)obj;
            if (xmlNode.Name.EqualsCaseInsensitive("Buff"))
            {
                var text = xmlNode.Attributes[0].Value;
                AdvLogging.DisplayLog(AdvFeatureClass, SoundGroupName + " Adding Trigger Buff: " + text);

                newSoundData.Buff = text;
            }

            if (xmlNode.Name.EqualsCaseInsensitive("Quest"))
            {
                var text = xmlNode.Attributes[0].Value;
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