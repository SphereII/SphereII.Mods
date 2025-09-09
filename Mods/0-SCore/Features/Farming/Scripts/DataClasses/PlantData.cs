using System;
using System.Collections.Generic; // Required for HashSet iteration
using UnityEngine;

// Updated PlantData class reflecting Manager changes
public class PlantData
{
    private static readonly string AdvFeatureClass = "CropManagement";

    public Vector3i BlockPos { get; private set; } = Vector3i.zero;
    public BlockValue BlockValue { get; private set; } = BlockValue.Air;
    public Vector3i WaterPos { get; private set; } = Vector3i.zero; // Cached water source pos

    private readonly bool _requireWater = false;
    private readonly float _waterRange = 5f;
    private readonly string _waterParticle = "NoParticle";

    public bool Visited { get; set; } = false;
    public ulong LastCheck { get; set; } = 0ul;
    public float LastMaintained { get; set; } = 0f;

    public PlantData(Vector3i blockPos)
    {
        BlockPos = blockPos;
        BlockValue = GameManager.Instance.World.GetBlock(BlockPos);

        // Read properties (unchanged)
        if (BlockValue.Block.Properties.Values.TryGetValue("RequireWater", out string requireWaterStr))
            _requireWater = StringParsers.ParseBool(requireWaterStr);
        if (BlockValue.Block.Properties.Values.TryGetValue("WaterRange", out string waterRangeStr))
            _waterRange = StringParsers.ParseFloat(waterRangeStr);
        _waterParticle = Configuration.GetPropertyValue(AdvFeatureClass, "WaterParticle");
        if (BlockValue.Block.Properties.Values.TryGetValue("WaterParticle", out string particleOverride))
            _waterParticle = particleOverride;
        if (string.IsNullOrWhiteSpace(_waterParticle))
            _waterParticle = "NoParticle";

        // Initialize WaterPos by checking nearby sources (unchanged)
        IsNearWater(_waterRange);
    }

    public void Manage()
    {
        Log.Out($"Checking for bugs at {BlockPos}");
        Visited = true;
        LastMaintained = GameManager.Instance.World.GetWorldTime();
    }

    /// <summary>
    /// Registers the plant with the CropManager.
    /// CropManager's Add method now handles spatial grid updates internally.
    /// </summary>
    public void RegisterPlant()
    {
        ToggleWaterParticle();
        CropManager.Instance.Add(this); // No change needed here
    }

    public bool CanStay()
    {
        if (!RequiresWater()) return true;
        if (IsNearWater()) // Updates WaterPos if found
        {
            Consume();
            return true;
        }
        return false;
    }

    private void Consume()
    {
        LastCheck = GameManager.Instance.World.GetWorldTime();
        AdvLogging.DisplayLog(AdvFeatureClass, $"Consume check for {BlockPos}. WaterPos: {WaterPos}");

        if (!RequiresWater() || WaterPos == Vector3i.zero) return;

        if (WeatherManager.Instance.GetCurrentRainfallPercent() > 0.25)
        {
            AdvLogging.DisplayLog(AdvFeatureClass, $"Using Rain Water for {BlockPos}.");
            return;
        }

        Vector3i sourceDamagePos = WaterPos;
        BlockValue waterBlockValue = GameManager.Instance.World.GetBlock(WaterPos);

        if (waterBlockValue.Block is BlockWaterSourceSDX waterSourceBlock)
        {
            if (waterSourceBlock.IsWaterSourceUnlimited())
            {
                AdvLogging.DisplayLog(AdvFeatureClass, $"Sprinkler at {WaterPos} is unlimited source for {BlockPos}.");
                ToggleWaterParticle();
                return;
            }
            // Use the refactored (and cached) lookup in WaterPipeManager
            sourceDamagePos = WaterPipeManager.Instance.GetWaterForPosition(WaterPos);
            if (sourceDamagePos == Vector3i.zero)
            {
                AdvLogging.DisplayLog(AdvFeatureClass, $"Sprinkler at {WaterPos} for {BlockPos} lost its pipe connection (checked via WaterPipeManager).");
                return;
            }
            waterBlockValue = GameManager.Instance.World.GetBlock(sourceDamagePos);
            AdvLogging.DisplayLog(AdvFeatureClass, $"Sprinkler at {WaterPos} draws from {sourceDamagePos} for {BlockPos}.");
        }

        // --- Consume from the sourceDamagePos ---
        var world = GameManager.Instance.World;
        if (world.IsWater(sourceDamagePos))
        {
             float waterPercent = world.GetWaterPercent(sourceDamagePos);
             if (waterPercent > 0.01f)
             {
                AdvLogging.DisplayLog(AdvFeatureClass, $"TODO: Implement A21+ water consumption at {sourceDamagePos} for {BlockPos}. Current percent: {waterPercent * 100}%");
                // --- FALLBACK ---
                DamageWaterBlockLegacy(sourceDamagePos, waterBlockValue);
                // --- END FALLBACK ---
                ToggleWaterParticle();
                return;
             }
        }

        if (waterBlockValue.Block is BlockLiquidv2)
        {
            DamageWaterBlockLegacy(sourceDamagePos, waterBlockValue);
            ToggleWaterParticle();
            return;
        }

        if (waterBlockValue.Block.Properties.GetString("WaterType").ToLower() == "unlimited")
        {
            AdvLogging.DisplayLog(AdvFeatureClass, $"Source at {sourceDamagePos} is unlimited type for {BlockPos}.");
            ToggleWaterParticle();
            return;
        }

        AdvLogging.DisplayLog(AdvFeatureClass, $"Source at {sourceDamagePos} for {BlockPos} is not a recognized consumable water type. Block: {waterBlockValue.Block.GetBlockName()}");
    }

    private void DamageWaterBlockLegacy(Vector3i sourcePos, BlockValue waterBlockVal)
    {
         AdvLogging.DisplayLog(AdvFeatureClass, $"Consuming legacy water block: {sourcePos} ({waterBlockVal.Block.GetBlockName()}) for {BlockPos}");
         if (waterBlockVal.Block.Properties.GetString("WaterType").ToLower() == "unlimited") return;

        int damage = WaterPipeManager.Instance.GetWaterDamage(sourcePos);
        if (BlockValue.Block.Properties.Contains("WaterDamage"))
            damage = BlockValue.Block.Properties.GetInt("WaterDamage");

        waterBlockVal.damage += damage;
        if (waterBlockVal.damage < waterBlockVal.Block.MaxDamage)
        {
            GameManager.Instance.World.SetBlockRPC(0, sourcePos, waterBlockVal);
        }
        else
        {
            AdvLogging.DisplayLog(AdvFeatureClass, $"Water block consumed completely: {sourcePos}");
            GameManager.Instance.World.SetBlockRPC(0, sourcePos, BlockValue.Air);
            // If our cached source was this block, clear the cache
            if (WaterPos == sourcePos) WaterPos = Vector3i.zero;
        }
    }

    private void ToggleWaterParticle()
    {
        if (GameManager.Instance.HasBlockParticleEffect(BlockPos))
            BlockUtilitiesSDX.removeParticles(BlockPos);
        if (_waterParticle != "NoParticle")
        {
            BlockUtilitiesSDX.addParticlesCentered(_waterParticle, BlockPos);
        }
    }

    /// <summary>
    /// Removes the plant from CropManager. CropManager handles spatial grid internally.
    /// Triggers base game check which may lead to wilting.
    /// </summary>
    public void Remove()
    {
        BlockUtilitiesSDX.removeParticles(BlockPos);
        CropManager.Instance.Remove(BlockPos); // No change needed here

        if (BlockValue.Block is BlockPlantGrowing block)
        {
             var chunk = GameManager.Instance.World.GetChunkFromWorldPos(BlockPos) as Chunk;
             if (chunk != null)
             {
                 block.CheckPlantAlive(GameManager.Instance.World, chunk.ClrIdx, BlockPos, BlockValue);
             }
        }
    }

    public bool RequiresWater()
    {
        return _requireWater;
    }

    /// <summary>
    /// Checks if the plant is near a valid water source (Direct, Pipe, Sprinkler).
    /// Updates WaterPos if a source is found. Interacts with refactored WaterPipeManager.
    /// </summary>
    public bool IsNearWater(float waterRange = -1f)
    {
        float rangeToCheck = (waterRange == -1f) ? _waterRange : waterRange;

        // 1. Check cached WaterPos validity (unchanged)
        if (WaterPos != Vector3i.zero)
        {
            // Use WaterPipeManager's check which handles various source types
            if (WaterPipeManager.Instance.IsWaterSource(WaterPos))
            {
                 // If it's a limited sprinkler, ensure it's still connected
                 var cachedBlockValue = GameManager.Instance.World.GetBlock(WaterPos);
                 if (cachedBlockValue.Block is BlockWaterSourceSDX sp && !sp.IsWaterSourceUnlimited())
                 {
                     if (WaterPipeManager.Instance.GetWaterForPosition(WaterPos) != Vector3i.zero) return true;
                 }
                 else // Direct water or unlimited sprinkler
                 {
                     return true;
                 }
            }
             // Cached position is no longer a valid source or sprinkler lost connection
            AdvLogging.DisplayLog(AdvFeatureClass, $"Cached WaterPos {WaterPos} for {BlockPos} is no longer valid.");
            WaterPos = Vector3i.zero;
        }

        // 2. Check Sprinklers in range
        // *** Optimization Note: Still iterates ALL active sprinklers via GetWaterValves ***
        // Requires WaterPipeManager spatial partitioning for sprinklers to optimize further.
        HashSet<Vector3i> activeSprinklers = WaterPipeManager.Instance.GetWaterValves();
        float rangeToCheckSq = rangeToCheck * rangeToCheck; // Check against plant's range? Or sprinkler's? Using Plant's here.

        foreach (Vector3i sprinklerPos in activeSprinklers) // Iterate HashSet directly
        {
            var blockValue = GameManager.Instance.World.GetBlock(sprinklerPos);
            if (blockValue.Block is not BlockWaterSourceSDX waterBlock) continue;

            rangeToCheckSq = waterBlock.GetWaterRange() * waterBlock.GetWaterRange(); // Check against sprinkler's range

            // Check distance from plant to sprinkler using the plant's waterRange
            if (Vector3.SqrMagnitude(BlockPos - sprinklerPos) <= rangeToCheckSq) // Use plant's range
            // Alternative: Check against sprinkler's range: <= waterBlock.GetWaterRange() * waterBlock.GetWaterRange() ?
            {
                // Check if this sprinkler is connected (uses WaterPipeManager cache/search) or unlimited
                if (waterBlock.IsWaterSourceUnlimited() || WaterPipeManager.Instance.GetWaterForPosition(sprinklerPos) != Vector3i.zero)
                {
                    AdvLogging.DisplayLog(AdvFeatureClass, $"Found valid sprinkler {sprinklerPos} for {BlockPos}");
                    WaterPos = sprinklerPos;
                    return true;
                }
            }
        }

        // 3. Scan for local direct water sources or connected pipes (unchanged)
        Vector3i foundSource = ScanForWater(BlockPos, rangeToCheck);
        if (foundSource != Vector3i.zero)
        {
             if (WaterPipeManager.Instance.IsDirectWaterSource(foundSource))
             {
                 AdvLogging.DisplayLog(AdvFeatureClass, $"Found direct water source {foundSource} via ScanForWater for {BlockPos}");
                 WaterPos = foundSource;
                 return true;
             }

             Vector3i ultimateSource = WaterPipeManager.Instance.GetWaterForPosition(foundSource);
             if (ultimateSource != Vector3i.zero)
             {
                AdvLogging.DisplayLog(AdvFeatureClass, $"Found indirect water via pipe/sprinkler {foundSource} (leading to {ultimateSource}) for {BlockPos}");
                 WaterPos = foundSource; // Cache intermediate connector block
                 return true;
             }
        }

        // 4. No water source found (unchanged)
        AdvLogging.DisplayLog(AdvFeatureClass, $"No water source found for {BlockPos} within range {rangeToCheck}");
        WaterPos = Vector3i.zero;
        return false;
    }

    // ScanForWater remains unchanged internally, but uses updated WaterPipeManager.IsWaterSource
    public Vector3i ScanForWater(Vector3i _blockPos, float range = -1f)
    {
        if (range == -1) range = _waterRange;
        int intRange = Mathf.Clamp(Mathf.CeilToInt(range), 1, 10);
        var world = GameManager.Instance.World;
        for (int x = -intRange; x <= intRange; x++)
        {
            for (int z = -intRange; z <= intRange; z++)
            {
                for (int y = _blockPos.y - intRange; y <= _blockPos.y + intRange; y++)
                {
                    Vector3i checkPos = new Vector3i(_blockPos.x + x, y, _blockPos.z + z);
                    // IsWaterSource check now uses the updated WaterPipeManager method
                    if (WaterPipeManager.Instance.IsWaterSource(checkPos))
                    {
                        return checkPos;
                    }
                }
            }
        }
        return Vector3i.zero;
    }
}