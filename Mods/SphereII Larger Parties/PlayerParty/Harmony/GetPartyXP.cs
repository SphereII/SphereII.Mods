using System;
using HarmonyLib;
using UnityEngine;

namespace LargerParties.PlayerParty.Harmony {
    [HarmonyPatch(typeof(Party))]
    [HarmonyPatch("GetPartyXP")]
    public class PartyGetPartyXP {
        public static void Postfix(ref int __result, Party __instance, EntityPlayer player, int startingXP) {
            // For large parties, provide a minimum level of exp to all players.
            var temp = __result;
            __result = Math.Max(PlayerPartySettings.MinExp, __result);
            Log.Out($"GetPartyXP(): Party Members: {__instance.MemberList.Count} startingXP: {startingXP} Calculated: {temp} Actual XP Given: {__result}");
        }
    }
}