using Platform;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawnCubeRepeater : BlockSpawnCube2SDX
{
    public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue,
        bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
    {
        // Spawning logic should only run on the server.
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            // CRITICAL FLOW CONTROL: Destroy if max spawns are reached.
            if (_blockValue.meta >= _maxSpawned)
            {
                DestroySelf(_blockPos, _blockValue); // Calls the virtual method from base.
                return false; // Block should stop ticking and be removed.
            }

            // If max spawns are NOT yet reached, attempt to spawn an entity.
            // All the complex spawning logic is now encapsulated in the base's TrySpawnEntity.
            if (TrySpawnEntity(_world, _clrIdx, _blockPos, _blockValue))
            {
                // If an entity was successfully spawned this tick, increment the meta count.
                // The TrySpawnEntity method itself does NOT increment meta, allowing derived classes to control it.
                _blockValue.meta++;
                GameManager.Instance.World.SetBlockRPC(_blockPos, _blockValue);
                // Debug.Log($"BlockControlledSpawnCube: Spawned entity. Current meta: {_blockValue.meta}"); // For debugging
            }

            // In either case (spawn success or failure due to temporary conditions),
            // if max spawns are not reached, schedule another tick for the next attempt.
            _world.GetWBT().AddScheduledBlockUpdate(0, _blockPos, blockID, GetTickRate());
            return true; // Keep the block ticking for future spawns.
        }

        // For clients, simply return true to keep the block rendered.
        // Its state will be synchronized from the server.
        return true;
    }
}