//using HarmonyLib;

//namespace SCore.Harmony.Blocks
//{
//    public class ChangeThisWaterBlock
//    {
//        [HarmonyPatch(typeof(Block))]
//        [HarmonyPatch("OnBlockValueChanged")]
//        public class SCoreBlockLiquidV2_OnBlockAdded
//        {
//            public static bool Prefix(Block __instance, Vector3i _blockPos, BlockValue _newBlockValue)
//            {
//                if (FireManager.Instance.Enabled == false) return true;

//                if (FireManager.Instance.isBurning(_blockPos))
//                {
//                    Log.Out("Burning");
//                    Log.Out($"OnBlockValueChanged: {_newBlockValue.meta}");
//                }

//                return true;
//            }
//        }

//        [HarmonyPatch(typeof(Block))]
//        [HarmonyPatch("OnBlockLoaded")]
//        public class SCoreBlock_OnLoaded
//        {
//            public static bool Prefix(Block __instance, Vector3i _blockPos, BlockValue _blockValue)
//            {
//                if (FireManager.Instance.Enabled == false) return true;

//                if (_blockValue.Block is BlockDoor) return true;

//                if (_blockValue.meta == 10)
//                {
//                    Log.Out($"OnBlockLoaded: {_blockValue.meta}");
//                    FireManager.Instance.Add(_blockPos);
//                }
//                return true;
//            }
//        }

//        [HarmonyPatch(typeof(Block))]
//        [HarmonyPatch("OnBlockAdded")]
//        public class SCoreBlock_OnBlockAdded
//        {
//            public static bool Prefix(Block __instance, Vector3i _blockPos, BlockValue _blockValue)
//            {
//                if (FireManager.Instance.Enabled == false) return true;

//                if (_blockValue.Block is BlockDoor) return true;

//                if (_blockValue.meta == 10)
//                {
//                    Log.Out($"OnBlockAdded: {_blockValue.meta}");
//                    FireManager.Instance.Add(_blockPos);
//                }
//                return true;
//            }
//        }

//    }
//}
