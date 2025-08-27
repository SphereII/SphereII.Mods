using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Harmony.ZombieFeatures
{
    /**
     * SCoreRandomSize
     * * This class includes a Harmony patch that allows an entity to spawn in with a random size.
     * * You may specify a property on the entityclasses.xml to specify a range. if this property does not exist, the following range
     * is used:
     * * { 0.7f, 0.8f, 0.9f, 0.9f, 1.0f, 1.0f, 1.0f, 1.1f, 1.2f };
     * * Usage XML:
     * * <property name="RandomSizes" value="1.2,1.2,1.4" />
     */
    public class RandomSize
    {
        private static readonly string AdvFeatureClass = "AdvancedZombieFeatures";
        private static readonly string Feature = "RandomSize";
        private static readonly Random random = new Random();
        private static readonly float[] DefaultSizes = { 0.7f, 0.8f, 0.9f, 0.9f, 1.0f, 1.0f, 1.0f, 1.1f, 1.2f };

        public class RandomSizeHelper
        {
            public static bool AllowedRandomSize(global::EntityAlive entity)
            {
                if (entity.isEntityRemote || entity is EntityPlayerLocal)
                    return false;

                var entityClass = EntityClass.list[entity.entityClass];
                var hasRandomSizes = entityClass.Properties.Values.ContainsKey("RandomSizes");
                var hasRandomSizeBool = entityClass.Properties.Values.ContainsKey("RandomSize") && StringParsers.ParseBool(entityClass.Properties.Values["RandomSize"]);

                bool isAllowed = entity is EntityZombie || hasRandomSizes || hasRandomSizeBool;
                AdvLogging.DisplayLog(AdvFeatureClass, $"Entity: {entity.DebugNameInfo} Random Size Allowed: {isAllowed}");
                return isAllowed;
            }

            public static List<float> ParseFloatList(string value)
            {
                return value.Split(',')
                            .Select(s => StringParsers.ParseFloat(s.Trim()))
                            .ToList();
            }
        }

        [HarmonyPatch(typeof(global::EntityAlive), nameof(global::EntityAlive.Init))]
        public class EntityAliveInit
        {
            public static void Postfix(ref global::EntityAlive __instance)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;
                if (!RandomSizeHelper.AllowedRandomSize(__instance)) return;

                var scale = 1f;
                var entityClass = __instance.EntityClass;
                
                if (entityClass.Properties.Values.ContainsKey("RandomSizes"))
                {
                    List<float> ranges = RandomSizeHelper.ParseFloatList(entityClass.Properties.Values["RandomSizes"]);
                    if (ranges.Any())
                    {
                        scale = ranges[random.Next(ranges.Count)];
                    }
                }
                else
                {
                    scale = DefaultSizes[random.Next(DefaultSizes.Length)];
                }
                
                AdvLogging.DisplayLog(AdvFeatureClass, $" Random Size: {scale}");
                __instance.Buffs.AddCustomVar("RandomSize", scale);
            }
        }

        [HarmonyPatch(typeof(global::EntityAlive), nameof(global::EntityAlive.Update))]
        public class EntityAliveUpdate
        {
            public static void Postfix(global::EntityAlive __instance)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;
                if (!RandomSizeHelper.AllowedRandomSize(__instance)) return;

                var scale = __instance.Buffs.GetCustomVar("RandomSize");
                if (scale == 0) return;
                if (Mathf.Approximately(scale, __instance.OverrideSize)) return;

                __instance.SetScale(scale);
                __instance.OverrideSize = scale;
            }
        }
    }
}