using HarmonyLib;
using UnityEngine;

namespace Harmony.EntityAlive {
    public class EntityTraderRektMute {
        
        [HarmonyPatch(typeof(EntityTrader))]
        [HarmonyPatch("PlayVoiceSetEntry")]
        public class EntityTraderPlayVoiceSetEntry 
        {
            private static bool Prefix(EntityTrader __instance) {

                var localPlayer = GameManager.Instance.myEntityPlayerLocal;
                if (localPlayer == null) return true;

                if (localPlayer.Buffs.GetCustomVar("MuteTraderAll") > 0) return false;
               
                if (__instance.EntityName != "npcTraderRekt") return true;

                return !(localPlayer.Buffs.GetCustomVar("MuteTraderRekt") > 0);
            }
        }
    }
}