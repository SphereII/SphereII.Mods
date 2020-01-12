using DMT;
using Harmony;
using System;
using System.IO;
using UnityEngine;

public class SphereII_RandomDeathSpawn
{

    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("OnEntityDeath")]
    public class SphereII_EntityAlive_OnEntityDeath
    {
        public static void Postfix(EntityAlive __instance)
        {
            String strSpawnGroup = "";
            EntityClass entityClass = EntityClass.list[__instance.entityClass];
            if (entityClass.Properties.Values.ContainsKey("SpawnOnDeath"))
            {
                // <property name="SpawnOnDeath" value="EnemyAnimalsForest" />
                strSpawnGroup = entityClass.Properties.Values["SpawnOnDeath"];

                int classID = 0;
                Entity entity = EntityFactory.CreateEntity(EntityGroups.GetRandomFromGroup(strSpawnGroup, ref classID), __instance.position);
                if (entity != null)
                {
                    Vector3i blockPos = default(Vector3i);
                    blockPos.x = (int)__instance.position.x;
                    blockPos.y = (int)__instance.position.y;
                    blockPos.z = (int)__instance.position.z;
                    __instance.world.SetBlockRPC(blockPos, BlockValue.Air);
                    entity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
                    GameManager.Instance.World.SpawnEntityInWorld(entity);
                }
            }
        }

    }

}

