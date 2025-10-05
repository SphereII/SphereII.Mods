using HarmonyLib;
using UnityEngine;

namespace Features.Fire.Harmony {
    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch(nameof(Block.Init))]
    public class Blockinit {
        public static void Postfix(Block __instance) {
            if (__instance.Properties.Values.ContainsKey("FireParticle"))
            {
                var particle= __instance.Properties.Values["FireParticle"];
                BlockUtilitiesSDX.CheckAndLoadParticles(particle);
            }
            if (__instance.Properties.Values.ContainsKey("SmokeParticle"))
            {
                var particle= __instance.Properties.Values["SmokeParticle"];
                BlockUtilitiesSDX.CheckAndLoadParticles(particle);
            }

            if (__instance.blockMaterial.Properties.Values.ContainsKey("FireParticle"))
            {
                var particle= __instance.blockMaterial.Properties.Values["FireParticle"];
                BlockUtilitiesSDX.CheckAndLoadParticles(particle);
            }
            if (__instance.blockMaterial.Properties.Values.ContainsKey("SmokeParticle"))
            {
                var particle= __instance.blockMaterial.Properties.Values["SmokeParticle"];
                BlockUtilitiesSDX.CheckAndLoadParticles(particle);
            }

        }
    }
}