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
            public static bool Prefix(global::EntityAlive __instance)
            {
                var entityClass = EntityClass.list[__instance.entityClass];
                if (!entityClass.Properties.Values.ContainsKey("SpawnOnDeath")) return true;
                if (__instance.Buffs.HasCustomVar("NoSpawnOnDeath")) return true ;

                // Spawn location
                Vector3i blockPos;
                blockPos.x = (int)__instance.position.x;
                blockPos.y = (int)__instance.position.y;
                blockPos.z = (int)__instance.position.z;


                // <property name="SpawnOnDeath" value="EnemyAnimalsForest" />
                var strSpawnGroup = entityClass.Properties.Values["SpawnOnDeath"];

                //Begin Guppy
                foreach (var buff in __instance.Buffs.CVars)
                {
                    if (buff.Key.StartsWith("spawnOverride"))
                    {
                        string strDeathCvar = buff.Key; //Sets this string to that positive check, so if it  has a cvar guppyzombieBiker then strDeathCvar is now guppyzombieBiker
                        string strCvarGroup = strDeathCvar.Replace("spawnOverride", ""); //Strips the word guppy from the found cvar, aka guppyzombieBiker is now zombieBiker
                     //   Log.Out(strDeathCvar + " got replaced with " + strCvarGroup); //Just a little message to see what gets spawned from what in the logs
                        strSpawnGroup = strCvarGroup; //Replaces strSpawnGroup, which is used later in code, with the new zombie spawn group, aka zombieBiker
                        break;
                    }

                    if (buff.Key.StartsWith("spawn2ndLife")) //Looks through all buffs to see if there are any stop death spawn cvars.  What are the chances another cvar begins with this?
                    {
                        return true; //Says "Fuck you, I won't do what you tell me".  I thought this broke the entire code but it seems to only break this foreach loop.
                    }
                }
                //End Guppy

                var classID = 0;
                // try to spawn from a group
                var entity = EntityFactory.CreateEntity(EntityGroups.GetRandomFromGroup(strSpawnGroup, ref classID), __instance.position);
                if (entity != null)
                {
                    __instance.world.SetBlockRPC(blockPos, BlockValue.Air);
                    GameManager.Instance.World.SpawnEntityInWorld(entity);
                    __instance.ForceDespawn();
                    return true;
                }

                // If no group, then assume its an entity
                var entityID = EntityClass.FromString(strSpawnGroup);
                entity = EntityFactory.CreateEntity(entityID, __instance.position);
                if (entity != null) 
                    GameManager.Instance.World.SpawnEntityInWorld(entity);

                __instance.ForceDespawn();
                return true;
            }
        }
    }
}