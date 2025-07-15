using HarmonyLib;

namespace Features.Fire.Harmony
{
    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("OnEntityWalking")]
    public class BlockOnEntityWalking
    {
        public static void Postfix(int _x, int _y, int _z, Entity entity)
        {
            if (FireManager.Instance == null || FireManager.Instance.Enabled == false) return;
            var blockPosition = new Vector3i(_x, _y, _z);
           
            // Check to see if the player is near fire for sounds
            FireManager.Instance.CheckForPlayer(entity);

            if (!FireManager.Instance.IsBurning(blockPosition)) return;
            
            if ( !GameManager.IsDedicatedServer)
                if (!GameManager.Instance.HasBlockParticleEffect(blockPosition)) return;

            if (entity is not EntityAlive entityAlive) return;
            
            var buff = Configuration.GetPropertyValue("FireManagement", "BuffOnFire");
            if (!string.IsNullOrEmpty(buff))
            {
                entityAlive.Buffs.AddBuff(buff, -1, entityAlive.isEntityRemote);
            }


        }
    }
}