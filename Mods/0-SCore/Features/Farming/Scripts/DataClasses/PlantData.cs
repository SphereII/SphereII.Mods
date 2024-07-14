using System;
using UnityEngine;

public class PlantData
{
    private static readonly string AdvFeatureClass = "CropManagement";

    public Vector3i BlockPos { get; set; } = Vector3i.zero;
    public Vector3i WaterPos { get; set; } = Vector3i.zero;
    private BlockValue BlockValue { get; set; } = BlockValue.Air;

    private readonly bool _requireWater = false;
    private readonly float _waterRange = 5f;
    public bool Visited = false;
    private readonly string _waterParticle;

    public PlantData(Vector3i blockPos) : this(blockPos, Vector3i.zero)
    {
    }

    private PlantData(Vector3i _blockPos, Vector3i _waterPos)
    {
        BlockPos = _blockPos;
        WaterPos = _waterPos;
        BlockValue = GameManager.Instance.World.GetBlock(BlockPos);

        if (BlockValue.Block.Properties.Values.ContainsKey("RequireWater"))
            _requireWater = StringParsers.ParseBool(BlockValue.Block.Properties.Values["RequireWater"]);

        if (BlockValue.Block.Properties.Values.ContainsKey("WaterRange"))
            _waterRange = StringParsers.ParseFloat(BlockValue.Block.Properties.Values["WaterRange"]);

        _waterParticle = Configuration.GetPropertyValue(AdvFeatureClass, "WaterParticle");
        if (string.IsNullOrWhiteSpace(_waterParticle))
            _waterParticle = "NoParticle";
    }

    public void Manage()
    {
        Log.Out($"Checking for bugs at {BlockPos}");
        Visited = true;
        LastMaintained = GameManager.Instance.World.GetWorldTime();
    }

    public void RegisterPlant()
    {
        BlockValue = GameManager.Instance.World.GetBlock(BlockPos);
        ToggleWaterParticle();
        CropManager.Instance.Add(this);
    }

    public bool CanStay()
    {
        var results = IsNearWater();
        if (results)
            Consume();
        return results;
    }

    private void Consume()
    {
        AdvLogging.DisplayLog(AdvFeatureClass,
            $"Last Checked: {BlockPos} {WaterPos} Checked at: {LastCheck} Maintained at: {LastMaintained} Current Time: {GameManager.Instance.World.GetWorldTime()} Difference: {GameManager.Instance.World.GetWorldTime() - LastCheck}");

        if (!RequiresWater()) return;
        if (WaterPos == Vector3i.zero) return;

        // If its raining, don't consume water.
        if ((double) WeatherManager.Instance.GetCurrentRainfallValue() > 0.25)
        {
            AdvLogging.DisplayLog(AdvFeatureClass, $"Using Rain Water for {BlockPos}.");
            return;
        }

        // Damage the water block to simulate usage.
        var waterBlock = GameManager.Instance.World.GetBlock(WaterPos);

        var source = WaterPos;


        // If the water block is a sprinkler, find out where its getting its water from.
        if (waterBlock.Block is BlockWaterSourceSDX waterSource)
        {
            if (waterSource.IsWaterSourceUnlimited())
            {
                ToggleWaterParticle();
                return;
            }

            source = WaterPipeManager.Instance.GetWaterForPosition(WaterPos);
            waterBlock = GameManager.Instance.World.GetBlock(source);
        }

        var waterValue = GameManager.Instance.World.GetWater(source);
        if (waterValue.GetMassPercent() > 0.25f)
        {
            AdvLogging.DisplayLog(AdvFeatureClass,
                $"Consuming Water: {BlockPos} {source} {waterBlock.Block.GetBlockName()}");
            var currentMass = waterValue.GetMass();
            currentMass--;
            waterValue.SetMass(currentMass);
            var package = NetPackageManager.GetPackage<NetPackageWaterSet>();
            package.AddChange(source, waterValue);
            GameManager.Instance.SetWaterRPC(package);
            return;
        }


        if (waterBlock.Block is not BlockLiquidv2 water) return;
        
        AdvLogging.DisplayLog(AdvFeatureClass,
            $"Consuming Water: {BlockPos} {source} {waterBlock.Block.GetBlockName()}");

        if (water.Properties.Values.ContainsKey("WaterType"))
        {
            if (water.Properties.Values["WaterType"].ToLower() == "unlimited")
            {
                ToggleWaterParticle();
                return;
            }
        }

        // Check the global value of damaging water for this growth cycle
        var damage = WaterPipeManager.Instance.GetWaterDamage(source);

        // If the blant block itself has its own water damage, use it instead.
        if (BlockValue.Block.Properties.Contains("WaterDamage"))
            damage = BlockValue.Block.Properties.GetInt("WaterDamage");

        // Grab the damage value from a) the water block itself, or b) the globa one
        waterBlock.damage += damage;
        if (waterBlock.damage <= waterBlock.Block.MaxDamage)
            GameManager.Instance.World.SetBlockRPC(0, source, waterBlock);
        else
        {
            AdvLogging.DisplayLog(AdvFeatureClass, $"Water is completely consumed here: {source}");
            GameManager.Instance.World.SetBlockRPC(0, source, BlockValue.Air);
        }

        ToggleWaterParticle();
    }

    private void ToggleWaterParticle()
    {
        if (GameManager.Instance.HasBlockParticleEffect(BlockPos))
            BlockUtilitiesSDX.removeParticles(BlockPos);

        var waterParticle = GetWaterParticle();
        BlockUtilitiesSDX.addParticlesCentered(waterParticle, BlockPos);
    }

    private string GetWaterParticle()
    {
        var block = GameManager.Instance.World.GetBlock(BlockPos);

        var waterParticle = this._waterParticle;
        if (block.Block.Properties.Contains("WaterParticle"))
            waterParticle = block.Block.Properties.GetString("WaterParticle");

        if (block.Block.blockMaterial.Properties.Contains("WaterParticle"))
            waterParticle = block.Block.blockMaterial.Properties.GetString("WaterParticle");

        return waterParticle;
    }

    public void Remove()
    {
        if (BlockValue.Block is not BlockPlantGrowing block) return;
        BlockUtilitiesSDX.removeParticles(BlockPos);
        CropManager.Instance.Remove(BlockPos);
        var chunk = GameManager.Instance.World.GetChunkFromWorldPos(BlockPos) as Chunk;
        if (chunk == null) return;
        block.CheckPlantAlive(GameManager.Instance.World, chunk.ClrIdx, BlockPos, BlockValue);
    }

    public ulong LastCheck { get; set; } = 0ul;
    public float LastMaintained { get; set; } = 0f;

    public bool RequiresWater()
    {
        return _requireWater;
    }

    public bool IsNearWater(float waterRange = -1f)
    {
        if (waterRange == -1f)
            waterRange = _waterRange;

        if (WaterPos != Vector3i.zero)
        {
            if (GameManager.Instance.World.IsWater(WaterPos))
                return true;
        }

        // Check if we still have a water valve reference, and its on.
        var valves = WaterPipeManager.Instance.GetWaterValves();

        // Check to see if there's a valve that can provide coverage for this.
        foreach (var valve in valves)
        {
            var blockValue = GameManager.Instance.World.GetBlock(valve.Key);
            if (blockValue.Block is not BlockWaterSourceSDX waterBlock) continue;
            if (waterBlock.IsWaterSourceUnlimited()) return true;

            // If the valve itself is not connected to water, skip it.
            if (WaterPipeManager.Instance.GetWaterForPosition(valve.Key) == Vector3i.zero)
                continue;

            var num = Vector3.Distance(BlockPos, valve.Key);
            if (num <= waterBlock.GetWaterRange())
            {
                WaterPos = valve.Key;
                return true;
            }
        }

        // search for a direct water source.
        var newWaterSource = ScanForWater(BlockPos, waterRange);
        if (WaterPipeManager.Instance.IsDirectWaterSource(newWaterSource))
        {
            WaterPos = newWaterSource;
            return true;
        }

        // If there's no water source
        if (newWaterSource == Vector3i.zero)
            newWaterSource = BlockPos;

        // If this is an indirect water source, check its piping to make sure its connected to a BlockLiquidv2 or legit water source
        newWaterSource = WaterPipeManager.Instance.GetWaterForPosition(newWaterSource);
        if (newWaterSource == Vector3i.zero) // No water source connected?
            return false;

        WaterPos = newWaterSource;
        return true;
    }

    public Vector3i ScanForWater(Vector3i _blockPos, float range = -1f)
    {
        if (range == -1)
            range = _waterRange;

        var _world = GameManager.Instance.World;
        for (var x = -range; x <= range; x++)
        {
            for (var z = -range; z <= range; z++)
            {
                for (var y = _blockPos.y - 2; y <= _blockPos.y + 2; y++)
                {
                    var waterCheck = new Vector3i(_blockPos.x + x, y, _blockPos.z + z);

                    // If it's a straight up water block, grab it.
                    if (WaterPipeManager.Instance.IsWaterSource(waterCheck))
                        return waterCheck;
                }
            }
        }

        return Vector3i.zero;
    }
}