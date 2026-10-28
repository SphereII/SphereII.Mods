using System.IO;

/// <summary>
/// Client → Server: request the harvest inventory for a trader-type NPC.
/// The server responds with NetPackageHarvestInventoryData: the contents, or a refusal when
/// someone else has the container open.
/// </summary>
public class NetPackageHarvestInventoryRequest : NetPackage
{
    private int _npcEntityId;
    private int _playerId;

    public NetPackageHarvestInventoryRequest Setup(int npcEntityId, int playerId)
    {
        _npcEntityId = npcEntityId;
        _playerId    = playerId;
        return this;
    }

    public override void write(PooledBinaryWriter bw)
    {
        base.write(bw);
        bw.Write(_npcEntityId);
        bw.Write(_playerId);
    }

    public override void read(PooledBinaryReader br)
    {
        _npcEntityId = br.ReadInt32();
        _playerId    = br.ReadInt32();
    }

    public override void ProcessPackage(World world, GameManager callbacks)
    {
        if (world == null) return;

        // Runs on the SERVER. Trust the connection, not the payload, for who is asking.
        var playerId = Sender?.entityId ?? _playerId;
        var reply    = NetPackageManager.GetPackage<NetPackageHarvestInventoryData>();

        // One copy out at a time. A second copy would let two players take the same items, or
        // let one player's close put back what the other took.
        if (HarvestManager.IsOpenByOther(_npcEntityId, playerId))
        {
            reply.SetupInUse(_npcEntityId);
        }
        else
        {
            // We do NOT clear the container here. The client sends back what the player left
            // when the window closes, and HarvestManager merges it against this snapshot.
            var container = HarvestManager.GetOrCreate(_npcEntityId);
            HarvestManager.BeginRemoteSession(_npcEntityId, playerId);
            reply.Setup(_npcEntityId, EntitySyncUtils.SerializeItemStackArray(container.items));
        }

        SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(reply, false, playerId);
    }

}
