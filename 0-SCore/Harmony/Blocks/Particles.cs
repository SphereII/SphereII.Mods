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
                var strParticleName = "";
                if (__instance.Properties.Values.ContainsKey("ParticleName"))
                {
                    strParticleName = __instance.Properties.Values["ParticleName"];
                    if (!ParticleEffect.IsAvailable(strParticleName))
                        ParticleEffect.LoadAsset(strParticleName);
                }

                if (__instance.blockMaterial != null)
                {
                    if (__instance.blockMaterial.Properties.Contains("FireParticle"))
                    {
                        var fireParticle = __instance.blockMaterial.Properties.Values["FireParticle"];
                        if (!ParticleEffect.IsAvailable(fireParticle))
                            ParticleEffect.LoadAsset(fireParticle);
                    }
                    if (__instance.blockMaterial.Properties.Contains("SmokeParticle"))
                    {
                        var smokeParticle = __instance.blockMaterial.Properties.Values["SmokeParticle"];
                        if (!ParticleEffect.IsAvailable(smokeParticle))
                            ParticleEffect.LoadAsset(smokeParticle);
                    }

                }

                if (__instance.Properties.Contains("FireParticle"))
                {
                    var fireParticle = __instance.Properties.Values["FireParticle"];
                    if (!ParticleEffect.IsAvailable(fireParticle))
                        ParticleEffect.LoadAsset(fireParticle);
                }
                if (__instance.Properties.Contains("SmokeParticle"))
                {
                    var smokeParticle = __instance.Properties.Values["SmokeParticle"];
                    if (!ParticleEffect.IsAvailable(smokeParticle))
                        ParticleEffect.LoadAsset(smokeParticle);
                }

            }
        }
    }
}