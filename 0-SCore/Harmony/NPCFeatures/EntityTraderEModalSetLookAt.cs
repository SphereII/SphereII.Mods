using HarmonyLib;

namespace Harmony.NPCFeatures
{
    [HarmonyPatch(typeof(EModelBase))]
    [HarmonyPatch("SetLookAt")]
    public class EmodelBaseSetLookAt
    {
        private static bool Prefix(EModelBase __instance)
        {
            return false;
        }
    }
}