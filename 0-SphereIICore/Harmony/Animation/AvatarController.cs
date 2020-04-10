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




    [HarmonyPatch(typeof(LegacyAvatarController))]
    [HarmonyPatch("SetInRightHand")]
    [HarmonyPatch(new Type[] { typeof(Transform) })]
    public class SphereII_SetInRightHand
    {
        public static void Postfix(AvatarController __instance, Transform _transform, ref Transform ___rightHandItemTransform, ref Animator ___rightHandAnimator, ref Transform ___bipedTransform)
        {
            if (_transform != null)
            {
                Quaternion localRotation = (___rightHandItemTransform != null) ? ___rightHandItemTransform.localRotation : Quaternion.identity;
                _transform.parent = __instance.GetRightHandTransform();
                if ((!__instance.Entity.emodel.IsFPV || __instance.Entity.isEntityRemote) && __instance.Entity.inventory != null && __instance.Entity.inventory.holdingItem != null)
                {
                    _transform.localPosition = AnimationGunjointOffsetData.AnimationGunjointOffset[__instance.Entity.inventory.holdingItem.HoldType.Value].position;
                    _transform.localRotation = Quaternion.Euler(AnimationGunjointOffsetData.AnimationGunjointOffset[__instance.Entity.inventory.holdingItem.HoldType.Value].rotation);
                }
                else
                {
                    _transform.localPosition = Vector3.zero;
                    _transform.localRotation = localRotation;
                }
            }
            ___rightHandItemTransform = _transform;
            ___rightHandAnimator = ((_transform != null) ? _transform.GetComponent<Animator>() : null);
            if (___rightHandAnimator != null)
            {
                ___rightHandAnimator.logWarnings = false;
            }
            int num = (__instance.Entity.inventory.holdingItem != null) ? __instance.Entity.inventory.holdingItem.HoldType.Value : 0;

        }
    }

    //[HarmonyPatch(typeof(AvatarController))]
    //[HarmonyPatch("SetInt")]
    //[HarmonyPatch(new Type[] { typeof(string), typeof(int) })]
    //public class SphereII_AnimatorMapper_SetInt
    //{
    //    public static bool Prefix(AvatarController __instance, string _property, ref int _value)
    //    {
    //        if (_property == "Attack" && _value == 2)
    //        {
    //            if (EntityUtilities.IsHuman(__instance.Entity.entityId))
    //            {
    //                Uf the hold type is melee, and we are trying to do a left arm swing(no weapons), then don't swing with a left hand.
    //                if (EntityUtilities.CurrentHoldingItemType(__instance.Entity.entityId, typeof(ItemActionMelee)))
    //                    _value = 1;
    //            }

    //        }
    //        return true;
    //    }
    //}
}