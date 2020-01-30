//using DMT;
//using Harmony;
//using System;
//using UnityEngine;

//public class SphereII_AnimatorMapper
//{
//    private static string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
//    private static string Feature = "AnimatorMapper";

//    [HarmonyPatch(typeof(AvatarController))]
//    [HarmonyPatch("SetTrigger")]
//    [HarmonyPatch(new Type[] { typeof(string) })]
//    public class SphereII_AnimatorMapperTrigger
//    {
//        public static bool Prefix(AvatarController __instance, string _property)
//        {
//            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
//                return true;

//            AdvLogging.DisplayLog(AdvFeatureClass, "Set Trigger(): " + _property);
//            return true;
//        }
//    }

//}

//[HarmonyPatch(typeof(XmlPatcher))]
//[HarmonyPatch("singlePatch")]
//public class SphereII_XmlPatcher_SinglePatch
//{

//        static bool Prefix(XmlFile _targetFile, XmlElement _patchElement, string _patchName)
//        {
//            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
//            return true;