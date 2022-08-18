using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCore.Harmony.Fire
{
    public class SCoreExplosion
    {

        // Allows the spread of the particles to catch things on fire.
        [HarmonyPatch(typeof(Explosion))]
        [HarmonyPatch("AttackBlocks")]
        public class SCoreExplosion_AttackBlocks
        {
            public static void Postfix(Explosion __instance, ExplosionData ___explosionData)
            {
                if (FireManager.Instance.Enabled == false) return;

                foreach (var position in __instance.ChangedBlockPositions)
                {
                    // BlockDamage set to 0 does nothing.
                    if (___explosionData.BlockDamage == 0) return;

                    // Negative block damages extinguishes
                    if (___explosionData.BlockDamage < 0f)
                        FireManager.Instance.Extinguish(position.Key);
                    else
                        FireManager.Instance.Add(position.Key);
                }
            }
        }
    }
}
