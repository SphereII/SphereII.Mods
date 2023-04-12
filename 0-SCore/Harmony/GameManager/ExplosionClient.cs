using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SCore.Harmony.GameManagerPatches
{
    [HarmonyPatch(typeof(GameManager))]
    [HarmonyPatch("ExplosionClient")]
    public class GameManagerExplosionClient
    {
        private static GameObject Postfix(GameObject __result, Vector3 _center, Quaternion _rotation,
            int _index, int _blastPower, float _blastRadius)
        {
            if (GameManager.Instance.World == null) return __result;

            // If it's a normal system particle, don't do anything.
            if (_index >= 0 && _index <= 20) return __result;

            var particleEffect = ParticleEffect.GetDynamicParticleEffect(_index);
            if (particleEffect == null) return __result;
            var result = Object.Instantiate(particleEffect.gameObject, _center - Origin.position, _rotation);
            ApplyExplosionForce.Explode(_center, _blastPower, _blastRadius);
            return result;
        }
    }
}