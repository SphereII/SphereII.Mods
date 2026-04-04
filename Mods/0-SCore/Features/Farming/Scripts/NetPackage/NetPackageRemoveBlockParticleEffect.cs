/// <summary>
/// Server → Clients: tells each client to call RemoveBlockParticleEffect at the given position.
/// Used by BlockUtilitiesSDX.removeParticlesCenteredServer so the server can clean up particles
/// that were spawned via addParticlesCenteredServer / SpawnParticleEffectServer.
/// </summary>
public class NetPackageRemoveBlockParticleEffect : NetPackage
{
    private Vector3i _position;

    public NetPackageRemoveBlockParticleEffect Setup(Vector3i position)
    {
        _position = position;
        return this;
    }

    public override void write(PooledBinaryWriter bw)
    {
        base.write(bw);
        bw.Write(_position.x);
        bw.Write(_position.y);
        bw.Write(_position.z);
    }

    public override void read(PooledBinaryReader br)
    {
        _position = new Vector3i(br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
    }

    public override void ProcessPackage(World world, GameManager callbacks)
    {
        if (world == null) return;
        if (GameManager.IsDedicatedServer) return;

        GameManager.Instance.RemoveBlockParticleEffect(_position);
    }

    public override int GetLength() => 12;
}
