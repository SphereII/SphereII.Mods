using HarmonyLib;
using UnityEngine;

namespace Harmony.EntityAlive {
    public class EntityTraderRektMute {
        
        [HarmonyPatch(typeof(EntityTrader))]
        [HarmonyPatch("PlayVoiceSetEntry")]
        public class EntityTraderPlayVoiceSetEntry 
        {
            private static bool Prefix(EntityTrader __instance, EntityPlayer player) {

                if (player.Buffs.GetCustomVar("MuteTraderAll") > 0) return false;
                if (__instance.npcID != "traderrekt") return true;
                return !(player.Buffs.GetCustomVar("MuteTraderRekt") > 0);
            }
        }
    }
}