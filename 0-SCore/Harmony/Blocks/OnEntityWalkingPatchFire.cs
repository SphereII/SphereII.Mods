using HarmonyLib;

namespace SCore.Harmony.Blocks
{
    public class OnEntityWalkingPatchFire
    {
        [HarmonyPatch(typeof(Block))]
        [HarmonyPatch("OnEntityWalking")]
        public class SCoreBlock_OnEntityWalking
        {
            public static bool Prefix(Block __instance, WorldBase _world, int _x, int _y, int _z, BlockValue _blockValue, Entity entity)
            {
                if (FireManager.Instance.Enabled == false) return true;

                var entityAlive = entity as EntityAlive;
                if (entityAlive == null) return true;
                if (entityAlive is EntityVehicle) return true;

                var pos = new Vector3i(_x, _y, _z);
                if (FireManager.Instance.isBurning(pos))
                {
                    var buff = Configuration.GetPropertyValue("FireManagement", "BuffOnFire");
                    if (!string.IsNullOrEmpty(buff))
                        entityAlive.Buffs.AddBuff(buff, -1, true, false, false);
                }

                return true;
            }
        }
    }
}
