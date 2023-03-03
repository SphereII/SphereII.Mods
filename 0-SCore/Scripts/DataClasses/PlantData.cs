using System;
using UnityEngine;
public class PlantData
{
    private static readonly string AdvFeatureClass = "CropManagement";

    public Vector3i BlockPos { get; set; } = Vector3i.zero;
    public Vector3i WaterPos { get; set; } = Vector3i.zero;
    public BlockValue blockValue { get; set; } = BlockValue.Air;
    
    private bool _requireWater = false;
    private float _waterRange = 5f;
    public bool Visited = false;
    private string waterParticle;

    public PlantData(Vector3i _blockPos) : this(_blockPos, Vector3i.zero) { }
    public PlantData(Vector3i _blockPos, Vector3i _waterPos)
    {
        this.BlockPos = _blockPos;
        this.WaterPos = _waterPos;
        blockValue = GameManager.Instance.World.GetBlock(BlockPos);

        if (blockValue.Block.Properties.Values.ContainsKey("RequireWater"))
            _requireWater = StringParsers.ParseBool(blockValue.Block.Properties.Values["RequireWater"]);

        if (blockValue.Block.Properties.Values.ContainsKey("WaterRange"))
            _waterRange = StringParsers.ParseFloat(blockValue.Block.Properties.Values["WaterRange"]);

         waterParticle = Configuration.GetPropertyValue(AdvFeatureClass, "WaterParticle");
        if (string.IsNullOrWhiteSpace(waterParticle))
            waterParticle = "NoParticle";


    }

    public void Manage()
    {
        Log.Out($"Checking for bugs at {BlockPos}");
        Visited = true;
        LastMaintained = GameManager.Instance.World.GetWorldTime();

    }
    public void RegisterPlant()
    {
        blockValue = GameManager.Instance.World.GetBlock(BlockPos);
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

    public void Consume()
    {

        AdvLogging.DisplayLog(AdvFeatureClass, $"Last Checked: {BlockPos} {WaterPos} Checked at: {LastCheck} Maintained at: {LastMaintained} Current Time: {GameManager.Instance.World.GetWorldTime()} Difference: {GameManager.Instance.World.GetWorldTime() - LastCheck}");

        if (!RequiresWater()) return;
        if (WaterPos == Vector3i.zero) return;

        // If its raining, don't consume water.
        if ((double)WeatherManager.Instance.GetCurrentRainfallValue() > 0.25)
        {
            AdvLogging.DisplayLog(AdvFeatureClass, $"Using Rain Water for {BlockPos}.");
            return;
        }

        // Damage the water block to simulate usage.
        var waterBlock = GameManager.Instance.World.GetBlock(WaterPos);

        var _waterSource = WaterPos;

        
        // If the water block is a sprinkler, find out where its getting its water from.
        if ( waterBlock.Block is BlockWaterSourceSDX waterSource)
        {
            if ( waterSource.IsWaterSourceUnlimited() )
            {
                ToggleWaterParticle();
                return;
            }
            _waterSource = WaterPipeManager.Instance.GetWaterForPosition(WaterPos);
            waterBlock = GameManager.Instance.World.GetBlock(_waterSource);

        }
        if (waterBlock.Block is BlockLiquidv2 water )
        {
            AdvLogging.DisplayLog(AdvFeatureClass, $"Consuming Water: {BlockPos} {_waterSource} {waterBlock.Block.GetBlockName()}");

            if (water.Properties.Values.ContainsKey("WaterType") )
            {
                if (water.Properties.Values["WaterType"].ToLower() == "unlimited")
                {
                    ToggleWaterParticle();
                    return;
                }
            }

            // Check the global value of damaging water for this growth cycle
            var damage = WaterPipeManager.Instance.GetWaterDamage(_waterSource);

            // If the blant block itself has its own water damage, use it instead.
            if (blockValue.Block.Properties.Contains("WaterDamage"))
                damage = blockValue.Block.Properties.GetInt("WaterDamage");

            // Grab the damage value from a) the water block itself, or b) the globa one
            waterBlock.damage += damage;
            if (waterBlock.damage <= waterBlock.Block.MaxDamage)
                GameManager.Instance.World.SetBlockRPC(0, _waterSource, waterBlock);
            else
            {
                AdvLogging.DisplayLog(AdvFeatureClass, $"Water is completely consumed here: {_waterSource}");
                GameManager.Instance.World.SetBlockRPC(0, _waterSource, BlockValue.Air);
            }

            ToggleWaterParticle();
        }
    }

    private void ToggleWaterParticle()
    {
        if (GameManager.Instance.HasBlockParticleEffect(BlockPos))
            BlockUtilitiesSDX.removeParticles(BlockPos);

        var _waterParticle = GetWaterParticle();
        BlockUtilitiesSDX.addParticlesCentered(_waterParticle, BlockPos);

    }
    private string GetWaterParticle()
    {
        var block = GameManager.Instance.World.GetBlock(BlockPos);

        var _waterParticle = waterParticle;
        if (block.Block.Properties.Contains("WaterParticle"))
            _waterParticle = block.Block.Properties.GetString("WaterParticle");

        if (block.Block.blockMaterial.Properties.Contains("WaterParticle"))
            _waterParticle = block.Block.blockMaterial.Properties.GetString("WaterParticle");

        return _waterParticle;
    }
    public void Remove()
    {
        if (blockValue.Block is BlockPlantGrowing block)
        {
            BlockUtilitiesSDX.removeParticles(BlockPos);
            CropManager.Instance.Remove(BlockPos);
            var chunk = GameManager.Instance.World.GetChunkFromWorldPos(BlockPos) as Chunk;
            if (chunk == null) return;
            block.CheckPlantAlive(GameManager.Instance.World, chunk.ClrIdx, BlockPos, blockValue);
        }
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

        if ( WaterPos != Vector3i.zero )
            if ( GameManager.Instance.World.IsWater(WaterPos))
                return true;

        // Check if we still have a water valve reference, and its on.
        var valves = WaterPipeManager.Instance.GetWaterValves();

        // Check to see if there's a valve that can provide coverage for this.
        foreach (var valve in valves)
        {
            var blockValue = GameManager.Instance.World.GetBlock(valve.Key);
            if (blockValue.Block is BlockWaterSourceSDX waterBlock)
            {
                if (waterBlock.IsWaterSourceUnlimited()) return true;

                // If the valve itself is not connected to water, skip it.
                if (WaterPipeManager.Instance.GetWaterForPosition(valve.Key) == Vector3i.zero)
                    continue;

                float num = Vector3.Distance(BlockPos, valve.Key);
                if (num <= waterBlock.GetWaterRange())
                {
                    WaterPos = valve.Key;
                    return true;
                }
            }
        }
        // search for a direct water source.
        var newWaterSource = ScanForWater(BlockPos, waterRange);
        if(WaterPipeManager.Instance.IsDirectWaterSource(newWaterSource))
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

