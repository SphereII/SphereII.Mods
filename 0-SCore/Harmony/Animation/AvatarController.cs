using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Harmony.Animation
{
    public class AvatarControllerSetTrig
    {
        private const string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private const string Feature = "AnimatorMapper";

        /// <summary>
        /// Patch to the allow set a RandomIndex integer, with a range of 0 to 10, to allow more flexibility in custom animators 
        /// </summary>
        [HarmonyPatch(typeof(AvatarController))]
        [HarmonyPatch("TriggerEvent")]
        [HarmonyPatch(new[] { typeof(string) })]
        public class AvatarControllerSetTrigger
        {
            public static bool Prefix(global::AvatarController __instance, string _property)
            {
                if (Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    AdvLogging.DisplayLog(AdvFeatureClass, "Set Trigger(): " + _property);

                // Provides a random index value to the default animator.
                __instance.UpdateInt("RandomIndex", Random.Range(0, 10));
                __instance.UpdateInt(_property, Random.Range(0, 10));
                return true;
            }
        }
        
        [HarmonyPatch(typeof(AvatarController))]
        [HarmonyPatch("SetCrouching")]
        public class AvatarControllerSetCrouching
        {
            private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
            public static bool Prefix(global::AvatarController __instance, Animator ___anim, bool _bEnable, Dictionary<int, AnimParamData> ___ChangedAnimationParameters, global::EntityAlive ___entity)
            {
                if (___anim == null || ___anim.GetBool(IsCrouchingHash) == _bEnable) return true;
                ___anim.SetBool(IsCrouchingHash, _bEnable);
                if (IsCrouchingHash == AvatarController.isFPVHash)
                {
                    return true;
                }
                if (!___entity.isEntityRemote)
                {
                    ___ChangedAnimationParameters[IsCrouchingHash] = new AnimParamData(IsCrouchingHash, AnimParamData.ValueTypes.Bool, _bEnable);
                }
                return true;
            }
        }
        
        [HarmonyPatch(typeof(AvatarZombieController))]
        [HarmonyPatch("FindBodyParts")]
        public class AvatarControllerFindBodyParts
        {
            public static void Postfix(global::AvatarZombieController __instance,  global::EntityAlive ___entity, Transform ___bipedT, ref Transform ___rightHandT)
            {
                if (___entity is not EntityAliveSDX entityAliveSdx) return;
                
                // Since we allow weapon switching, the right hand transform may change depending on the weapon.
                // Re-read the right hand transform, which will give it the option to specify another one through a property entry on the item.
                ___rightHandT = ___bipedT.FindInChilds(entityAliveSdx.GetRightHandTransformName(), false);
            }
        }

     
        [HarmonyPatch(typeof(AvatarZombieController))]
        [HarmonyPatch("Update")]
        public class AvatarControllerUpdate
        {
            public static void Postfix(global::AvatarZombieController __instance,  global::EntityAlive ___entity)
            {
                if ( ___entity == null) return;
                if (___entity is not EntityAliveSDX && ___entity is not EntityEnemySDX) return;
                if (___entity.IsFlyMode.Value) return;
                
                
                __instance.TryGetFloat(AvatarController.forwardHash, out var num);
                __instance.TryGetFloat(AvatarController.strafeHash, out var num2);

                if (num < 0.01)
                {
                    __instance.UpdateFloat(AvatarController.forwardHash, 0f, false);
                }

                if (num2 < 0.01)
                {
                    __instance.UpdateFloat(AvatarController.strafeHash, 0f, false);
                }
                
                if (num < 0.01f || num2 < 0.01f)
                {
                    __instance.UpdateBool(AvatarController.isMovingHash, false, false);
                }
            }
        }
    }
}