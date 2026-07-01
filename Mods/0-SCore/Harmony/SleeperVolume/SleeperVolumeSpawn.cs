using HarmonyLib;

namespace SCore.Harmony.SleeperVolume
{
    [HarmonyPatch(typeof(global::SleeperVolume))]
    [HarmonyPatch("AddEnemyToWorld")]
    public class SleeperVolumeSpawn
    {
        private static bool _initialized;
        private static bool _enabled;

        // Spawn now creates entities async; AddEnemyToWorld is the callback where
        // the entity is first available. ___flags is an ETriggerType bitmask on SleeperVolume.
        public static void Postfix(object[] __args, int ___flags)
        {
            if (!Enabled())
                return;

            var entity = __args[1] as EntityAlive;
            if (entity == null)
                return;

            // ETriggerType "Attack" is 2
            int trigger = ___flags & 7;
            if (trigger == 2 && (entity is IEntityAliveSDX || entity is EntityEnemySDX))
            {
                entity.ConditionalTriggerSleeperWakeUp();
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
