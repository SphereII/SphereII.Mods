using DMT;
using Harmony;
using System;
using UnityEngine;

public class SphereII_AvatarController
{
    [HarmonyPatch(typeof(AvatarController))]
    [HarmonyPatch("SetTrigger")]
    [HarmonyPatch(new Type[] { typeof(string) })]
    public class SphereII_AnimatorMapperTrigger
    {
        public static bool Prefix(AvatarController __instance, string _property)
        {
            // Provides a random index value to the default animator.
            __instance.SetInt("RandomIndex",  UnityEngine.Random.Range(0, 10));
            __instance.SetInt(_property, UnityEngine.Random.Range(0, 10));
            return true;
        }
    }

}