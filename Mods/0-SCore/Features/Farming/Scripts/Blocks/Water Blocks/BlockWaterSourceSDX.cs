using UnityEngine;

public class BlockWaterSourceSDX : BlockBaseWaterSystem
{
    private float WaterRange = 5f;
    private string waterType = "Limited";
    private static readonly int IsSprinklerOn = Animator.StringToHash("isSprinklerOn");

    public override void LateInit()
    {
        base.LateInit();
        if (this.Properties.Values.ContainsKey("WaterRange"))
            WaterRange = StringParsers.ParseFloat(this.Properties.Values["WaterRange"]);

        if (Properties.Values.ContainsKey("WaterType"))
            waterType = Properties.Values["WaterType"];
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
        ToggleSprinkler(_blockPos, false);
        WaterPipeManager.Instance.RemoveValve(_blockPos);
        
        StopSprinklerSound(_blockPos);

        base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
    }

    public override void OnBlockUnloaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue) {
        base.OnBlockUnloaded(_world, _clrIdx, _blockPos, _blockValue);
        StopSprinklerSound(_blockPos);

    }

    private void StopSprinklerSound(Vector3i _blockPos)
    {
        var _ebcd = GameManager.Instance.World.GetChunkFromWorldPos(_blockPos)?.GetBlockEntity(_blockPos);
        if (_ebcd != null && _ebcd.transform != null)
        {
            var audioSource = _ebcd.transform.GetComponentInChildren<AudioSource>();
            if (audioSource != null)
            {
                audioSource.Stop();
            }
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
        ToggleSprinkler(_blockPos, IsConnectedToWaterPipe(_blockPos));
    }
    public void ToggleSprinkler(Vector3i _blockPos, bool enabled = true)
    {
        if (GameManager.Instance.World.IsRemote() == false)
            GameManager.Instance.World.GetWBT().AddScheduledBlockUpdate(0, _blockPos, this.blockID, GetTickRate());

        var ebcd = GameManager.Instance.World.GetChunkFromWorldPos(_blockPos)?.GetBlockEntity(_blockPos);
        if (ebcd == null || ebcd.transform == null)
            return;

        var animator = ebcd.transform.GetComponentInChildren<Animator>();
        if (animator == null) return;

        var isConnected = IsConnectedToWaterPipe(_blockPos);

        if (enabled && isConnected)
        {
            animator.SetBool(IsSprinklerOn, true);
            WaterPipeManager.Instance.AddValve(_blockPos);
        }
        else
        {
            animator.SetBool(IsSprinklerOn, false);
            WaterPipeManager.Instance.RemoveValve(_blockPos);
        }
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

        foreach (Vector3i pos in adjacentPositions)
        {
            BlockValue blockValue = GameManager.Instance.World.GetBlock(pos);
        
            if (blockValue.Block is BlockWaterPipeSDX)
            {
                Vector3i waterSource = WaterPipeManager.Instance.GetWaterForPosition(pos);
                if (waterSource != Vector3i.zero)
                {
                    // The pipe is connected to a water source
                    return true;
                }
            }
        }

        return false;
    }

  
    public override ulong GetTickRate()
    {
        return (ulong)10f;
    }
    public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
    {
        ToggleSprinkler(_blockPos);
        return base.UpdateTick(_world, _clrIdx, _blockPos, _blockValue, _bRandomTick, _ticksIfLoaded, _rnd);
    }

 
}

