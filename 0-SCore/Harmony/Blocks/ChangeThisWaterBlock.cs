//using HarmonyLib;

//namespace SCore.Harmony.Blocks
//{
//    public class ChangeThisWaterBlock
//    {
//        [HarmonyPatch(typeof(Block))]
//        [HarmonyPatch("OnNeighborBlockChange")]
//        public class SCoreBlockLiquidV2_OnBlockAdded
//        {
//            public static bool Prefix(Block __instance, WorldBase world, int _clrIdx, Vector3i _myBlockPos, BlockValue _myBlockValue, Vector3i _blockPosThatChanged, BlockValue _newNeighborBlockValue, BlockValue _oldNeighborBlockValue)
//            {
//                if (FireManager.Instance.Enabled == false) return true;

//                if (FireManager.Instance.isBurning(_myBlockPos))
//                {
//                    if (_newNeighborBlockValue.isWater)
//                    {
//                        if ( !FireManager.ExtinguishPositions.Contains(_myBlockPos) )
//                            FireManager.ExtinguishPositions.Add(_myBlockPos);
//                    }
                        
//                }

//                return true;
//            }
//        }
//    }
//}
