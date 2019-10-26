using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
class QuestActionSpawnEntitySDX : QuestActionSpawnEnemy
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


    public void HandleSpawnEntities()
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

            Vector3 a = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f));
            a.Normalize();

            // Spawn the entity close by
            float d = UnityEngine.Random.Range(1f, 1f);
        
            Vector3 transformPos = questEntity.position + a * d;
            Vector3 rotation = new Vector3(0f, questEntity.transform.eulerAngles.y + 180f, 0f);
            float num = (float)GameManager.Instance.World.GetHeight((int)transformPos.x, (int)transformPos.z);
            float num2 = (float)GameManager.Instance.World.GetTerrainHeight((int)transformPos.x, (int)transformPos.z);
            transformPos.y = (num + num2) / 2f + 0.5f;

            Entity entity = EntityFactory.CreateEntity(spawnedEntityID, transformPos, rotation);
            entity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
            GameManager.Instance.World.SpawnEntityInWorld(entity);
        }
    }

    public override BaseQuestAction Clone()
    {
        QuestActionSpawnEntitySDX questActionSpawnEntity = new QuestActionSpawnEntitySDX();
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
                QuestActionSpawnEntitySDX.SpawnQuestEntity(spawnKey, OwnerQuest.SharedOwnerID, null);
            }
            else
            {
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEntitySpawn>().Setup(spawnKey, -1), false);
            }
        }
        yield break;
    }


}
