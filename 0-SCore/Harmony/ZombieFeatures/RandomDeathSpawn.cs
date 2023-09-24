using HarmonyLib;
using UnityEngine;

namespace Harmony.ZombieFeatures
{
    /**
     * SCoreRandomDeathSpawn
     * 
     * This class includes a Harmony patch allows a new entity from a spawn group to spawn on the death of the original.
     * 
     * Usage XML entityclasses.xml
     * <property name="SpawnOnDeath" value="mySpawnGroup" />
     * 
     * Usage CVar, remmeber to begin the name of the entitygroup with spawnOverride, so spawnOverridezombieBurnt calls from group zombieBurnt, while spawnOverride zombieBiker calls from group zombieBiker
     * <triggered_effect trigger="onSelfBuffUpdate" action="ModifyCVar" target="self" cvar="spawnOverridezombieBurnt" operation="set" value="1">
     * 
     * If an entity has a random size (has a customCVar called "RandomSize", and has a customcvar called "SpawnCopyScale", the
     * new entity will inherit the original entity's scale
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

                if (__instance.Buffs.HasCustomVar("NoSpawnOnDeath")) //Checks to see if there is a cvar called NoSpawnOnDeath
                {
                    float floatNoSpawnOnDeath = __instance.Buffs.GetCustomVar("NoSpawnOndeath"); //If there is, get its value
                    if (floatNoSpawnOnDeath == 1) return true; //If the value is 1, don't continue the code.  This allows cvars to set that stop the cvar spawning even if spawnOverride is set
                }

                // Only trigger a respawn when the local machine controls it.
                if (__instance.isEntityRemote)
                {
                    return true;
                }

                // Spawn location
                Vector3i blockPos;
                blockPos.x = (int)__instance.position.x;
                blockPos.y = (int)__instance.position.y;
                blockPos.z = (int)__instance.position.z;

                var leaveBody = false;
                if (entityClass.Properties.Values.ContainsKey("SpawnOnDeathLeaveBody"))
                {
                    if (StringParsers.ParseBool(entityClass.Properties.Values["SpawnOnDeathLeaveBody"]))
                        leaveBody = true;
                }

                // <property name="SpawnOnDeath" value="EnemyAnimalsForest" />
                var strSpawnGroup = entityClass.Properties.Values["SpawnOnDeath"]; //Gets the entity group name from the property

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
                        return true; //If there's a cvar called spawn2ndLife then it breaks the spawn chain.  This way spawns don't spawn into other spawns repeatedly.
                    }
                }

                if (strSpawnGroup == "SpawnNothing") return true; //Allows a value for the entity group that spawns nothing, so that the property value can be turned off while allowing the cvar value to still work.  
                //Usage: <property name="SpawnOnDeath" value="SpawnNothing" />
                //End Guppy

                var classID = 0;
                // try to spawn from a group
                var entity = EntityFactory.CreateEntity(EntityGroups.GetRandomFromGroup(strSpawnGroup, ref classID), __instance.position);

                if (entity != null)
                {
                    var alivefromGroup = entity as global::EntityAlive; //Newly spawned entity from group
                    if (alivefromGroup) //Variety of methods to add the cvar spawn2ndLife to the new entity in a variety of cases
                    {
                        if (alivefromGroup.Buffs.HasCustomVar("spawn2ndLife"))
                            alivefromGroup.Buffs.SetCustomVar("spawn2ndLife", 1);
                        else
                            alivefromGroup.Buffs.AddCustomVar("spawn2ndLife", 1);

                        if (__instance.Buffs.HasCustomVar("RandomSize") && __instance.Buffs.HasCustomVar("SpawnCopyScale") )
                        {
                            var scale = __instance.gameObject.transform.localScale.x;
                            alivefromGroup.gameObject.transform.localScale = new Vector3(scale, scale, scale);
                        }
                    }

                    // This is meant to destroy the corpse block, but may not exist anymore.
                    //__instance.world.SetBlockRPC(blockPos, BlockValue.Air);

                    // Generate a creation data object to be sent to the server for spawning. This will avoid duplicate spawns in dedi environments.
                    var entityCreationData = new EntityCreationData(entity);
                    entityCreationData.id = -1;
                    GameManager.Instance.RequestToSpawnEntityServer(entityCreationData);
                    entity.OnEntityUnload();
                    if (leaveBody) return true; 

                    __instance.ForceDespawn();
                    return true;
                }

                // If no group, then assume its an entity
                var entityID = EntityClass.FromString(strSpawnGroup);
                entity = EntityFactory.CreateEntity(entityID, __instance.position);
                if (entity != null)
                {
                    var entityCreationData = new EntityCreationData(entity);
                    entityCreationData.id = -1;
                    GameManager.Instance.RequestToSpawnEntityServer(entityCreationData);
                    entity.OnEntityUnload();
                    //GameManager.Instance.World.SpawnEntityInWorld(entity);
                }
                if (leaveBody) return true;
                __instance.ForceDespawn();
                return true;
            }
        }
    }
}