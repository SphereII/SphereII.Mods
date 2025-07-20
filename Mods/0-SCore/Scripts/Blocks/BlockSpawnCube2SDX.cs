using Platform;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawnCube2SDX : BlockMotionSensor
{
    private int OwnerID = -1;
    private float _tickRate = 10UL; // Default tick rate
    protected int _maxSpawned = 1;    // Max entities to spawn
    private int _numberToSpawn = 1; // Number of entities to spawn per tick (if applicable)
    private int _spawnRadius = 0;   // Radius for random spawn points
    private int _spawnArea = 15;    // Area for random spawn points

    private string _entityGroup = ""; // Entity group defined in XML
    private string _signText = "";    // Text from a sign, used for config

    public override void Init()
    {
        base.Init();
        if (Properties.Values.ContainsKey("TickRate"))
            Properties.ParseFloat("TickRate", ref _tickRate);
        if (Properties.Values.ContainsKey("MaxSpawned"))
            Properties.ParseInt("MaxSpawned", ref _maxSpawned);
        if (Properties.Values.ContainsKey("NumberToSpawn"))
            Properties.ParseInt("NumberToSpawn", ref _numberToSpawn);
        if (Properties.Values.ContainsKey("SpawnRadius"))
            Properties.ParseInt("SpawnRadius", ref _spawnRadius);
        if (Properties.Values.ContainsKey("SpawnArea"))
            Properties.ParseInt("SpawnArea", ref _spawnArea);
        if (Properties.Values.ContainsKey("EntityGroup"))
            _entityGroup = Properties.Values["EntityGroup"];
        if (Properties.Values.ContainsKey("Config"))
            _signText = Properties.Values["Config"];
    }

    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos,
        EntityAlive _entityFocusing)
    {
        return "";
    }

    public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue,
        PlatformUserIdentifierAbs _addedByPlayer)
    {
        base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;
        if (GameManager.Instance.IsEditMode()) return;

        // Schedule an initial tick for the block
        _world.GetWBT().AddScheduledBlockUpdate(0, _blockPos, blockID, (ulong)1UL);
    }

    // Made public virtual by your previous request, which is good for overriding.
    public virtual void DestroySelf(Vector3i _blockPos, BlockValue _blockValue)
    {
        var keep = PathingCubeParser.GetValue(_signText, "keep");
        if (string.IsNullOrEmpty(keep))
            DamageBlock(GameManager.Instance.World, 0, _blockPos, _blockValue, Block.list[_blockValue.type].MaxDamage,
                -1, null, false);
        else
            GameManager.Instance.World.GetWBT().AddScheduledBlockUpdate(0, _blockPos, blockID, (ulong)10000UL);
    }

    // Made public virtual by your previous request, which is good for overriding.
    public virtual void ApplySignData(EntityAlive entity, Vector3i _blockPos)
    {
        // Read the sign for expected values.
        var Task = PathingCubeParser.GetValue(_signText, "task");
        var Buff = PathingCubeParser.GetValue(_signText, "buff");
        var PathingCode = PathingCubeParser.GetValue(_signText, "pc");
        var setLeader = PathingCubeParser.GetValue(_signText, "leader");

        if (Task.ToLower() == "stay")
            entity.Buffs.AddBuff("buffOrderStay");
        if (Task.ToLower() == "wander")
            entity.Buffs.AddBuff("buffOrderWander");
        if (Task.ToLower() == "guard")
            entity.Buffs.AddBuff("buffOrderGuard");
        if (Task.ToLower() == "follow")
            entity.Buffs.AddBuff("buffOrderFollow");

        if (!string.IsNullOrEmpty(Buff))
        {
            foreach (var buff in Buff.Split(','))
                if (!string.IsNullOrEmpty(buff.Trim())) // Ensure no empty strings from split
                    entity.Buffs.AddBuff(buff.Trim());
        }


        // Set up the pathing cube if available
        if (!string.IsNullOrEmpty(PathingCode))
        {
            if (StringParsers.TryParseFloat(PathingCode, out var pathingCode))
                entity.Buffs.SetCustomVar("PathingCode", pathingCode);
        }

        // We are using the tile entity to transfer the owner ID from the client to the player.
        var tileEntity = GameManager.Instance.World.GetTileEntity(0, _blockPos) as TileEntityPoweredTrigger;
        if (tileEntity != null)
        {
            var persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
            var playerData = persistentPlayerList.GetPlayerData(tileEntity.GetOwner());
            if (playerData != null)
                OwnerID = playerData.EntityId; // Set OwnerID for this instance of the block class

            // Set up ownership, but only after the entity is spawned.
            if (OwnerID > 0 && !string.IsNullOrEmpty(setLeader))
            {
                EntityUtilities.SetLeaderAndOwner(entity.entityId, (int)OwnerID, false);
            }
        }
    }

    // New: Helper method to provide the tick rate as ulong for scheduling.
    protected ulong GetTickRate()
    {
        // Ensure _tickRate is positive to avoid issues with scheduling
        if (_tickRate <= 0f) return 10UL; // Default to 10 ticks if misconfigured
        return (ulong)_tickRate;
    }

    /// <summary>
    /// Attempts to spawn an entity based on the block's configuration.
    /// Does NOT handle _blockValue.meta increment or scheduling.
    /// </summary>
    /// <returns>True if an entity was successfully created and spawned, false otherwise.</returns>
    protected virtual bool TrySpawnEntity(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        var size = Vector3.one * 2f;
        if (isMultiBlock)
        {
            size = multiBlockPos.dim;
        }

        var chunkCluster = _world.ChunkClusters[_clrIdx];
        if (chunkCluster == null) return false;
        if ((Chunk)chunkCluster.GetChunkFromWorldPos(_blockPos) == null) return false;

        var entityId = -1;
        var text = PathingCubeParser.GetValue(_signText, "ec"); // Explicit Class ID
        if (string.IsNullOrEmpty(text))
        {
            var group = PathingCubeParser.GetValue(_signText, "eg"); // Entity Group
            if (string.IsNullOrEmpty(group))
            {
                if (string.IsNullOrEmpty(_entityGroup))
                {
                    // No entity group from sign or properties, cannot proceed.
                    Debug.LogWarning($"BlockSpawnCube2SDX at {_blockPos}: No EntityGroup or 'eg' specified. Cannot spawn.");
                    return false;
                }
                group = _entityGroup;
            }

            var ClassID = 0;
            entityId = EntityGroups.GetRandomFromGroup(group, ref ClassID);
            if (entityId == 0)
            {
                Debug.LogWarning($"BlockSpawnCube2SDX at {_blockPos}: Entity group '{group}' is invalid or empty. Cannot spawn.");
                return false;
            }
        }
        else
        {
                entityId = text.GetHashCode();
        }

        var transformPos = _blockPos.ToVector3() + new Vector3(0.5f, 0.25f, 0.5f);
        if (_spawnRadius > 0)
        {
            int x, y, z;
            if (!GameManager.Instance.World.FindRandomSpawnPointNearPosition(_blockPos, 15, out x, out y, out z, new Vector3(_spawnArea, _spawnArea, _spawnArea), true))
            {
                return false; // Failed to find a suitable random spawn point
            }
            transformPos.x = x;
            transformPos.y = y;
            transformPos.z = z;
        }

        var rotation = new Vector3(0f, (float)(45f * (_blockValue.rotation & 3)), 0f);
        var blockEntity = ((Chunk)_world.GetChunkFromWorldPos(_blockPos)).GetBlockEntity(_blockPos);
        if (blockEntity != null && blockEntity.bHasTransform)
            rotation = blockEntity.transform.rotation.eulerAngles;

        var entity = EntityFactory.CreateEntity(entityId, transformPos, rotation) as EntityAlive;
        if (entity == null)
        {
            Debug.LogWarning($"BlockSpawnCube2SDX at {_blockPos}: Failed to create entity with ID {entityId}.");
            return false;
        }

        entity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
        GameManager.Instance.World.SpawnEntityInWorld(entity);
        ApplySignData(entity, _blockPos);

        return true; // Entity successfully spawned
    }

    // The base class's default UpdateTick behavior:
    // Spawns one entity, increments meta, schedules next tick, and destroys itself if maxSpawned is reached.
    public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue,
        bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
    {
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            if (_blockValue.meta >= _maxSpawned)
            {
                DestroySelf(_blockPos, _blockValue);
                return false; // Max spawns reached, stop ticking
            }

            // Attempt to spawn an entity using the common logic
            if (TrySpawnEntity(_world, _clrIdx, _blockPos, _blockValue))
            {
                // If successful, increment meta and update block RPC
                _blockValue.meta++;
                GameManager.Instance.World.SetBlockRPC(_blockPos, _blockValue);
                // Debug.Log($"BlockSpawnCube2SDX: Spawned entity. Current meta: {_blockValue.meta}"); // For debugging
            }
            DestroySelf(_blockPos, _blockValue);

        }

        // For clients, just ensure the block remains in the world.
        return true;
    }
}