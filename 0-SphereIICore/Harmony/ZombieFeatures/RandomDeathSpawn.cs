using DMT;
using HarmonyLib;
using System;
using System.IO;
using UnityEngine;


/**
 * SphereII_RandomDeathSpawn
 *
 * This class includes a Harmony patch allows a new entity from a spawn group to spawn on the death of the original.
 * 
 * Usage XML entityclasses.xml
 * 
 * <property name="SpawnOnDeath" value="mySpawnGroup" />
 */

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
                // Spawn location
                Vector3i blockPos = default(Vector3i);
                blockPos.x = (int)__instance.position.x;
                blockPos.y = (int)__instance.position.y;
                blockPos.z = (int)__instance.position.z;


                // <property name="SpawnOnDeath" value="EnemyAnimalsForest" />
                strSpawnGroup = entityClass.Properties.Values["SpawnOnDeath"];

                int classID = 0;
                // try to spawn from a group
                Entity entity = EntityFactory.CreateEntity(EntityGroups.GetRandomFromGroup(strSpawnGroup, ref classID), __instance.position);
                if (entity != null)
                {
                    __instance.world.SetBlockRPC(blockPos, BlockValue.Air);
                    GameManager.Instance.World.SpawnEntityInWorld(entity);
                    return;
                }

                // If no group, then assume its an entity
                int entityID = EntityClass.FromString(strSpawnGroup);
                entity = EntityFactory.CreateEntity(entityID, __instance.position);
                if(entity != null)
                {
                    GameManager.Instance.World.SpawnEntityInWorld(entity);
                }
            }
        }

    }

}

