using UnityEngine;

internal class BlockSpawnerSDX : Block
{
    public string SpawnGroup;
    public string Task = "Wander";

    public override void Init()
    {
        base.Init();
        if (Properties.Values.ContainsKey("SpawnGroup"))
            SpawnGroup = Properties.Values["SpawnGroup"];

        if (Properties.Values.ContainsKey("Task"))
            Task = Properties.Values["Task"];
    }

    public void CheckForSpawn(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        if (string.IsNullOrEmpty(SpawnGroup))
        {
            Debug.Log("Spawner does not have a SpawnGroup property set: " + GetBlockName());
            return;
        }

        if (_blockValue.meta2 == 0)
        {
            var ClassID = 0;
            var EntityID = EntityGroups.GetRandomFromGroup(SpawnGroup, ref ClassID);
            var NewEntity = EntityFactory.CreateEntity(EntityID, _blockPos.ToVector3() + Vector3.up);
            if (NewEntity)
            {
                NewEntity.SetSpawnerSource(EnumSpawnerSource.Dynamic);
                GameManager.Instance.World.SpawnEntityInWorld(NewEntity);
                if (Task == "Stay")
                    EntityUtilities.SetCurrentOrder(NewEntity.entityId, EntityUtilities.Orders.Stay);
                if (Task == "Patrol")
                    EntityUtilities.SetCurrentOrder(NewEntity.entityId, EntityUtilities.Orders.Patrol);
                if (Task == "Wander")
                    EntityUtilities.SetCurrentOrder(NewEntity.entityId, EntityUtilities.Orders.Wander);

                _blockValue.meta2 = 1;
                _world.SetBlockRPC(_clrIdx, _blockPos, _blockValue);
            }
        }
    }

    public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        Debug.Log("OnBlockAdded()");
        base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
        CheckForSpawn(_world, _chunk.ClrIdx, _blockPos, _blockValue);
    }

    public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        Debug.Log("OnBlockLoaded()");
        base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
        CheckForSpawn(_world, _clrIdx, _blockPos, _blockValue);
    }
}