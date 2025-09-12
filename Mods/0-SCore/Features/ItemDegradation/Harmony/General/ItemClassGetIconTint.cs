using HarmonyLib;
using SCore.Features.ItemDegradation.Utils;
using UnityEngine;

namespace SCore.Features.ItemDegradation.Harmony.General
{
    [HarmonyPatch(typeof(ItemClass))]
    [HarmonyPatch(nameof(ItemClass.GetIconTint))]
    public class ItemClassGetIconTint
    {
        public static void Postfix(ref Color __result, ItemValue _instance)
        {
            if (_instance == null || !ItemDegradationHelpers.IsDegraded(_instance)) return;
            var itemClass = _instance.ItemClass;
            var tintVector = new Vector3i(210, 0, 0) ;
            if (itemClass.Properties.Contains("BrokenTint"))
            {
                tintVector = StringParsers.ParseVector3i(itemClass.Properties.GetStringValue("BrokenTint"));
            }
            else
            {
                var brokenTint = Configuration.GetPropertyValue(ItemDegradationHelpers.AdvFeatureClass, "BrokenTint");
                // Default tint
                tintVector = string.IsNullOrEmpty(brokenTint) ? tintVector : StringParsers.ParseVector3i(brokenTint);
            }

            __result = new Color(tintVector.x, tintVector.y, tintVector.z);
        }
    }
}