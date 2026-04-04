using System.IO;

public class NetPackagePortalAddPosition : NetPackage
{
    private Vector3i _position;
    private string _name;

    public NetPackagePortalAddPosition Setup(Vector3i position, string name)
    {
        _position = position;
        _name = name;
        return this;
    }

    public override void write(PooledBinaryWriter _bw)
    {
        base.write(_bw);
        _bw.Write(_position.x);
        _bw.Write(_position.y);
        _bw.Write(_position.z);
        _bw.Write(_name);
    }

    public override void read(PooledBinaryReader _reader)
    {
        _position = new Vector3i(_reader.ReadInt32(), _reader.ReadInt32(), _reader.ReadInt32());
        _name = _reader.ReadString();
    }

    public override void ProcessPackage(World _world, GameManager _gameManager)
    {
        var cm = SingletonMonoBehaviour<ConnectionManager>.Instance;

        // Always update the local map directly — do NOT call AddPosition, which routes
        // back to the server on clients and creates an infinite packet loop.
        PortalManager.Instance.AddEntry(_position, _name);

        if (cm.IsServer)
        {
            // Server received this from a client: broadcast to all clients and persist.
            cm.SendPackage(NetPackageManager.GetPackage<NetPackagePortalAddPosition>().Setup(_position, _name));
            PortalManager.Instance.Save();
        }
    }

    public override int GetLength()
    {
        return 20;
    }
}
