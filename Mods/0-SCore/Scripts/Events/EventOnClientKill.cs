using HarmonyLib;
using UnityEngine;

public static class EventOnClientKill {
    public delegate bool OnClientKill(DamageResponse _dmResponse, EntityAlive entityDamaged);

    public static event OnClientKill OnClientKillEvent;

    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("ClientKill")]
    public class EntityAliveProcessDamageResponseLocal {
        private static bool Prefix(DamageResponse _dmResponse, EntityAlive __instance) {
            // Let's go through some pre-checks, since we do not want the game kills to trigger this event.
            // from killall
            if (_dmResponse.Strength == 99999) return true;
            // From blood moon party kill, time of day
            if (_dmResponse.Source == null) return true;
            // Also from kill all, but also a good idea.
            if (_dmResponse.Source.GetDamageType() == EnumDamageTypes.Suicide) return true;
            if (__instance.IsDead()) return true;
            OnClientKillEvent?.Invoke(_dmResponse, __instance);
            return true;
        }
    }
}