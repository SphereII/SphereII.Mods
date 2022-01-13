using HarmonyLib;
namespace Harmony.PlayerFeatures
{

    //[HarmonyPatch(typeof(GameManager))]
    //[HarmonyPatch("PlayerDisconnected")]
    //public class DespawnHiredNPCs
    //{
    //    private static bool Prefix(ClientInfo _cInfo)
    //    {
    //        if (_cInfo.entityId != -1)
    //        {
    //            EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(_cInfo.entityId) as EntityPlayer;
    //            if ( entityPlayer != null)
    //            {
    //                EntityUtilities.Despawn(entityPlayer.entityId);
    //            }
          
    //        }
    //        return true;
          
    //    }
    //}

    [HarmonyPatch(typeof(GameManager))]
    [HarmonyPatch("PlayerSpawnedInWorld")]
    public class TeleportNPCs
    {
        private static void Postfix(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos, int _entityId)
        {
            Entity entity;
            if (!GameManager.Instance.World.Entities.dict.TryGetValue(_entityId, out entity)) return;
            EntityPlayer entityPlayer = entity as EntityPlayer;
            if (entityPlayer == null) return;

            // Check if the player has any hired NPCs to respawn with it.
            EntityUtilities.Respawn(entityPlayer.entityId, _respawnReason);
        }
    }

    [HarmonyPatch(typeof(EntityVehicle))]
    [HarmonyPatch("DetachEntity")]
    public class EntityVehicle_DetactEntity
    {
        private static void Postfix(Entity _entity)
        {
            Entity entity;
            if (!GameManager.Instance.World.Entities.dict.TryGetValue(_entity.entityId, out entity)) return;
            EntityPlayer entityPlayer = entity as EntityPlayer;
            if (entityPlayer == null) return;

            // Check if the player has any hired NPCs to respawn with it.
            EntityUtilities.Respawn(entityPlayer.entityId, RespawnType.Teleport);
        }
    }
}