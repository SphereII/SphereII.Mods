
using HarmonyLib;
using UnityEngine;
using SCore.Features.ItemDegradation.Harmony;

[HarmonyPatch(typeof(ItemClass))]
[HarmonyPatch(nameof(ItemClass.GetIconTint))]
public class ItemClassGetIconTint
{
    public static void Postfix(ref Color __result, ItemValue _instance, ItemClass __instance)
    {
        if (_instance == null) return;
        if (!ItemDegradationHelpers.IsDegraded(_instance)) return;

        if (__instance.Properties.Contains("BrokenTint"))
        {
            var colourString = __instance.Properties.GetStringValue("BrokenTint");
            var tint = StringParsers.ParseVector3i(colourString);
            __result = new Color( tint.x, tint.y, tint.z);
            return;
        }
        // Default tint.
        __result = new Color(210,0,0);

    }
}
