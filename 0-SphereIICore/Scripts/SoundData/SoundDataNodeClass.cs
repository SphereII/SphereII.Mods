using System;
using System.Collections;
using System.Collections.Generic;   
using System.Globalization;
using System.Xml;

public static class SoundDataNodeClassSDX
{
    private static string AdvFeatureClass = "AdvancedSoundFeatures";

    public struct SoundDataSDX
    {
        public BuffClass Buff;
        public Quest Quest;
    }

    public static Dictionary<string, SoundDataSDX> SoundDataSDXInfo = new Dictionary<string, SoundDataSDX>();

    public static void AddSoundData(XmlNode node)
    {
        SoundDataSDX newSoundData = new SoundDataSDX();
        string SoundGroupName = node.Attributes[0].Value;
        foreach (object obj in node.ChildNodes)
        {
            XmlNode xmlNode = (XmlNode)obj;
            if (xmlNode.Name.EqualsCaseInsensitive("Buff"))
            {
                string text = xmlNode.Attributes[0].Value;
                AdvLogging.DisplayLog(AdvFeatureClass, SoundGroupName + " Adding Buff: " + text);

                newSoundData.Buff = BuffManager.GetBuff(text);
            }
            if (xmlNode.Name.EqualsCaseInsensitive("Quest"))
            {
                string text = xmlNode.Attributes[0].Value;
                AdvLogging.DisplayLog(AdvFeatureClass, SoundGroupName + " Adding Quest: " + text);
                newSoundData.Quest = QuestClass.CreateQuest(text);

            }

        }
        if (!SoundDataSDXInfo.ContainsKey(SoundGroupName))
            SoundDataSDXInfo.Add(SoundGroupName, newSoundData);


    }
}

