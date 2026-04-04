/// <summary>
/// Client → Server: sends the items remaining in the harvest loot window after the player
/// closes it, so the server-side HarvestManager container stays accurate.
/// Sent by the XUiC_LootWindowGroup_OnClose Harmony patch on dedicated-server clients.
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

        // Runs on the SERVER.
        // Replace the server-side harvest container contents with whatever the player
        // left behind.  Items they took are already in their inventory via vanilla sync.
        var remaining  = EntitySyncUtils.DeserializeItemStackArray(_serializedItems);
        var container  = HarvestManager.GetOrCreate(_npcEntityId);

        for (int i = 0; i < container.items.Length; i++)
            container.items[i] = i < remaining.Length ? remaining[i] : ItemStack.Empty.Clone();

        HarvestManager.Save();
    }

    public override int GetLength() => 4 + (_serializedItems?.Length ?? 0);
}
