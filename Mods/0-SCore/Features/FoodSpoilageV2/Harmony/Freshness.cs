using HarmonyLib;
using UnityEngine; // For Mathf

namespace SphereII.FoodSpoilage.HarmonyPatches
{
    /// <summary>
    /// Harmony patches related to applying freshness effects, e.g., when consuming food.
    /// </summary>
    public class FreshnessPatches
    {
        [HarmonyPatch(typeof(MinEventActionModifyCVar))]
        [HarmonyPatch("Execute")]
        public class MinEventActionModifyCVarExecutePatch
        {
            private static void Postfix(MinEventActionModifyCVar __instance, MinEventParams _params)
            {
                 // Check if the core feature is enabled
                if (!SpoilageConfig.IsFoodSpoilageEnabled) return;

                // Ensure we have valid parameters and item data
                if (_params?.ItemValue == null || _params.ItemValue.IsEmpty()) return;
                 if (_params.ItemActionData is not ItemActionEat.MyInventoryData) return; // Only care about eat actions for now?

                // Check if the event is adding a value (we only boost additions)
                if (__instance.operation != CVarOperation.add) return;

                var itemValue = _params.ItemValue;
                var itemClass = itemValue.ItemClass;
                if (itemClass == null) return;

                // Check if this item provides a freshness bonus
                 if (!itemClass.Properties.GetBool(SpoilageConstants.PropFreshnessBonus)) return;

                // Check if the CVar being modified is allowed for this item's freshness bonus
                 string allowedCVars = itemClass.Properties.GetString(SpoilageConstants.PropFreshnessCVar);
                 if (allowedCVars.Equals(SpoilageConstants.NoneString, System.StringComparison.OrdinalIgnoreCase)) return; // Explicitly none
                 if (!allowedCVars.Equals(SpoilageConstants.AllString, System.StringComparison.OrdinalIgnoreCase) && // Not 'all'
                     !allowedCVars.Contains(__instance.cvarName)) // And not in the allowed list
                 {
                     return;
                 }

                // Get freshness value (0.0 to 1.0)
                 float freshness = 0f;
                 if (itemValue.HasMetadata(SpoilageConstants.MetaFreshness))
                 {
                     if(itemValue.GetMetadata(SpoilageConstants.MetaFreshness) is float f)
                        freshness = f;
                 }
                freshness = Mathf.Clamp01(freshness); // Ensure it's within 0-1 range

                 // Calculate multiplier: 1.0 at 0% fresh, up to 2.0 at 100% fresh (linear scale)
                 // Adjust this formula if a different scaling is desired.
                 float multiplier = 1.0f + freshness;

                 // Apply the multiplied bonus
                 // Note: The original event already executed once. This Postfix ADDS the BONUS portion.
                 // We need to calculate the original value added and then add the extra based on freshness.
                 float originalValue = __instance.value; // Value specified in the XML event action
                 float bonusValue = originalValue * freshness; // Bonus is the original value scaled by freshness (0% to 100%)

                if (bonusValue > 0.01f) // Only apply if the bonus is significant
                {
                    foreach (var target in __instance.targets)
                    {
                        if (target == null) continue;
                        try
                        {
                            // Add ONLY the calculated bonus amount
                             float currentValue = target.Buffs.GetCustomVar(__instance.cvarName);
                             target.Buffs.SetCustomVar(__instance.cvarName, currentValue + bonusValue,
                                 (target.isEntityRemote && !_params.Self.isEntityRemote) || _params.IsLocal); // Network sync logic from original

                            if(AdvLogging.LogEnabled(SpoilageConstants.AdvFeatureClass))
                                AdvLogging.DisplayLog(SpoilageConstants.AdvFeatureClass, $"Applied Freshness Bonus: CVar={__instance.cvarName}, Item={itemClass.GetItemName()}, Freshness={freshness:P1}, OriginalAdd={originalValue}, BonusAdd={bonusValue:F2}");

                        }
                        catch (System.Exception ex)
                        {
                            Log.Error($"Exception applying freshness bonus to target {target.entityId} for CVar {__instance.cvarName}: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}