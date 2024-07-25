using HarmonyLib;

namespace SCore.Harmony.SleeperVolume
{
    [HarmonyPatch(typeof(global::SleeperVolume))]
    [HarmonyPatch("Spawn")]
    public class SleeperVolumeSpawn
    {
        private static bool _initialized;
        private static bool _enabled;

        public static void Postfix(ref EntityAlive __result, int ___flags)
        {
            if (!Enabled())
                return;

            // ___flags has an int representation of ETriggerType; "Attack" is 2
            int trigger = ___flags & 7;
            if (trigger == 2 && (__result is EntityAliveSDX || __result is EntityEnemySDX))
            {
                __result.ConditionalTriggerSleeperWakeUp();
            }
        }

        private static bool Enabled()
        {
            if (!_initialized)
            {
                _enabled = Configuration.CheckFeatureStatus("AdvancedNPCFeatures", "AttackVolumeInstantAwake");
                _initialized = true;
            }
            return _enabled;
        }
    }
}
