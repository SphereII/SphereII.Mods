using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
class QuestActionReplaceEntitySDX : QuestActionSpawnEntitySDX
{
    private List<int> entityIDs = new List<int>();
    private int count = 1;

    // read in the entities IDs that we want to spawn.
    public override void SetupAction()
    {
        string[] array = base.ID.Split(new char[]
        {
            ','
        });
        for (int i = 0; i < array.Length; i++)
        {
            foreach (KeyValuePair<int, EntityClass> keyValuePair in EntityClass.list.Dict)
            {
                if (keyValuePair.Value.entityClassName == array[i])
                {
                    // we'll only need their entity ID to store, not their name.
                    this.entityIDs.Add(keyValuePair.Key);
                    if (this.entityIDs.Count == array.Length)
                    {
                        break;
                    }
                }
            }
        }
    }
    public override void PerformAction()
    {
        this.HandleSpawnEntities();
    }


    public  void HandleSpawnEntities()
    {
        if (base.Value != null && base.Value != string.Empty)
        {
            if (!int.TryParse(base.Value, out this.count))
            {
                if (base.Value.Contains("-"))
                {
                    string[] array = base.Value.Split(new char[]
                    {
                        '-'
                    });
                    int min = Convert.ToInt32(array[0]);
                    int max = Convert.ToInt32(array[1]);
                    this.count = UnityEngine.Random.Range(min, max);
                }
            }
        }
        GameManager.Instance.StartCoroutine(this.SpawnEntities());
    }
    public new static void SpawnQuestEntity(int spawnedEntityID, int entityIDQuestHolder, EntityPlayer player )
    {
        if (GameManager.Instance.World.Entities.dict.ContainsKey(entityIDQuestHolder))
        {
            EntityAlive questEntity = GameManager.Instance.World.Entities.dict[entityIDQuestHolder] as EntityAlive;
            if (questEntity == null)
                return;
            
            Vector3 transformPos = questEntity.position;
            Vector3 rotation = new Vector3(0f, questEntity.transform.eulerAngles.y + 180f, 0f);
            Entity entity = EntityFactory.CreateEntity(spawnedEntityID, transformPos, rotation);
            entity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
            GameManager.Instance.World.SpawnEntityInWorld(entity);
            questEntity.MarkToUnload();
        }
    }

    public override BaseQuestAction Clone()
    {
        QuestActionReplaceEntitySDX questActionSpawnEntity = new QuestActionReplaceEntitySDX();
        base.CopyValues(questActionSpawnEntity);
        questActionSpawnEntity.entityIDs.AddRange(this.entityIDs);
        return questActionSpawnEntity;
    }

    private IEnumerator SpawnEntities()
    {
        for (int i = 0; i < this.count; i++)
        {
            yield return new WaitForSeconds(0.5f);
            int spawnKey = this.entityIDs[UnityEngine.Random.Range(0, this.entityIDs.Count)];
            if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            {
                QuestActionReplaceEntitySDX.SpawnQuestEntity(spawnKey, OwnerQuest.SharedOwnerID, null);
            }
            else
            {
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEntitySpawn>().Setup(spawnKey, -1), false);
            }
        }
        yield break;
    }


}
