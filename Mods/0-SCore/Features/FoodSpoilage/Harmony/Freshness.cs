using HarmonyLib;
using UnityEngine;

namespace SCore.Features.FoodSpoilage.Harmony {
    public class Freshness {
        private const string AdvFeatureClass = "FoodSpoilage";
        private const string Feature = "FoodSpoilage";

        private static readonly bool FoodSpoilage = Configuration.CheckFeatureStatus(AdvFeatureClass, Feature);

        [HarmonyPatch(typeof(MinEventActionModifyCVar))]
        [HarmonyPatch("Execute")]
        public class MinEventActionModifyCVarExecute {
            private static void Postfix(MinEventActionModifyCVar __instance, MinEventParams _params) {
                if (!FoodSpoilage) return;
                
             
                // No item action?
                if (_params.ItemActionData is not ItemActionEat.MyInventoryData actionData) return;
                // Not an add?
                if (__instance.operation != MinEventActionModifyCVar.OperationTypes.add) return;

                // Check to see if we need to re-run for freshness
                if (_params.ItemValue.IsEmpty()) return;
                
                // If freshnessonly isn't on the item, skip it.
                var freshness = false;
                if (_params.ItemValue.ItemClass.Properties.Contains("FreshnessBonus"))
                    freshness = _params.ItemValue.ItemClass.Properties.GetBool("FreshnessBonus");
                if (!freshness) return;
                
                if (!_params.ItemValue.HasMetadata("Freshness")) return;
                var freshNess = (float)_params.ItemValue.GetMetadata("Freshness");
                if (freshNess < 0.1f) return;
                var multiplier = freshNess + 1f;

                if (_params.ItemValue.ItemClass.Properties.Contains("FreshnessCVar"))
                {
                    var approvedCVar = _params.ItemValue.ItemClass.Properties.GetString("FreshnessCVar");
                    if (approvedCVar.Contains("none")) return;
                    if (approvedCVar != "all")
                    {
                        if (!approvedCVar.Contains(__instance.cvarName))
                            return;
                    }
                }
                foreach (var t in __instance.targets)
                {
                    var currentValue = t.Buffs.GetCustomVar(__instance.cvarName);
                    switch (__instance.operation)
                    {
                        case MinEventActionModifyCVar.OperationTypes.add:
                            currentValue += __instance.value * multiplier;
                            break;
                    }

                    t.Buffs.SetCustomVar(__instance.cvarName, currentValue,
                        (t.isEntityRemote && !_params.Self.isEntityRemote) || _params.IsLocal);
                }
            }
        }
    }
}