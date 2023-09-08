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

    private static CropManager _instance = null;
    private static Dictionary<Vector3i, PlantData> _cropMap = new Dictionary<Vector3i, PlantData>();

    private float _checkTime = 120f;
    private float _currentTime = 0f;
    private GameRandom _random;
    private bool _debugMode = false;
    public bool DebugMode
    {
        get => _debugMode;
        set
        {
            _debugMode = value;
            WaterPipeManager.Instance.ToggleWaterParticles(_debugMode);
        }
    }


    private bool Enabled { set; get; }
    public static CropManager Instance
    {
        get
        {
            if (_instance != null) return _instance;
            _instance = new CropManager();
            _instance.Init();
            return _instance;
        }
    }

    private void Init()
    {
        // Water Management
        var option = Configuration.GetPropertyValue(AdvFeatureClass, "CropEnable");
        if (!StringParsers.ParseBool(option))
        {
            Log.Out("Crop Manager is disabled.");
            Enabled = false;
            return;
        }
        _random = GameManager.Instance.World.GetGameRandom();

        Enabled = true;
        option = Configuration.GetPropertyValue(AdvFeatureClass, "CheckInterval");
        if (!string.IsNullOrEmpty(option))
            _checkTime = StringParsers.ParseFloat(option);

        _currentTime = _checkTime;

        Log.Out("Starting Crop Manager");
        Log.Out($" :: Crop Interval Check time: {_checkTime}");
        ModEvents.GameUpdate.RegisterHandler(new Action(this.CropUpdate));
    }


    private void CropUpdate()
    {
        if (_cropMap.Count == 0) return;

        _currentTime -= Time.deltaTime;
        if (_currentTime > 0f) return;

        CheckCrops();
    }
 
    public PlantData GetRandomClosePlant( Vector3i position, float range = 50)
    {
        var closePlants = GetClosePlants(position, range);
        if ( closePlants.Count > 0 )
            return closePlants[_random.RandomRange(0, closePlants.Count)];
        return null;
    }
   
    public List<PlantData> GetClosePlants( Vector3i position, float range = 50 )
    {
        var closePlants = new List<PlantData>();
        foreach( var plant in _cropMap)
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
        foreach (var plant in _cropMap)
        {
            var plantDistance = Vector3.Distance(position, plant.Key);
            if (plantDistance < range)
                closePlants.Add(plant.Key);
        }
        return closePlants;
    }

    public PlantData GetPlantDataNearby(Vector3i position)
    {
        var range = 1;
        for (var x = -range; x <= range; x++)
        {
            for (var z = -range; z <= range; z++)
            {
                for (var y = position.y - 2; y <= position.y + 2; y++)
                {
                    var blockPos = new Vector3i(position.x + x, y, position.z + z);
                    if (_cropMap.ContainsKey(blockPos) && _cropMap[blockPos].Visited == false)
                        return _cropMap[blockPos];
                }
            }
        }

        //foreach (var neighbor in Vector3i.AllDirections)
        //{
        //    var blockPos = position + neighbor;
        //    if (CropMap.ContainsKey(blockPos) && CropMap[blockPos].Visited == false)
        //        return CropMap[blockPos];
        //}
        return null;
    }

    public PlantData GetClosesUnmaintained(Vector3i position, float range = 50)
    {
        var lastMaintainedTime = float.MaxValue;
        var plantData = new PlantData(position);
        foreach (var plant in _cropMap)
        {
            if (!(lastMaintainedTime > plant.Value.LastMaintained)) continue;
            lastMaintainedTime = plant.Value.LastMaintained;
            plantData = plant.Value;
        }
        return plantData;
    }
    public PlantData GetPlant(Vector3i position)
    {
        return _cropMap.ContainsKey(position) ? _cropMap[position] : null;
    }
    public void CheckCrops()
    {
        AdvLogging.DisplayLog(AdvFeatureClass, $"Checking Crops for Water: {_cropMap.Count} Plants registered.");
        _currentTime = _checkTime;
    }
    public bool IsNearWater(Vector3i _blockPos, float waterRange)
    {
        var plantData = Get(_blockPos) ?? new PlantData(_blockPos);
        return plantData.IsNearWater(waterRange);
    }

    public void Remove(Vector3i _blockPos)
    {
        AdvLogging.DisplayLog(AdvFeatureClass, $"Removing {_blockPos}");

        if (_cropMap.ContainsKey(_blockPos))
            _cropMap.Remove(_blockPos);
    }
    public PlantData Get(Vector3i _blockPos)
    {
        // AdvLogging.DisplayLog(AdvFeatureClass, $"Getting Water Value from {_blockPos}");
        return _cropMap.ContainsKey(_blockPos) ? _cropMap[_blockPos] : null;
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
        if ( _cropMap.ContainsKey(plantData.BlockPos))
            _cropMap[plantData.BlockPos] = plantData;
        else
            _cropMap.Add(plantData.BlockPos, plantData);

    }
    public void Add(Vector3i _blockPos, int range = 5)
    {
        AdvLogging.DisplayLog(AdvFeatureClass, $"Searching for water block {_blockPos} at range of {range}");
        var plantData = new PlantData(_blockPos);
        if (plantData.IsNearWater())
            plantData.RegisterPlant();
    }
  
}

