using System;
using UnityEngine;

public class NetPackageWeaponSwap : NetPackage
{
    private int entityId;
    private string item;

    public NetPackageWeaponSwap Setup(EntityAlive _entity, string _item)
    {
        if (_entity == null)
        {
            Log.Out("WeaponSwap Entity null");
            Log.Out(Environment.StackTrace);
            return null;
        }
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

        var entityAlive = _world.GetEntity(entityId) as EntityAliveSDX;
        if (!entityAlive) return;
        var itemValue = ItemClass.GetItem(item);
        entityAlive.UpdateWeapon(itemValue);

        // If you are the server, relay the swap out to the other clients.
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
                NetPackageManager.GetPackage<NetPackageWeaponSwap>().Setup(entityAlive, item), false, -1,
                base.Sender.entityId);
        }
    }

    public override int GetLength()
    {
        return 20;
    }
}