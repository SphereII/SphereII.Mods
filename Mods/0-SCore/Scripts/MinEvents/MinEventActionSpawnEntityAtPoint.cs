using System.Xml;
using System.Xml.Linq;
using UnityEngine;


//        <triggered_effect trigger="onProjectileImpact" action="SpawnEntityAtPoint, SCore" SpawnGroup="ZombiesBurntForest" />


public class MinEventActionSpawnEntityAtPoint : MinEventActionRemoveBuff
{
    private string strCvar;
    private string strSpawnGroup = "";

    public override void Execute(MinEventParams _params)
    {
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            return;


        var position = _params.Position;
        if (targetType != TargetTypes.positionAOE)
        {
            if (Voxel.voxelRayHitInfo.bHitValid)
            {
                var hitInfo = Voxel.voxelRayHitInfo;
                if (hitInfo == null) return;
                position = hitInfo.hit.blockPos;
            }
        }
        position += Vector3i.up;

        int EntityID = -1;

        // If the group is set, then use it.
        if (!string.IsNullOrEmpty(strSpawnGroup))
        {
            var ClassID = 0;
            EntityID = EntityGroups.GetRandomFromGroup(strSpawnGroup, ref ClassID);
        }

        if (EntityID == -1) return;

        var NewEntity = EntityFactory.CreateEntity(EntityID, position) as EntityAlive;
        if (NewEntity)
        {
            var entityCreationData = new EntityCreationData(NewEntity);
            entityCreationData.id = -1;
            GameManager.Instance.RequestToSpawnEntityServer(entityCreationData);
            NewEntity.OnEntityUnload();
        }
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            var name = _attribute.Name.LocalName;
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