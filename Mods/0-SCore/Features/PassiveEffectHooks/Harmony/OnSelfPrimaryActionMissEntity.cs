using HarmonyLib;

namespace SCore.Features.PassiveEffectHooks
{
    [HarmonyPatch(typeof(ItemActionMelee))]
    [HarmonyPatch(nameof(ItemActionMelee.GetExecuteActionTarget))]
    public class ItemActionMeleeGetExecuteActionTarget
    {
        public static void Postfix(ref WorldRayHitInfo __result, ItemActionMelee __instance, ItemActionData _actionData)
        {
            var inventoryDataMelee = (ItemActionMelee.InventoryDataMelee)_actionData;
            if (inventoryDataMelee == null) return;
            if (inventoryDataMelee.HasExecuted) return;
            if (__result is not { bHitValid: true }) return;
            if (__result.tag == null || !__result.tag.StartsWith("E_") ||
                !(__result.hit.distanceSq > __instance.Range * __instance.Range))
            {
                return;
            }

            // Considered a miss.
            inventoryDataMelee.invData.holdingEntity.FireEvent(MinEventTypes.onSelfPrimaryActionEnd);
            inventoryDataMelee.invData.holdingEntity.FireEvent(MinEventTypes.onSelfPrimaryActionMissEntity);
        }
    }
    
}