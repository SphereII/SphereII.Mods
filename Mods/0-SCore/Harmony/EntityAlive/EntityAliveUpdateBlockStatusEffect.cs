using HarmonyLib;

namespace Harmony.EntityAlive {
    public class EntityAliveUpdateBlockStatusEffect {
        [HarmonyPatch(typeof(global::EntityAlive))]
        [HarmonyPatch("OnUpdateLive")]
        public class EntityAliveOnUpdateLive 
        {
            private static void Postfix(global::EntityAlive __instance) {
                if (__instance.Buffs.GetCustomVar("UpdateBlockStatusEffect") == 0)
                    return;
                EntityUtilities.UpdateBlockRadiusEffects(__instance);
            }
        }
    }
}