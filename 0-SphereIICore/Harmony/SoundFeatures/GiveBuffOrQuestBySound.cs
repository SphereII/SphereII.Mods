using Audio;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/**
 * SphereII_GiveBuffOrQuestBySound
 *
 * This class includes a Harmony patch to hook the delivery of a buff or quest through the sound system.
 * 
 * Usage XML:
 * 
 *   <!-- Adds the buffCursed to any entity that hears the screamer -->
 *   <append xpath="/Sounds/SoundDataNode[@name='zombiefemalescoutalert']">
 *       <buff value="buffCursed" />
 *       <Quest value="myQuest" />
 *   </append>
 */
public class SphereII_GiveBuffOrQuestBySound
{
    private static string AdvFeatureClass = "AdvancedSoundFeatures";
    public static void CheckForBuffOrQuest(string soundGroupName, Vector3 position)
    {
        AdvLogging.DisplayLog(AdvFeatureClass, "Searching for " + soundGroupName);

        // Dictionary search for substring
        if (SoundDataNodeClassSDX.SoundDataSDXInfo.ContainsKey(soundGroupName))
        {
            AdvLogging.DisplayLog(AdvFeatureClass, "Found Sound Node. Checking for buffs");

            // use xmlData to grab the sound node information, which can contain how far away the sound can be heard.
            XmlData xmlData;
            if (!Manager.audioData.TryGetValue(soundGroupName, out xmlData))
                return;

            int Radius = Utils.Fastfloor(xmlData.distantFadeEnd);
            SoundDataNodeClassSDX.SoundDataSDX data = SoundDataNodeClassSDX.SoundDataSDXInfo[soundGroupName];

            if (data.Buff != null)
            {
                AdvLogging.DisplayLog(AdvFeatureClass, ": Found Buff for Sound Node: " + data.Buff);
                EntityUtilities.AddBuffToRadius(data.Buff, position, Radius);
            }
            AdvLogging.DisplayLog(AdvFeatureClass, "Scanning for quest");
            if (data.Quest != null)
            {
                AdvLogging.DisplayLog(AdvFeatureClass, "Adding Quest " + data.Quest + " to surrounding entities");
                EntityUtilities.AddQuestToRadius(data.Quest, position, Radius);
            }
        }
        return;
    }
}

[HarmonyPatch(typeof(Audio.Manager))]
[HarmonyPatch("Play")]
[HarmonyPatch(new Type[] { typeof(Vector3), typeof(string), typeof(int) })]
public class SphereII_Audio_Manager_Play
{
    private static string AdvFeatureClass = "AdvancedSoundFeatures";

    static bool Prefix(Audio.Manager __instance, Vector3 position, string soundGroupName)
    {
        AdvLogging.DisplayLog(AdvFeatureClass, "AudioManager.Play(): Vector3, String, int: " + soundGroupName);

        if(String.IsNullOrEmpty(soundGroupName))
            return true;

        AdvLogging.DisplayLog(AdvFeatureClass, "Audio.Client.Play(): Vector3, string: " + soundGroupName.Split('/').Last());


        SphereII_GiveBuffOrQuestBySound.CheckForBuffOrQuest(soundGroupName.Split('/').Last(), position);
        return true;
    }
}



[HarmonyPatch(typeof(Audio.Manager))]
[HarmonyPatch("Play")]
[HarmonyPatch(new Type[] { typeof(Entity), typeof(string), typeof(float), typeof(bool) })]
public class SphereII_Audio_Server_Play
{
    private static string AdvFeatureClass = "AdvancedSoundFeatures";


    static bool Prefix(Audio.Manager __instance, Entity entity, string soundGroupName)
    {
        AdvLogging.DisplayLog(AdvFeatureClass, "AudioManager.Play(): Entity, String, float, bool: " + soundGroupName);
        if (entity == null)
            return true;

        if(String.IsNullOrEmpty(soundGroupName))
            return true;

        AdvLogging.DisplayLog(AdvFeatureClass, "Audio.Client.Play(): Vector3, string: " + soundGroupName.Split('/').Last());
        SphereII_GiveBuffOrQuestBySound.CheckForBuffOrQuest(soundGroupName.Split('/').Last(), entity.position);
        return true;
    }
}

[HarmonyPatch(typeof(Audio.Client))]
[HarmonyPatch("Play")]
[HarmonyPatch(new Type[] { typeof(int), typeof(string), typeof(float) })]
public class SphereII_Audio_Client_Play_1
{
    private static string AdvFeatureClass = "AdvancedSoundFeatures";

    static bool Prefix(int playOnEntityId, string soundGoupName, float _occlusion)
    {
        AdvLogging.DisplayLog(AdvFeatureClass, "Audio.Client.Play(): int, string, float: " + soundGoupName);
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(playOnEntityId) as EntityAlive;
        if (myEntity == null)
            return true;

        if(String.IsNullOrEmpty(soundGoupName))
            return true;

        AdvLogging.DisplayLog(AdvFeatureClass, "Audio.Client.Play(): Vector3, string: " + soundGoupName.Split('/').Last());
        SphereII_GiveBuffOrQuestBySound.CheckForBuffOrQuest(soundGoupName.Split('/').Last(), myEntity.position);
        return true;
    }
}


[HarmonyPatch(typeof(Audio.Client))]
[HarmonyPatch("Play")]
[HarmonyPatch(new Type[] { typeof(Vector3), typeof(string), typeof(float), typeof(int) })]
public class SphereII_Audio_Client_Play_2
{
    private static string AdvFeatureClass = "AdvancedSoundFeatures";

    static bool Prefix(Vector3 position, string soundGoupName)
    {
        AdvLogging.DisplayLog(AdvFeatureClass, "Audio.Client.Play(): Vector3, string: " + soundGoupName);

        if(String.IsNullOrEmpty(soundGoupName))
            return true;

        AdvLogging.DisplayLog(AdvFeatureClass, "Audio.Client.Play(): Vector3, string: " + soundGoupName.Split('/').Last());
        SphereII_GiveBuffOrQuestBySound.CheckForBuffOrQuest(soundGoupName.Split('/').Last(), position);
        return true;
    }
}
