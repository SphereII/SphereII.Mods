using HarmonyLib;

namespace Harmony.Animation
{
    public class Harmony_AnimalAnimation
    {
        private const string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private const string Feature = "EntitySpeedCheck";

        /// <summary>
        /// When enabled via the SCore's block's AdvancedTroubleshootingFeatures, EntitySpeedCheck, it will aggressively spam the animals' forward and strafe speeds.
        /// </summary>
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
