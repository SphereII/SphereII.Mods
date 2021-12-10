using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

internal class QuestActionReplaceEntitySDX : QuestActionSpawnEntitySDX
{
    private readonly List<int> entityIDs = new List<int>();
    private int count = 1;

    // read in the entities IDs that we want to spawn.
    public override void SetupAction()
    {
        var array = ID.Split(',');
        for (var i = 0; i < array.Length; i++)
            foreach (var keyValuePair in EntityClass.list.Dict)
                if (keyValuePair.Value.entityClassName == array[i])
                {
                    // we'll only need their entity ID to store, not their name.
                    entityIDs.Add(keyValuePair.Key);
                    if (entityIDs.Count == array.Length) break;
                }
    }

    public override void PerformAction(Quest action)
    {
        HandleSpawnEntities();
    }


    public new void HandleSpawnEntities()
    {
        if (Value != null && Value != string.Empty)
            if (!int.TryParse(Value, out count))
                if (Value.Contains("-"))
                {
                    var array = Value.Split('-');
                    var min = Convert.ToInt32(array[0]);
                    var max = Convert.ToInt32(array[1]);
                    count = Random.Range(min, max);
                }

        GameManager.Instance.StartCoroutine(SpawnEntities());
    }

    public new static void SpawnQuestEntity(int spawnedEntityID, int entityIDQuestHolder, EntityPlayer player)
    {
        if (GameManager.Instance.World.Entities.dict.ContainsKey(entityIDQuestHolder))
        {
            var questEntity = GameManager.Instance.World.Entities.dict[entityIDQuestHolder] as EntityAlive;
            if (questEntity == null)
                return;

            var transformPos = questEntity.position;
            var rotation = new Vector3(0f, questEntity.transform.eulerAngles.y + 180f, 0f);
            var entity = EntityFactory.CreateEntity(spawnedEntityID, transformPos, rotation);
            entity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
            GameManager.Instance.World.SpawnEntityInWorld(entity);
            questEntity.MarkToUnload();
        }
    }

    public override BaseQuestAction Clone()
    {
        var questActionSpawnEntity = new QuestActionReplaceEntitySDX();
        CopyValues(questActionSpawnEntity);
        questActionSpawnEntity.entityIDs.AddRange(entityIDs);
        return questActionSpawnEntity;
    }

    private IEnumerator SpawnEntities()
    {
        for (var i = 0; i < count; i++)
        {
            yield return new WaitForSeconds(0.5f);
            var spawnKey = entityIDs[Random.Range(0, entityIDs.Count)];
            if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
                SpawnQuestEntity(spawnKey, OwnerQuest.SharedOwnerID, null);
            else
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEntitySpawn>().Setup(spawnKey));
        }
    }
}