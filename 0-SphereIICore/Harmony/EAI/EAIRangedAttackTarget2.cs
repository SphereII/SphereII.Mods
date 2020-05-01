using Harmony;
class SphereII_EAIRangedAttackTarget2_tweaks
{
    [HarmonyPatch(typeof(EAIRangedAttackTarget2))]
    [HarmonyPatch("Update")]
    public class SphereII_EAIRangedAttackTarget2_Update
    {
        public static bool Prefix(EAIRangedAttackTarget2 __instance)
        {
            // Make the entity face its target
            if (__instance.entityTarget != null)
                __instance.theEntity.RotateTo(__instance.entityTarget, 45f, 45f);

            return true;
        }
    }
}

