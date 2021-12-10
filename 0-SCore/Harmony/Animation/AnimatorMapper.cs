using HarmonyLib;

namespace Harmony.Animation
{
    public class Harmony_AnimalAnimation
    {
        private static string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private static string Feature = "EntitySpeedCheck";

        [HarmonyPatch(typeof(AvatarAnimalController))]
        [HarmonyPatch("LateUpdate")]
        public class SphereII_AnimalAvatarController
        {
            static void Postfix(AvatarAnimalController __instance, global::EntityAlive ___entity)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;

                AdvLogging.DisplayLog(AdvFeatureClass, Feature,
                    $" AnimalAvatarController: {___entity.EntityName} ( {___entity.entityId} Entity Speed Forward: " + ___entity.speedForward + "   Strafe: " + ___entity.speedStrafe);
            }
        }
    }

}
//[HarmonyPatch(typeof(XmlPatcher))]
//[HarmonyPatch("singlePatch")]
//public class SphereII_XmlPatcher_SinglePatch
//{

//        static bool Prefix(XmlFile _targetFile, XmlElement _patchElement, string _patchName)
//        {
//            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
//            return true;*/