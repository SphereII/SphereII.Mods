using UnityEngine;

internal class EntityAliveEventSpawnerSDX : EntityAlive
{
    public string strEntityGroup = "";
    public int MaxSpawn = 1;

    private readonly bool blDisplayLog = false;
    private int LeaderEntityID = -1;

    private string strLeaderEntity = "";

    public void DisplayLog(string strMessage)
    {
        if (blDisplayLog)
            Debug.Log(GetType() + " : " + strMessage);
    }

    public override void OnAddedToWorld()
    {
        DisplayLog("EntityClass: " + this.entityClass);

        var entityClass = EntityClass.list[this.entityClass];
        if (entityClass.Properties.Classes.ContainsKey("SpawnSettings"))
        {
            DisplayLog(" Found Spawn Settings.. reading...");
            var dynamicProperties3 = entityClass.Properties.Classes["SpawnSettings"];
            foreach (var keyValuePair in dynamicProperties3.Values.Dict.Dict)
            {
                DisplayLog("Key: " + keyValuePair.Key);
                if (keyValuePair.Key == "Leader")
                {
                    DisplayLog(" Found a Leader");
                    strLeaderEntity = dynamicProperties3.Values[keyValuePair.Key];

                    SpawnEntity(EntityClass.FromString(strLeaderEntity), true);
                    continue;
                }

                if (keyValuePair.Key == "Followers")
                {
                    DisplayLog("Found Followers");

                    var Range = GetRange(dynamicProperties3, "Followers");
                    for (var x = 0; x <= Range; x++)
                    {
                        // if it contains commas, it's individual entities.
                        var strValue = dynamicProperties3.Values[keyValuePair.Key];
                        if (strValue.Contains(","))
                        {
                            foreach (var strEntity in strValue.Split(','))
                                SpawnEntity(EntityClass.FromString(strEntity), false);

                            // If we spawn in each entity, break, as we don't want multiple spawns.
                            break;
                        }

                        SpawnFromGroup(strValue, 1);
                    }
                }
            }
        }
        else
        {
            DisplayLog(" No Spawn settings found.");
        }

        SetDead();
    }

    // This reads the param1, if it exists, to grab a count of how many to spawn in.
    public int GetRange(DynamicProperties dynamicProperties3, string strindex)
    {
        var minCount = 1;
        var maxCount = 1;
        var strRange = "";
        dynamicProperties3.Params1.TryGetValue(strindex, out strRange);

        // If there's no value, just do a single spawn.
        if (string.IsNullOrEmpty(strRange))
            return 1;

        StringParsers.ParseMinMaxCount(strRange, out minCount, out maxCount);
        var Count = Random.Range(minCount, (float)maxCount);
        DisplayLog(" Count is: " + Count);
        return (int)Count;
    }


    public void SpawnFromGroup(string strGroup, int Count)
    {
        var EntityID = -1;
        for (var x = 0; x < MaxSpawn; x++)
            // Verify that it's an entity group or individual entity.. just in case.
            if (EntityGroups.list.ContainsKey(strGroup))
            {
                DisplayLog(" Spawning from : " + strGroup);
                var ClassID = 0;

                EntityID = EntityGroups.GetRandomFromGroup(strGroup, ref ClassID);
                SpawnEntity(EntityID, false);
            }
            else
            {
                SpawnEntity(EntityClass.FromString(strGroup), false);
            }
    }

    public void SpawnEntity(int EntityID, bool isLeader)
    {
        // Grab a random position.
        Vector3 transformPos;
        if (!world.GetRandomSpawnPositionMinMaxToPosition(position, 2, 6, 2, true, out transformPos))
        {
            DisplayLog(" No position available");
            return;
        }

        DisplayLog(" EntityID: " + EntityID);
        var NewEntity = EntityFactory.CreateEntity(EntityID, transformPos);
        if (NewEntity)
        {
            NewEntity.SetSpawnerSource(EnumSpawnerSource.Dynamic);
            GameManager.Instance.World.SpawnEntityInWorld(NewEntity);

            if (isLeader)
            {
                DisplayLog(" Leader Entity ID: " + NewEntity.entityId);
                LeaderEntityID = NewEntity.entityId;
                NewEntity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);

                // EntityUtilities.SetLeaderAndOwner(this.LeaderEntityID, this.LeaderEntityID);
            }
            // Set the leaderID if its configured.
            else if (LeaderEntityID > 0 && NewEntity is EntityAliveSDX)
            {
                DisplayLog(" Setting Leader ID to: " + LeaderEntityID);
                EntityUtilities.SetLeaderAndOwner(NewEntity.entityId, LeaderEntityID);

                (NewEntity as EntityAliveSDX).Buffs.SetCustomVar("Herd", LeaderEntityID);
            }
        }
    }
}