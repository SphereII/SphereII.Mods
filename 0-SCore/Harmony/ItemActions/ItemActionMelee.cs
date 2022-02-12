//using HarmonyLib;
//using UnityEngine;
//using UAI;
//namespace SCore.Harmony.ItemActions
//{

//    //public class ItemActionMelee
//    //{


//    // Adds new feature where Durability affects the damage a weapon can do.
//    [HarmonyPatch(typeof(ItemActionMelee))]
//    [HarmonyPatch("GetExecuteActionTarget")]
//    public class ItemActionHit_GetExecuteActionTarget
//    {
//        public class InventoryDataMelee : ItemActionAttackData
//        {
//            public InventoryDataMelee(ItemInventoryData _invData, int _indexInEntityOfAction) : base(_invData, _indexInEntityOfAction)
//            {
//            }

//            public bool bAttackStarted;

//            public Ray ray;

//            public bool bHarvesting;

//            public bool bFirstHitInARow;
//        }

//        public static WorldRayHitInfo Postfix(WorldRayHitInfo __result, ref ItemActionData _actionData, float ___Range, float ___BlockRange)
//        {
//            Log.Out("GetExecuteActionTarget()");
//            EntityAlive holdingEntity = _actionData.invData.holdingEntity;
//            float distance = Utils.FastMax(___Range, ___BlockRange) + 0.15f;
//            if (holdingEntity.IsBreakingBlocks)
//            {
//                var ray = holdingEntity.GetLookRay();
//                Log.Out($"Is Breaking Blocks: {distance}");
//                var hitMask = SCoreUtils.GetHitMaskByWeaponBuff(holdingEntity);
//                var hit = Voxel.Raycast(_actionData.invData.world, ray, distance, 1073807360, 128, 20f);
//                if (hit)
//                {
//                    Log.Out($"I hit something! Distance {distance}");
//                }

//                for(float x = 0f; x< distance; x++)
//                {
//                    hit = Voxel.Raycast(_actionData.invData.world, ray, x, 1073807360, hitMask, 20f);
//                    if (hit)
//                    {
//                        Log.Out($"I hit something!  {x}");
//                    }

//                }

//                __result.ray = ray;
//            }

//            return __result;

//        }
//    }
//    //    }
//}
