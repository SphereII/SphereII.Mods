using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Quality.Harmony
{
    /// <summary>
    /// Patches XUiM_ItemStack.GetCustomDisplayValueForItem so display_value tier lists work
    /// with extended quality ranges (e.g. 1-600 instead of vanilla 1-6).
    ///
    /// A display_value tier list can be written two ways:
    ///   - raw quality:  tier="1,100,200,300,400,500,600"
    ///   - colour tiers: tier="1,2,3,4,5,6"
    /// If the list goes above the highest colour tier it is read as raw quality, matching how
    /// passive_effect applies the value. Otherwise the quality is first converted to a 1-6 tier,
    /// matching the tierList handling in MinEventModifyCVar.cs.
    ///
    /// Vanilla EffectDisplayValue.GetValue only interpolates inside pairs of levels (1-100,
    /// 200-300, ...), so values between pairs read as 0. The lookup here uses every
    /// neighbouring range, the same way PassiveEffect.ModValue does.
    /// </summary>
    [HarmonyPatch(typeof(XUiM_ItemStack))]
    [HarmonyPatch(nameof(XUiM_ItemStack.GetCustomDisplayValueForItem))]
    public class XUiMItemStackGetCustomDisplayValueForItem
    {
        private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
        private static readonly string Feature = "CustomQualityLevels";

        public static bool Prefix(ref float __result, ItemValue itemValue, DisplayInfoEntry entry)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;

            // Guard: let vanilla handle null/empty items and items without a class.
            if (itemValue == null || itemValue.IsEmpty() || itemValue.ItemClass == null) return true;

            int quality = itemValue.Quality;
            int tier = QualityUtils.CalculateTier(quality);

            // Non-quality items have quality 0. Let vanilla handle those.
            if (quality <= 0) return true;

            float newValue = 0f;

            MinEffectController effects = itemValue.ItemClass.Effects;
            var list = effects?.EffectGroups;
            if (list == null)
            {
                __result = newValue;
                return false;
            }

            for (int i = 0; i < list.Count; i++)
            {
                MinEffectGroup minEffectGroup = list[i];
                MinEventParams.CachedEventParam.ItemValue = itemValue;
                MinEventParams.CachedEventParam.Seed = (int)itemValue.Seed;

                if (minEffectGroup.EffectDisplayValues.ContainsKey(entry.CustomName) &&
                    minEffectGroup.EffectDisplayValues[entry.CustomName].IsValid(MinEventParams.CachedEventParam))
                {
                    newValue += GetDisplayValue(minEffectGroup.EffectDisplayValues[entry.CustomName], quality, tier);
                }

                var primaryEffects = minEffectGroup.GetTriggeredEffects(MinEventTypes.onSelfPrimaryActionEnd);
                if (primaryEffects != null)
                    foreach (var action in primaryEffects)
                        AddValueIfValid(action, entry, ref newValue);

                var secondaryEffects = minEffectGroup.GetTriggeredEffects(MinEventTypes.onSelfSecondaryActionEnd);
                if (secondaryEffects != null)
                    foreach (var action in secondaryEffects)
                        AddValueIfValid(action, entry, ref newValue);
            }

            __result = newValue;
            return false;
        }

        private static float GetDisplayValue(EffectDisplayValue display, int quality, int tier)
        {
            var levels = display.Levels;
            var values = display.Values;

            // No tier list, or a shape we don't reinterpret (random ranges, single level).
            if (levels == null || values == null || levels.Length < 2 || levels.Length != values.Length)
                return display.GetValue(tier > 0 ? tier : quality);

            // A list that goes past the top colour tier is written in raw quality.
            var maxColourTier = QualityInfo.qualityColors.Length - 1;
            float level = levels[levels.Length - 1] > maxColourTier ? quality : tier;

            // Same range walk as PassiveEffect.ModValue: every neighbouring pair is a range.
            for (int i = levels.Length - 1; i > 0; i--)
            {
                if (level < levels[i - 1] || level > levels[i]) continue;
                var span = levels[i] - levels[i - 1];
                if (span <= 0f) return values[i];
                return Mathf.Lerp(values[i - 1], values[i], (level - levels[i - 1]) / span);
            }

            return 0f;
        }

        // Replicates the vanilla compiler-generated local function
        // <GetCustomDisplayValueForItem>g__AddValueForDisplayIfValid|15_0
        private static void AddValueIfValid(MinEventActionBase actionBase, DisplayInfoEntry entry, ref float newValue)
        {
            if (!(actionBase is MinEventActionModifyCVar modifyCVar)) return;
            if (!actionBase.CanExecute(actionBase.EventType, MinEventParams.CachedEventParam)) return;
            if (modifyCVar.cvarName == entry.CustomName &&
                modifyCVar.targetType == MinEventActionTargetedBase.TargetTypes.self)
            {
                newValue += modifyCVar.GetValueForDisplay();
            }
        }
    }
}
