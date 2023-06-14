namespace SCore.Scripts.NetPackage
{
    public class NetPackageEntityAliveSDXCollect : NetPackageEntityCollect
    {
        private int entityId;
        private int playerId;
        public override void ProcessPackage(World _world, GameManager _callbacks)
        {
            if (_world == null)
            {
                return;
            }
            CollectEntityServer(entityId, playerId);

        }
        
        private void CollectEntityServer(int _entityId, int _playerId)
        {
            if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            {
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityAliveSDXCollect>().Setup(_entityId, _playerId), false);
                return;
            }
            var entity = GameManager.Instance.World.GetEntity(_entityId);
            if (GameManager.Instance.World.IsLocalPlayer(_playerId))
            {
                var myEntity = GameManager.Instance.World.GetEntity(_entityId) as EntityAliveSDX;
                if (myEntity == null) return;
                myEntity.Collect(_playerId);
            }
            else
            {
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAliveSDXCollect>().Setup(_entityId, _playerId), false, _playerId, -1, -1, -1);
            }
            GameManager.Instance.World.RemoveEntity(entity.entityId, EnumRemoveEntityReason.Killed);
        }
    }
}