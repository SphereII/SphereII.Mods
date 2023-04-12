using HarmonyLib;
using UnityEngine;

namespace Harmony.Animation
{
    public class Harmony_Animation
    {
        private const string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private const string SpeedFeature = "EntitySpeedCheck";
        private static readonly int StrafeHash = Animator.StringToHash("Strafe");

        /// <summary>
        /// When enabled via the SCore's block's AdvancedTroubleshootingFeatures, EntitySpeedCheck, it will aggressively spam the entity's forward and strafe speeds.
        /// </summary>
        [HarmonyPatch(typeof(AvatarZombie01Controller))]
        [HarmonyPatch("LateUpdate")]
        public class AvatarControllerLateUpdate
        {
            private static bool Prefix(AvatarZombie01Controller __instance, global::EntityAlive ___entity)
            {
                var strafe = ___entity.speedStrafe;

                AdvLogging.DisplayLog(AdvFeatureClass, SpeedFeature,
                    ___entity.entityId + " Before: AvatarZombie01Controller: Entity Speed Forward: " + ___entity.speedForward + "   Strafe: " + strafe);

                if (___entity.IsFlyMode.Value)
                    strafe = 0f;
                if (strafe >= 1234f)
                    strafe = 0f;
                __instance.SetFloat(StrafeHash, strafe);

                AdvLogging.DisplayLog(AdvFeatureClass, SpeedFeature,
                    ___entity.entityId + "  After: AvatarZombie01Controller: Entity Speed Forward: " + ___entity.speedForward + "   Strafe: " + strafe);

                return true;
            }
        }
    }
}