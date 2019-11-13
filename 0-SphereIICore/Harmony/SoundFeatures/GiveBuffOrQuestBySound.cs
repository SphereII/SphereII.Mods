using Audio;
using Harmony;
using System;
using UnityEngine;

// This class populates a static variable that will help us link Sound Data with Buff / Quests.
public class SphereII_GiveBuffOrQuestBySound
{
    private static string AdvFeatureClass = "AdvancedSoundFeatures";
    public static void CheckForBuffOrQuest(Audio.Manager __instance, string soundGroupName, Vector3 position)
    {
        AdvLogging.DisplayLog(AdvFeatureClass, "Searching for " + soundGroupName);

        if(SoundDataNodeClassSDX.SoundDataSDXInfo.ContainsKey(soundGroupName))
        {
            AdvLogging.DisplayLog(AdvFeatureClass, "Found Sound Node. Checking for buffs");

            // use xmlData to grab the sound node information, which can contain how far away the sound can be heard.
            XmlData xmlData;
            if(!Manager.audioData.TryGetValue(soundGroupName, out xmlData))
                return;

            int Radius = Utils.Fastfloor(xmlData.distantFadeEnd);
            SoundDataNodeClassSDX.SoundDataSDX data = SoundDataNodeClassSDX.SoundDataSDXInfo[soundGroupName];

            if(data.Buff != null)
            {
                AdvLogging.DisplayLog(AdvFeatureClass, ": Found Buff for Sound Node: " + data.Buff.Name);
                EntityUtilities.AddBuffToRadius(data.Buff.Name, position, Radius);
            }
            AdvLogging.DisplayLog(AdvFeatureClass, "Scanning for quest");
            if(data.Quest != null)
            {
                AdvLogging.DisplayLog(AdvFeatureClass, "Adding Quest " + data.Quest.ID + " to surrounding entities");
                EntityUtilities.AddQuestToRadius(data.Quest.ID, position, Radius);
            }
        }
        return ;
    }
}

// This class populates a static variable that will help us link Sound Data with Buff / Quests.
[HarmonyPatch(typeof(Audio.Manager))]
[HarmonyPatch("Play")]
[HarmonyPatch(new Type[] { typeof(Vector3), typeof(string), typeof(int) })]
public class SphereII_Audio_Manager_Play
{
    static bool Prefix(Audio.Manager __instance, Vector3 position, string soundGroupName)
    {
        XmlData xmlData;
        if(!Manager.audioData.TryGetValue(soundGroupName, out xmlData))
            return true;

        if(xmlData.Update())
            SphereII_GiveBuffOrQuestBySound.CheckForBuffOrQuest(__instance, soundGroupName,position);
        return true;
    }
}



// This class populates a static variable that will help us link Sound Data with Buff / Quests.
[HarmonyPatch(typeof(Audio.Manager))]
[HarmonyPatch("Play")]
[HarmonyPatch(new Type[] { typeof(Entity), typeof(string), typeof(float), typeof(bool) })]
public class SphereII_Audio_Server_Play
{
    static bool Prefix(Audio.Manager __instance, Entity entity, string soundGroupName)
    {
        if(entity == null)
            return true;

        XmlData xmlData;
        if(!Manager.audioData.TryGetValue(soundGroupName, out xmlData))
            return true;

        if(xmlData.Update())
            SphereII_GiveBuffOrQuestBySound.CheckForBuffOrQuest(__instance, soundGroupName, entity.position);
        return true;
    }
}

//// This class populates a static variable that will help us link Sound Data with Buff / Quests.
//[HarmonyPatch(typeof(Audio.Manager))]
//[HarmonyPatch("Play")]
//[HarmonyPatch(new Type[] { typeof(Vector3), typeof(string), typeof(int) })]
//public class SphereII_GiveBuffOrQuestBySound_Play2
//{
//    private static string AdvFeatureClass = "AdvancedSoundFeatures";

//    static bool Prefix(Audio.Manager __instance, Entity ___localPlayer, Vector3 position, string soundGroupName, int entityId = -1)
//    {
//        // Sound system may not be initialized yet
//        if(___localPlayer == null)
//            return true;
//        SphereII_GiveBuffOrQuestBySound.CheckForBuffOrQuest(__instance, soundGroupName, ___localPlayer as EntityPlayerLocal);

//        return true;
//    }
//}
