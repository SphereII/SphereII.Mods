using UnityEngine;

/// <summary>
/// Server → Clients: tells each client to spawn a block-anchored particle effect via
/// SpawnBlockParticleEffect, which registers the particle in m_BlockParticles so that a
/// subsequent RemoveBlockParticleEffect (sent via NetPackageRemoveBlockParticleEffect) can
/// clean it up.
/// Used by BlockUtilitiesSDX.addParticlesCenteredServer on dedicated servers, replacing the
/// SpawnParticleEffectServer path which does not register particles in m_BlockParticles.
/// </summary>
public class NetPackageAddBlockParticleEffect : NetPackage
{
    private Vector3i _position;
    private string   _particleName;

    public NetPackageAddBlockParticleEffect Setup(Vector3i position, string particleName)
    {
        _position     = position;
        _particleName = particleName;
        return this;
    }

    public override void write(PooledBinaryWriter bw)
    {
        base.write(bw);
        bw.Write(_position.x);
        bw.Write(_position.y);
        bw.Write(_position.z);
        bw.Write(_particleName ?? string.Empty);
    }

    public override void read(PooledBinaryReader br)
    {
        _position     = new Vector3i(br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
        _particleName = br.ReadString();
    }

    public override void ProcessPackage(World world, GameManager callbacks)
    {
        if (world == null) return;
        if (GameManager.IsDedicatedServer) return;
        if (string.IsNullOrEmpty(_particleName) || _particleName == "NoParticle") return;

        if (!ParticleEffect.IsAvailable(_particleName))
        {
            if (ThreadManager.IsMainThread())
                ParticleEffect.LoadAsset(_particleName);
            else
            {
                Log.Warning($"[SCore] NetPackageAddBlockParticleEffect: cannot load '{_particleName}' off main thread.");
                return;
            }
        }

        if (GameManager.Instance.HasBlockParticleEffect(_position)) return;

        var centerPosition = EntityUtilities.CenterPosition(_position);
        var particle = new ParticleEffect(_particleName, centerPosition, Quaternion.identity, 1f, Color.white);
        GameManager.Instance.SpawnBlockParticleEffect(_position, particle);
    }

    public override int GetLength() => 12 + (_particleName?.Length ?? 0);
}
