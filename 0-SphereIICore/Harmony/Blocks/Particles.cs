using HarmonyLib;
using System;


/**
 * SphereII_Blocks_Particles
 * 
 * This class includes a Harmony patch to allow particles to be loaded from any blocks.
 * 
 * Usage XML: 
 *
 * Adding to new blocks:
 *   	<property name="ParticleName" value="#@modfolder(0-SphereIICore):Resources/PathSmoke.unity3d?P_PathSmoke_X"/>
 *
 */
public class SphereII_Blocks_Particles
{
    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("Init")]
    public class SphereII_Block_Init
    {
        public static void Postfix(ref Block __instance)
        {
            if (__instance.Properties.Values.ContainsKey("ParticleName"))
            {
                String strParticleName = __instance.Properties.Values["ParticleName"];
                if (!ParticleEffect.IsAvailable(strParticleName))
                    ParticleEffect.RegisterBundleParticleEffect(strParticleName);
            }


            if (__instance is BlockDoorSecure)
                return;
        }
    }

}