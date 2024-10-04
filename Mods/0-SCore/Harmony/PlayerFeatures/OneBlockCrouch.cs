using HarmonyLib;
using UnityEngine;

namespace Harmony.PlayerFeatures
{
    /**
     * SCore_OneBlockCrouch
     * 
     * This class includes a Harmony patches to the EntityPlayer Local to allow players to crawl through a single block
     */
    public class OneBlockCrouch
    {
        private static readonly string AdvFeatureClass = "AdvancedPlayerFeatures";
        private static readonly string Feature = "OneBlockCrouch";

        [HarmonyPatch(typeof(EntityPlayerLocal))]
        [HarmonyPatch("Init")]
        public class EntityPlayerLocalInit
        {
            private static void Postfix(EntityPlayerLocal __instance)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;

                AdvLogging.DisplayLog(AdvFeatureClass, "Activating One Block Crouch");
                var heightModifier = 0.49f;
                var strPhysicsCrouchHeightModifier = Configuration.GetPropertyValue(AdvFeatureClass, "PhysicsCrouchHeightModifier");
                if (!string.IsNullOrEmpty(strPhysicsCrouchHeightModifier))
                {
                    // Read in and validate the crouch modifier to make sure it's not too small.
                    heightModifier = StringParsers.ParseFloat(strPhysicsCrouchHeightModifier);
                    if (heightModifier < 0.10)
                        heightModifier = 0.10f;
                    // We don't really care if the modders want to make it higher. Bad knees, maybe?
                }
                //__instance.vp_FPController.PhysicsCrouchHeightModifier = 0.49f;
                __instance.vp_FPController.PhysicsCrouchHeightModifier = heightModifier;
                __instance.vp_FPController.SyncCharacterController();
            }
        }


        [HarmonyPatch(typeof(vp_FPController))]
        [HarmonyPatch("SyncCharacterController")]
        public class SCoreOneBlockCrouch_GetEyeHeight
        {
            private static void Postfix(vp_FPController __instance, ref float ___m_NormalHeight)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;


                //if (__instance.playerInput.Crouch.IsPressed && !___entityPlayerLocal.IsFlyMode.Value) ___entityPlayerLocal.cameraTransform.position -= Vector3.down;
                AdvLogging.DisplayLog(AdvFeatureClass,
                    $"Crouch Height {___m_NormalHeight} Crouch Height: {___m_NormalHeight * __instance.PhysicsCrouchHeightModifier}");
            }
        }


        [HarmonyPatch(typeof(vp_FPCamera))]
        [HarmonyPatch("FixedUpdate")]
        public class vpFPCameraFixedUpdate
        {
            private static void Postfix(vp_FPCamera __instance,  vp_FPPlayerEventHandler ___m_Player)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;

                if (___m_Player.Driving.Active) return;
                
                // If the player is not crouching, do not lower the camera.
                if (!___m_Player.Crouch.Active)
                {
                    return;
                }
                
                
                // this lowers the camera to prevent clipping of the terrain.
                if (__instance.PositionOffset.y == 1.30f)
                    __instance.PositionOffset.y -= 0.31f;
            }
        }
    }
}