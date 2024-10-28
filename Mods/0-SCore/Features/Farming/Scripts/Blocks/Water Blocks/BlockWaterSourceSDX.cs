using UnityEngine;

public class BlockWaterSourceSDX : BlockBaseWaterSystem
{
    private float WaterRange = 5f;
    private string waterType = "Limited";
    private static readonly int IsSprinklerOn = Animator.StringToHash("isSprinklerOn");
    private bool _muteSprinklerSound = false;
    public override void LateInit()
    {
        base.LateInit();
        if (this.Properties.Values.ContainsKey("WaterRange"))
            WaterRange = StringParsers.ParseFloat(this.Properties.Values["WaterRange"]);

        if (Properties.Values.ContainsKey("WaterType"))
            waterType = Properties.Values["WaterType"];
        
        if (this.Properties.Values.ContainsKey("MuteSound"))
            _muteSprinklerSound = StringParsers.ParseBool(this.Properties.Values["MuteSound"]);
    }

    public bool IsWaterSourceUnlimited() {
        return waterType.ToLower() == "unlimited";
    }
    public float GetWaterRange()
    {
        return WaterRange;
    }


    // This is not a direct source of water in itself. However, if it's connected to a series of BlockWaterPipeSDXs which connect up to 
    // a BlockLiquidV2, then it can act like the water block itself, providing the same range of water power as if its the block itself.
    // If there is no BlockWaterPipeSDXs connected to water, it does nothing.
    public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        WaterPipeManager.Instance.RemoveValve(_blockPos);
        base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
    }

    public override void OnBlockUnloaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue) {
        base.OnBlockUnloaded(_world, _clrIdx, _blockPos, _blockValue);
        StopSprinklerSound(_blockPos);

    }

    private void StopSprinklerSound(Vector3i _blockPos)
    {
        var ebcd = GameManager.Instance.World.GetChunkFromWorldPos(_blockPos)?.GetBlockEntity(_blockPos);
        if (ebcd == null || ebcd.transform == null) return;
        var audioSource = ebcd.transform.GetComponentInChildren<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
        if (_blockValue.ischild) return;

        var chunkCluster = _world.ChunkClusters[_clrIdx];
        if (chunkCluster == null) return;

        var chunk = (Chunk)chunkCluster.GetChunkFromWorldPos(_blockPos);
        if (chunk == null) return;
        chunk.AddEntityBlockStub(new BlockEntityData(_blockValue, _blockPos)
        {
            bNeedsTemperature = true
        });
        if (!_world.IsRemote())
        {
            _world.GetWBT().AddScheduledBlockUpdate(chunk.ClrIdx, _blockPos, this.blockID, this.GetTickRate());
        }
        WaterPipeManager.Instance.AddValve(_blockPos);

    }
  
    public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
        _chunk.AddEntityBlockStub(new BlockEntityData(_blockValue, _blockPos)
        {
            bNeedsTemperature = true
        });
        if (!_world.IsRemote())
        {
            _world.GetWBT().AddScheduledBlockUpdate(_chunk.ClrIdx, _blockPos, this.blockID, this.GetTickRate());
        }
        WaterPipeManager.Instance.AddValve(_blockPos);
    }
    public BlockEntityData GetBlockEntity(Vector3i position) {
        return GameManager.Instance.World.GetChunkFromWorldPos(position)?.GetBlockEntity(position);
    }
    public void ToggleSprinkler(Vector3i _blockPos, bool enabled = true) {
        var ebcd = GetBlockEntity(_blockPos);
        if (ebcd == null || ebcd.transform == null) return ;

        var animator = ebcd.transform.GetComponentInChildren<Animator>();
        if (animator == null) return ;
        if (enabled)
        {
            animator.SetBool(IsSprinklerOn, true);
        }
        else
        {
            animator.SetBool(IsSprinklerOn, false);
            StopSprinklerSound(_blockPos);
        }
        if ( _muteSprinklerSound)
            StopSprinklerSound(_blockPos);
    }

    private bool IsConnectedToWaterPipe(Vector3i _blockPos)
    {
        // If pipes are not required, always return true
        if (!CropManager.Instance.RequirePipesForSprinklers)
        {  
            return true;
        }

        Vector3i[] adjacentPositions = new Vector3i[]
        {
            _blockPos + Vector3i.forward,
            _blockPos + Vector3i.back,
            _blockPos + Vector3i.left,
            _blockPos + Vector3i.right,
            _blockPos + Vector3i.down
        };

        foreach (var pos in adjacentPositions)
        {
            
            var waterSource = WaterPipeManager.Instance.GetWaterForPosition(pos);
            if (WaterPipeManager.Instance.IsDirectWaterSource(waterSource)) return true;
            if (waterSource != Vector3i.zero)
            {
                return true;
            }
        }
        return false;
    }
    public override ulong GetTickRate()
    {
        return (ulong)10f;
    }
    public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd) {
        _world.GetWBT().AddScheduledBlockUpdate(_clrIdx, _blockPos, this.blockID, this.GetTickRate());
        var isConnected = IsConnectedToWaterPipe(_blockPos);
        ToggleSprinkler(_blockPos, isConnected);
        WaterPipeManager.Sync(_blockPos, isConnected);

        return true;
    }

}

