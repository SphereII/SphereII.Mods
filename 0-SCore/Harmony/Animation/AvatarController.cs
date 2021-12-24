using HarmonyLib;
using UnityEngine;

namespace Harmony.Animation
{
    public class AvatarControllerSetTrig
    {
        private const string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private const string Feature = "AnimatorMapper";

        /**
         * AvatarController.SetTrigger()
         * 
         * This includes a logging feature to show which trigger is called, when the AnimatorMapper Feature is enabled in the Config/blocks.xml
         *  
         * This includes a RandomIndex integer, with a range of 0 to 10, to allow more flexibility in custom animators
         */
        [HarmonyPatch(typeof(AvatarController))]
        [HarmonyPatch("SetTrigger")]
        [HarmonyPatch(new[] { typeof(string) })]
        public class AvatarControllerSetTrigger
        {
            public static bool Prefix(global::AvatarController __instance, string _property)
            {
                if (Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    AdvLogging.DisplayLog(AdvFeatureClass, "Set Trigger(): " + _property);

                // Provides a random index value to the default animator.
                __instance.SetInt("RandomIndex", Random.Range(0, 10));
                __instance.SetInt(_property, Random.Range(0, 10));
                return true;
            }
        }

        //[HarmonyPatch(typeof(AvatarZombieController))]
        //[HarmonyPatch("Start")]
        //public class AvatarControllerStart
        //{
        //    public static void Postfix(global::AvatarController __instance, ref int ___hitLayerIndex)
        //    {
        //        if (EntityUtilities.IsHuman(__instance.Entity.entityId))
        //            ___hitLayerIndex = 5;
        //        return;
        //    }
        //}
    }
}