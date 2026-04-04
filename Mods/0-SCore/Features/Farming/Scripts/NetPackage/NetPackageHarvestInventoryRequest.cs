using System.IO;

/// <summary>
/// Client → Server: request the harvest inventory for a trader-type NPC.
/// The server responds with NetPackageHarvestInventoryData.
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

        // Runs on the SERVER.
        // Serialise current harvest contents and send to the requesting client.
        // We do NOT clear the container here — the client will send back whatever
        // items the player didn't take via NetPackageHarvestInventoryUpdate when the
        // loot window closes, keeping the server-side container accurate.
        var container  = HarvestManager.GetOrCreate(_npcEntityId);
        var serialized = EntitySyncUtils.SerializeItemStackArray(container.items);

        SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
            NetPackageManager.GetPackage<NetPackageHarvestInventoryData>()
                .Setup(_npcEntityId, serialized),
            false, _playerId);
    }

    public override int GetLength() => 8;
}
