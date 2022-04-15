using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
public class CropManager : Block
{
    private static readonly string AdvFeatureClass = "CropManagement";

    private static CropManager instance = null;
    private static Dictionary<Vector3i, PlantData> CropMap = new Dictionary<Vector3i, PlantData>();
    public WaterData WaterData = new WaterData();

    private float checkTime = 120f;
    private float currentTime = 0f;
    public static CropManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CropManager();
                instance.Init();
            }
            return instance;
        }
    }
    public override void Init()
    {
        // Water Manament
        var option = Configuration.GetPropertyValue(AdvFeatureClass, "CropEnable");
        if (!StringParsers.ParseBool(option))
        {
            Log.Out("Crop Manager is disabled.");
            return;
        }

        option = Configuration.GetPropertyValue(AdvFeatureClass, "CheckInterval");
        if (!string.IsNullOrEmpty(option))
            checkTime = StringParsers.ParseFloat(option);

        currentTime = checkTime;

        Log.Out("Starting Crop Manager");
        Log.Out($" :: Crop Interval Check time: {checkTime}");
        ModEvents.GameUpdate.RegisterHandler(new Action(this.CropUpdate));
    }


    public void CropUpdate()
    {
        if (CropMap.Count == 0) return;

        currentTime -= Time.deltaTime;
        if (currentTime > 0f) return;

        CheckCrops();
    }
    public void CheckCrops()
    {
        AdvLogging.DisplayLog(AdvFeatureClass, $"Checking Crops for Water: {CropMap.Count} Plants registered.");

        // Check our water map to see if the block is still near a water source. 
        // If not, flag for removal, so that it can rescan
        var world = GameManager.Instance.World;
        List<PlantData> remove = new List<PlantData>();
        foreach (var pair in CropMap)
        {
            // Check if it can stay, has water, etc.
            if ( !pair.Value.CanStay()) remove.Add(pair.Value);
        }

        // For each plant that we are removing, we want to check to see if it has a new water source.
        // We'll call the CheckPlantAlive to re-do its checks. If no water is found, it'll wilt
        foreach (var pair in remove)
            pair.Remove();

        currentTime = checkTime;
        AdvLogging.DisplayLog(AdvFeatureClass, $"End Checking Crops for Water: {CropMap.Count} Plants registered. Check Interval: {checkTime}");
    }
    public bool IsNearWater(Vector3i _blockPos)
    {
        var plantData = Get(_blockPos);
        if ( plantData == null )
            plantData = new PlantData(_blockPos); 
        return plantData.IsNearWater();
    }

    public void Remove(Vector3i _blockPos)
    {
        AdvLogging.DisplayLog(AdvFeatureClass, $"Removing {_blockPos}");

        if (CropMap.ContainsKey(_blockPos))
            CropMap.Remove(_blockPos);
    }
    public PlantData Get(Vector3i _blockPos)
    {
        AdvLogging.DisplayLog(AdvFeatureClass, $"Getting Water Value from {_blockPos}");
        if (CropMap.ContainsKey(_blockPos))
            return CropMap[_blockPos];
        return null;
    }

    // Forcefully adds the block to the mapper. This will be checked on the updater to see if its still valid,
    // and wilted if its not, after the update timer expires. This will be used when a block is loaded.
    public void ForceAdd(Vector3i _blockPos)
    {
        var plantData = new PlantData(_blockPos);
        plantData.RegisterPlant();
    }

    public void Add(PlantData plantData)
    {
        if ( CropMap.ContainsKey(plantData.BlockPos))
            CropMap[plantData.BlockPos] = plantData;
        else
            CropMap.Add(plantData.BlockPos, plantData);

    }
    public void Add(Vector3i _blockPos, int range = 5)
    {
        AdvLogging.DisplayLog(AdvFeatureClass, $"Searching for water block {_blockPos} at range of {range}");
        var plantData = new PlantData(_blockPos);
        if (plantData.IsNearWater())
            plantData.RegisterPlant();
    }


    // Helper method to determine what the crop manager considers a water source
    public bool IsWaterSource(Vector3i position)
    {
        var blockValue = GameManager.Instance.World.GetBlock(position);
        if (blockValue.Block is BlockLiquidv2) return true;

        // Treat bedrock as a water block.
        if (blockValue.Block.GetBlockName() == "terrBedrock") return true;

        if (blockValue.Block.Properties.Contains("WaterSource"))
        {
            if (blockValue.Block.Properties.GetBool("WaterSource"))
                return true;
            return false;
        }
        return false;
    }

}

