//using HarmonyLib;
//using UnityEngine;

//namespace SCore.Harmony.ItemActions
//{

//    //public class ItemActionMelee
//    //{
      

//        // Adds new feature where Durability affects the damage a weapon can do.
//        [HarmonyPatch(typeof(ItemActionMelee))]
//        [HarmonyPatch("GetExecuteActionTarget")]
//        public class ItemActionHit_GetExecuteActionTarget
//        {
//            public class InventoryDataMelee : ItemActionAttackData
//            {
//                public InventoryDataMelee(ItemInventoryData _invData, int _indexInEntityOfAction) : base(_invData, _indexInEntityOfAction)
//                {
//                }

//                public bool bAttackStarted;

//                public Ray ray;

//                public bool bHarvesting;

//                public bool bFirstHitInARow;
//            }

//            public static void Postfix(ref WorldRayHitInfo __result, ref ItemActionData _actionData, float ___Range, float ___BlockRange)
//            {
//                Log.Out("GetExecuteActionTarget()");
//                InventoryDataMelee inventoryDataMelee = (InventoryDataMelee)_actionData;
//                EntityAlive holdingEntity = inventoryDataMelee.invData.holdingEntity;
//                float distance = Utils.FastMax(___Range, ___BlockRange) + 0.15f;
//                if (holdingEntity.IsBreakingBlocks)
//                {
//                    Log.Out("Is Breaking Blocks.");
//                    Voxel.Raycast(inventoryDataMelee.invData.world, inventoryDataMelee.ray, distance, 1073807360, 128, 0.4f);
//                }

//                //return __result;

//            }
//        }
////    }
//}
