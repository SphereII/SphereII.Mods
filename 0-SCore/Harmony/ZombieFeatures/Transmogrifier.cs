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
     * { 1, 2, 2, 3, 4, 5, 6, 7, 8 }
     *  
     * Usage XML:
     * <!-- enforce a specific walk type range for an entity class -->
     * <property name="RandomWalkTypes" value="2,3,4,5,6,7" />
     */
    public class Transmogrifier
    {
        private static readonly string AdvFeatureClass = "AdvancedZombieFeatures";
        private static readonly string Feature = "RandomWalk";

        [HarmonyPatch(typeof(global::EntityAlive))]
        [HarmonyPatch("CopyPropertiesFromEntityClass")]
        public class EntityAliveCopyPropertiesFromEntityClass
        {
            public static void Postfix(global::EntityAlive __instance, ref int ___walkType)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;
                if (___walkType is 21 or 22 || __instance is not EntityZombie) return;

                var random = new Random();

                var entityClass = __instance.EntityClass;
                if (entityClass.Properties.Values.ContainsKey("RandomWalkTypes"))
                {
                    var ranges = new List<int>();

                    foreach (var text in entityClass.Properties.Values["RandomWalkTypes"].Split(','))
                        ranges.Add(StringParsers.ParseSInt32(text));

                    var randomIndex = random.Next(0, ranges.Count);
                    ___walkType = ranges[randomIndex];
                    AdvLogging.DisplayLog(AdvFeatureClass, " Random Walk Type: " + ___walkType);
                    return;
                }


                // Distribution of Walk Types in an array. Adjust the numbers as you want for distribution.
                var numbers = new int[] {1, 2, 2, 3, 21,22,5, 6, 7, 7};

                // Randomly generates a number between 0 and the maximum number of elements in the numbers.
                var randomNumber = random.Next(0, numbers.Length);

                // return the randomly selected walk type
                ___walkType = numbers[randomNumber];
                AdvLogging.DisplayLog(AdvFeatureClass, " Random Walk Type: " + ___walkType);
            }
        }
    }
}