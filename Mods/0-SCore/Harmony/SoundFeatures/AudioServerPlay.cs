using Audio;
using HarmonyLib;
using System.Linq;

namespace Harmony.SoundFeatures
{
    [HarmonyPatch(typeof(Manager))]
    [HarmonyPatch("Play")]
    [HarmonyPatch(new[] { typeof(Entity), typeof(string), typeof(float), typeof(bool) })]
    public class AudioServerPlay
    {
        private static readonly string AdvFeatureClass = "AdvancedSoundFeatures";


        private static bool Prefix(Manager __instance, Entity entity, string soundGroupName)
        {
            AdvLogging.DisplayLog(AdvFeatureClass, "AudioManager.Play(): Entity, String, float, bool: " + soundGroupName);
            if (entity == null)
                return true;

            if (string.IsNullOrEmpty(soundGroupName))
                return true;

            AdvLogging.DisplayLog(AdvFeatureClass, "Audio.Client.Play(): Vector3, string: " + soundGroupName.Split('/').Last());
            GiveBuffOrQuestBySound.CheckForBuffOrQuest(soundGroupName.Split('/').Last(), entity.position);
            return true;
        }
    }
}