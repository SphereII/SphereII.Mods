using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Updated CropManager class with Spatial Grid and aligned with other refactoring
public class CropManager
{
    private static readonly string AdvFeatureClass = "CropManagement";
    public bool RequirePipesForSprinklers { get; private set; }

    private static CropManager _instance = null;

    // --- Data Structures ---
    // Main map for direct lookups by position
    private static Dictionary<Vector3i, PlantData> _cropMap = new Dictionary<Vector3i, PlantData>();

    // Spatial Grid for efficient range queries
    private static Dictionary<Vector3i, List<Vector3i>> _spatialGrid = new Dictionary<Vector3i, List<Vector3i>>();
    private const int _cellSize = 16; // Example cell size
    // --- End Data Structures ---

    // --- Instance Members ---
    private float _checkTime = 120f; // Default check interval
    private float _currentTime = 0f;
    private GameRandom _random;
    private bool _debugMode = false;
    private bool Enabled { set; get; } // Is the crop system enabled?
    // --- End Instance Members ---


    public bool DebugMode {
        get => _debugMode;
        set {
            _debugMode = value;
            // If WaterPipeManager particle toggling exists/is desired, call it here.
            // WaterPipeManager.Instance.ToggleWaterParticles(_debugMode); // This method was removed from WaterPipeManager
            Log.Out($"[CropManager] Debug Mode {(value ? "Enabled" : "Disabled")}.");
            // Add alternative debug visualization if needed
        }
    }


    public static CropManager Instance {
        get {
            if (_instance != null) return _instance;
            _instance = new CropManager();
            _instance.Init();
            return _instance;
        }
    }

    // --- Helper Methods ---
    private Vector3i GetGridCellCoordinate(Vector3i position)
    {
        return new Vector3i(
            Mathf.FloorToInt((float)position.x / _cellSize),
            Mathf.FloorToInt((float)position.y / _cellSize),
            Mathf.FloorToInt((float)position.z / _cellSize)
        );
    }
    // --- End Helper Methods ---


    private void Init()
    {
        var option = Configuration.GetPropertyValue(AdvFeatureClass, "CropEnable");
        if (!StringParsers.ParseBool(option))
        {
            Log.Out("[CropManager] Disabled via configuration.");
            Enabled = false;
            return;
        }

        Enabled = true; // Enable if not explicitly disabled

        option = Configuration.GetPropertyValue(AdvFeatureClass, "RequirePipesForSprinklers");
        RequirePipesForSprinklers = string.IsNullOrEmpty(option) ? true : StringParsers.ParseBool(option);

        _random = GameManager.Instance.World.GetGameRandom();

        option = Configuration.GetPropertyValue(AdvFeatureClass, "CheckInterval");
        if (!string.IsNullOrEmpty(option))
            _checkTime = StringParsers.ParseFloat(option);
        // Ensure checkTime is reasonable
        if (_checkTime < 10f) _checkTime = 10f; // Minimum check interval
        _currentTime = _checkTime; // Initialize timer

        Log.Out(
            $"[CropManager] Initialized. Enabled: {Enabled}, CheckInterval: {_checkTime}, Spatial Cell Size: {_cellSize}");
        ModEvents.GameUpdate.RegisterHandler(this.CropUpdate);
    }


    private void CropUpdate(ref ModEvents.SGameUpdateData data)
    {
        if (!Enabled || _cropMap.Count == 0) return; // Exit if disabled or no crops

        _currentTime -= Time.deltaTime;
        if (_currentTime > 0f) return; // Wait for timer

        _currentTime = _checkTime; // Reset timer
        CheckCrops(); // Perform periodic checks
    }

    /// <summary>
    /// Periodic check method. Currently only logs.
    /// Expand this method if checks/actions need to be performed on all crops periodically.
    /// </summary>
    public void CheckCrops()
    {
        // Note: If iterating all crops is needed here, use: foreach(PlantData plant in _cropMap.Values) { ... }
        AdvLogging.DisplayLog(AdvFeatureClass, $"Periodic Crop Check Running: {_cropMap.Count} Plants registered.");
        // Add logic here if needed (e.g., applying environment effects)
    }


    // --- Add/Remove Methods ---

    /// <summary>
    /// Adds a fully initialized PlantData object to the manager.
    /// Updates both the main map and the spatial grid.
    /// </summary>
    public void Add(PlantData plantData)
    {
        if (plantData == null) return;
        Vector3i blockPos = plantData.BlockPos;

        if (_cropMap.ContainsKey(blockPos))
        {
            _cropMap[blockPos] = plantData; // Update if already exists
        }
        else
        {
            _cropMap.Add(blockPos, plantData); // Add to main map

            // Add position to spatial grid
            Vector3i cellPos = GetGridCellCoordinate(blockPos);
            if (!_spatialGrid.TryGetValue(cellPos, out List<Vector3i> plantsInCell))
            {
                plantsInCell = new List<Vector3i>();
                _spatialGrid.Add(cellPos, plantsInCell);
            }

            if (!plantsInCell.Contains(blockPos)) // Avoid duplicates
            {
                plantsInCell.Add(blockPos);
            }
            // AdvLogging.DisplayLog(AdvFeatureClass, $"Added plant at {blockPos} to cell {cellPos}.");
        }
    }

    /// <summary>
    /// Creates and adds a plant at a position if it's near water (using PlantData's check).
    /// </summary>
    public void Add(Vector3i _blockPos, int waterRange) // waterRange seems unused now PlantData handles it
    {
        if (_cropMap.ContainsKey(_blockPos)) return; // Don't add duplicates

        AdvLogging.DisplayLog(AdvFeatureClass, $"Attempting to add plant at {_blockPos}.");
        // PlantData constructor now checks properties and initial water state
        var plantData = new PlantData(_blockPos);

        // Check if the PlantData instance determined it requires water and has it, or doesn't require water
        if (!plantData.RequiresWater() || plantData.WaterPos != Vector3i.zero)
        {
            // Plant is valid (either doesn't need water or found water initially)
            Add(plantData); // Add the initialized PlantData
            plantData.RegisterPlant(); // Call registration (adds particle, maybe other logic)
            AdvLogging.DisplayLog(AdvFeatureClass, $"Successfully added plant at {_blockPos}.");
        }
        else
        {
            AdvLogging.DisplayLog(AdvFeatureClass,
                $"Failed to add plant at {_blockPos} - requires water but none found initially.");
        }
    }


    /// <summary>
    /// Forcefully adds a plant, bypassing initial water checks (used for loading existing plants).
    /// </summary>
    public void ForceAdd(Vector3i _blockPos)
    {
        if (!_cropMap.ContainsKey(_blockPos))
        {
            AdvLogging.DisplayLog(AdvFeatureClass, $"Force adding plant at {_blockPos}.");
            var plantData = new PlantData(_blockPos);
            Add(plantData); // Add the initialized PlantData
            // Do we need RegisterPlant here? Maybe not if particles are handled on load?
            // plantData.RegisterPlant(); // Consider if needed for force add scenario
        }
    }

    /// <summary>
    /// Removes a plant from the manager.
    /// Updates both the main map and the spatial grid.
    /// </summary>
    public void Remove(Vector3i _blockPos)
    {
        // AdvLogging.DisplayLog(AdvFeatureClass, $"Removing plant at {_blockPos}");
        // Remove from main map
        _cropMap.Remove(_blockPos);

        // Remove from spatial grid
        Vector3i cellPos = GetGridCellCoordinate(_blockPos);
        if (_spatialGrid.TryGetValue(cellPos, out List<Vector3i> plantsInCell))
        {
            plantsInCell.Remove(_blockPos);
            if (plantsInCell.Count == 0)
            {
                _spatialGrid.Remove(cellPos);
            }
        }
    }

    // --- End Add/Remove Methods ---


    // --- Query Methods ---

    /// <summary>
    /// Gets PlantData for a specific position. Direct lookup.
    /// </summary>
    public PlantData GetPlant(Vector3i position)
    {
        return _cropMap.GetValueOrDefault(position);
    }

    /// <summary>
    /// Checks if a position is near water by creating a temporary PlantData instance.
    /// Note: This creates a temporary object. Consider direct WaterPipeManager call if possible,
    /// but this leverages PlantData's full check (direct, pipe, sprinkler).
    /// </summary>
    public bool IsNearWater(Vector3i _blockPos, float waterRange)
    {
        // Creates a temporary PlantData which runs IsNearWater in its constructor.
        // This checks properties and performs the necessary water lookups via WaterPipeManager.
        var tempPlantData = new PlantData(_blockPos);
        // Check the result of the water check performed during construction / first call.
        // The range passed here might be redundant if PlantData only uses its configured range.
        // return tempPlantData.IsNearWater(waterRange); // Re-calling might be redundant/inefficient
        return tempPlantData.WaterPos != Vector3i.zero; // Check if the constructor found water
    }


    // --- Spatial Query Methods (Refactored with Grid) ---

    public List<PlantData> GetClosePlants(Vector3i position, float range = 50)
    {
        var closePlants = new List<PlantData>();
        float rangeSq = range * range;
        Vector3i minCell = GetGridCellCoordinate(position - new Vector3i((int)range, (int)range, (int)range));
        Vector3i maxCell = GetGridCellCoordinate(position + new Vector3i((int)range, (int)range, (int)range));
        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                for (int z = minCell.z; z <= maxCell.z; z++)
                {
                    if (_spatialGrid.TryGetValue(new Vector3i(x, y, z), out var plantsInCell))
                    {
                        foreach (var plantPos in plantsInCell)
                        {
                            if (Vector3.SqrMagnitude(position - plantPos) < rangeSq)
                            {
                                if (_cropMap.TryGetValue(plantPos, out var plantData)) closePlants.Add(plantData);
                            }
                        }
                    }
                }
            }
        }

        return closePlants;
    }

    public List<Vector3i> GetClosePlantPositions(Vector3i position, float range = 50)
    {
        var closePlantsPos = new List<Vector3i>();
        float rangeSq = range * range;
        Vector3i minCell = GetGridCellCoordinate(position - new Vector3i((int)range, (int)range, (int)range));
        Vector3i maxCell = GetGridCellCoordinate(position + new Vector3i((int)range, (int)range, (int)range));
        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                for (int z = minCell.z; z <= maxCell.z; z++)
                {
                    if (_spatialGrid.TryGetValue(new Vector3i(x, y, z), out var plantsInCell))
                    {
                        foreach (var plantPos in plantsInCell)
                        {
                            if (Vector3.SqrMagnitude(position - plantPos) < rangeSq) closePlantsPos.Add(plantPos);
                        }
                    }
                }
            }
        }

        return closePlantsPos;
    }

    public PlantData GetRandomClosePlant(Vector3i position, float range = 50)
    {
        List<PlantData> closePlants = GetClosePlants(position, range);
        if (closePlants.Count > 0)
        {
            if (_random == null) _random = GameManager.Instance.World.GetGameRandom();
            return closePlants[_random.RandomRange(0, closePlants.Count)];
        }

        return null;
    }

    // Keeping original logic for small fixed range check
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
                    if (_cropMap.TryGetValue(blockPos, out PlantData plantData) && !plantData.Visited) return plantData;
                }
            }
        }

        return null;
    }


    public PlantData GetClosesUnmaintained(Vector3i position, float range = 50)
    {
        PlantData bestCandidate = null;
        float oldestTime = float.MaxValue;
        float rangeSq = range * range;
        Vector3i minCell = GetGridCellCoordinate(position - new Vector3i((int)range, (int)range, (int)range));
        Vector3i maxCell = GetGridCellCoordinate(position + new Vector3i((int)range, (int)range, (int)range));
        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                for (int z = minCell.z; z <= maxCell.z; z++)
                {
                    if (_spatialGrid.TryGetValue(new Vector3i(x, y, z), out var plantsInCell))
                    {
                        foreach (var plantPos in plantsInCell)
                        {
                            if (Vector3.SqrMagnitude(position - plantPos) < rangeSq)
                            {
                                if (_cropMap.TryGetValue(plantPos, out var currentPlant))
                                {
                                    if (currentPlant.LastMaintained < oldestTime)
                                    {
                                        oldestTime = currentPlant.LastMaintained;
                                        bestCandidate = currentPlant;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return bestCandidate;
    }

    // --- End Spatial Query Methods ---
} // End of CropManager class