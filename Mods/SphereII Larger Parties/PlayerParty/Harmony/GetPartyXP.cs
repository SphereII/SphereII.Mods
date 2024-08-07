using System;
using HarmonyLib;

namespace LargerParties.PlayerParty.Harmony {
    [HarmonyPatch(typeof(Party))]
    [HarmonyPatch("GetPartyXP")]
    public class PartyGetPartyXP {
        public static void Postfix(ref int __result, Party __instance, EntityPlayer player, int startingXP) {
            if (__instance.MemberList.Count < 9) return;

            // For large parties, provide a minimum level of exp to all players.
            __result = Math.Max(PlayerPartySettings.MinExp, __result);
        }
    }
}