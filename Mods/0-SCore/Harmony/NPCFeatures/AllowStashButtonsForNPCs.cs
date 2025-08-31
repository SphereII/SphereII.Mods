using System;
using HarmonyLib;

namespace Harmony.NPCFeatures
{
    [HarmonyPatch(typeof(XUiC_BackpackWindow))]
    [HarmonyPatch("GetBindingValueInternal")]
    public class xuic_Backpack_GetBindingValue
    {
        private static void Postfix(ref bool __result, XUiC_Backpack __instance, ref  string value, string bindingName)
        {
            if (bindingName == "lootingorvehiclestorage")
            {
                if (value == "true") return;
                if (__instance.xui.lootContainer == null) return;
                var entityId = __instance.xui.lootContainer.EntityId;
                if (entityId == -1) return;
                var owner = EntityUtilities.GetLeaderOrOwner(entityId);
                if ( owner == null) return; 
                if (owner.entityId != __instance.xui.mPlayerUI.entityPlayer.entityId) return;
                value = "true";
                __result = true;
            }
        }
    }
    
    [HarmonyPatch(typeof(XUiC_BackpackWindow))]
    [HarmonyPatch("TryGetMoveDestinationInventory")]
    public class xuic_Backpack_TryGetMoveDestinationInventory
    {
        private static bool Prefix(ref bool __result,XUiC_BackpackWindow __instance, out IInventory _dstInventory)
        {
            _dstInventory = null;
            if (__instance.xui.lootContainer == null) return true;
            var entityId = __instance.xui.lootContainer.EntityId;
            if (entityId == -1) return true;
            var playerEntity = EntityUtilities.GetLeaderOrOwner(entityId);
            if (playerEntity == null) return true;
            _dstInventory = __instance.xui.lootContainer;
            __result = true;
            return false;
        }
    }
}
