using System;
using System.Collections.Generic;
using UnityEngine;

internal class BlockSpawnCube2SDX : BlockPlayerSign
{

    private int OwnerEntityID = -1;


    public void CheckForSpawn(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool force = false)
    {
      //  if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            //return;

        if (!Properties.Values.ContainsKey("Config"))
            return;

        ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
        if (chunkCluster == null)
        {
            return;
        }
        Chunk chunk = (Chunk)chunkCluster.GetChunkFromWorldPos(_blockPos);
        if (chunk == null)
        {
            return;
        }

        var signText = Properties.Values["Config"];
        var entityClassID = PathingCubeParser.GetValue(signText, "entityid");

        // If there's already an entityID, check 
        if (!string.IsNullOrEmpty(entityClassID))
        {
            // make sure its an int.
            if (StringParsers.TryParseSInt32(entityClassID, out var entityid))
            {
                // Check if the entity is still spawned, and if so, don't respawn.
                var spawnedEntity = GameManager.Instance.World.GetEntity(entityid);
                if (spawnedEntity != null)
                    return;
            }
        }
        EntityAlive myEntity = null;
        EntityAlive leader = null;

        // entityclass:zombieWightFeral;task:wander
        if (string.IsNullOrEmpty(signText))
            return;

        try
        {
            // Read the entity class
            // ec = entityclass:   ec=zombieBoe
            // eg = entitygroup:   eg=ZombiesAll
            // task = Tasks:        Wander, Stay
            // pc : Pathing Code:  pc=3
            // Sign String:     ec=zombieBoe;task=Stay;pc=4
            var entityClass = PathingCubeParser.GetValue(signText, "ec");
            var entityGroup = PathingCubeParser.GetValue(signText, "eg");
            var Task = PathingCubeParser.GetValue(signText, "task");
            var Buff = PathingCubeParser.GetValue(signText, "buff");
            var PathingCode = PathingCubeParser.GetValue(signText, "pc");

            if (OwnerEntityID != -1)
                leader = GameManager.Instance.World.GetEntity(OwnerEntityID) as EntityAlive;

            Vector3 transformPos = _blockPos.ToVector3() + new Vector3(0.5f, 0.25f, 0.5f);
            Vector3 rotation = new Vector3(0f, (float)(90 * (_blockValue.rotation & 3)), 0f);

            // If the class is empty, check to see if we have a group to spawn from.
            if (string.IsNullOrEmpty(entityClass))
            {
                // No entity class or group? Do nothing.
                if (string.IsNullOrEmpty(entityGroup))
                    return;

                var ClassID = 0;
                var EntityID = EntityGroups.GetRandomFromGroup(entityGroup, ref ClassID);
                if (EntityID == 0) // Invalid group.
                    return;

                myEntity = EntityFactory.CreateEntity(EntityID, transformPos, rotation) as EntityAlive;

            }
            else
            {
                myEntity = EntityFactory.CreateEntity(EntityClass.FromString(entityClass), transformPos, rotation) as EntityAlive;
            }


            // Not a valid entity.
            if (myEntity == null)
                return;


          //  myEntity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
            //myEntity.RotateTo(leader, 45f, 45f);

        //    GameManager.Instance.World.SpawnEntityInWorld(myEntity);

            var entityCreationData = new EntityCreationData(myEntity);
            entityCreationData.id = -1;
            entityCreationData.pos = _blockPos.ToVector3() + new Vector3(0.5f, 0.25f, 0.5f);
            entityCreationData.rot = rotation;
            chunk.AddEntityStub(entityCreationData);
            GameManager.Instance.RequestToSpawnEntityServer(entityCreationData);
            myEntity.OnEntityUnload();

            var nearbyEntities = new List<Entity>();

            // Search in the bounds are to try to find the most appealing entity to follow.
            var bb = new Bounds(_blockPos, new Vector3(1, 2, 1));


            GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityAlive), bb, nearbyEntities);
            for (var i = nearbyEntities.Count - 1; i >= 0; i--)
            {
                var x = nearbyEntities[i] as EntityAlive;

                if (x == null) return;

                // Set the leader here, which will auto-assign the follow task
                if (leader != null)
                {
                    EntityUtilities.SetLeader(x.entityId, leader.entityId);
                 //   x.RotateTo(leader, 45f, 45f);
                }
                // We need to apply the buffs during this scan, as the creation of the entity + adding buffs is not really MP safe.
                if (Task.ToLower() == "stay")
                    x.Buffs.AddBuff("buffOrderStay");
                if (Task.ToLower() == "wander")
                    x.Buffs.AddBuff("buffOrderWander");
                if (Task.ToLower() == "guard")
                    x.Buffs.AddBuff("buffOrderGuard");

                if (Task.ToLower() == "follow")
                    x.Buffs.AddBuff("buffOrderFollow");

                if (!string.IsNullOrEmpty(Buff))
                    x.Buffs.AddBuff(Buff);

                //if (leader)
                //{
                //    //x.SetRotationAndStopTurning(rotation);
                //    x.SetLookPosition(leader.position);
                //    x.RotateTo(leader.position.x, leader.position.y, leader.position.z, 30f, 30f);
                //}
                // Center the entity to its block position.
                //     x.SetPosition(EntityUtilities.CenterPosition(_blockPos));
                //x.SetRotation(new Vector3(0f, (float)(90 * (_blockValue.rotation & 3)), 0f));




            }


        }
        catch (Exception ex)
        {
            Debug.Log("Invalid String on Sign: " + signText + " Example:  ec=zombieBoe;buff=buffOrderStay;pc=0 or  eg=zombiesAll: " + ex);
        }
    }

    public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
    {
        if (Properties.Values.ContainsKey("Config"))
        {
            
            this.OwnerEntityID = _ea.entityId;
            this.CheckForSpawn(_world, 0, _result.blockPos, _result.blockValue, true);
        }
    }

}