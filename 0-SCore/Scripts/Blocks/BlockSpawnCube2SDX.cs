using Platform;
using System;
using System.Collections.Generic;
using UnityEngine;

internal class BlockSpawnCube2SDX : BlockMotionSensor
{
    private int OwnerID = -1;
    public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);

        if (!Properties.Values.ContainsKey("Config")) return;
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;

        var signText = Properties.Values["Config"];
        var text = PathingCubeParser.GetValue(signText, "ec");
        if (string.IsNullOrEmpty(text))
            text = PathingCubeParser.GetValue(signText, "eg");
        if (string.IsNullOrEmpty(text)) return;

        // Match the rotation, and create the stub for the entity
        var entityCreationData = new EntityCreationData();
        entityCreationData.id = -1;
        entityCreationData.entityClass = text.GetHashCode();
        entityCreationData.pos = _blockPos.ToVector3() + new Vector3(0.5f, 0.25f, 0.5f);

        // We need to check if this is a block entity or not, and match the rotation of the entity to the block, in case its a model preview.
        var rotation = new Vector3(0f, (float)(45f * (_blockValue.rotation & 3)), 0f);
        var blockEntity = ((Chunk)_world.GetChunkFromWorldPos(_blockPos)).GetBlockEntity(_blockPos);
        if (blockEntity != null && blockEntity.bHasTransform)
            rotation = blockEntity.transform.rotation.eulerAngles;

        entityCreationData.rot = rotation;
        _chunk.AddEntityStub(entityCreationData);

        // Set up the tick delay to be pretty short, as we'll just destroy the block anyway.
        _world.GetWBT().AddScheduledBlockUpdate(0, _blockPos, blockID, 10UL);
    }

    public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
    {
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            var chunkCluster = _world.ChunkClusters[_clrIdx];
            if (chunkCluster == null) return false;

            if ((Chunk)chunkCluster.GetChunkFromWorldPos(_blockPos) == null) return false;

            if (!Properties.Values.ContainsKey("Config")) return false;

            var signText = Properties.Values["Config"];
            var text = PathingCubeParser.GetValue(signText, "ec");
            if (string.IsNullOrEmpty(text))
                text = PathingCubeParser.GetValue(signText, "eg");
            if (string.IsNullOrEmpty(text)) return false;

            // Read the sign for expected values.
            var Task = PathingCubeParser.GetValue(signText, "task");
            var Buff = PathingCubeParser.GetValue(signText, "buff");
            var PathingCode = PathingCubeParser.GetValue(signText, "pc");
            var setLeader = PathingCubeParser.GetValue(signText, "leader");

            // Match the rotation of the block for the entity, so it faces in the same direction.
            var transformPos = _blockPos.ToVector3() + new Vector3(0.5f, 0.25f, 0.5f);

            // We need to check if this is a block entity or not, and match the rotation of the entity to the block, in case its a model preview.
            var rotation = new Vector3(0f, (float)(45f * (_blockValue.rotation & 3)), 0f);
            var blockEntity = ((Chunk)_world.GetChunkFromWorldPos(_blockPos)).GetBlockEntity(_blockPos);
            if (blockEntity != null && blockEntity.bHasTransform)
                rotation = blockEntity.transform.rotation.eulerAngles;

            var entity = EntityFactory.CreateEntity(text.GetHashCode(), transformPos, rotation) as EntityAlive;

            // We need to apply the buffs during this scan, as the creation of the entity + adding buffs is not really MP safe.
            if (Task.ToLower() == "stay")
                entity.Buffs.AddBuff("buffOrderStay");
            if (Task.ToLower() == "wander")
                entity.Buffs.AddBuff("buffOrderWander");
            if (Task.ToLower() == "guard")
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

            entity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
            GameManager.Instance.World.SpawnEntityInWorld(entity);

            // We are using the tile entity to transfer the owner ID from the client to the player.
            var tileEntity = _world.GetTileEntity(0, _blockPos) as TileEntityPoweredTrigger;
            if (tileEntity != null)
            {
                var persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
                var playerData = persistentPlayerList.GetPlayerData(tileEntity.GetOwner());
                OwnerID = playerData.EntityId;

            }
            // Set up ownership, but only after the entity is spawned.
            if (OwnerID > 0 && !string.IsNullOrEmpty(setLeader))
                EntityUtilities.SetLeaderAndOwner(entity.entityId, (int)OwnerID, false);

            // Destroy the block after creating the entity.
            DamageBlock(_world, 0, _blockPos, _blockValue, Block.list[_blockValue.type].MaxDamage, -1, false, false);

        }
        return base.UpdateTick(_world, _clrIdx, _blockPos, _blockValue, _bRandomTick, _ticksIfLoaded, _rnd);
    }
   
}