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
        _name = _reader.ReadString();    }

    public override void ProcessPackage(World _world, GameManager _gameManager)
    {
        PortalManager.Instance.AddPosition(_position, _name); // true to indicate it came from the network
    }

    public override int GetLength()
    {
        return 20;
    }

}