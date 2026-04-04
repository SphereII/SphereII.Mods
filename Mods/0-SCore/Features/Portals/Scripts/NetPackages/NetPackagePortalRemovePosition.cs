using System.IO;

public class NetPackagePortalRemovePosition : NetPackage
{
    private Vector3i _position;

    public NetPackagePortalRemovePosition Setup(Vector3i position)
    {
        _position = position;
        return this;
    }

    public override void read(PooledBinaryReader _br)
    {
        _position = new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32());
    }

    public override void write(PooledBinaryWriter _bw)
    {
        base.write(_bw);
        _bw.Write(_position.x);
        _bw.Write(_position.y);
        _bw.Write(_position.z);
    }

    public override void ProcessPackage(World _world, GameManager _gameManager)
    {
        var cm = SingletonMonoBehaviour<ConnectionManager>.Instance;

        // Always update the local map directly — do NOT call RemovePosition, which routes
        // back to the server on clients and creates an infinite packet loop.
        PortalManager.Instance.RemoveEntry(_position);

        if (cm.IsServer)
        {
            // Server received this from a client: broadcast to all clients and persist.
            cm.SendPackage(NetPackageManager.GetPackage<NetPackagePortalRemovePosition>().Setup(_position));
            PortalManager.Instance.Save();
        }
    }

    public override int GetLength()
    {
        return 20;
    }
}
