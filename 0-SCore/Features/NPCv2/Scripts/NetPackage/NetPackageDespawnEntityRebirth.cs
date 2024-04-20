public class NetPackageDespawnEntityRebirth : NetPackage
{
    private int entityToUpdate;

    public NetPackageDespawnEntityRebirth Setup(int _entityToUpdate)
    {
        this.entityToUpdate = _entityToUpdate;
        return this;
    }

    public override void read(PooledBinaryReader _br)
    {
        this.entityToUpdate = _br.ReadInt32();
    }

    public override void write(PooledBinaryWriter _bw)
    {
        base.write(_bw);
        _bw.Write(this.entityToUpdate);
    }

    public override int GetLength()
    {
        return 2;
    }

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        if (_world == null)
        {
            return;
        }

        Entity myEntity = _world.GetEntity(this.entityToUpdate);
        if (myEntity != null)
        {
            EntityAlive entity = (EntityAlive)myEntity;

            if (entity != null)
            {
                entity.bWillRespawn = false;
                GameManager.Instance.World.RemoveEntity(entity.entityId, EnumRemoveEntityReason.Despawned);
            }
        }
    }
}

