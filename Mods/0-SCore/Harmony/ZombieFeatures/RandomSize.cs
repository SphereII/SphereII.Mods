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
     * * The rolled value multiplies the entity class's own SizeScale rather than replacing it.
     * * Usage XML:
     * * <property name="RandomSizes" value="1.2,1.2,1.4" />
     * * Zombies and any class with RandomSizes are randomized by default. RandomSize overrides that either way:
     * * <property name="RandomSize" value="false" />  <!-- opts a zombie out -->
     * * <property name="RandomSize" value="true" />   <!-- opts a non-zombie in -->
     */
    public class RandomSize
    {
        private static readonly string AdvFeatureClass = "AdvancedZombieFeatures";
        private static readonly string Feature = "RandomSize";
        private static readonly Random random = new Random();
        private static readonly float[] DefaultSizes = { 0.7f, 0.8f, 0.9f, 0.9f, 1.0f, 1.0f, 1.0f, 1.1f, 1.2f };

        public class RandomSizeHelper
        {
            public const string ScaleCVar = "RandomSize";

            // Kept allocation- and log-free: EntityAliveUpdate calls this for every EntityAlive.
            public static bool AllowedRandomSize(global::EntityAlive entity)
            {
                if (entity.isEntityRemote || entity is EntityPlayerLocal)
                    return false;

                var entityClass = EntityClass.list[entity.entityClass];

                // An explicit RandomSize is authoritative in both directions, so RandomSize="false" opts an entity
                // out even when it is a zombie or carries a RandomSizes list.
                if (entityClass.Properties.Values.ContainsKey("RandomSize"))
                    return StringParsers.ParseBool(entityClass.Properties.Values["RandomSize"]);

                return entity is EntityZombie || entityClass.Properties.Values.ContainsKey("RandomSizes");
            }

            public static List<float> ParseFloatList(string value)
            {
                return value.Split(',')
                            .Select(s => StringParsers.ParseFloat(s.Trim()))
                            .ToList();
            }

            // EntityClass.SizeScale is what EntityFactory put on the model before PostInit ran, so it is the
            // baseline every other scale here is measured against.
            public static float GetClassScale(global::EntityAlive entity)
            {
                var entityClass = entity.EntityClass;
                if (entityClass == null || entityClass.SizeScale <= 0f)
                    return 1f;

                return entityClass.SizeScale;
            }

            public static void SetScaleCVar(global::EntityAlive entity, float value)
            {
                if (entity.Buffs.HasCustomVar(ScaleCVar))
                    entity.Buffs.SetCustomVar(ScaleCVar, value);
                else
                    entity.Buffs.AddCustomVar(ScaleCVar, value);
            }
        }

        [HarmonyPatch(typeof(global::EntityAlive), nameof(global::EntityAlive.Init))]
        public class EntityAliveInit
        {
            public static void Postfix(ref global::EntityAlive __instance)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;

                var allowed = RandomSizeHelper.AllowedRandomSize(__instance);
                AdvLogging.DisplayLog(AdvFeatureClass, $"Entity: {__instance.DebugNameInfo} Random Size Allowed: {allowed}");
                if (!allowed) return;

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

                // Only the roll happens here. Scaling the model now would be undone a few lines later, because
                // EntityFactory calls SetScale(EntityClass.SizeScale) after Init returns.
                RandomSizeHelper.SetScaleCVar(__instance, scale);
            }
        }

        /**
         * EntityFactory.CreateEntityOperation.CompleteEntity runs, in order: Init (where the size is rolled),
         * ApplyToEntity, SetScale(EntityClass.SizeScale), SetScale(EntityCreationData.overrideSize), PostInit.
         *
         * Doing the work in PostInit puts the scale on the model and populates OverrideSize before the entity can
         * be handed to anyone. That matters on a server: EntityCreationData snapshots OverrideSize off the live
         * entity when NetEntityDistributionEntry builds a spawn packet, so an entity distributed before this ran
         * would arrive on the client at its unmodified size and stay there for good.
         */
        [HarmonyPatch(typeof(global::EntityAlive), nameof(global::EntityAlive.PostInit))]
        public class EntityAlivePostInit
        {
            public static void Postfix(global::EntityAlive __instance)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;
                if (!RandomSizeHelper.AllowedRandomSize(__instance)) return;

                var modelTransform = __instance.ModelTransform;
                if (modelTransform == null) return;

                var classScale = RandomSizeHelper.GetClassScale(__instance);
                var current = modelTransform.localScale.x;

                // A model that is not sitting on the plain class scale was already sized by CompleteEntity from an
                // incoming EntityCreationData -- a chunk reload, or a prefab copy. Entity.SetScale multiplies the
                // physics extents on every call, so scaling again on top of that would compound them. Adopt the
                // size the entity arrived with and publish it, so clients resolve to the same number we did.
                if (!Mathf.Approximately(current, classScale))
                {
                    __instance.OverrideSize = current;
                    RandomSizeHelper.SetScaleCVar(__instance, current / classScale);
                    AdvLogging.DisplayLog(AdvFeatureClass, $" Kept existing size {current} for {__instance.DebugNameInfo}");
                    return;
                }

                var scale = __instance.Buffs.GetCustomVar(RandomSizeHelper.ScaleCVar);

                // A roll of 1 means leave it alone. OverrideSize stays at 1 so the client skips its own SetScale
                // and both sides keep exactly the size the entity class asked for.
                if (scale == 0f || Mathf.Approximately(scale, 1f)) return;

                var target = classScale * scale;
                __instance.SetScale(target);
                __instance.OverrideSize = target;
                AdvLogging.DisplayLog(AdvFeatureClass, $" Applied size {target} ({classScale} x {scale}) to {__instance.DebugNameInfo}");
            }
        }

        /**
         * Fallback only -- EntityAlivePostInit is where the size is normally applied. This repairs an entity whose
         * model has drifted from the size recorded in OverrideSize. It is keyed off OverrideSize rather than off the
         * CVar so that a vanilla buff-driven MinEventActionSetScale, which sets both the field and the model, is left
         * alone instead of being fought every frame.
         */
        [HarmonyPatch(typeof(global::EntityAlive), nameof(global::EntityAlive.Update))]
        public class EntityAliveUpdate
        {
            public static void Postfix(global::EntityAlive __instance)
            {
                // Runs for every EntityAlive on every frame, so the field read gates everything else.
                var target = __instance.OverrideSize;
                if (target <= 0f || Mathf.Approximately(target, 1f)) return;

                var modelTransform = __instance.ModelTransform;
                if (modelTransform == null || Mathf.Approximately(modelTransform.localScale.x, target)) return;

                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;
                if (!RandomSizeHelper.AllowedRandomSize(__instance)) return;

                AdvLogging.DisplayLog(AdvFeatureClass, $" Restoring size {target} on {__instance.DebugNameInfo}");
                __instance.SetScale(target);
            }
        }
    }
}
