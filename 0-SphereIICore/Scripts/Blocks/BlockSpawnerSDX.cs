using UnityEngine;
class BlockSpawnerSDX : Block
{
    public string SpawnGroup;
    public string Task = "Wander";
    public override void Init()
    {
        base.Init();
        if (this.Properties.Values.ContainsKey("SpawnGroup"))
            SpawnGroup = this.Properties.Values["SpawnGroup"];

        if (this.Properties.Values.ContainsKey("Task"))
            Task = this.Properties.Values["Task"];
    }

    public void CheckForSpawn(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        if (string.IsNullOrEmpty(SpawnGroup))
        {
            Debug.Log("Spawner does not have a SpawnGroup property set: " + this.GetBlockName());
            return;
        }

        if (_blockValue.meta2 == 0)
        {
            int ClassID = 0;
            int EntityID = EntityGroups.GetRandomFromGroup(this.SpawnGroup, ref ClassID);
            Entity NewEntity = EntityFactory.CreateEntity(EntityID, _blockPos.ToVector3() + Vector3.up) as Entity;
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

                _blockValue.meta2 = (byte)1;
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

