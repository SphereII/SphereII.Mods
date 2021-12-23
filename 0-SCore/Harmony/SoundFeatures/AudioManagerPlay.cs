using Audio;
using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace Harmony.SoundFeatures
{
    [HarmonyPatch(typeof(Manager))]
    [HarmonyPatch("Play")]
    [HarmonyPatch(new[] { typeof(Vector3), typeof(string), typeof(int) })]
    public class AudioManagerPlay
    {
        private static readonly string AdvFeatureClass = "AdvancedSoundFeatures";

        private static bool Prefix(Manager __instance, Vector3 position, string soundGroupName)
        {
            AdvLogging.DisplayLog(AdvFeatureClass, "AudioManager.Play(): Vector3, String, int: " + soundGroupName);

            if (string.IsNullOrEmpty(soundGroupName))
                return true;

            AdvLogging.DisplayLog(AdvFeatureClass, "Audio.Client.Play(): Vector3, string: " + soundGroupName.Split('/').Last());


            GiveBuffOrQuestBySound.CheckForBuffOrQuest(soundGroupName.Split('/').Last(), position);
            return true;
        }
    }
}