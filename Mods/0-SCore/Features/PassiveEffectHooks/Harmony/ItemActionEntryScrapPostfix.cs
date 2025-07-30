using HarmonyLib;
using UnityEngine;

namespace SCore.Features.PassiveEffectHooks.Harmony
{
    [HarmonyPatch(typeof(ItemActionEntryScrap))]
    [HarmonyPatch(nameof(ItemActionEntryScrap.OnActivated))]
    public class ItemActionEntryScrapOnActivated
    {
        public static bool Prefix(ref ItemActionEntryScrap __instance)
        {
            var originalStack = (XUiC_ItemStack)__instance.ItemController;
            ((XUiC_ItemStack)__instance.ItemController).ItemStack.itemValue.SetMetadata("OriginalItemName", originalStack.itemClass.GetItemName(), TypedMetadataValue.TypeTag.String);
            return true;
        }
    }
}