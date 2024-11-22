using HarmonyLib;

namespace Features.Fire.Harmony
{
    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("OnEntityWalking")]
    public class BlockOnEntityWalking
    {
        public static void Postfix(int _x, int _y, int _z, Entity entity)
        {
            if (FireManager.Instance == null) return;
           // FireManager.Instance.CheckForPlayer(entity);
            
            var blockPosition = new Vector3i(_x, _y, _z);
            
            if (!FireManager.IsBurning(blockPosition)) return;
            
            if ( !GameManager.IsDedicatedServer)
                if (!GameManager.Instance.HasBlockParticleEffect(blockPosition)) return;

            if (entity is not EntityAlive entityAlive) return;
            
            var buff = Configuration.GetPropertyValue("FireManagement", "BuffOnFire");
            if (!string.IsNullOrEmpty(buff))
            {
                entityAlive.Buffs.AddBuff(buff, -1, entityAlive.isEntityRemote);
            }
              //  entityAlive.Buffs.AddBuff(buff, entityAlive.entityId);
        }
    }
}