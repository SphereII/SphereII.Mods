using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Quality.Harmony
{
    /// <summary>
    /// Makes the crafting entries in the skills window report the quality the player can actually
    /// craft, instead of the number of unlock thresholds they have passed.
    /// <para>
    /// <c>ProgressionClass.DisplayData.GetQualityLevel</c> walks the QualityStarts array - parsed
    /// from the <c>unlock_level</c> attribute of a <c>display_entry</c> - and returns the index it
    /// stops at, capped at the array length. In vanilla that count happens to equal the quality,
    /// which is the whole of the display. It never reads the CraftingTier passive effect, so on a
    /// custom quality range the panel keeps counting 0 to 6 while the workbench turns out a 300.
    /// </para>
    /// <para>
    /// Rather than patching GetQualityLevel, which also drives the locked and available colours in
    /// this same controller and the "next unlock" points, this rewrites only the four quality
    /// bindings after the fact. A locked entry still reports zero, so the locked styling is
    /// untouched.
    /// </para>
    /// </summary>
    [HarmonyPatch(typeof(XUiC_SkillCraftingInfoEntry))]
    [HarmonyPatch(nameof(XUiC_SkillCraftingInfoEntry.GetBindingValueInternal))]
    public class XUiCSkillCraftingInfoEntryQuality
    {
        private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
        private static readonly string Feature = "CustomQualityLevels";

        public static void Postfix(XUiC_SkillCraftingInfoEntry __instance, bool __result,
            ref string _value, string _bindingName)
        {
            if (!__result) return;
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;

            switch (_bindingName)
            {
                case "currentqualitytext":
                case "currentqualitycolor":
                case "nextqualitytext":
                case "nextqualitycolor":
                    break;
                default:
                    return;
            }

            var data = __instance.data;
            if (data == null || data.Owner == null) return;

            var player = __instance.xui?.playerUI?.entityPlayer;
            if (player == null) return;

            var level = player.Progression.GetProgressionValue(data.Owner.Name).Level;

            // Zero means not unlocked yet. Leave that alone so the locked styling still reads.
            if (data.GetQualityLevel(level) == 0) return;

            var quality = GetCraftableQuality(data, player);
            if (quality <= 0) return;

            var perTier = QualityUtils.GetQualityPerTier();
            var next = Mathf.Min(quality + perTier, QualityUtils.GetMaxQuality());

            switch (_bindingName)
            {
                case "currentqualitytext":
                    _value = quality.ToString();
                    break;
                case "currentqualitycolor":
                    // Reuse the controller's own formatters rather than hand-rolling the string,
                    // so the format stays whatever the game says it is and the caching still works.
                    _value = __instance.durabilitycolorFormatter.Format(QualityInfo.GetTierColor(quality));
                    break;
                case "nextqualitytext":
                    _value = next.ToString();
                    break;
                case "nextqualitycolor":
                    _value = __instance.nextdurabilitycolorFormatter.Format(QualityInfo.GetTierColor(next));
                    break;
            }
        }

        /// <summary>
        /// Resolves what this entry's item can currently be crafted at, the same way the crafting
        /// window does - the recipe's crafting tier, clamped by CraftingMaxTier.
        /// </summary>
        private static int GetCraftableQuality(ProgressionClass.DisplayData data, EntityPlayer player)
        {
            // A display_entry usually carries only an icon, leaving ItemName empty, and vanilla
            // names that icon after the item. Try the explicit name first regardless.
            var itemName = data.ItemName;
            if (string.IsNullOrEmpty(itemName) && data.CustomIcon != null && data.CustomIcon.Length > 0)
                itemName = data.CustomIcon[0];

            if (string.IsNullOrEmpty(itemName)) return 0;

            var recipe = CraftingManager.GetRecipe(itemName);
            if (recipe == null) return 0;

            var tier = recipe.GetCraftingTier(player);
            if (tier > XUiM_Recipes.CraftingMaxTier)
                tier = XUiM_Recipes.CraftingMaxTier;

            return tier;
        }
    }
}
