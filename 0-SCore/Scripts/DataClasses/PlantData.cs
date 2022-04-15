public class PlantData
{
    public Vector3i BlockPos { get; set; } = Vector3i.zero;
    public Vector3i WaterPos { get; set; } = Vector3i.zero;
    public BlockValue blockValue { get; set; } = BlockValue.Air;
    
    private bool _requireWater = false;
    private int _waterRange = 5;

    
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
        if (!RequiresWater()) return;
        if (WaterPos == Vector3i.zero) return;

        // Damage the water block to simulate usage.
        var waterBlock = GameManager.Instance.World.GetBlock(WaterPos);
        if (waterBlock.Block is BlockLiquidv2 water )
        {
            if ( waterBlock.damage <= waterBlock.Block.MaxDamage)
                GameManager.Instance.World.SetBlockRPC(0, WaterPos, waterBlock);
            else
                GameManager.Instance.World.SetBlockRPC(0, WaterPos, BlockValue.Air);
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

        // Check what kind of block we have found.
        if(CropManager.Instance.IsWaterSource(newWaterSource))
        {
            WaterPos = newWaterSource;
            return true;
        }

        // If this is a water source, check its piping to make sure its connected od BlockLiquidv2
        var waterSourceBlock = GameManager.Instance.World.GetBlock(newWaterSource);
        if ( waterSourceBlock.Block is BlockWaterSourceSDX)
            newWaterSource = CropManager.Instance.WaterData.GetWaterSource(newWaterSource);

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
                    if (_world.IsWater(waterCheck))
                        return waterCheck;

                    // we did find a water source though, so let's see if we can use it.
                    var blockValue = GameManager.Instance.World.GetBlock(waterCheck);
                    if (blockValue.Block is BlockWaterSourceSDX)
                        return waterCheck;
                }
            }
        }

        return Vector3i.zero;
    }
}

