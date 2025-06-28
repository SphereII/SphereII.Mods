using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
// using System.Text; // Likely not needed
// using System.Threading.Tasks; // Likely not needed
using UnityEngine;


// Refactored Water Pipe Manager
public class WaterPipeManager
{
    private static readonly string AdvFeatureClass = "CropManagement";
    private static WaterPipeManager instance = null;

    // Cache for water lookup results: Maps Queried Position -> Found Water Source Position (or Zero)
    private Dictionary<Vector3i, Vector3i> _waterSourceCache = new Dictionary<Vector3i, Vector3i>();

    // Set of active sprinkler block positions
    private HashSet<Vector3i> _activeSprinklers = new HashSet<Vector3i>();

    private int maxPipes = 50; // Default max pipe length for searches


    public static WaterPipeManager Instance
    {
        get
        {
            if (instance == null)
            {
                Log.Out("Initializing Water Pipe Manager");
                instance = new WaterPipeManager();
                instance.Init();
            }
            return instance;
        }
    }

    // Initialize manager, read config
    public void Init()
    {
        var option = Configuration.GetPropertyValue(AdvFeatureClass, "MaxPipeLength");
        if (!string.IsNullOrEmpty(option))
        {
            if (StringParsers.TryParseSInt32(option, out int maxpipesValue, 0, -1, NumberStyles.Integer))
                maxPipes = maxpipesValue;
        }
        Log.Out($"[WaterPipeManager] Max Pipe Length: {maxPipes}");
    }


    /// <summary>
    /// Gets the set of active sprinkler positions.
    /// </summary>
    public HashSet<Vector3i> GetWaterValves() // Renamed from GetWaterValves for clarity maybe -> GetActiveSprinklers?
    {
        return _activeSprinklers;
    }

    /// <summary>
    /// Gets the configured water damage amount for consuming water at a specific position.
    /// Checks block properties, material properties, and global config.
    /// </summary>
    public int GetWaterDamage(Vector3i waterPos)
    {
        var waterBlock = GameManager.Instance.World.GetBlock(waterPos);
        int value = -1;

        // 1. Check specific block property
        if (waterBlock.Block.Properties.Contains("WaterDamage"))
        {
            value = waterBlock.Block.Properties.GetInt("WaterDamage");
        }
        // 2. TODO: Check material property (if materials.xml has WaterDamage)
        // else if (waterBlock.Block.blockMaterial.Properties.Contains("WaterDamage")) { ... }
        // 3. Check global config property
        else
        {
            // Use TryParse for safety
            int.TryParse(Configuration.GetPropertyValue(AdvFeatureClass, "WaterDamage"), out value);
        }

        // Default to 1 if no valid value found or value is negative
        return (value >= 0) ? value : 1;
    }

    /// <summary>
    /// Provides a summary string about water connection for tooltips/debugging.
    /// </summary>
    public string GetWaterSummary(Vector3i checkPos) // Changed param name for clarity
    {
        Vector3i waterSourcePos = GetWaterForPosition(checkPos); // Use the main lookup function

        var sourceBlockValue = GameManager.Instance.World.GetBlock(checkPos); // Block at the location being checked
        string blockName = sourceBlockValue.Block?.GetLocalizedBlockName() ?? "Unknown Block";

        if (waterSourcePos == Vector3i.zero)
        {
            // Attempt a direct scan if pipe search failed initially (e.g., for plants not next to pipes)
            // This uses PlantData's logic as a fallback check.
             PlantData tempPlantCheck = new PlantData(checkPos);
             bool directCheck = tempPlantCheck.IsNearWater(); // This might find water missed by pipe search

             return $"Block: {blockName} at {checkPos} - Has Water: {directCheck} (Direct Scan)";
        }
        else
        {
            var waterBlockValue = GameManager.Instance.World.GetBlock(waterSourcePos);
            string waterBlockName = waterBlockValue.Block?.GetBlockName() ?? "Unknown Source";
            int damage = GetWaterDamage(waterSourcePos);
            int durability = waterBlockValue.Block != null ? (waterBlockValue.Block.MaxDamage - waterBlockValue.damage) : 0;
            int maxDurability = waterBlockValue.Block?.MaxDamage ?? 0;

             // Check for A21+ water
             float waterPercent = GameManager.Instance.World.GetWaterPercent(waterSourcePos);
             string waterInfo;
             if (waterPercent > 0.01f) {
                 waterInfo = $"Modern Water ({waterPercent * 100:F0}%)";
             } else if (maxDurability > 0) {
                 waterInfo = $"Legacy Water Block '{waterBlockName}', Damage/Tick: {damage}, Durability: {durability} / {maxDurability}";
             } else {
                 waterInfo = $"Custom Source Block '{waterBlockName}'";
             }

            return $"Block: {blockName} at {checkPos} - Has Water: True - Source: {waterSourcePos} ({waterInfo})";
        }
    }

    /// <summary>
    /// Gets the ultimate water source position connected to the given position via pipes.
    /// Uses caching and the iterative pipe search.
    /// </summary>
    /// <param name="position">The position to check (e.g., a plant, a sprinkler, a pipe).</param>
    /// <returns>The position of the water source, or Vector3i.zero if none found.</returns>
    public Vector3i GetWaterForPosition(Vector3i position)
    {
        // 1. Check cache first
        if (_waterSourceCache.TryGetValue(position, out Vector3i cachedSource))
        {
            // Optional: Add a check here to verify if the cached source is STILL valid water?
            // Could prevent issues if water block was removed without cache invalidation.
            // e.g., if (cachedSource != Vector3i.zero && !IsDirectWaterSource(cachedSource)) { /* invalidate and continue */ }
            return cachedSource;
        }

        // 2. If not cached, perform the search
        // Create a temporary PipeData instance to run the search
        // Pass position for context, and maxPipes from this manager
        var pipeSearch = new PipeData(position, this.maxPipes);
        Vector3i waterSource = pipeSearch.DiscoverWaterFromPipes(position);

        // 3. Cache the result (even if zero)
        _waterSourceCache[position] = waterSource;

        return waterSource;
    }


    /// <summary>
    /// Registers a sprinkler block as active. Called by BlockWaterSourceSDX.
    /// </summary>
    public void AddValve(Vector3i valvePosition)
    {
        _activeSprinklers.Add(valvePosition);
        InvalidateWaterCacheNear(valvePosition); // Invalidate cache near the added sprinkler
         // Optional: Sync state if needed immediately upon adding? Block syncs on update.
        // Sync(valvePosition, true); // Likely redundant if block handles its own sync
    }

    /// <summary>
    /// Unregisters a sprinkler block. Called by BlockWaterSourceSDX.
    /// </summary>
    public void RemoveValve(Vector3i valvePosition)
    {
        _activeSprinklers.Remove(valvePosition);
        InvalidateWaterCacheNear(valvePosition); // Invalidate cache near the removed sprinkler
        // Optional: Sync removal? Block syncs its state usually.
        // Sync(valvePosition, false);
    }

    /// <summary>
    /// Sends sprinkler state synchronization packet. Called by BlockWaterSourceSDX.
    /// </summary>
    public static void Sync(Vector3i blockPos, bool isEnabled)
    {
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                NetPackageManager.GetPackage<NetPackageToggleSprinkler>().Setup(blockPos, isEnabled));
            return;
        }
        // Ensure package is sent reliably if needed
        SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
            NetPackageManager.GetPackage<NetPackageToggleSprinkler>().Setup(blockPos, isEnabled)/*, true*/); // Optional reliable flag
    }


    /// <summary>
    /// Gets the configured maximum pipe length for water searches.
    /// </summary>
    public int GetMaxPipeCount()
    {
        return maxPipes;
    }

    /// <summary>
    /// Invalidates the entire water source cache. Call when pipes/sources change.
    /// </summary>
    public void InvalidateWaterCache()
    {
        AdvLogging.DisplayLog(AdvFeatureClass,"Invalidating entire water source cache.");
        _waterSourceCache.Clear();
    }

    /// <summary>
    /// Invalidates the water source cache for positions within a certain range of a change.
    /// More granular than clearing the entire cache.
    /// </summary>
    /// <param name="changedPosition">The position where a change occurred (pipe/sprinkler added/removed).</param>
    /// <param name="range">The range around the change to invalidate.</param>
    public void InvalidateWaterCacheNear(Vector3i changedPosition, int range = 5) // Example range
    {
         AdvLogging.DisplayLog(AdvFeatureClass, $"Invalidating water cache near {changedPosition} (Range: {range})");
         List<Vector3i> keysToRemove = new List<Vector3i>();
         int rangeSq = range * range;

         // Find cached keys within range of the change
         foreach (var kvp in _waterSourceCache)
         {
             Vector3i cachedQueryPos = kvp.Key;
             if (Vector3.SqrMagnitude(cachedQueryPos - changedPosition) <= rangeSq)
             {
                 keysToRemove.Add(cachedQueryPos);
             }
             // Also consider if the *result* was near the change? Maybe less critical.
             // Vector3i cachedResultPos = kvp.Value;
             // if (cachedResultPos != Vector3i.zero && Vector3.SqrMagnitude(cachedResultPos - changedPosition) <= rangeSq) { ... }
         }

         // Remove the identified keys
         foreach (var key in keysToRemove)
         {
            _waterSourceCache.Remove(key);
         }
         AdvLogging.DisplayLog(AdvFeatureClass, $"Invalidated {keysToRemove.Count} cache entries near {changedPosition}.");

         // Alternative: Instead of precise range, maybe just clear cache if changes are frequent?
         // Consider performance trade-off of iterating cache vs. blanket clear.
         // If InvalidateWaterCacheNear is slow, revert to InvalidateWaterCache().
         // InvalidateWaterCache(); // Simpler alternative
    }


    /// <summary>
    /// Checks if a block at a given position should be considered a source of water
    /// (Direct water, custom water source block, active sprinkler).
    /// </summary>
    public bool IsWaterSource(Vector3i position)
    {
        if (IsDirectWaterSource(position)) return true;

        var blockValue = GameManager.Instance.World.GetBlock(position);
        // Check if it's a registered sprinkler block (even if currently off/disconnected, it's a *potential* source)
        if (blockValue.Block is BlockWaterSourceSDX) return true;

        return false;
    }

    /// <summary>
    /// Checks if a position contains direct water (A21+ system, legacy liquid block, or custom unlimited block).
    /// Does NOT check for sprinklers or pipes.
    /// </summary>
    public bool IsDirectWaterSource(Vector3i position)
    {
        // A21+ water system check
        if (GameManager.Instance.World.IsWater(position)) return true; // Includes rivers, lakes etc.

        var blockValue = GameManager.Instance.World.GetBlock(position);

        // Legacy water block check
        if (blockValue.Block is BlockLiquidv2) return true;

        // Custom block property check (e.g., bedrock as unlimited source)
        if (blockValue.Block.Properties.GetBool("WaterSource")) return true; // Explicit WaterSource=true property
        if (blockValue.Block.Properties.GetString("WaterType").ToLower() == "unlimited") return true; // WaterType=unlimited property

        return false;
    }

    /// <summary>
    /// Counts water blocks in the surrounding area (cubic scan).
    /// NOTE: Performance concern if range is large and called frequently.
    /// </summary>
    public int CountWaterBlocks(Vector3i position, int range = 4)
    {
        int count = 0;
        int rangeCeil = Mathf.Clamp(range, 0, 10); // Clamp range

        for (int x = -rangeCeil; x <= rangeCeil; x++)
        {
            for (int z = -rangeCeil; z <= rangeCeil; z++)
            {
                for (int y = position.y - rangeCeil; y <= position.y + rangeCeil; y++)
                {
                    var waterCheck = new Vector3i(position.x + x, y, position.z + z);
                    // Use IsDirectWaterSource for consistency
                    if (IsDirectWaterSource(waterCheck))
                        count++;
                }
            }
        }
        return count;
    }

} // End of WaterPipeManager class