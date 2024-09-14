using HarmonyLib;

public static class EventOnClientKill {
    public delegate bool OnClientKill(DamageResponse _dmResponse, EntityAlive entityDamaged);

    public static event OnClientKill OnClientKillEvent;

    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("ClientKill")]
    public class EntityAliveProcessDamageResponseLocal {
        private static void Postfix(DamageResponse _dmResponse, EntityAlive __instance) {
            OnClientKillEvent?.Invoke(_dmResponse, __instance);
        }
    }
}