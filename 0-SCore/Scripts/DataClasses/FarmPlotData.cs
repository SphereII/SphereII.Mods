using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FarmPlotData
{
    private Vector3i BlockPos = Vector3i.zero;

    public bool Visited = false;
    public FarmPlotData(Vector3i blockPos)
    {
        BlockPos = blockPos;
    }

    public Vector3i GetBlockPos()
    {
        return BlockPos;
    }

    public void UpdateData()
    {
        Visited = true;
    }

    public void Reset()
    {
        Visited = false;
    }

    public List<Block.SItemDropProb> Manage(string blockName)
    {
        Log.Out($"Managing Farming Plot: {BlockPos}");
        Visited = true;
        var replant = false;
        var plantPos = BlockPos + Vector3i.up;
        var plantData = CropManager.Instance.GetPlant(plantPos);
        var currentBlock = GameManager.Instance.World.GetBlock(plantPos);
        List<Block.SItemDropProb> harvestItems = null;
        if (plantData != null)
        {
            // If we are a plant growing, then check to see if its the final stage.
            if (currentBlock.Block.Properties.Contains("PlantGrowing.Next"))
            {
                plantData.LastCheck = GameManager.Instance.World.GetWorldTime();
                Log.Out("Checking for bugs...");
                return null;
            }
            else
            {
                Log.Out("Harvesting...");
                currentBlock.Block.itemsToDrop.TryGetValue(EnumDropEvent.Harvest, out harvestItems);

                replant = true;
            }
        }
        if (currentBlock.isair)
            replant = true;

        if ( replant )
        {
            // if we don't have any seeds, check if our harvest gave us anything.
            if ( string.IsNullOrEmpty(blockName))
            {
                if (harvestItems != null)
                {
                    foreach (var item in harvestItems)
                    {
                        if (item.name.StartsWith("planted") && item.name.EndsWith("1"))
                        {
                            Log.Out("using Seed from inventory.");
                            blockName = item.name;
                            break;
                        }
                    }
                }
            }

            // Nothing to plant.
            if (string.IsNullOrEmpty(blockName))
                return harvestItems;

            Log.Out($"Planting {blockName}");
            var cropBlock = Block.GetBlockByName(blockName);
            var blockValue = cropBlock.ToBlockValue();

            WorldRayHitInfo worldRayHitInfo = new WorldRayHitInfo();
            worldRayHitInfo.hit.blockPos = plantPos;

            BlockPlacement.Result result = blockValue.Block.BlockPlacementHelper.OnPlaceBlock(BlockPlacement.EnumRotationMode.Auto, 0, GameManager.Instance.World, blockValue, worldRayHitInfo.hit, plantPos);
            blockValue.Block.PlaceBlock(GameManager.Instance.World, result, null);
        }

        return harvestItems;
        
    }


}

