using Audio;
using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace Harmony.SoundFeatures
{
    [HarmonyPatch(typeof(Client))]
    [HarmonyPatch("Play")]
    [HarmonyPatch(new[] { typeof(int), typeof(string), typeof(float), typeof(float) })]
    public class AudioClientPlay
    {
        private static readonly string AdvFeatureClass = "AdvancedSoundFeatures";

        private static bool Prefix(int playOnEntityId, string soundGroupName)
        {
            AdvLogging.DisplayLog(AdvFeatureClass, "Audio.Client.Play(): int, string, float: " + soundGroupName);
            var myEntity = GameManager.Instance.World.GetEntity(playOnEntityId) as global::EntityAlive;
            if (myEntity == null)
                return true;

            if (string.IsNullOrEmpty(soundGroupName))
                return true;

            AdvLogging.DisplayLog(AdvFeatureClass, "Audio.Client.Play(): Vector3, string: " + soundGroupName.Split('/').Last());
            GiveBuffOrQuestBySound.CheckForBuffOrQuest(soundGroupName.Split('/').Last(), myEntity.position);
            return true;
        }
    }

    [HarmonyPatch(typeof(Client))]
    [HarmonyPatch("Play")]
    [HarmonyPatch(new[] { typeof(Vector3), typeof(string), typeof(float), typeof(int) , typeof(float)})]
    public class AudioClientPlay2
    {
        private static readonly string AdvFeatureClass = "AdvancedSoundFeatures";

        private static bool Prefix(Vector3 position, string soundGroupName)
        {
            AdvLogging.DisplayLog(AdvFeatureClass, "Audio.Client.Play(): Vector3, string: " + soundGroupName);

            if (string.IsNullOrEmpty(soundGroupName))
                return true;

            AdvLogging.DisplayLog(AdvFeatureClass, "Audio.Client.Play(): Vector3, string: " + soundGroupName.Split('/').Last());
            GiveBuffOrQuestBySound.CheckForBuffOrQuest(soundGroupName.Split('/').Last(), position);
            return true;
        }
    }
}