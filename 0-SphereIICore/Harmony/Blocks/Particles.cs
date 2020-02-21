using DMT;
using Harmony;
using System;
using UnityEngine;


public class SphereII_Blocks_Particles
{
    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("Init")]
    public class SphereII_Block_Init
    {
        public static void Postfix(ref Block __instance)
        {
            if (__instance.Properties.Values.ContainsKey("ParticleName"))
                ParticleEffect.RegisterBundleParticleEffect(__instance.Properties.Values["ParticleName"]);

            __instance.IsPathSolid = true;
        }
    }

 


}