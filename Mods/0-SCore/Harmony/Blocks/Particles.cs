using HarmonyLib;

namespace Harmony.Blocks
{
    /**
     * * Adding to new blocks:
     * *
     * <property name="ParticleName" value="#@modfolder(0-SCore):Resources/PathSmoke.unity3d?P_PathSmoke_X" />
     */
    public class Particles
    {

     
        [HarmonyPatch(typeof(Block))]
        [HarmonyPatch(nameof(Block.Init))]
        public class Init
        {
            public static void Postfix(ref Block __instance)
            {
                BlockUtilitiesSDX.CheckAndLoadParticles(__instance.Properties, "ParticleName");
                if (__instance.blockMaterial != null)
                {
                    BlockUtilitiesSDX.CheckAndLoadParticles(__instance.blockMaterial.Properties, "FireParticle");
                    BlockUtilitiesSDX.CheckAndLoadParticles(__instance.blockMaterial.Properties, "SmokeParticle");
                }
                BlockUtilitiesSDX.CheckAndLoadParticles(__instance.Properties, "FireParticle");
                BlockUtilitiesSDX.CheckAndLoadParticles(__instance.Properties, "SmokeParticle");
            }
        }
    }
}