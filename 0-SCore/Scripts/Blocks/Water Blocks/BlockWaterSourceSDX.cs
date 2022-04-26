using UnityEngine;

public class BlockWaterSourceSDX : BlockBaseWaterSystem
{
    // This is not a direct source of water in itself. However, if it's connected to a series of BlockWaterPipeSDXs which connect up to 
    // a BlockLiquidV2, then it can act like the water block itself, providing the same range of water power as if its the block itself.
    // If there is no BlockWaterPipeSDXs connected to water, it does nothing.
    public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
       // ToggleSprinkler(_blockPos);
        base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
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
    }

    public void ToggleSprinkler(Vector3i _blockPos)
    {
        if ( GameManager.Instance.World.IsRemote() == false)
            GameManager.Instance.World.GetWBT().AddScheduledBlockUpdate(0, _blockPos, this.blockID, GetTickRate());

        // Check to see if we have a transform. If not, no need to try to animate.
        var _ebcd = GameManager.Instance.World.GetChunkFromWorldPos(_blockPos).GetBlockEntity(_blockPos);
        if (_ebcd == null || _ebcd.transform == null)
            return;

        // Confirm the existence of an animator.
        var animator = _ebcd.transform.GetComponentInChildren<Animator>();
        if (animator == null) return;

        //  No water, no sprinkler
        if (WaterPipeManager.Instance.GetWaterForPosition(_blockPos) == Vector3i.zero)
        {
            animator.SetBool("isSprinklerOn", false);
            return;
        }

        animator.SetBool("isSprinklerOn", true);
        //var sprinklerOn = animator.GetBool("isSprinklerOn");
        //if (sprinklerOn == false)
        //{
        //    animator.SetBool("isSprinklerOn", true);
        //}
        //else
        //{
        //    animator.SetBool("isSprinklerOn", false);
        //}
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

