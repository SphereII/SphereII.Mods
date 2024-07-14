using UnityEngine;

public class NetPackageWeaponSwap : NetPackage
{
    private int entityId;
    private string item;

    public NetPackageWeaponSwap Setup(EntityAlive _entity, string _item)
    {
        entityId = _entity.entityId;
        item = _item;
        return this;
    }

    public override void read(PooledBinaryReader _reader)
    {
        entityId = _reader.ReadInt32();
        item = _reader.ReadString();
    }

    public override void write(PooledBinaryWriter _writer)
    {
        base.write(_writer);
        _writer.Write(entityId);
        _writer.Write(item);
    }

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        if (_world == null)
        {
            return;
        }

        // If you are the server, send it out to the clients.
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            var entity = _world.GetEntity(entityId) as EntityAlive;
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
                NetPackageManager.GetPackage<NetPackageWeaponSwap>().Setup(entity, item), false, -1,
                base.Sender.entityId);
            return;
        }

        var entityAlive = _world.GetEntity(entityId) as EntityAliveSDX;
        if (!entityAlive) return;
        var itemValue = ItemClass.GetItem(item);
        entityAlive.UpdateWeapon(itemValue);
    }

    public override int GetLength()
    {
        return 20;
    }
}