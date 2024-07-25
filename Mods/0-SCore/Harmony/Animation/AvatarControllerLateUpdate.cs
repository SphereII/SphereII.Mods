using HarmonyLib;
using UnityEngine;

namespace Harmony.Animation
{
    public class Harmony_Animation
    {
        private const string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private static string SpeedFeature = "EntitySpeedCheck";
        private static int _strafeHash = Animator.StringToHash("Strafe");


        [HarmonyPatch(typeof(AvatarZombieController))]
        [HarmonyPatch("LateUpdate")]
        public class AvatarControllerLateUpdate
        {
            private static bool Prefix(AvatarZombieController __instance, global::EntityAlive ___entity)
            {
                float strafe = ___entity.speedStrafe;

                AdvLogging.DisplayLog(AdvFeatureClass, SpeedFeature,
                    ___entity.entityId + " Before: AvatarZombie01Controller: Entity Speed Forward: " + ___entity.speedForward + "   Strafe: " + strafe);

                if (___entity.IsFlyMode.Value)
                    strafe = 0f;
                if (strafe >= 1234f)
                    strafe = 0f;
                __instance.UpdateFloat(_strafeHash, strafe);

                AdvLogging.DisplayLog(AdvFeatureClass, SpeedFeature,
                    ___entity.entityId + "  After: AvatarZombie01Controller: Entity Speed Forward: " + ___entity.speedForward + "   Strafe: " + strafe);

                return true;
            }
        }
    }
}