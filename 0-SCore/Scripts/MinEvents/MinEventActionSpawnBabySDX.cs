using System.Xml;
using System.Xml.Linq;
using UnityEngine;

//         <triggered_effect trigger = "onSelfBuffFinish" action="SpawnBabySDX, SCore" target="self" SpawnGroup="farmAnimalsCow" Cvar="Mother" />
//         <triggered_effect trigger = "onSelfBuffFinish" action="SpawnEntitySDX, SCore" target="self" SpawnGroup="farmAnimalsCow" Cvar="Mother" />

// Dummy class to preserve backwards compatibility
public class MinEventActionSpawnBabySDX : MinEventActionSpawnEntitySDX
{
}

public class MinEventActionAddBuffToPrimaryPlayer : MinEventActionAddBuff
{
    public override void Execute(MinEventParams _params)
    {
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            return;
        EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
        if (primaryPlayer == null) return;

        foreach (var name in this.buffNames)
        {
            primaryPlayer.Buffs.AddBuff(name, -1, true, false, false);
        }
    }
}


public class MinEventActionSpawnEntitySDX : MinEventActionRemoveBuff
{
    private string strCvar = "Mother";
    private string strSpawnGroup = "";

    public override void Execute(MinEventParams _params)
    {
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            return;

        for (var j = 0; j < targets.Count; j++)
        {
            var entity = targets[j];
            if (entity)
            {
                var EntityID = entity.entityClass;

                // If the group is set, then use it.
                if (!string.IsNullOrEmpty(strSpawnGroup))
                {
                    var ClassID = 0;
                    EntityID = EntityGroups.GetRandomFromGroup(strSpawnGroup, ref ClassID);
                }

                GameManager.Instance.World.GetRandomSpawnPositionMinMaxToPosition(entity.position, 2, 6, 2, false, out var transformPos);
                if (transformPos == Vector3.zero)
                    transformPos = entity.position + Vector3.back;

                var NewEntity = EntityFactory.CreateEntity(EntityID, transformPos, entity.rotation) as EntityAlive;
                if (NewEntity)
                {
                    var entityCreationData = new EntityCreationData(NewEntity);
                    entityCreationData.id = -1;
                    GameManager.Instance.RequestToSpawnEntityServer(entityCreationData);
                    NewEntity.OnEntityUnload();
                }
                else
                {
                    Debug.Log(" Could not Spawn baby for: " + entity.name + " : " + entity.entityId);
                }
            }
            else
            {
                Debug.Log("SpawnBabySDX(): Not an EntityAlive");
            }
        }
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            var name = _attribute.Name;
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