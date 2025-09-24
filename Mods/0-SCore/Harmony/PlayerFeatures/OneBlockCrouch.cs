using HarmonyLib;
using InControl.NativeDeviceProfiles;
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

        [HarmonyPatch(typeof(vp_FPController))]
        [HarmonyPatch(nameof(vp_FPController.OnStart_Crouch))]
        public class vp_FPControllerOnStart_Crouch
        {
            private const float DefaultCrouchHeightModifier = 0.49f;
            private const float StandardCrouchHeightModifier = 0.7f;

            private static bool Prefix(ref vp_FPController __instance)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                {
                    return true;
                }
        
                float heightModifier;

                if (__instance.localPlayer.Buffs.GetCustomVar("NoOneBlockCrouch") > 0)
                {
                    heightModifier = StandardCrouchHeightModifier;
                }
                else
                {
                    string strPhysicsCrouchHeightModifier = Configuration.GetPropertyValue(AdvFeatureClass, "PhysicsCrouchHeightModifier");
                    if (!string.IsNullOrEmpty(strPhysicsCrouchHeightModifier) && float.TryParse(strPhysicsCrouchHeightModifier, out float parsedModifier))
                    {
                        heightModifier = Mathf.Max(parsedModifier, 0.10f);
                    }
                    else
                    {
                        heightModifier = DefaultCrouchHeightModifier;
                    }
                }
        
                __instance.m_CrouchHeight = __instance.m_NormalHeight * heightModifier;
                __instance.m_CrouchCenter = __instance.m_NormalCenter * heightModifier;
        
                return true;
            }
        }

        [HarmonyPatch(typeof(vp_FPCamera))]
        [HarmonyPatch(nameof(vp_FPCamera.FixedUpdate))]
        public class vpFPCameraFixedUpdate
        {
            private const float NormalPositionOffset = 1.30f;
            private const float CrouchCameraOffset = 0.31f;

            private static void Postfix(vp_FPCamera __instance, vp_FPPlayerEventHandler ___m_Player)
            {
                // Early exit for conditions that don't require the patch logic
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature) ||
                    ___m_Player.Driving.Active ||
                    !___m_Player.Crouch.Active)
                {
                    return;
                }

                // It is safer to get the player instance on demand
                EntityPlayerLocal playerLocal = GameManager.Instance.World.GetPrimaryPlayer();
                if (playerLocal == null)
                {
                    return;
                }

                // If the player has the buff, set to the standard crouch height and exit
                if (playerLocal.Buffs.GetCustomVar("NoOneBlockCrouch") > 0)
                {
                    __instance.PositionOffset.y = NormalPositionOffset;
                    return;
                }

                // Adjust the camera height for one-block crouching
                // Check for an approximate value to avoid floating-point comparison issues
                if (Mathf.Abs(__instance.PositionOffset.y - NormalPositionOffset) < 0.001f)
                {
                    __instance.PositionOffset.y -= CrouchCameraOffset;
                }
            }
        }
    }
}