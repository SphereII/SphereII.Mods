using HarmonyLib;

namespace Harmony.ZombieFeatures
{
    /**
     * SCoreRandomDeathSpawn
     * 
     * This class includes a Harmony patch allows a new entity from a spawn group to spawn on the death of the original.
     * 
     * Usage XML entityclasses.xml
     * <property name="SpawnOnDeath" value="mySpawnGroup" />
     */
    public class SCoreRandomDeathSpawn
    {
        [HarmonyPatch(typeof(global::EntityAlive))]
        [HarmonyPatch("OnEntityDeath")]
        public class EntityAliveOnEntityDeath
        {
            public static void Postfix(global::EntityAlive __instance)
            {
                var entityClass = EntityClass.list[__instance.entityClass];
                if (!entityClass.Properties.Values.ContainsKey("SpawnOnDeath")) return;

                // Spawn location
                Vector3i blockPos;
                blockPos.x = (int)__instance.position.x;
                blockPos.y = (int)__instance.position.y;
                blockPos.z = (int)__instance.position.z;


                // <property name="SpawnOnDeath" value="EnemyAnimalsForest" />
                var strSpawnGroup = entityClass.Properties.Values["SpawnOnDeath"];

                var classID = 0;
                // try to spawn from a group
                var entity = EntityFactory.CreateEntity(EntityGroups.GetRandomFromGroup(strSpawnGroup, ref classID), __instance.position);
                if (entity != null)
                {
                    __instance.world.SetBlockRPC(blockPos, BlockValue.Air);
                    GameManager.Instance.World.SpawnEntityInWorld(entity);
                    return;
                }

                // If no group, then assume its an entity
                var entityID = EntityClass.FromString(strSpawnGroup);
                entity = EntityFactory.CreateEntity(entityID, __instance.position);
                if (entity != null) GameManager.Instance.World.SpawnEntityInWorld(entity);
            }
        }
    }
}