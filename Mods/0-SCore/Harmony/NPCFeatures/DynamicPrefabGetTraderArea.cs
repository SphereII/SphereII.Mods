using HarmonyLib;

namespace Harmony.NPCFeatures
{
    [HarmonyPatch(typeof(DynamicPrefabDecorator))]
    [HarmonyPatch("GetRandomPOINearTrader")]
    public class DynamicPrefabDecoratorGetRandomPOINearTrader
    {
        private static bool Prefix(DynamicPrefabDecorator __instance, EntityTrader trader)
        {
            return trader.traderArea != null;
        }
    }
}