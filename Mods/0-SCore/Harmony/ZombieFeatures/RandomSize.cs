using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = System.Random;

namespace Harmony.ZombieFeatures
{
    /**
     * SCoreRandomSize
     * 
     * This class includes a Harmony patch that allows an entity to spawn in with a random size.
     * 
     * You may specify a property on the entityclasses.xml to specify a range. if this property does not exist, the following range
     * is used:
     *  
     * { 0.7f, 0.8f, 0.9f, 0.9f, 1.0f, 1.0f, 1.0f, 1.1f, 1.2f };
     *  
     * Usage XML:
     * <!-- enforce a specific size range for an entity class -->
     * <property name="RandomSizes" value="1.2,1.2,1.4" />
     */
    public class RandomSize
    {
        private static readonly string AdvFeatureClass = "AdvancedZombieFeatures";
        private static readonly string Feature = "RandomSize";

        public class RandomSizeHelper
        {
            public static bool AllowedRandomSize(global::EntityAlive entity)
            {
                var bRandomSize = false;

                if (entity.isEntityRemote)
                    return false;

                if (entity is EntityZombie)
                {
                    AdvLogging.DisplayLog(AdvFeatureClass, " Random Size: Is A Zombie. Random size is true");
                    bRandomSize = true;
                }

                var entityClass = EntityClass.list[entity.entityClass];
                if (entityClass.Properties.Values.ContainsKey("RandomSize"))
                    bRandomSize = StringParsers.ParseBool(entityClass.Properties.Values["RandomSize"]);

                AdvLogging.DisplayLog(AdvFeatureClass, "Entity: " + entity.DebugNameInfo + " Random Size:  " + bRandomSize);
                return bRandomSize;
            }
        }

        // <property name="RandomSizes" value="1.2,1.2,1.4" />
        [HarmonyPatch(typeof(global::EntityAlive))]
        [HarmonyPatch("CopyPropertiesFromEntityClass")]
        public class EntityAliveCopyPropertiesFromEntityClass
        {
            public static void Postfix(ref global::EntityAlive __instance)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;

                if (__instance is EntityPlayerLocal)
                    return;


                if (RandomSizeHelper.AllowedRandomSize(__instance))
                {
                    // This is the distributed random heigh multiplier. Add or adjust values as you see fit. By default, it's just a small adjustment.
                    var numbers = new float[9] { 0.7f, 0.8f, 0.9f, 0.9f, 1.0f, 1.0f, 1.0f, 1.1f, 1.2f };
                    var random = new Random();

                    var randomIndex = random.Next(0, numbers.Length);
                    var flScale = numbers[randomIndex];

                    AdvLogging.DisplayLog(AdvFeatureClass, " Random Size: " + flScale);
                    __instance.Buffs.AddCustomVar("RandomSize", flScale);
                    // scale down the zombies, or upscale them
                    __instance.gameObject.transform.localScale = new Vector3(flScale, flScale, flScale);
                }

                // Check if there's random ranges
                var entityClass = __instance.EntityClass;
                if (entityClass.Properties.Values.ContainsKey("RandomSizes"))
                {
                    var Ranges = new List<float>();
                    var flScale = 1f;
                    foreach (var text in entityClass.Properties.Values["RandomSizes"].Split(','))
                        Ranges.Add(StringParsers.ParseFloat(text));

                    var random = new Random();
                    var randomIndex = random.Next(0, Ranges.Count);
                    flScale = Ranges[randomIndex];
                    AdvLogging.DisplayLog(AdvFeatureClass, " Random Size: " + flScale);
                    __instance.Buffs.AddCustomVar("RandomSize", flScale);

                    // scale down the zombies, or upscale them
                    __instance.gameObject.transform.localScale = new Vector3(flScale, flScale, flScale);
                }
            }
        }

        // Read Helper to make sure the size of the zombies are distributed properly
        [HarmonyPatch(typeof(global::EntityAlive))]
        [HarmonyPatch("Read")]
        public class EntityAliveRead
        {
            public static void Postfix(global::EntityAlive __instance, BinaryReader _br)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;

                try
                {
                    if (RandomSizeHelper.AllowedRandomSize(__instance))
                    {
                        var flScale = _br.ReadSingle();
                        AdvLogging.DisplayLog(AdvFeatureClass, " Read Size: " + flScale);

                        __instance.gameObject.transform.localScale = new Vector3(flScale, flScale, flScale);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        // Write Helper to make sure the size of the zombies are distributed properly
        [HarmonyPatch(typeof(global::EntityAlive))]
        [HarmonyPatch("Write")]
        public class EntityAliveWrite
        {
            public static void Postfix(global::EntityAlive __instance, BinaryWriter _bw)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;
                try
                {
                    if (RandomSizeHelper.AllowedRandomSize(__instance))
                    {
                        var flScale = __instance.gameObject.transform.localScale.x;
                        AdvLogging.DisplayLog(AdvFeatureClass, " Write Size: " + flScale);
                        _bw.Write(flScale);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}