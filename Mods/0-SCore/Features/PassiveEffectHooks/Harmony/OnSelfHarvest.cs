using HarmonyLib;
using UnityEngine;

namespace SCore.Features.PassiveEffectHooks
{
    [HarmonyPatch(typeof(ItemActionDynamicMelee))]
    [HarmonyPatch(nameof(ItemActionDynamicMelee.ExecuteAction))]
    public class ItemActionDynamicMeleeExecuteAction
    {
        public static void Postfix(ItemActionDynamicMelee __instance, ItemActionData _actionData)
        {
            if (_actionData is not ItemActionDynamicMelee.ItemActionDynamicMeleeData itemActionDynamicMeleeData) return;
            if (!itemActionDynamicMeleeData.IsHarvesting) return;
            if (Voxel.voxelRayHitInfo == null) return;
            if (!Voxel.voxelRayHitInfo.bHitValid) return;
            if (Voxel.voxelRayHitInfo.tag == null) return;
            if (GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag))
            {
                // Gore blocks
                _actionData.invData.holdingEntity.FireEvent(MinEventTypes.onSelfHarvestBlock);
                return;
            }
            // Dead Animals
            _actionData.invData.holdingEntity.FireEvent(MinEventTypes.onSelfHarvestOther);
        }
    }
}