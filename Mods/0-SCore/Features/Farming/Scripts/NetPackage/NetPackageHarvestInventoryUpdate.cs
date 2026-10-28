/// <summary>
/// Client → Server: sends the items remaining in the harvest loot window after the player
/// closes it. Sent by the XUiC_LootWindowGroup_OnClose Harmony patch on dedicated-server clients.
/// The server applies it through HarvestManager.EndRemoteSession, which keeps any change the
/// server made while the window was open.
/// </summary>
public class NetPackageHarvestInventoryUpdate : NetPackage
{
    private int    _npcEntityId;
    private string _serializedItems;

    public NetPackageHarvestInventoryUpdate Setup(int npcEntityId, string serializedItems)
    {
        _npcEntityId     = npcEntityId;
        _serializedItems = serializedItems;
        return this;
    }

    public override void write(PooledBinaryWriter bw)
    {
        base.write(bw);
        bw.Write(_npcEntityId);
        bw.Write(_serializedItems ?? string.Empty);
    }

    public override void read(PooledBinaryReader br)
    {
        _npcEntityId     = br.ReadInt32();
        _serializedItems = br.ReadString();
    }

    public override void ProcessPackage(World world, GameManager callbacks)
    {
        if (world == null) return;
        if (Sender == null) return;

        // Runs on the SERVER. Items the player took are already in their inventory.
        var remaining = EntitySyncUtils.DeserializeItemStackArray(_serializedItems);
        HarvestManager.EndRemoteSession(_npcEntityId, Sender.entityId, remaining);
        HarvestManager.Save();
    }

}
