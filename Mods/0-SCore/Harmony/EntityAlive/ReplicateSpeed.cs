using HarmonyLib;
// Code from Zilox to fix the animals animating on dedi when they do not have root motion.
namespace Harmony.EntityAlivePatches
{
    [HarmonyPatch(typeof(global::EntityAlive))]
    [HarmonyPatch(nameof(global::EntityAlive.OnUpdatePosition))]
    public class ReplicateMovementSpeedsPatch
    {
        [HarmonyPatch]
        public class Patch
        {
            [HarmonyReversePatch]
            [HarmonyPatch(typeof(Entity), nameof(Entity.ReplicateSpeeds))]
            public static void ReplicateSpeeds(Entity instance)
            {
            }
        }

        private static void Postfix(global::EntityAlive __instance)
        {
            if (!__instance.RootMotion && !__instance.isEntityRemote)
            {
                Patch.ReplicateSpeeds(__instance);
            }
        }
    }
}