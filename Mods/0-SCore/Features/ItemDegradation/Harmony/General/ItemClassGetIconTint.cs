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
            var brokenTint = Configuration.GetPropertyValue(ItemDegradationHelpers.AdvFeatureClass, "BrokenTint");
            if (string.IsNullOrEmpty(brokenTint)) return;
            
            // Default tint
            var tintVector = StringParsers.ParseVector3i(brokenTint);
            if (itemClass.Properties.Contains("BrokenTint"))
            {
                tintVector = StringParsers.ParseVector3i(itemClass.Properties.GetStringValue("BrokenTint"));
            }

            if (tintVector == new Vector3i(255, 255, 255)) return;
            
            __result = new Color(tintVector.x, tintVector.y, tintVector.z);
        }
    }
}