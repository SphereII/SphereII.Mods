using HarmonyLib;
using System;

/**
 * SphereII_AvatarController:  0-SphereIICore/Harmony/Animation/AvatarController.cs
 * 
 * This class includes Harmony patches to change the AvatarController.
 */
public class SphereII_AvatarController
{

    /**
     * AvatarController.SetTrigger()
     * 
     *  This includes a logging feature to show which trigger is called, when the AnimatorMapper Feature is enabled in the Config/blocks.xml
     *  
     *  This includes a RandomIndex integer, with a range of 0 to 10, to allow more flexibility in custom animators
     */
    [HarmonyPatch(typeof(AvatarController))]
    [HarmonyPatch("SetTrigger")]
    [HarmonyPatch(new Type[] { typeof(string) })]
    public class SphereII_AnimatorMapperTrigger
    {
        private static readonly string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private static readonly string Feature = "AnimatorMapper";

        public static bool Prefix(AvatarController __instance, string _property)
        {
            if (Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                AdvLogging.DisplayLog(AdvFeatureClass, "Set Trigger(): " + _property);

            // Provides a random index value to the default animator.
            __instance.SetInt("RandomIndex", UnityEngine.Random.Range(0, 10));
            __instance.SetInt(_property, UnityEngine.Random.Range(0, 10));
            return true;
        }
    }

}