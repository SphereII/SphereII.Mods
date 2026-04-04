
using System.Collections.Generic;

// Updated BlockWaterPipeSDX
public class BlockWaterPipeSDX : BlockBaseWaterSystem // Assumes BlockBaseWaterSystem exists and is compatible
{
    // Optional: Properties like brokenPipe could be added if needed

    // Called when the block is destroyed or removed
    public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        InvalidateAll(_blockPos);
        BlockUtilitiesSDX.removeParticles(_blockPos);
        base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
    }

    // Helper: invalidate water path cache and immediately push updated state to all sprinklers.
    private static void InvalidateAll(Vector3i blockPos)
    {
        BlockWaterSourceSDX.RefreshAllSprinklers(blockPos);
    }

    // Called when the block is loaded into the world
    public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        InvalidateAll(_blockPos);
        base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
    }

    // Called when the block is first placed by a player/entity
    public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
    {
        InvalidateAll(_result.blockPos);
        base.PlaceBlock(_world, _result, _ea);
    }

    // Called when a neighboring block changes
    public override void OnNeighborBlockChange(WorldBase world, int _clrIdx, Vector3i _myBlockPos, BlockValue _myBlockValue,
                                           Vector3i _blockPosThatChanged, BlockValue _newNeighborBlockValue, BlockValue _oldNeighborBlockValue)
    {
        base.OnNeighborBlockChange(world, _clrIdx, _myBlockPos, _myBlockValue, _blockPosThatChanged, _newNeighborBlockValue, _oldNeighborBlockValue);
        InvalidateAll(_myBlockPos);
    }

    // GetCustomDescription method in BlockBaseWaterSystem likely handles showing water summary,
    // and it calls WaterPipeManager.GetWaterSummary, which was already updated. No change needed here.

} // End of BlockWaterPipeSDX class