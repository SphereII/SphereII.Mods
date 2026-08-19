using HarmonyLib;
using System;
using System.Collections.Generic;

namespace Harmony.ZombieFeatures
{
    /**
     * SCoreTransmogrifier
     * 
     * This class includes a Harmony patch that allows an entity to spawn in with a random walk type
     * 
     * You may specify a property on the entityclasses.xml to specify a range. if this property does not exist, the following range
     * is used:
     *
     * { 1, 2, 2, 3, 5, 6, 7, 7 }
     *
     * Only the upright walk types vanilla actually ships are in that default. Walk type 4 is unused by any entity class,
     * 8 is cWalkTypeCrouch (CrouchHeightFixedUpdate special cases it) and 15 is cWalkTypeBandit, so none of them belong
     * in a random pool for zombies. The crawl types (20+) are reachable only by naming them in RandomWalkTypes.
     *
     * Usage XML:
     * <!-- enforce a specific walk type range for an entity class -->
     * <property name="RandomWalkTypes" value="2,3,5,6,7" />
     */
    public class Transmogrifier
    {
        private static readonly string AdvFeatureClass = "AdvancedZombieFeatures";
        private static readonly string Feature = "RandomWalk";

        // A new Random() seeds off Environment.TickCount, so every entity built in the same tick would otherwise
        // draw the same walk type. Keep one instance for the whole feature.
        private static readonly Random Random = new Random();
        private static readonly FastTags<TagGroup.Global> CrawlerTag = FastTags<TagGroup.Global>.Parse("crawler");

        [HarmonyPatch(typeof(global::EntityAlive))]
        [HarmonyPatch("CopyPropertiesFromEntityClass")]
        public class EntityAliveCopyPropertiesFromEntityClass
        {
            public static void Postfix(global::EntityAlive __instance, ref int ___walkType)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;
                if (__instance is not EntityZombie) return;

                // Anything at or past cWalkTypeCrawlFirst (20) is a crawl and must keep the walk type it spawned with;
                // IsWalkTypeACrawl() is the game's own test, so new crawl types stay covered.
                if (__instance.IsWalkTypeACrawl()) return;
                if (__instance.HasAnyTags(CrawlerTag)) return;

                var entityClass = __instance.EntityClass;
                if (entityClass.Properties.Values.ContainsKey("RandomWalkTypes"))
                {
                    var ranges = new List<int>();

                    foreach (var text in entityClass.Properties.Values["RandomWalkTypes"].Split(','))
                    {
                        var trimmed = text.Trim();
                        if (trimmed.Length > 0)
                            ranges.Add(StringParsers.ParseSInt32(trimmed));
                    }

                    // An empty or malformed RandomWalkTypes leaves the entity on its configured walk type.
                    if (ranges.Count == 0)
                    {
                        AdvLogging.DisplayLog(AdvFeatureClass, $" {__instance.EntityName} has an empty RandomWalkTypes; keeping walk type {___walkType}");
                        return;
                    }

                    var randomIndex = Random.Next(0, ranges.Count);
                    ___walkType = ranges[randomIndex];
                    AdvLogging.DisplayLog(AdvFeatureClass, " Random Walk Type: " + ___walkType);
                    return;
                }


                // Distribution of Walk Types in an array. Adjust the numbers as you want for distribution.
                var numbers = new int[] {1, 2, 2, 3, 5, 6, 7, 7};

                // Randomly generates a number between 0 and the maximum number of elements in the numbers.
                var randomNumber = Random.Next(0, numbers.Length);

                // return the randomly selected walk type
                ___walkType = numbers[randomNumber];
                AdvLogging.DisplayLog(AdvFeatureClass, " Random Walk Type: " + ___walkType);
            }
        }
    }
}