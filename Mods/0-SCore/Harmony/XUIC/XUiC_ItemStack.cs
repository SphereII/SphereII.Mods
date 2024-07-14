using HarmonyLib;

namespace Harmony.XUiC
{
    public class XUiCItemStack
    {
        [HarmonyPatch(typeof(XUiC_ItemStack))]
        [HarmonyPatch("ItemNameText", MethodType.Getter)]
        public class XUiCItemStackItemNameText
        {
            public static bool Prefix(ref string __result, ItemStack ___itemStack)
            {
                if (___itemStack.itemValue.GetMetadata("NPCName") is not string itemName) return true;
                
                __result = itemName;
                return false;
                
            }
        }
    }
}