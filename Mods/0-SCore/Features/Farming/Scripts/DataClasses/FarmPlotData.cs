using System;
using System.Collections.Generic;
// using System.Linq; // Not currently used
// using System.Text; // Not currently used
// using System.Threading.Tasks; // Not currently used
using UnityEngine;

// Represents data and actions for a single farm plot block.
public class FarmPlotData
{
    private readonly Vector3i _blockPos = Vector3i.zero; // Use readonly if set only in constructor
    public bool Visited { get; set; } = false; // Use private set if only modified internally

    public FarmPlotData(Vector3i blockPos)
    {
        this._blockPos = blockPos;
    }

    public Vector3i GetBlockPos()
    {
        return _blockPos;
    }

    /// <summary>
    /// Marks the plot as visited (e.g., by an NPC).
    /// Consider renaming if 'Visited' has multiple meanings.
    /// </summary>
    public void UpdateData() // Consider a more descriptive name like MarkVisited()
    {
        Visited = true;
    }

    /// <summary>
    /// Checks if the farm plot has access to water via pipes or nearby sources.
    /// Uses WaterPipeManager's cached lookup and PlantData's checks.
    /// </summary>
    public bool HasWater()
    {
        var plotPos = _blockPos + Vector3i.up;

        // Check pipe/direct water at plot level and one above
        if (WaterPipeManager.Instance.GetWaterForPosition(plotPos) != Vector3i.zero) return true;
        if (WaterPipeManager.Instance.GetWaterForPosition(_blockPos) != Vector3i.zero) return true;

        // Check if any active connected sprinkler covers this position.
        // Avoids creating a temp PlantData on an air block (which has no WaterRange property).
        foreach (var sprinklerPos in WaterPipeManager.Instance.GetWaterValves())
        {
            var bv = GameManager.Instance.World.GetBlock(sprinklerPos);
            if (bv.Block is not BlockWaterSourceSDX sprinkler) continue;

            float rangeSq = sprinkler.GetWaterRange() * sprinkler.GetWaterRange();
            if (Vector3.SqrMagnitude(plotPos - sprinklerPos) > rangeSq) continue;

            if (sprinkler.IsWaterSourceUnlimited() ||
                WaterPipeManager.Instance.GetWaterForPosition(sprinklerPos) != Vector3i.zero)
                return true;
        }

        // Vanilla-style fallback: scan for a direct water source within 4 blocks horizontally
        // at the plot's Y level (and one below), matching the vanilla game's farm-plot water
        // check. This ensures farm plots that are within vanilla watering range of a trough or
        // lake — but not immediately adjacent — are still recognised as watered by NPC farmers.
        for (var dx = -4; dx <= 4; dx++)
        {
            for (var dz = -4; dz <= 4; dz++)
            {
                if (WaterPipeManager.Instance.IsDirectWaterSource(new Vector3i(_blockPos.x + dx, _blockPos.y, _blockPos.z + dz))) return true;
                if (WaterPipeManager.Instance.IsDirectWaterSource(new Vector3i(_blockPos.x + dx, _blockPos.y - 1, _blockPos.z + dz))) return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if the block directly above the farm plot is air.
    /// </summary>
    public bool IsEmpty()
    {
        var blockAbove = GameManager.Instance.World.GetBlock(_blockPos + Vector3i.up);
        return blockAbove.isair;
    }

    /// <summary>
    /// Checks if the block above the farm plot is a non-plant block (assumed dead/wilted).
    /// </summary>
    public bool IsDeadPlant()
    {
        var blockAbove = GameManager.Instance.World.GetBlock(_blockPos + Vector3i.up);
        if (blockAbove.isair) return false; // Air isn't a dead plant
        if (blockAbove.Block is BlockPlant) return false; // Living plants are not dead plants
        // Any other non-air block is considered a dead plant in this context
        return true;
    }

    /// <summary>
    /// Checks if the farm plot has any plant (living or dead) on it.
    /// </summary>
    public bool HasPlant()
    {
        if (IsEmpty()) return false;

        // Check if CropManager knows about a plant at or above the plot
        // Note: CropManager uses _blockPos + Vector3i.up frequently for plants on plots.
        if (CropManager.Instance.GetPlant(_blockPos + Vector3i.up) != null) return true;
        // Maybe also check at _blockPos itself? Depends on where plants are registered.
        // if (CropManager.Instance.GetPlant(_blockPos) != null) return true;

        // If not registered, check if it's a dead plant block type
        if (IsDeadPlant()) return true;

        // Could add a check here: Is the block above a BlockPlant, even if not registered?
        // var blockAbove = GameManager.Instance.World.GetBlock(_blockPos + Vector3i.up);
        // if (blockAbove.Block is BlockPlant) return true;

        return false;
    }

    /// <summary>
    /// Resets the Visited flag.
    /// </summary>
    public void Reset()
    {
        Visited = false;
    }

  /// <summary>
/// Logic for an NPC to manage this farm plot (harvest, clear dead plants, replant).
/// Note: This method is complex and could be a performance consideration under heavy NPC load.
/// </summary>
/// <param name="entityAlive">The NPC entity managing the plot.</param>
/// <returns>List of harvested items, or null/empty list if no harvest occurred.</returns>
public List<Block.SItemDropProb> Manage(EntityAlive entityAlive)
{
    Visited = true;
    bool shouldReplant = false;
    var plantPos = _blockPos + Vector3i.up;
    var currentBlockValue = GameManager.Instance.World.GetBlock(plantPos);
    var harvestItems = new List<Block.SItemDropProb>();
    string seedFromPlant = string.Empty;

    // --- Handle existing plant/block ---
    // Check harvest drops FIRST — a block with harvest drops should always be harvested,
    // regardless of whether it extends BlockPlant. Some mods use custom block types (e.g.
    // BlockPickUpAndReplace) for the final crop stage, which are not BlockPlant subclasses
    // and would incorrectly be treated as dead plants if we checked IsDeadPlant() first.
    if (!IsEmpty() && currentBlockValue.Block != null)
    {
        bool hasHarvestDrops = currentBlockValue.Block.HasItemsToDropForEvent(EnumDropEvent.Harvest);

        if (hasHarvestDrops)
        {
            // Work with local copies of the drop lists so we never mutate the block's shared
            // internal itemsToDrop data (the lists returned by TryGetValue are the actual list
            // objects stored on the block singleton, not copies).
            currentBlockValue.Block.itemsToDrop.TryGetValue(EnumDropEvent.Harvest, out var rawHarvest);
            if (rawHarvest != null) harvestItems.AddRange(rawHarvest);

            // Also add destroy drops — these typically contain the seed.
            currentBlockValue.Block.itemsToDrop.TryGetValue(EnumDropEvent.Destroy, out var destroyItems);
            if (destroyItems != null) harvestItems.AddRange(destroyItems);

            // Process harvested items: strip invalid entries, find seed, adjust counts based on NPC cvars
            for (int i = harvestItems.Count - 1; i >= 0; i--)
            {
                var item = harvestItems[i];

                // Remove empty-name or zero-count entries — malformed drop table entries.
                if (string.IsNullOrEmpty(item.name) || item.maxCount <= 0)
                {
                    harvestItems.RemoveAt(i);
                    continue;
                }

                if (item.name.StartsWith("planted") && seedFromPlant == string.Empty)
                {
                    seedFromPlant = item.name;
                    harvestItems.RemoveAt(i);
                }
                else if (entityAlive.Buffs.HasCustomVar(item.name))
                {
                    item.minCount = (int)entityAlive.Buffs.GetCustomVar(item.name);
                    item.maxCount = (int)entityAlive.Buffs.GetCustomVar(item.name);
                    harvestItems[i] = item;
                }
            }

            currentBlockValue = BlockValue.Air;
            shouldReplant = true;
        }
        else if (IsDeadPlant())
        {
            currentBlockValue = BlockValue.Air;
            shouldReplant = true;
        }
        // else: growing plant — leave it alone
    }
    else // Plot is empty
    {
        shouldReplant = true;
    }

    // --- Handle Replanting ---
    if (shouldReplant)
    {
        if (!HasWater())
        {
            // No water — clear any harvested/dead block but don't replant.
            if (!IsEmpty())
                GameManager.Instance.World.SetBlockRPC(plantPos, BlockValue.Air);
            return harvestItems;
        }

        string seedToPlant = seedFromPlant; // Use seed from harvested plant first

        // If no seed from harvest, try to find one in NPC inventory.
        // Modifying a stack's count in-place is safe for both TileEntityLootContainer and
        // TileEntityTrader — it doesn't replace the items[] array reference, so it cannot
        // trigger the TileEntityTrader setter that would corrupt TraderData.PrimaryInventory.
        if (string.IsNullOrEmpty(seedToPlant))
        {
            ItemStack[] inventoryItems = null;
            TileEntityLootContainer containerToMark = null;

            if (entityAlive is EntityTrader && HarvestManager.Has(entityAlive.entityId))
            {
                containerToMark = HarvestManager.GetOrCreate(entityAlive.entityId);
                inventoryItems = containerToMark.items;
            }
            else if (entityAlive.lootContainer != null)
            {
                containerToMark = entityAlive.lootContainer;
                inventoryItems = containerToMark.items;
            }

            if (inventoryItems != null)
            {
                foreach (var stack in inventoryItems)
                {
                    if (!stack.IsEmpty())
                    {
                        var itemClass = ItemClass.GetForId(stack.itemValue.type);
                        if (itemClass.Name.StartsWith("planted") && itemClass.Name.EndsWith("1"))
                        {
                            seedToPlant = itemClass.Name;
                            stack.count--;
                            if (stack.count <= 0) stack.Clear();
                            containerToMark.SetModified();
                            break;
                        }
                    }
                }
            }
        }

        BlockValue blockToPlace = BlockValue.Air;
        if (!string.IsNullOrEmpty(seedToPlant))
        {
            var seedBlock = Block.GetBlockByName(seedToPlant);
            if (seedBlock != null)
            {
                // Skip CanPlaceBlockAt — the harvested block is still present in the world at this
                // point, so the check would always fail. The RPC below replaces it directly.
                blockToPlace = seedBlock.ToBlockValue();
            }
        }

        if (!IsEmpty() || !blockToPlace.isair)
        {
            // Single-entry RPC: replace the old block directly with the seed (or air if no seed).
            // A two-entry approach (air then seed at the same position) causes the seed to fall
            // through the world because the intermediate air triggers a physics/support check
            // before the seed placement can stabilise.
            GameManager.Instance.World.SetBlocksRPC(new List<BlockChangeInfo>
            {
                new BlockChangeInfo(plantPos, blockToPlace, false)
            });
        }
    }

    return harvestItems;
}
}