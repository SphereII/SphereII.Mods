using Audio;
using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace Harmony.SoundFeatures
{
    [HarmonyPatch(typeof(Client))]
    [HarmonyPatch("Play")]
    [HarmonyPatch(new[] { typeof(int), typeof(string), typeof(float) })]
    public class AudioClientPlay
    {
        private static readonly string AdvFeatureClass = "AdvancedSoundFeatures";

        private static bool Prefix(int playOnEntityId, string soundGoupName, float _occlusion)
        {
            AdvLogging.DisplayLog(AdvFeatureClass, "Audio.Client.Play(): int, string, float: " + soundGoupName);
            var myEntity = GameManager.Instance.World.GetEntity(playOnEntityId) as global::EntityAlive;
            if (myEntity == null)
                return true;

            if (string.IsNullOrEmpty(soundGoupName))
                return true;

            AdvLogging.DisplayLog(AdvFeatureClass, "Audio.Client.Play(): Vector3, string: " + soundGoupName.Split('/').Last());
            GiveBuffOrQuestBySound.CheckForBuffOrQuest(soundGoupName.Split('/').Last(), myEntity.position);
            return true;
        }
    }

    [HarmonyPatch(typeof(Client))]
    [HarmonyPatch("Play")]
    [HarmonyPatch(new[] { typeof(Vector3), typeof(string), typeof(float), typeof(int) })]
    public class AudioClientPlay2
    {
        private static readonly string AdvFeatureClass = "AdvancedSoundFeatures";

        private static bool Prefix(Vector3 position, string soundGoupName)
        {
            AdvLogging.DisplayLog(AdvFeatureClass, "Audio.Client.Play(): Vector3, string: " + soundGoupName);

            if (string.IsNullOrEmpty(soundGoupName))
                return true;

            AdvLogging.DisplayLog(AdvFeatureClass, "Audio.Client.Play(): Vector3, string: " + soundGoupName.Split('/').Last());
            GiveBuffOrQuestBySound.CheckForBuffOrQuest(soundGoupName.Split('/').Last(), position);
            return true;
        }
    }
}