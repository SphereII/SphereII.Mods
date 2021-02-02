using System.Xml;
using UnityEngine;

//         <triggered_effect trigger = "onSelfBuffFinish" action="SpawnBabySDX, Mods" target="self" SpawnGroup="farmAnimalsCow" Cvar="Mother" />
//         <triggered_effect trigger = "onSelfBuffFinish" action="SpawnEntitySDX, Mods" target="self" SpawnGroup="farmAnimalsCow" Cvar="Mother" />

// Dummy class to preserve backwards compatibility
public class MinEventActionSpawnBabySDX : MinEventActionSpawnEntitySDX
{

}
public class MinEventActionSpawnEntitySDX : MinEventActionRemoveBuff
{
    string strSpawnGroup = "";
    string strCvar = "Mother";

    public override void Execute(MinEventParams _params)
    {
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            return;

        for (int j = 0; j < targets.Count; j++)
        {
            EntityAlive entity = targets[j];
            if (entity)
            {
                int EntityID = entity.entityClass;

                // If the group is set, then use it.
                if (!string.IsNullOrEmpty(strSpawnGroup))
                {
                    int ClassID = 0;
                    EntityID = EntityGroups.GetRandomFromGroup(strSpawnGroup, ref ClassID);
                }
                Vector3 transformPos;
                entity.world.GetRandomSpawnPositionMinMaxToPosition(entity.position, 2, 6, 2, true, out transformPos, false);

                Entity NewEntity = EntityFactory.CreateEntity(EntityID, transformPos, entity.rotation);
                if (NewEntity)
                {
                    NewEntity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
                    GameManager.Instance.World.SpawnEntityInWorld(NewEntity);

                    if (NewEntity is EntityAlive)
                    {
                        Debug.Log("Setting " + strCvar + " ID to: " + entity.entityId + " for " + NewEntity.entityId);
                        (NewEntity as EntityAlive).Buffs.SetCustomVar(strCvar, entity.entityId, true);
                        EntityUtilities.SetCurrentOrder(NewEntity.entityId, EntityUtilities.Orders.Follow);
                    }

                }
                else
                {
                    Debug.Log(" Could not Spawn baby for: " + entity.name + " : " + entity.entityId);
                }

            }
            else
                Debug.Log("SpawnBabySDX(): Not an EntityAlive");

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
                    strSpawnGroup = _attribute.Value;
                    return true;
                }

                if (name == "Cvar")
                {
                    strCvar = _attribute.Value;
                    return true;
                }

            }
        }
        return flag;
    }

}
