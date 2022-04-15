//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using HarmonyLib;

//namespace SCore.Harmony.Blocks
//{



//    [HarmonyPatch(typeof(BlockPlant))]
//    [HarmonyPatch("CheckPlantAlive")]
//    public class BlockPlantGrowingWaterCheck
//    {

//        public static bool IsByWater(Vector3i _blockPos, int range = 5)
//        {
//            var _world = GameManager.Instance.World;
//            for (int x = -range; x < range; x++)
//            {
//                for (int z = -range; z < range; z++)
//                {
//                    var waterCheck = _blockPos;
//                    waterCheck.x += x;
//                    waterCheck.z += z;
//                    if (_world.IsWater(waterCheck))
//                    {
//                        Log.Out($"Checking for water {waterCheck} from master {_blockPos}: Water Found");

//                                                return true;
//                    }
//                }

//            }
//            return false;
//            // return _world.IsWater(_blockPos.x, _blockPos.y + 1, _blockPos.z) | _world.IsWater(_blockPos.x + 1, _blockPos.y, _blockPos.z) | _world.IsWater(_blockPos.x - 1, _blockPos.y, _blockPos.z) | _world.IsWater(_blockPos.x, _blockPos.y, _blockPos.z + 1) | _world.IsWater(_blockPos.x, _blockPos.y, _blockPos.z - 1);
//        }

//        public static bool Prefix(BlockPlant __instance, ref bool __result, Vector3i _blockPos)
//        {
//            __result = true;
//            if (__instance.Properties.Contains("RequireWater"))
//            {
//                var underBlock = GameManager.Instance.World.GetBlock(_blockPos + Vector3i.down);
//                if (IsByWater(_blockPos + Vector3i.down))
//                {
//                    Log.Out($"{__instance.GetBlockName()}: Is By Water");
//                    return true;
//                }
//                Log.Out($"{__instance.GetBlockName()} is NOT by water.");
//                __result = false;
//                return false;
//            }

//            return true;
//        }
//    }
//}
