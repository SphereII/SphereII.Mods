using System;
using System.Collections.Generic;
using UnityEngine;
class EntityAliveEventSpawnerSDX : EntityAlive
{
    public String strEntityGroup = "";
    public int MaxSpawn = 1;

    String strLeaderEntity = "";
    int LeaderEntityID = -1;

    private readonly bool blDisplayLog = false;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(GetType() + " : " + strMessage);
    }

    public override void OnAddedToWorld()
    {
        DisplayLog("EntityClass: " + this.entityClass);

        EntityClass entityClass = EntityClass.list[this.entityClass];
        if (entityClass.Properties.Classes.ContainsKey("SpawnSettings"))
        {
            DisplayLog(" Found Spawn Settings.. reading...");
            DynamicProperties dynamicProperties3 = entityClass.Properties.Classes["SpawnSettings"];
            foreach (KeyValuePair<string, object> keyValuePair in dynamicProperties3.Values.Dict.Dict)
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

                    int Range = GetRange(dynamicProperties3, "Followers");
                    for (int x = 0; x <= Range; x++)
                    {
                        // if it contains commas, it's individual entities.
                        String strValue = dynamicProperties3.Values[keyValuePair.Key];
                        if (strValue.Contains(","))
                        {
                            foreach (String strEntity in strValue.Split(','))
                                SpawnEntity(EntityClass.FromString(strEntity), false);

                            // If we spawn in each entity, break, as we don't want multiple spawns.
                            break;
                        }
                        else  //  Spawn from Entity Group
                        {
                            SpawnFromGroup(strValue, 1);
                        }
                    }
                    continue;
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
    public int GetRange(DynamicProperties dynamicProperties3, String strindex)
    {
        int minCount = 1;
        int maxCount = 1;
        string strRange = "";
        dynamicProperties3.Params1.TryGetValue(strindex, out strRange);

        // If there's no value, just do a single spawn.
        if (string.IsNullOrEmpty(strRange))
            return 1;

        StringParsers.ParseMinMaxCount(strRange, out minCount, out maxCount);
        float Count = UnityEngine.Random.Range(minCount, (float)maxCount);
        DisplayLog(" Count is: " + Count);
        return (int)Count;
    }


    public void SpawnFromGroup(String strGroup, int Count)
    {
        int EntityID = -1;
        for (int x = 0; x < MaxSpawn; x++)
        {
            // Verify that it's an entity group or individual entity.. just in case.
            if (EntityGroups.list.ContainsKey(strGroup))
            {
                DisplayLog(" Spawning from : " + strGroup);
                int ClassID = 0;

                EntityID = EntityGroups.GetRandomFromGroup(strGroup, ref ClassID);
                SpawnEntity(EntityID, false);
            }
            else
            {
                SpawnEntity(EntityClass.FromString(strGroup), false);
            }
        }

    }

    public void SpawnEntity(int EntityID, bool isLeader)
    {
        // Grab a random position.
        Vector3 transformPos;
        if (!world.GetRandomSpawnPositionMinMaxToPosition(position, 2, 6, 2, true, out transformPos, false))
        {
            DisplayLog(" No position available");
            return;
        }

        DisplayLog(" EntityID: " + EntityID);
        Entity NewEntity = EntityFactory.CreateEntity(EntityID, transformPos);
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

                (NewEntity as EntityAliveSDX).Buffs.SetCustomVar("Herd", LeaderEntityID, true);
            }
        }

    }
}

