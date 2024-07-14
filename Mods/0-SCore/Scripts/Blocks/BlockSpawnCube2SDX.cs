using Platform;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawnCube2SDX : BlockMotionSensor
{
    private int OwnerID = -1;
    private float _tickRate = 10UL;
    private int _maxSpawned = 1;
    private int _numberToSpawn = 1;
    private int _spawnRadius = 0;
    private int _spawnArea = 15;

    private string _entityGroup = "";
    private string _signText = "";
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
    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        return "";

    }
   
    public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;
        if (GameManager.Instance.IsEditMode()) return;

        var entityId = -1;
        var text = PathingCubeParser.GetValue(_signText, "ec");
        if (string.IsNullOrEmpty(text))
        {
            var group = PathingCubeParser.GetValue(_signText, "eg");
            if (string.IsNullOrEmpty(group))
            {
                if (string.IsNullOrEmpty(_entityGroup))
                    return;
                group = _entityGroup;
            }
            var ClassID = 0;
            entityId = EntityGroups.GetRandomFromGroup(group, ref ClassID);
            if (entityId == 0) // Invalid group.
                return;
        }
        else
        {
            entityId = text.GetHashCode();
        }

        // Match the rotation, and create the stub for the entity
        var entityCreationData = new EntityCreationData();
        entityCreationData.id = -1;
        entityCreationData.entityClass = entityId;
        entityCreationData.pos = _blockPos.ToVector3() + new Vector3(0.5f, 0.25f, 0.5f);

        // We need to check if this is a block entity or not, and match the rotation of the entity to the block, in case its a model preview.
        var rotation = new Vector3(0f, (float)(45f * (_blockValue.rotation & 3)), 0f);
        var blockEntity = _chunk.GetBlockEntity(_blockPos);
        if (blockEntity != null && blockEntity.bHasTransform)
            rotation = blockEntity.transform.rotation.eulerAngles;

        entityCreationData.rot = rotation;
        _chunk.AddEntityStub(entityCreationData);

        // We'll use the Meta value as the spawn counter.
        _blockValue.meta = 0;
        GameManager.Instance.World.SetBlockRPC(_blockPos, _blockValue);

        // Set up the tick delay to be pretty short, as we'll just destroy the block anyway.
        _world.GetWBT().AddScheduledBlockUpdate(0, _blockPos, blockID, (ulong)1UL);
    }

    private void DestroySelf(Vector3i _blockPos, BlockValue _blockValue)
    {
        var keep = PathingCubeParser.GetValue(_signText, "keep");
        if ( string.IsNullOrEmpty(keep) )
            DamageBlock(GameManager.Instance.World, 0, _blockPos, _blockValue, Block.list[_blockValue.type].MaxDamage, -1, null, false);
        else
            GameManager.Instance.World.GetWBT().AddScheduledBlockUpdate(0, _blockPos, blockID, (ulong)10000UL);

    }

    public void ApplySignData(EntityAlive entity, Vector3i _blockPos)
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
            // Use the buff that issues the "guard" order, not the one that issues the "stay" order
            entity.Buffs.AddBuff("buffOrderGuard");
        if (Task.ToLower() == "follow")
            entity.Buffs.AddBuff("buffOrderFollow");

        foreach (var buff in Buff.Split(','))
            entity.Buffs.AddBuff(buff);

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
                OwnerID = playerData.EntityId;

            // Set up ownership, but only after the entity is spawned.
            if (OwnerID > 0 && !string.IsNullOrEmpty(setLeader))
            {
                EntityUtilities.SetLeaderAndOwner(entity.entityId, (int)OwnerID, false);
            }
        }
    }

    public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
    {
        Vector3 myVector = new Vector3(1, 2, 1);

        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            if (_blockValue.meta >= _maxSpawned)
            {
                DestroySelf(_blockPos, _blockValue);
                return false;
            }
            var chunkCluster = _world.ChunkClusters[_clrIdx];
            if (chunkCluster == null) return false;

            if ((Chunk)chunkCluster.GetChunkFromWorldPos(_blockPos) == null) return false;

            var entityId = -1;
            var text = PathingCubeParser.GetValue(_signText, "ec");
            if (string.IsNullOrEmpty(text))
            {

                var group = PathingCubeParser.GetValue(_signText, "eg");
                if (string.IsNullOrEmpty(group))
                {
                    if (string.IsNullOrEmpty(_entityGroup))
                    {
                        DestroySelf(_blockPos, _blockValue);
                        return false;
                    }
                    group = _entityGroup;
                }

                var ClassID = 0;
                entityId = EntityGroups.GetRandomFromGroup(group, ref ClassID);
                if (entityId == 0) // Invalid group.
                {
                    // Destroy the block after creating the entity.
                    DestroySelf(_blockPos, _blockValue);
                    return false;
                }
            }
            else
            {
                entityId = text.GetHashCode();
            }

            // Match the rotation of the block for the entity, so it faces in the same direction.
            var transformPos = _blockPos.ToVector3() + new Vector3(0.5f, 0.25f, 0.5f);
            if (_spawnRadius > 0)
            {
                var areaSize = new Vector3(_spawnArea, _spawnArea, _spawnArea);

                int x;
                int y;
                int z;
                if (!GameManager.Instance.World.FindRandomSpawnPointNearPosition(_blockPos, 15, out x, out y, out z, areaSize, true))
                {
                    return true;
                }
                transformPos.x = x;
                transformPos.y = y;
                transformPos.z = z;
            }
            // We need to check if this is a block entity or not, and match the rotation of the entity to the block, in case its a model preview.
            var rotation = new Vector3(0f, (float)(45f * (_blockValue.rotation & 3)), 0f);
            var blockEntity = ((Chunk)_world.GetChunkFromWorldPos(_blockPos)).GetBlockEntity(_blockPos);
            if (blockEntity != null && blockEntity.bHasTransform)
                rotation = blockEntity.transform.rotation.eulerAngles;

            var entity = EntityFactory.CreateEntity(entityId, transformPos, rotation) as EntityAlive;
            if (entity == null)
            {
                Log.Out($"No entity created: {_signText}");
                return false;
            }
            entity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
            GameManager.Instance.World.SpawnEntityInWorld(entity);

            ApplySignData(entity as EntityAlive, _blockPos);

            _blockValue.meta++;
            GameManager.Instance.World.SetBlockRPC(_blockPos, _blockValue);
        }

        DestroySelf(_blockPos, _blockValue);

        return true;
    }

}