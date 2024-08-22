using HarmonyLib;

namespace Harmony.EntityVehicleDropCorpse {
    public class EntityVehicleDropCorpse {
        [HarmonyPatch(typeof(EntityVehicle))]
        [HarmonyPatch("Kill")]
        public class EntityVehicleKill {
            public static void Postfix(EntityVehicle __instance) {
                __instance.dropCorpseBlock();
            }
        }
    }
}