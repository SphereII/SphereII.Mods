using System.IO;
using UnityEngine;

/// <summary>
/// Server → Client: delivers harvest inventory contents for a trader-type NPC so the client
/// can open a local loot window populated with the items.
/// The client stores the container reference in HarvestManager.ClientPendingContainer so that
/// XUiC_LootWindowGroup_OnClose can serialise remaining items and send them back to the server
/// via NetPackageHarvestInventoryUpdate when the player closes the window.
/// </summary>
public class NetPackageHarvestInventoryData : NetPackage
{
    private int    _npcEntityId;
    private string _serializedItems;

    public NetPackageHarvestInventoryData Setup(int npcEntityId, string serializedItems)
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

        // Runs on the CLIENT.
        var items = EntitySyncUtils.DeserializeItemStackArray(_serializedItems);

        // Build a local-only loot container with a valid chunk reference so the
        // looting window's OnOpen doesn't throw NullReferenceException.
        Chunk chunk = null;
        var entity = world.GetEntity(_npcEntityId);
        if (entity != null)
            chunk = world.GetChunkSync(
                World.toChunkXZ((int)entity.position.x),
                World.toChunkXZ((int)entity.position.z)) as Chunk;

        var container = new TileEntityLootContainer(chunk);
        container.entityId      = _npcEntityId;
        container.lootListName  = "traderNPC";
        container.SetContainerSize(new Vector2i(8, 8), true);
        container.bTouched      = true;   // suppress vanilla loot generation
        container.bPlayerStorage = true;  // suppress loot-respawn tick

        for (int i = 0; i < items.Length && i < container.items.Length; i++)
            container.items[i] = items[i];

        var playerLocal = world.GetPrimaryPlayer() as EntityPlayerLocal;
        if (playerLocal == null)
        {
            Log.Warning($"[0-SCore] NetPackageHarvestInventoryData: no local player found, cannot open loot window.");
            return;
        }

        // Store pending state BEFORE opening the window so the OnClose patch can
        // identify this container and send remaining items back to the server.
        HarvestManager.ClientPendingEntityId  = _npcEntityId;
        HarvestManager.ClientPendingContainer = container;

        EntityUtilities.OpenContainer(playerLocal, container);
    }

    public override int GetLength() => 4 + (_serializedItems?.Length ?? 0);
}
