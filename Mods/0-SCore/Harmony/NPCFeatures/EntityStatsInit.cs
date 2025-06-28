
using HarmonyLib;
using UnityEngine;

namespace Harmony.NPCFeatures
{
    public class EntityStatsInitPatches
    {
        [HarmonyPatch(typeof(EntityStats))]
        [HarmonyPatch(nameof(EntityStats.Init))]
        public class EntityStatsInit
        {
            private static void Postfix(EntityStats __instance)
            {
                if (__instance.m_entity is not EntityAliveSDX) return;
                var num2 = (int)EffectManager.GetValue(PassiveEffects.StaminaMax, null, 100f, __instance.m_entity);
                __instance.Stamina = new Stat(Stat.StatTypes.Stamina, __instance.m_entity, (float)num2, (float)num2)
                {
                    GainPassive = PassiveEffects.StaminaGain,
                    MaxPassive = PassiveEffects.StaminaMax,
                    LossPassive = PassiveEffects.StaminaLoss
                };

            }
        }
    }

}
