//using HarmonyLib;

//namespace SCore.Harmony.Blocks
//{
//    public class OnEntityWalkingPatchFire
//    {
//        [HarmonyPatch(typeof(Block))]
//        [HarmonyPatch("OnEntityWalking")]
//        public class SCoreBlock_OnEntityWalking
//        {
//            public static void Postfix(Block __instance, WorldBase _world, int _x, int _y, int _z, BlockValue _blockValue, Entity entity)
//            {

//                if (entity == null) return;
//                var entityAlive = entity as EntityAlive;
//                if (entityAlive == null) return;

//                var pos = new Vector3i(_x, _y, _z);
//                if (FireManager.Instance.isBurning(pos) && GameManager.Instance.HasBlockParticleEffect(pos))
//                {
//                    var buff = Configuration.GetPropertyValue("FireManagement", "BuffOnFire");
//                    if (!string.IsNullOrEmpty(buff))
//                    {
//                        entityAlive.Buffs.AddBuff(buff);
//                    }
//                }

//                return;
//            }
//        }


//    }
//}
