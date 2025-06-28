
using System.Collections.Generic;

// Updated BlockWaterPipeSDX
public class BlockWaterPipeSDX : BlockBaseWaterSystem // Assumes BlockBaseWaterSystem exists and is compatible
{
    // Optional: Properties like brokenPipe could be added if needed

    // Called when the block is destroyed or removed
    public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        // 1. Invalidate the WaterPipeManager's result cache.
        //    Use InvalidateWaterCacheNear for potentially better performance than global clear.
        WaterPipeManager.Instance.InvalidateWaterCacheNear(_blockPos);
        // Or, if granular invalidation causes issues, revert to global clear:
        // WaterPipeManager.Instance.InvalidateWaterCache();

        // 2. Trigger recheck for all known active sprinklers
        HashSet<Vector3i> activeSprinklers = WaterPipeManager.Instance.GetWaterValves();
        if (activeSprinklers != null)
        {
            // Iterate directly over the HashSet
            foreach (var sprinklerPos in activeSprinklers)
            {
                // Call the static method on BlockWaterSourceSDX to invalidate its cache
                BlockWaterSourceSDX.InvalidateConnectionCache(sprinklerPos);
            }
        }

        // 3. Original cleanup (if BlockBaseWaterSystem doesn't handle it)
        BlockUtilitiesSDX.removeParticles(_blockPos); // Assuming BlockUtilitiesSDX is available
        base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
    }

    // Called when the block is loaded into the world
    public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        // Invalidate cache near this pipe when loaded, as its presence might affect paths
        WaterPipeManager.Instance.InvalidateWaterCacheNear(_blockPos);
        // Or: WaterPipeManager.Instance.InvalidateWaterCache();
        base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
    }

    // Called when the block is first placed by a player/entity
    public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
    {
        // Invalidate cache near this pipe when placed
        WaterPipeManager.Instance.InvalidateWaterCacheNear(_result.blockPos);
        // Or: WaterPipeManager.Instance.InvalidateWaterCache();
        base.PlaceBlock(_world, _result, _ea);
    }

    // Called when a neighboring block changes
    public override void OnNeighborBlockChange(WorldBase world, int _clrIdx, Vector3i _myBlockPos, BlockValue _myBlockValue,
                                           Vector3i _blockPosThatChanged, BlockValue _newNeighborBlockValue, BlockValue _oldNeighborBlockValue)
    {
        base.OnNeighborBlockChange(world, _clrIdx, _myBlockPos, _myBlockValue, _blockPosThatChanged, _newNeighborBlockValue, _oldNeighborBlockValue);

        // Invalidate cache near this pipe when a neighbor changes, as connectivity might change
        WaterPipeManager.Instance.InvalidateWaterCacheNear(_myBlockPos);
        // Or: WaterPipeManager.Instance.InvalidateWaterCache();

        // Original logic to check pipe count seems less relevant now that water pathing is dynamic.
        // The cache invalidation is the more important action here.

        // If you need to force an immediate re-check for blocks *using* this pipe,
        // more complex logic would be needed to find dependent blocks. Relying on
        // sprinklers' own UpdateTick re-checking after cache invalidation is simpler.
    }

    // GetCustomDescription method in BlockBaseWaterSystem likely handles showing water summary,
    // and it calls WaterPipeManager.GetWaterSummary, which was already updated. No change needed here.

} // End of BlockWaterPipeSDX class