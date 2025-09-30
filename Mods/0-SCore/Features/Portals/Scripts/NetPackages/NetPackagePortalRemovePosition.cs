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
        _bw.Write(_position.x);
        _bw.Write(_position.y);
        _bw.Write(_position.z);
    }

    public override void ProcessPackage(World _world, GameManager _gameManager)
    {
        PortalManager.Instance.RemovePosition(_position);
    }

    public override int GetLength()
    {
        return 20;
    }

}