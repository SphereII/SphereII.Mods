using UnityEngine;
public class PlantData
{
    private static readonly string AdvFeatureClass = "CropManagement";

    public Vector3i BlockPos { get; set; } = Vector3i.zero;
    public Vector3i WaterPos { get; set; } = Vector3i.zero;
    public BlockValue blockValue { get; set; } = BlockValue.Air;
    
    private bool _requireWater = false;
    private int _waterRange = 5;
    public bool Visited = false;
    public PlantData(Vector3i _blockPos) : this(_blockPos, Vector3i.zero) { }
    public PlantData(Vector3i _blockPos, Vector3i _waterPos)
    {
        this.BlockPos = _blockPos;
        this.WaterPos = _waterPos;
        blockValue = GameManager.Instance.World.GetBlock(BlockPos);
        if (blockValue.Block.Properties.Values.ContainsKey("RequireWater"))
            _requireWater = StringParsers.ParseBool(blockValue.Block.Properties.Values["RequireWater"]);
        if (blockValue.Block.Properties.Values.ContainsKey("WaterRange"))
            _waterRange = int.Parse(blockValue.Block.Properties.Values["WaterRange"]);
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
        if (waterBlock.Block is BlockLiquidv2 water )
        {
            AdvLogging.DisplayLog(AdvFeatureClass, $"Consuming Water: {BlockPos} {WaterPos} {waterBlock.Block.GetBlockName()}");

            // Check the global value of damaging water for this growth cycle
            var damage = WaterPipeManager.Instance.GetWaterDamage(WaterPos);

            // If the blant block itself has its own water damage, use it instead.
            if (blockValue.Block.Properties.Contains("WaterDamage"))
                damage = blockValue.Block.Properties.GetInt("WaterDamage");

            // Grab the damage value from a) the water block itself, or b) the globa one
            waterBlock.damage += damage;
            if (waterBlock.damage <= waterBlock.Block.MaxDamage)
                GameManager.Instance.World.SetBlockRPC(0, WaterPos, waterBlock);
            else
            {
                AdvLogging.DisplayLog(AdvFeatureClass, $"Water is completely consumed here: {WaterPos}");
                GameManager.Instance.World.SetBlockRPC(0, WaterPos, BlockValue.Air);
            }
        }
    }
    public void Remove()
    {
        if (blockValue.Block is BlockPlantGrowing block)
        {
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
    public bool IsNearWater()
    {
        if ( WaterPos != Vector3i.zero )
            if ( GameManager.Instance.World.IsWater(WaterPos))
                return true;
        
        // search for a direct water source.
        var newWaterSource = ScanForWater(BlockPos);
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

    public Vector3i ScanForWater(Vector3i _blockPos, int range = -1)
    {
        if (range == -1)
            range = _waterRange;

        var _world = GameManager.Instance.World;
        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                for (int y = _blockPos.y - 2; y <= _blockPos.y + 2; y++)
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

