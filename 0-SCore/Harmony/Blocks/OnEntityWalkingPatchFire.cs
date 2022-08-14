//using HarmonyLib;

//namespace SCore.Harmony.Blocks
//{
//    public class OnEntityWalkingPatchFire
//    {
//        [HarmonyPatch(typeof(Block))]
//        [HarmonyPatch("OnEntityWalking")]
//        public class SCoreBlock_OnEntityWalking
//        {
//            public static bool Prefix(Block __instance, WorldBase _world, int _x, int _y, int _z, BlockValue _blockValue, Entity entity)
//            {
//                Log.Out("OnEntityWalking");
//                if (FireManager.Instance.Enabled == false) return true;

//                var entityAlive = entity as EntityAlive;
//                if (entityAlive == null) return true;
//                if (entityAlive is EntityVehicle) return true;

//                var pos = new Vector3i(_x, _y, _z);
//                Log.Out($"Is Position Burning: {FireManager.Instance.isBurning(pos)} {_blockValue.Block.GetBlockName()}");
//                if (FireManager.Instance.isBurning(pos))
//                {
//                    Log.Out("Position is burning.");
//                    var buff = Configuration.GetPropertyValue("FireManagement", "BuffOnFire");
//                    if (!string.IsNullOrEmpty(buff))
//                    {
//                        Log.Out($"Adding buff to {entityAlive.entityId}");
//                        entityAlive.Buffs.AddBuffNetwork(buff);
//                    }
//                }

//                return true;
//            }
//        }

     
//    }
//}
