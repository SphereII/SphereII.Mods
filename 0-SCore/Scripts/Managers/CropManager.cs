using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// Class to manager to keep track of all crops
public class CropManager 
{
    private static readonly string AdvFeatureClass = "CropManagement";

    private static CropManager instance = null;
    private static Dictionary<Vector3i, PlantData> CropMap = new Dictionary<Vector3i, PlantData>();

    private float checkTime = 120f;
    private float currentTime = 0f;
    private GameRandom random;
    private bool _debugMode = false;
    public bool DebugMode
    {
        get
        {
            return _debugMode;
        }
        set
        {
            _debugMode = value;
            WaterPipeManager.Instance.ToggleWaterParticles(_debugMode);
        }
    }
    
  
    public bool Enabled { private set; get; }
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
    public void Init()
    {

        // Water Manament
        var option = Configuration.GetPropertyValue(AdvFeatureClass, "CropEnable");
        if (!StringParsers.ParseBool(option))
        {
            Log.Out("Crop Manager is disabled.");
            Enabled = false;
            return;
        }
        random = GameManager.Instance.World.GetGameRandom();

        Enabled = true;
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
 
    public PlantData GetRandomClosePlant( Vector3i position, float range = 50)
    {
        var closePlants = GetClosePlants(position, range);
        if ( closePlants.Count > 0 )
            return closePlants[random.RandomRange(0, closePlants.Count)];
        return null;
    }
   
    public List<PlantData> GetClosePlants( Vector3i position, float range = 50 )
    {
        var closePlants = new List<PlantData>();
        foreach( var plant in CropMap)
        {
            var plantDistance = Vector3.Distance(position, plant.Key);
            if (plantDistance < 50)
                closePlants.Add(plant.Value);
        }
        return closePlants;
    }

    public List<Vector3> GetClosePlantPositions(Vector3i position, float range = 50)
    {
        var closePlants = new List<Vector3>();
        foreach (var plant in CropMap)
        {
            var plantDistance = Vector3.Distance(position, plant.Key);
            if (plantDistance < range)
                closePlants.Add(plant.Key);
        }
        return closePlants;
    }

    public PlantData GetPlantDataNearby(Vector3i position)
    {
        foreach (var neighbor in Vector3i.AllDirections)
        {
            var blockPos = position + neighbor;
            if (CropMap.ContainsKey(blockPos) && CropMap[blockPos].Visited == false)
                return CropMap[blockPos];
        }
        return null;
    }

    public PlantData GetClosesUnmaintained(Vector3i position, float range = 50)
    {
        var lastMaintainedTime = float.MaxValue;
        PlantData plantData = new PlantData(position);
        foreach (var plant in CropMap)
        {
            if ( lastMaintainedTime > plant.Value.LastMaintained)
            {
                lastMaintainedTime = plant.Value.LastMaintained;
                plantData = plant.Value;
            }
        }
        return plantData;
    }
    public PlantData GetPlant(Vector3i position)
    {
        if (CropMap.ContainsKey(position))
            return CropMap[position];
        return null;
    }
    public void CheckCrops()
    {
        AdvLogging.DisplayLog(AdvFeatureClass, $"Checking Crops for Water: {CropMap.Count} Plants registered.");
        currentTime = checkTime;
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
       // AdvLogging.DisplayLog(AdvFeatureClass, $"Getting Water Value from {_blockPos}");
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
  
}

