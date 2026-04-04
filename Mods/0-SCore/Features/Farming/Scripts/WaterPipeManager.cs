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

    public string GetWaterSummary(Vector3i checkPos)
    {
        // 1. Perform the actual connection search
        // This now uses the neighbor-check + BFS logic we established
        Vector3i waterSourcePos = GetWaterForPosition(checkPos);

        var currentBlockValue = GameManager.Instance.World.GetBlock(checkPos);
        string blockName = currentBlockValue.Block?.GetLocalizedBlockName() ?? "Unknown Block";

        // 2. Handle the "No Water" case clearly
        if (waterSourcePos == Vector3i.zero)
        {
            return $"Block: {blockName} at {checkPos}\nStatus: [NO WATER] - No path to a valid water source found.";
        }

        // 3. We found a source! Determine exactly what it is for the UI
        var waterBlockValue = GameManager.Instance.World.GetBlock(waterSourcePos);
        string waterBlockName = waterBlockValue.Block?.GetBlockName() ?? "Unknown Source";
    
        // A21+ voxel water — GetWaterPercent returns 0.0 (empty) to 1.0 (full).
        float waterPercent = GameManager.Instance.World.GetWaterPercent(waterSourcePos);
        string waterTypeInfo;

        int sourceMaxHp = waterBlockValue.Block.MaxDamage;
        int sourceHp    = sourceMaxHp > 0 ? Mathf.Max(0, sourceMaxHp - waterBlockValue.damage) : -1;
        string hpSuffix = sourceHp >= 0 ? $"  Health: {sourceHp}/{sourceMaxHp}" : string.Empty;

        if (waterPercent > 0.01f)
        {
            waterTypeInfo = $"Voxel Water Source ({waterPercent * 100:F0}%){hpSuffix}";
        }
        else if (waterBlockValue.Block is BlockLiquidv2)
        {
            // Legacy placed water block — damage field tracks depletion.
            if (sourceMaxHp <= 0)
                waterTypeInfo = $"Legacy Water Block '{waterBlockName}' (Unlimited)";
            else
            {
                int remaining = sourceHp;
                waterTypeInfo = $"Legacy Water Block '{waterBlockName}' (Water: {remaining}/{sourceMaxHp})";
            }
        }
        else
        {
            string hpInfo = sourceHp >= 0 ? $" (Health: {sourceHp}/{sourceMaxHp})" : " (Unlimited)";
            waterTypeInfo = $"Source: '{waterBlockName}'{hpInfo}";
        }

        // 4. Return the unified "Truth"
        return $"Block: {blockName} at {checkPos}\nStatus: [CONNECTED] - Source found at {waterSourcePos}\nType: {waterTypeInfo}";
    }

    public Vector3i GetWaterForPosition(Vector3i position)
    {
        // 1. Check cache, but validate the cached result is still a live water source.
        // If the source was removed the entry becomes stale; evict and re-search.
        // Note: "no water" (zero) is never cached — see below.
        if (_waterSourceCache.TryGetValue(position, out Vector3i cachedSource))
        {
            if (IsWaterSource(cachedSource))
                return cachedSource;
            // Source no longer valid — evict and fall through to a fresh search.
            _waterSourceCache.Remove(position);
        }

        // 2. Check if there is direct water touching this block first.
        // Include horizontal diagonals so corner farm plots (diagonally adjacent to water)
        // are correctly detected as watered — matching what the player planting system sees.
        Vector3i[] neighbors = {
            position + Vector3i.up, position + Vector3i.down,
            position + Vector3i.left, position + Vector3i.right,
            position + Vector3i.forward, position + Vector3i.back,
            // horizontal diagonals
            position + new Vector3i( 1, 0,  1), position + new Vector3i( 1, 0, -1),
            position + new Vector3i(-1, 0,  1), position + new Vector3i(-1, 0, -1)
        };

        foreach (var neighbor in neighbors)
        {
            if (IsDirectWaterSource(neighbor))
            {
                _waterSourceCache[position] = neighbor;
                return neighbor;
            }
        }

        // 3. If no direct water, perform the pipe search.
        var pipeSearch = new PipeData(position, this.maxPipes);
        Vector3i waterSource = pipeSearch.DiscoverWaterFromPipes(position);

        // Only cache a positive result. "No water" (zero) is intentionally not cached so
        // that newly placed water is detected on the next UpdateTick without needing an
        // explicit cache invalidation — which may not fire for A21+ voxel water placement.
        if (waterSource != Vector3i.zero)
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
        var connectionManager = SingletonMonoBehaviour<ConnectionManager>.Instance;

        if (!connectionManager.IsServer)
        {
            // Pure client: send the request to the server.
            connectionManager.SendToServer(
                NetPackageManager.GetPackage<NetPackageToggleSprinkler>().Setup(blockPos, isEnabled));
            return;
        }

        // Broadcast to any connected remote clients.
        connectionManager.SendPackage(
            NetPackageManager.GetPackage<NetPackageToggleSprinkler>().Setup(blockPos, isEnabled));

        // On singleplayer / listen-server SendPackage doesn't loop back to the host.
        // Update the local view directly. (Dedicated server has no local player.)
        if (!GameManager.IsDedicatedServer)
        {
            var world = GameManager.Instance.World;
            if (world?.GetBlock(blockPos).Block is BlockWaterSourceSDX sprinkler)
                sprinkler.ToggleSprinkler(blockPos, isEnabled);
        }
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
    /// Invalidates the water source cache for entries where either the query position OR the
    /// cached result position is within range of a change. Uses maxPipes as the default range
    /// since pipe networks can extend that far.
    /// </summary>
    /// <param name="changedPosition">The position where a change occurred (pipe/sprinkler added/removed).</param>
    /// <param name="range">The range around the change to invalidate. Defaults to maxPipes.</param>
    public void InvalidateWaterCacheNear(Vector3i changedPosition, int range = -1)
    {
         if (range < 0) range = maxPipes;
         AdvLogging.DisplayLog(AdvFeatureClass, $"Invalidating water cache near {changedPosition} (Range: {range})");
         List<Vector3i> keysToRemove = new List<Vector3i>();
         int rangeSq = range * range;

         foreach (var kvp in _waterSourceCache)
         {
             Vector3i cachedQueryPos = kvp.Key;
             Vector3i cachedResultPos = kvp.Value;

             // Invalidate if the query position is near the change
             if (Vector3.SqrMagnitude(cachedQueryPos - changedPosition) <= rangeSq)
             {
                 keysToRemove.Add(cachedQueryPos);
                 continue;
             }

             // Also invalidate if the cached result pointed at a position near the change
             // (e.g. a water source was removed — all pipes that cached it as their result are stale)
             if (cachedResultPos != Vector3i.zero && Vector3.SqrMagnitude(cachedResultPos - changedPosition) <= rangeSq)
             {
                 keysToRemove.Add(cachedQueryPos);
             }
         }

         foreach (var key in keysToRemove)
         {
            _waterSourceCache.Remove(key);
         }
         AdvLogging.DisplayLog(AdvFeatureClass, $"Invalidated {keysToRemove.Count} cache entries near {changedPosition}.");
    }


    /// <summary>
    /// Checks if a block at a given position should be considered a source of water
    /// (Direct water, custom water source block, active sprinkler).
    /// </summary>
    public bool IsWaterSource(Vector3i position)
    {
        // Always return true for direct water (rivers, lakes, etc.)
        if (IsDirectWaterSource(position)) return true;

        var blockValue = GameManager.Instance.World.GetBlock(position);

        // Only count a sprinkler as a SOURCE if it is an "Unlimited" type
        // This prevents standard sprinklers from acting as their own water source
        if (blockValue.Block is BlockWaterSourceSDX waterSourceBlock)
        {
            return waterSourceBlock.IsWaterSourceUnlimited();
        }

        return false;
    }

    /// <summary>
    /// Checks if a position contains direct water (A21+ system, legacy liquid block, or custom block).
    /// Does NOT check for sprinklers or pipes.
    /// Used for plant proximity scanning where terrain water (rivers, lakes) is a valid source.
    /// </summary>
    public bool IsDirectWaterSource(Vector3i position)
    {
        var blockValue = GameManager.Instance.World.GetBlock(position);

        // Exclude pipe blocks first — water may flow through them in the voxel sim,
        // so world.IsWater() can return true at pipe positions. Checking block type
        // before world.IsWater() ensures pipes are never treated as water sources.
        if (blockValue.Block is BlockWaterPipeSDX) return false;

        // A21+ water system check — valid when a plant is standing next to a river/lake
        if (GameManager.Instance.World.IsWater(position)) return true;

        // Legacy placed water block
        if (blockValue.Block is BlockLiquidv2) return true;

        // Custom block properties
        if (blockValue.Block.Properties.GetBool("WaterSource")) return true;
        if (blockValue.Block.Properties.GetString("WaterType").ToLower() == "unlimited") return true;

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