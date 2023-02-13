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

    public bool HasWater()
    {
        if (WaterPipeManager.Instance.GetWaterForPosition(BlockPos) == Vector3i.zero) return false;
        return true;
    }

    public bool HasPlant()
    {
        if (CropManager.Instance.Get(BlockPos) != null)
            return true;

        if (CropManager.Instance.Get(BlockPos + Vector3i.up) != null)
            return true;

        return false;

    }
    public void Reset()
    {
        Visited = false;
    }

    public List<Block.SItemDropProb> Manage( EntityAlive entityAlive)
    {
        Visited = true;
        var replant = false;
        var plantPos = BlockPos + Vector3i.up;
        var plantData = CropManager.Instance.GetPlant(plantPos);
        var currentBlock = GameManager.Instance.World.GetBlock(plantPos);
        List<Block.SItemDropProb> harvestItems = new List<Block.SItemDropProb>();
        if (plantData != null)
        {
            // If we are a plant growing, then check to see if its the final stage.
            if (currentBlock.Block.Properties.Contains("PlantGrowing.Next"))
            {
                plantData.LastCheck = GameManager.Instance.World.GetWorldTime();
                return null;
            }
            else
            {
                currentBlock.Block.itemsToDrop.TryGetValue(EnumDropEvent.Harvest, out harvestItems);

                // Including the breaking of the plant here.
                currentBlock.Block.itemsToDrop.TryGetValue(EnumDropEvent.Destroy, out var harvestItems2);
                if (harvestItems != null && harvestItems2 != null)
                {
                        foreach (var item in harvestItems2)
                            harvestItems.Add(item);

                }

                //GameManager.Instance.World.SetBlockRPC(plantPos, BlockValue.Air);
                GameManager.Instance.World.SetBlock(0, plantPos, BlockValue.Air, false, false);
                replant = true;
            }
        }
        if (currentBlock.isair)
            replant = true;

        if ( replant )
        {
            var seedName = string.Empty;
            ItemStack itemStack = new ItemStack();
            foreach (var stack in entityAlive.lootContainer.items)
            {
                if (stack.IsEmpty()) continue;
                var itemname = stack.itemValue.ItemClass.GetItemName();
                if (itemname.StartsWith("planted") && itemname.EndsWith("1"))
                {
                    seedName = itemname;
                    itemStack = stack;
                    break;
                }
            }

            // if we don't have any seeds, check if our harvest gave us anything.
            if ( string.IsNullOrEmpty(seedName))
            {
                if (harvestItems != null)
                {
                    foreach (var item in harvestItems)
                    {
                        if (item.name.StartsWith("planted") && item.name.EndsWith("1"))
                        {
                            seedName = item.name;
                            break;
                        }
                    }
                }
            }

            // Still nothing to plant?
            if (string.IsNullOrEmpty(seedName))
            {
                 return harvestItems;
            }

            if (plantData == null)
                plantData = new PlantData(BlockPos);

            if (plantData != null)
            {
                if (!plantData.IsNearWater())
                {
                    Log.Out("Is not near water.");
                     return harvestItems;
                }
            }
             var cropBlock = Block.GetBlockByName(seedName);
             var blockValue = cropBlock.ToBlockValue();
 
            WorldRayHitInfo worldRayHitInfo = new WorldRayHitInfo();
            worldRayHitInfo.hit.blockPos = plantPos;

            if (blockValue.Block.CanPlaceBlockAt(GameManager.Instance.World, 0, plantPos, blockValue))
            {
                 BlockPlacement.Result result = blockValue.Block.BlockPlacementHelper.OnPlaceBlock(BlockPlacement.EnumRotationMode.Auto, 0, GameManager.Instance.World, blockValue, worldRayHitInfo.hit, plantPos);
                blockValue.Block.PlaceBlock(GameManager.Instance.World, result, null);
                itemStack.count--;
            }
        }

        return harvestItems;
        
    }


}

