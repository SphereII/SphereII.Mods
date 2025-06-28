// using System.Collections.Generic;
// using HarmonyLib;
//
// public class ItemValueClonePatch
// {
//     [HarmonyPatch(typeof(ItemValue))]
//     [HarmonyPatch("Clone")]
//     public class ItemValueClone {
//         private static void Postfix(ref ItemValue __result, ItemValue __instance) {
//             if (!SpoilageConfig.IsEnabled || !SpoilageConfig.UseAlternateItemValue) return;
//
//             if (__instance.ItemClass == null || !__instance.ItemClass.Properties.Contains(SpoilageConstants.PROP_SPOILABLE) ||
//                 !__instance.ItemClass.Properties.GetBool(SpoilageConstants.PROP_SPOILABLE))
//                 return;
//
//             if (__instance.Metadata == null) return;
//             __result.Metadata = new Dictionary<string, TypedMetadataValue>();
//             foreach (var text in __instance.Metadata.Keys)
//             {
//                 __result.SetMetadata(text, __instance.Metadata[text].Clone());
//             }
//         }
//     }
// }
