using System.Collections.Generic;
using System.Xml;
using Audio;
using UnityEngine;

public static class SoundDataNodeClassSDX
{
    private static readonly string AdvFeatureClass = "AdvancedSoundFeatures";

    // Use this value to hold a reference for each SoundData group that can contain special buffs or quests.
    public static Dictionary<string, SoundDataSdx> SoundDataSdxInfo = new Dictionary<string, SoundDataSdx>();

    private static string GetFirstAttribute(XmlNode node)
    {
        if (node?.Attributes == null ) return string.Empty;
        return node.Attributes.Count == 0 ? string.Empty :  node.Attributes[0].Value;
    }
    
    public static void AddSoundData(XmlNode node)
    {
        var newSoundData = new SoundDataSdx();
        var targetText = GetFirstAttribute(node);
        foreach (var obj in node.ChildNodes)
        {
            var xmlNode = (XmlNode)obj;
            if (xmlNode.Name.EqualsCaseInsensitive("Buff"))
            {
                var buffName = GetFirstAttribute(xmlNode);
                if (string.IsNullOrEmpty(buffName)) continue;

                AdvLogging.DisplayLog(AdvFeatureClass, targetText + " Adding Trigger Buff: " + buffName);
                newSoundData.Buff = buffName;
            }

            if (!xmlNode.Name.EqualsCaseInsensitive("Quest")) continue;
            var questName = GetFirstAttribute(xmlNode);
            if (string.IsNullOrEmpty(questName)) continue;
            
            AdvLogging.DisplayLog(AdvFeatureClass, targetText + " Adding Trigger Quest: " +  targetText);
            newSoundData.Quest = questName;
        }

        // If it has a buff or a quest, add it to the monitored sound.
        if (newSoundData.Buff == null && newSoundData.Quest == null) return;
        if (!SoundDataSdxInfo.ContainsKey(targetText))
        {
            AdvLogging.DisplayLog(AdvFeatureClass, targetText + " Adding to the SoundDataSDXInfo.");
            SoundDataSdxInfo.Add(targetText, newSoundData);
        }
        else
        {
            AdvLogging.DisplayLog(AdvFeatureClass, targetText + " SoundDataSDXInfo Already has this added.");
        }

        if (!Manager.audioData.TryGetValue(targetText, out var xmlData))
        {
            AdvLogging.DisplayLog(AdvFeatureClass, targetText + " Not found in Manager.audioData! Adding...");
        }
        else
        {
            AdvLogging.DisplayLog(AdvFeatureClass, targetText + " Found in AudioManager.");
        }
    }

    public struct SoundDataSdx
    {
        public string Buff;
        public string Quest;
    }
}