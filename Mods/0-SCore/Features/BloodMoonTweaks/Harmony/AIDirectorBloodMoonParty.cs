using Challenges;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.BloodMoonTweaks
{
    [HarmonyPatch(typeof(AIDirectorBloodMoonParty))]
    [HarmonyPatch(nameof(AIDirectorBloodMoonParty.InitParty))]
    public class AIDirectorBloodMoonPartyInitParty
    {
        private static readonly string AdvFeatureClass = "AdvancedZombieFeatures";
        public static bool Prefix(ref AIDirectorBloodMoonParty __instance)
        {
            var defaultProperty = Configuration.GetPropertyValue(AdvFeatureClass, "EnemyActiveMax");
            if (string.IsNullOrEmpty(defaultProperty)) return true;
            
            var enemyActiveMax = StringParsers.ParseSInt32(defaultProperty);
            if (enemyActiveMax <= 0) return true;
            var num = __instance.partySpawner.CalcPartyLevel();
            var num2 = GameStats.GetInt(EnumGameStats.BloodMoonEnemyCount) * __instance.partySpawner.partyMembers.Count;
            __instance.enemyActiveMax = Utils.FastMin(enemyActiveMax, num2);
            var num3 = Utils.FastMax(1f, num2 / (float)__instance.enemyActiveMax);
            num3 = Utils.FastLerp(1f, num3, num / 60f);
            __instance.partySpawner.SetScaling(num3);
            __instance.partySpawner.SetPartyLevel(num);
            __instance.bonusLootSpawnCount = __instance.partySpawner.bonusLootEvery / 2;
        

            return false;
        }
    }
}