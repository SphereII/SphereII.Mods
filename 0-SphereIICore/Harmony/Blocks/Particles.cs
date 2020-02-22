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

            if (__instance is BlockDoorSecure)
                return;

            //    __instance.IsPathSolid = true;
        }
    }

    // Let the NPCs pass by traps without being hurt.
    [HarmonyPatch(typeof(BlockDamage))]
    [HarmonyPatch("OnEntityCollidedWithBlock")]
    public class SphereII_BlockDamage_OnEntityCollidedWithBlock
    {
        public static bool Prefix( Entity _targetEntity)
        {
            if (_targetEntity is EntityNPC)
                return false;
            return true;
        }
    }



}