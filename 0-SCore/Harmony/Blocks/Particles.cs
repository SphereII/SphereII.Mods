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
        [HarmonyPatch("Init")]
        public class Init
        {
            public static void Postfix(ref Block __instance)
            {
                if (!__instance.Properties.Values.ContainsKey("ParticleName"))
                    return;

                var strParticleName = __instance.Properties.Values["ParticleName"];
                if (!ParticleEffect.IsAvailable(strParticleName))
                    ParticleEffect.RegisterBundleParticleEffect(strParticleName);
            }
        }
    }
}