using HarmonyLib;
using UnityEngine;

namespace Features.Fire.Harmony
{

    // Light reduction patch from ocbMaurice
    [HarmonyPatch(typeof(ParticleEffect))]
    [HarmonyPatch("SpawnParticleEffect")]
    public class ParticleEffectspawnParticleEffect
    {
        private const int Odds = 4;
        private static int _count = Odds;
        
        public static void Postfix(ref Transform __result)
        {
            if (__result == null) return;
            if (__result.GetComponentInChildren<Light>() is not Light light) return;
            if (_count == Odds)
            {
                _count = 0;
                return;
            }

            light.enabled = false;
            var go = light.gameObject;
            go.transform.parent = null;
            UnityEngine.Object.Destroy(go);
            _count += 1;
        }
    }
}