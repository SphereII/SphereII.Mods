using System;
using System.Collections.Generic;
using UnityEngine;

internal class BlockSpawnCube2SDX : Block
{

    private int OwnerEntityID = -1;

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
        EntityCreationData entityCreationData = new EntityCreationData();
        entityCreationData.id = -1;
        entityCreationData.entityClass = text.GetHashCode();
        entityCreationData.pos = _blockPos.ToVector3() + new Vector3(0.5f, 0.25f, 0.5f);
        entityCreationData.rot = new Vector3(0f, (float)(45 * (_blockValue.rotation & 3)), 0f);
        _chunk.AddEntityStub(entityCreationData);

        // Set up the tick delay to be pretty short, as we'll just destroy the block anyway.
        _world.GetWBT().AddScheduledBlockUpdate(0, _blockPos, this.blockID, 5UL);

    }

    public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
    {
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && GameManager.Instance.World.GetEntitiesInBounds(null, new Bounds(_blockPos.ToVector3(), Vector3.one * 2f)).Count == 0)
        {
            ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
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
            Vector3 transformPos = _blockPos.ToVector3() + new Vector3(0.5f, 0.25f, 0.5f);
            Vector3 rotation = new Vector3(0f, (float)(45 * (_blockValue.rotation & 3)), 0f);
            var entity = EntityFactory.CreateEntity(text.GetHashCode(), transformPos, rotation) as EntityAlive;

            // Set up ownership
            if (OwnerEntityID != -1 && !string.IsNullOrEmpty(setLeader))
                EntityUtilities.SetLeader(entity.entityId, OwnerEntityID);

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

            // Destroy the block after creating the entity.
            DamageBlock(_world, 0, _blockPos, _blockValue, Block.list[_blockValue.type].MaxDamage, -1, false, false);
        }
        return base.UpdateTick(_world, _clrIdx, _blockPos, _blockValue, _bRandomTick, _ticksIfLoaded, _rnd);
    }


    public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
    {
        if (Properties.Values.ContainsKey("Config"))
            this.OwnerEntityID = _ea.entityId;
        base.PlaceBlock(_world, _result, _ea);
    }
}