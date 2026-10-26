using HarmonyLib;

namespace SCore.Features.Quality.Harmony
{
    
    [HarmonyPatch(typeof(XUiC_CraftingInfoWindow))]
    [HarmonyPatch(nameof(XUiC_CraftingInfoWindow.SetRecipe))]
    public class XUiCCraftingInfoWindowSetRecipe
    {
        private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
        private static readonly string Feature = "CustomQualityLevels";

        public static bool Prefix( XUiC_CraftingInfoWindow __instance, XUiC_RecipeEntry _recipeEntry)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;
            if (_recipeEntry?.recipe == null ) return true;
            
            if( _recipeEntry.recipe.craftingTier <=1)
                _recipeEntry.recipe.craftingTier = QualityUtils.GetMinQuality();
            
            if (Configuration.CheckFeatureStatus(AdvFeatureClass, "Logging"))
                Log.Out($"XUiC_CraftingInfoWindow ::SetRecipe(): Recipe: {_recipeEntry.recipe.ToString()} Quality: {_recipeEntry.recipe.craftingTier} Selected Tier: {__instance.selectedCraftingTier} ");
            return true;
        }
    }
 
}