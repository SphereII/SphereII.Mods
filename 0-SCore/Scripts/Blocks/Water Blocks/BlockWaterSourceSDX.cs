public class BlockWaterSourceSDX : BlockBaseWaterSystem
{
    // This is not a direct source of water in itself. However, if it's connected to a series of BlockWaterPipeSDXs which connect up to 
    // a BlockLiquidV2, then it can act like the water block itself, providing the same range of water power as if its the block itself.
    // If there is no BlockWaterPipeSDXs connected to water, it does nothing.
    public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
    }

}

