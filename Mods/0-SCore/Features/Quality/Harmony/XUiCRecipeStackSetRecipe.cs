using System;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Quality.Harmony
{
    [HarmonyPatch(typeof(XUiC_RecipeStack))]
    [HarmonyPatch(nameof(XUiC_RecipeStack.outputStack))]
    public class XUiCRecipeStackSetRecipe
    {
        private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
        private static readonly string Feature = "CustomQualityLevels";

        public static bool Prefix( XUiC_RecipeStack __instance)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;
            if (__instance.recipe == null ) return true;
            

            Log.Out($"Recipe: {__instance.recipe.ToString()} Quality: {__instance.recipe.craftingTier} OutputQuality: {__instance.outputQuality}");
            return true;
        }
    }
}