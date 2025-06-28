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
        // Check directly above and at the plot level using the cached pipe manager lookup
        if (WaterPipeManager.Instance.GetWaterForPosition(_blockPos + Vector3i.up) != Vector3i.zero) return true;
        if (WaterPipeManager.Instance.GetWaterForPosition(_blockPos) != Vector3i.zero) return true;

        // Fallback: Use PlantData's more comprehensive check (includes sprinklers and local scan)
        // Note: Creates a temporary PlantData object, which is slightly inefficient but leverages full check.
        var tempPlant = new PlantData(_blockPos + Vector3i.up);
        return tempPlant.WaterPos != Vector3i.zero; // Check if the temp PlantData found water
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
        Visited = true; // Mark as visited for this cycle
        bool shouldReplant = false;
        var plantPos = _blockPos + Vector3i.up;
        var plantData = CropManager.Instance.GetPlant(plantPos);
        var currentBlockValue = GameManager.Instance.World.GetBlock(plantPos);
        var harvestItems = new List<Block.SItemDropProb>();
        string seedFromPlant = string.Empty; // Seed item name derived from the harvested plant

        // --- Handle existing plant/block ---
        if (IsDeadPlant()) // Check if it's a dead/wilted plant block
        {
            AdvLogging.DisplayLog("FarmPlotData", $"NPC {entityAlive.entityId} clearing dead plant at {plantPos}");
            // Clear the dead plant block
            var blockChangeInfos = new List<BlockChangeInfo>();
            var blockChangeInfo = new BlockChangeInfo(plantPos, BlockValue.Air, false);
            blockChangeInfos.Add(blockChangeInfo);
            GameManager.Instance.World.SetBlocksRPC(blockChangeInfos);
            
            currentBlockValue = BlockValue.Air; // Update local variable
            shouldReplant = true;
        }
        else if (!IsEmpty() && currentBlockValue.Block != null) // If there's a non-air block (might be a plant)
        {
            // Check if it's harvestable (fully grown)
            // A plant is harvestable if it doesn't have a 'PlantGrowing.Next' property, implying it's the final stage.
            bool isHarvestable = !currentBlockValue.Block.Properties.Contains("PlantGrowing.Next");

            if (isHarvestable)
            {
                 AdvLogging.DisplayLog("FarmPlotData", $"NPC {entityAlive.entityId} harvesting at {plantPos}");
                 // Try to get harvest drops
                currentBlockValue.Block.itemsToDrop.TryGetValue(EnumDropEvent.Harvest, out harvestItems);
                harvestItems ??= new List<Block.SItemDropProb>(); // Ensure list is not null

                // Also add destroy drops, as harvesting removes the block
                currentBlockValue.Block.itemsToDrop.TryGetValue(EnumDropEvent.Destroy, out var destroyItems);
                if (destroyItems != null) harvestItems.AddRange(destroyItems);

                // Process harvested items: find seed, adjust counts based on NPC cvars
                for (int i = harvestItems.Count - 1; i >= 0; i--)
                {
                    var item = harvestItems[i];
                    // Identify the seed item associated with this plant
                    if (item.name.StartsWith("planted") && seedFromPlant == string.Empty) // Take the first seed found
                    {
                        seedFromPlant = item.name;
                        harvestItems.RemoveAt(i); // Remove seed from harvest list
                    }
                    else if (entityAlive.Buffs.HasCustomVar(item.name)) // Adjust counts based on NPC skill/cvar
                    {
                        item.minCount = (int)entityAlive.Buffs.GetCustomVar(item.name);
                        item.maxCount = (int)entityAlive.Buffs.GetCustomVar(item.name);
                        harvestItems[i] = item; // Update struct in list
                    }
                }

                // Remove the harvested plant block
                var blockChangeInfos = new List<BlockChangeInfo>();
                var blockChangeInfo = new BlockChangeInfo(plantPos, BlockValue.Air, false);
                blockChangeInfos.Add(blockChangeInfo);
                GameManager.Instance.World.SetBlocksRPC(blockChangeInfos);
                
                currentBlockValue = BlockValue.Air;
                shouldReplant = true;
            }
            else if (plantData != null)
            {
                 // Plant exists but isn't fully grown, update its last check time?
                 // plantData.LastCheck = GameManager.Instance.World.GetWorldTime(); // Or maybe LastMaintained?
                 AdvLogging.DisplayLog("FarmPlotData", $"NPC {entityAlive.entityId} tending non-harvestable plant at {plantPos}");
                 // Perform other tending actions if needed (e.g., call plantData.Manage())
            }
        }
        else // Plot is empty
        {
            shouldReplant = true;
        }

        // --- Handle Replanting ---
        if (shouldReplant)
        {
             AdvLogging.DisplayLog("FarmPlotData", $"NPC {entityAlive.entityId} checking replant for {plantPos}");
            // Check for water before attempting to replant
            if (!HasWater())
            {
                 AdvLogging.DisplayLog("FarmPlotData", $"NPC {entityAlive.entityId} cannot replant at {plantPos} - no water.");
                return harvestItems; // Return any harvested items, but don't replant
            }

            string seedToPlant = seedFromPlant; // Use seed from harvested plant first

            // If no seed from harvest, try to find one in NPC inventory
            if (string.IsNullOrEmpty(seedToPlant))
            {
                foreach (var stack in entityAlive.lootContainer.items)
                {
                    if (!stack.IsEmpty())
                    {
                        var itemClass = ItemClass.GetForId(stack.itemValue.type);
                        // Assuming seeds are named like "planted<CropName>1"
                        if (itemClass.Name.StartsWith("planted") && itemClass.Name.EndsWith("1"))
                        {
                            seedToPlant = itemClass.Name;
                            stack.count--; // Decrement item count in inventory
                            if (stack.count <= 0) stack.Clear(); // Clear stack if empty
                            entityAlive.lootContainer.SetModified(); // Mark inventory changed
                            break; // Found a seed
                        }
                    }
                }
            }

            // If a seed was found (either from harvest or inventory), try to plant it
            if (!string.IsNullOrEmpty(seedToPlant))
            {
                var seedBlock = Block.GetBlockByName(seedToPlant);
                if (seedBlock != null)
                {
                    var blockValueToPlace = seedBlock.ToBlockValue();
                    // Check if block can be placed (e.g., requires specific conditions)
                    if (seedBlock.CanPlaceBlockAt(GameManager.Instance.World, 0, plantPos, blockValueToPlace))
                    {
                        AdvLogging.DisplayLog("FarmPlotData", $"NPC {entityAlive.entityId} replanting {seedToPlant} at {plantPos}");
                        // Use world.SetBlockRPC or block.PlaceBlock as appropriate
                        GameManager.Instance.World.SetBlockRPC(0, plantPos, blockValueToPlace);
                        // OR: More complex placement if rotation/helper needed:
                        // var placementResult = blockValueToPlace.Block.BlockPlacementHelper?.OnPlaceBlock(...);
                        // if (placementResult != null) blockValueToPlace.Block.PlaceBlock(GameManager.Instance.World, placementResult, entityAlive);
                    }
                    else {
                         AdvLogging.DisplayLog("FarmPlotData", $"NPC {entityAlive.entityId} Cannot place {seedToPlant} at {plantPos} (CanPlaceBlockAt failed).");
                    }
                } else {
                     AdvLogging.DisplayLog("FarmPlotData", $"NPC {entityAlive.entityId} Could not find block for seed name {seedToPlant}.");
                }
            } else {
                 AdvLogging.DisplayLog("FarmPlotData", $"NPC {entityAlive.entityId} has no seed to replant at {plantPos}.");
            }
        }

        return harvestItems;
    }
}