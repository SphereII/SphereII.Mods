using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FarmPlotData
{
    private Vector3i BlockPos = Vector3i.zero;

    public bool Visited { get; set; } = false;

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
        if (WaterPipeManager.Instance.GetWaterForPosition(BlockPos + Vector3i.up) != Vector3i.zero) return true;
        if (WaterPipeManager.Instance.GetWaterForPosition(BlockPos) != Vector3i.zero) return true;

        var tempPlant = new PlantData(BlockPos + Vector3i.up);
        return tempPlant.IsNearWater();
    }

    public bool IsEmpty()
    {
        var blockAbove = GameManager.Instance.World.GetBlock(BlockPos + Vector3i.up);
        if (blockAbove.isair) return true;
        return false;
    }

    public bool IsDeadPlant()
    {
        var blockAbove = GameManager.Instance.World.GetBlock(BlockPos + Vector3i.up);
        if (blockAbove.Block is BlockPlant) return false;
        if (blockAbove.isair) return false;
        return true;
    }

    public bool HasPlant()
    {
        if (IsEmpty()) return false;

        if (CropManager.Instance.Get(BlockPos) != null)
            return true;

        if (CropManager.Instance.Get(BlockPos + Vector3i.up) != null)
            return true;

        if (IsDeadPlant())
            return true;

        return false;
    }

    public void Reset()
    {
        Visited = false;
    }

    public List<Block.SItemDropProb> Manage(EntityAlive entityAlive)
    {
        Visited = true;
        var replant = false;
        var plantPos = BlockPos + Vector3i.up;
        var plantData = CropManager.Instance.GetPlant(plantPos);
        var currentBlock = GameManager.Instance.World.GetBlock(plantPos);
        var harvestItems = new List<Block.SItemDropProb>();
        var seedFromPlant = string.Empty;
        if (IsDeadPlant())
        {
            plantData = new PlantData(plantPos);
        }

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

                // Clean all all the seeds
                for (var x = harvestItems.Count - 1; x >= 0; x--)
                {
                    var harvestItem = harvestItems[x];
                    if (harvestItem.name.StartsWith("planted"))
                    {
                        seedFromPlant = harvestItem.name;
                        harvestItems.RemoveAt(x);
                    }

                    // If the NPC has cvar of the name of the harvested item, then this will restrict the amount of items they get.
                    if (entityAlive.Buffs.HasCustomVar(harvestItem.name))
                    {
                        var count = entityAlive.Buffs.GetCustomVar(harvestItem.name);
                        harvestItem.minCount = (int) count;
                        harvestItem.maxCount = (int) count;
                        harvestItems[x] = harvestItem;
                    }
                }

                //GameManager.Instance.World.SetBlockRPC(plantPos, BlockValue.Air);
                GameManager.Instance.World.SetBlock(0, plantPos, BlockValue.Air, false, false);
                replant = true;
            }
        }

        if (currentBlock.isair)
            replant = true;

        if (!replant) return harvestItems;

        // use the seed we got from the plant itself, if its set. If not, then search for one.
        var seedName = seedFromPlant;
        if (string.IsNullOrEmpty(seedName))
        {
            foreach (var stack in entityAlive.lootContainer.items)
            {
                if (stack.IsEmpty()) continue;
                var itemname = stack.itemValue.ItemClass.GetItemName();
                if (!itemname.StartsWith("planted") || !itemname.EndsWith("1")) continue;
                seedName = itemname;
                stack.count--;
                break;
            }
            entityAlive.lootContainer.SetModified();
        }

        // Still nothing to plant?
        if (string.IsNullOrEmpty(seedName))
            return harvestItems;

        plantData ??= new PlantData(BlockPos);
        if (!plantData.IsNearWater())
        {
            return harvestItems;
        }

        var cropBlock = Block.GetBlockByName(seedName);
        var blockValue = cropBlock.ToBlockValue();

        var worldRayHitInfo = new WorldRayHitInfo();
        worldRayHitInfo.hit.blockPos = plantPos;

        if (!blockValue.Block.CanPlaceBlockAt(GameManager.Instance.World, 0, plantPos, blockValue)) return harvestItems;
       var result = blockValue.Block.BlockPlacementHelper.OnPlaceBlock(
            BlockPlacement.EnumRotationMode.Auto, 0, GameManager.Instance.World, blockValue, worldRayHitInfo.hit,
            plantPos);
        blockValue.Block.PlaceBlock(GameManager.Instance.World, result, null);

        return harvestItems;
    }
}