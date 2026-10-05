public class NetPackageEntityAliveSDXCollect : NetPackage
{
    private int entityId;
    private int playerId;

    public NetPackageEntityAliveSDXCollect Setup(int _entityId, int _playerId)
    {
        this.entityId = _entityId;
        this.playerId = _playerId;
        return this;
    }

    public override void read(PooledBinaryReader _br)
    {
        this.entityId = _br.ReadInt32();
        this.playerId = _br.ReadInt32();
    }

    public override void write(PooledBinaryWriter _bw)
    {
        base.write(_bw);
        _bw.Write(this.entityId);
        _bw.Write(this.playerId);
    }

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        if (_world == null)
        {
            return;
        }
        if (!base.ValidEntityIdForSender(this.playerId, false))
        {
            return;
        }
        if (!_world.IsRemote())
        {
            EntitySyncUtils.Collect(this.entityId, this.playerId);
            return;
        }
        EntitySyncUtils.CollectClient(this.entityId, this.playerId);
    }

}