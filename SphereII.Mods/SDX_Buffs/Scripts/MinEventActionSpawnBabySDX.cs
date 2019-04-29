using System.Collections.Generic;
using System.Xml;
using UnityEngine;

//         <triggered_effect trigger = "onSelfBuffFinish" action="SpawnBabySDX, Mods" target="self" SpawnGroup="farmAnimalsCow" />

public class MinEventActionSpawnBabySDX : MinEventActionRemoveBuff
{
    string strSpawnGroup = "";
    public override void Execute(MinEventParams _params)
    {
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            return;

        for (int j = 0; j < this.targets.Count; j++)
        {
            EntityAliveSDX entity = this.targets[j] as EntityAliveSDX;
            if (entity)
            {
                int EntityID = entity.entityClass;

                // If the group is set, then use it.
                if (!string.IsNullOrEmpty(this.strSpawnGroup))
                    EntityID = EntityGroups.GetRandomFromGroup(this.strSpawnGroup);

                Vector3 transformPos;
                entity.world.GetRandomSpawnPositionMinMaxToPosition(entity.position, 2, 6, 2, true, out transformPos, false);

                Entity NewEntity = EntityFactory.CreateEntity(EntityID, transformPos, entity.rotation);
                if (NewEntity)
                {
                    NewEntity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
                    GameManager.Instance.World.SpawnEntityInWorld(NewEntity);
                    Debug.Log("An entity was created: " + NewEntity.ToString());
                    if (NewEntity is EntityAliveSDX)
                    {
                        Debug.Log("Setting Mother ID to Baby: " + entity.entityId + " for " + NewEntity.entityId);
                        (NewEntity as EntityAliveSDX).Buffs.SetCustomVar("Mother", entity.entityId, true);
                    }
                }
                else
                {
                    Debug.Log(" Could not Spawn baby for: " + entity.EntityName + " : " + entity.entityId);
                }

            }

        }
    }

    public override bool ParseXmlAttribute(XmlAttribute _attribute)
    {
        bool flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            string name = _attribute.Name;
            if (name != null)
            {
                if (name == "SpawnGroup")
                {
                    this.strSpawnGroup = _attribute.Value;
                    return true;
                }
            }
        }
        return flag;
    }

}
