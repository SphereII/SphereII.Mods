using HarmonyLib;
using UnityEngine;

namespace SCore.Features.PassiveEffectHooks.Harmony
{
    [HarmonyPatch(typeof(XUiC_RecipeStack))]
    [HarmonyPatch(nameof(XUiC_RecipeStack.outputStack))]
    public class XUiCRecipeStackOutputStackOnrepairPatch
    {
        public static bool Prefix(ref XUiC_RecipeStack __instance)
        {
            if (__instance.AmountToRepair <= 0) return true;
            if (__instance.originalItem == null) return true;
            if (__instance.originalItem.Equals(ItemValue.None)) return true;

            __instance.originalItem?.SetMetadata("DamageAmount", __instance.originalItem.UseTimes, TypedMetadataValue.TypeTag.Float);
            __instance.originalItem?.SetMetadata("PercentDamaged", __instance.originalItem.UseTimes / __instance.originalItem.MaxUseTimes, TypedMetadataValue.TypeTag.Float);
            return true;
        }

        public static void Postfix(ref XUiC_RecipeStack __instance)
        {
            
        }
    }
}