public class NetPackageToggleSprinkler : NetPackage {
    private Vector3i _position;
    private bool _isEnabled;

    public NetPackageToggleSprinkler Setup(Vector3i position, bool isEnabled) {
        _position = position;
        _isEnabled = isEnabled;
        return this;
    }

    public override void read(PooledBinaryReader br) {
        _position = new Vector3i(br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
        _isEnabled = br.ReadBoolean();
    }

    public override void write(PooledBinaryWriter bw) {
        base.write(bw);
        bw.Write(_position.x);
        bw.Write(_position.y);
        bw.Write(_position.z);
        bw.Write(_isEnabled);
    }

    public override int GetLength() {
        return 20;
    }

    public override void ProcessPackage(World world, GameManager callbacks) {
        if (world == null) return;
        var block = world.GetBlock(_position);
        if (block.Block is BlockWaterSourceSDX waterSourceSdx)
        {
            waterSourceSdx.ToggleSprinkler(_position, _isEnabled);
        }
    }
}