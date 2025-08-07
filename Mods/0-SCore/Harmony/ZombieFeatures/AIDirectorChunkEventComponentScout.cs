using System;
using HarmonyLib;
using UnityEngine;

// Recommended: Make the containing class static if it only holds patches
namespace SCore.Harmony.ZombieFeatures
{
    public static class AIDirectorChunkEventComponentScout
    {
        // Configuration properties for readability and easy modification
        private static readonly string AdvFeatureClass = "AdvancedZombieFeatures";
        private static readonly string Feature = "ScoutSpawnChance";

        // Constants for specific configuration values
        private const float ConfigDisableCustomSpawn = -1f; // Value to completely disable custom scout spawning via this patch
        private const float ConfigAllowOriginalChance = 0.2f; // Value to allow original method's default 0.2f chance (if applicable)
        private const float MinActivityLevelForEvent = 25f;

        [HarmonyPatch(typeof(AIDirectorChunkEventComponent))]
        [HarmonyPatch(nameof(AIDirectorChunkEventComponent.CheckToSpawn))]
        [HarmonyPatch(new Type[] { typeof(AIDirectorChunkData) })]
        public static class AIDirectorChunkEventComponentScoutCheckToSpawnPatch // Renamed for clarity as it's a patch container
        {
            /// Customizes scout spawning behavior based on configuration, activity level, and game state.
            /// False if custom scout spawning occurs (skips original method);
            /// True if original method should execute (e.g., custom logic disabled or conditions not met).
            public static bool Prefix(AIDirectorChunkEventComponent __instance, AIDirectorChunkData _chunkData)
            {
                // Attempt to parse the configured scout spawn chance.
                // If parsing fails, fall back to a default that allows the original method to run.
                if (!StringParsers.TryParseFloat(Configuration.GetPropertyValue(AdvFeatureClass, Feature), out var scoutSpawnChanceConfig))
                {
                    // Config value not found or invalid, defer to original method.
                    // Could also log a warning here if this is an unexpected state.
                    return true; 
                }

                // Early exit based on specific configuration values:
                // -1f: Explicitly disables this custom scout spawning patch, allowing original method.
                // 0.2f: Also allows original method to run (perhaps to use a hardcoded 0.2f from original).
                if (Mathf.Approximately(scoutSpawnChanceConfig, ConfigDisableCustomSpawn) || Mathf.Approximately(scoutSpawnChanceConfig, ConfigAllowOriginalChance))
                {
                    return true; 
                }

                // Check primary conditions for triggering a chunk event and potential scout spawn.
                var isHordeMeterActive = GameStats.GetBool(EnumGameStats.ZombieHordeMeter);
                var isEnemySpawningEnabled = GameStats.GetBool(EnumGameStats.IsSpawnEnemies);
                var hasSufficientActivity = _chunkData.ActivityLevel >= MinActivityLevelForEvent;
                if (!isHordeMeterActive || !isEnemySpawningEnabled || !hasSufficientActivity) return true;

                var bestChunkEvent = _chunkData.FindBestEventAndReset();
                if (bestChunkEvent == null) return true;

                // Determine if scouts should be spawned based on configured chance and playtesting status.
                
                bool flag = __instance.Director.random.RandomFloat < scoutSpawnChanceConfig && !GameUtils.IsPlaytesting();

                if ((__instance.Director.random.RandomFloat > scoutSpawnChanceConfig) || GameUtils.IsPlaytesting()) return true;

                __instance.StartCooldownOnNeighbors(bestChunkEvent.Position, flag);
                if (!flag) return true;

                _chunkData.SetLongDelay();
                __instance.SpawnScouts(bestChunkEvent.Position.ToVector3());
                return false;
            }
        }
    }
}