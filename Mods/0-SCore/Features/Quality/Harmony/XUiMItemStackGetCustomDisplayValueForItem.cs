using HarmonyLib;

namespace SCore.Features.Quality.Harmony
{
    /// <summary>
    /// Patches XUiM_ItemStack.GetCustomDisplayValueForItem to fix display_value tier lookups
    /// when using extended quality ranges (1-600 instead of vanilla 1-6).
    ///
    /// The vanilla method calls GetValue((int)itemValue.Quality), which uses quality as a
    /// direct array index (quality - 1). With raw quality 1-600 this produces an out-of-bounds
    /// index on a 6-element value array and always returns 0, so item tooltips show
    /// the wrong value for any display_value that uses a tier list.
    ///
    /// The fix mirrors what MinEventModifyCVar.cs already does for triggered effects:
    /// convert the raw quality to a tier (1-6) via QualityUtils.CalculateTier() before
    /// passing it to GetValue().
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

            // Convert raw quality (e.g. 350) to a vanilla-compatible tier (1-6).
            // GetValue() treats the argument as an array index (tier - 1), so passing
            // the raw quality would read far out of bounds and return 0.
            int tier = QualityUtils.CalculateTier((int)itemValue.Quality);

            // For non-quality items quality == 0 → tier == 0; GetValue(0) would be index -1.
            // Let vanilla handle those.
            if (tier == 0) return true;

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
                    // Use tier (1-6) instead of raw quality (1-600)
                    newValue += minEffectGroup.EffectDisplayValues[entry.CustomName].GetValue(tier);
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
