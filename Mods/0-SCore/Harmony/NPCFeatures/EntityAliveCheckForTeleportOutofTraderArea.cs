using HarmonyLib;

namespace SCore.Harmony.NPCFeatures
{
    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("checkForTeleportOutOfTraderArea")]
    public class EntityAliveCheckForTeleportOutofTraderArea
    {
        private static string cvarID = "traderStayTicket";
    
        public static bool Prefix(EntityAlive __instance)
        {
            return !(__instance.Buffs.GetCustomVar(cvarID) > 0);
        }
    }
}
