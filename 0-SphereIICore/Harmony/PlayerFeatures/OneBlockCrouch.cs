using Harmony;

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
}