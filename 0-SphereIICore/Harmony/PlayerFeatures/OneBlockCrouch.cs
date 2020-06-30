using HarmonyLib;
using UnityEngine;

/**
 * SphereII__OneBlockCrouch
 *
 * This class includes a Harmony patches to the EntityPlayer Local to allow players to crawl through a single block
 * 
 */
public class SphereII__OneBlockCrouch
{
    private static string AdvFeatureClass = "AdvancedPlayerFeatures";
    private static string Feature = "OneBlockCrouch";

    [HarmonyPatch(typeof(EntityPlayerLocal))]
    [HarmonyPatch("Init")]
    public class SphereII_EntityPlayerLocal_Init
    {
        static void Postfix(EntityPlayerLocal __instance)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return;

            AdvLogging.DisplayLog(AdvFeatureClass, "Activating One Block Crouch");
            __instance.vp_FPController.PhysicsCrouchHeightModifier = 0.49f;
            __instance.vp_FPController.SyncCharacterController();
        }
    }

    [HarmonyPatch(typeof(PlayerMoveController))]
    [HarmonyPatch("Update")]
    public class SphereII_OneBlockCrouch_GetEyeHeight
    {

        static void Postfix(PlayerMoveController __instance, ref EntityPlayerLocal ___entityPlayerLocal)
        {
       
        // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return;

            if (__instance.playerInput.Crouch.IsPressed && !___entityPlayerLocal.IsFlyMode.Value)
            {
                ___entityPlayerLocal.cameraTransform.position -= Vector3.down;
            }

        }
    }

}