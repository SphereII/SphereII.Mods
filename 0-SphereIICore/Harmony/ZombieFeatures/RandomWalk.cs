using HarmonyLib;
using System.Collections.Generic;


/**
 * SphereII_Transmogrifier
 *
 * This class includes a Harmony patch that allows an entity to spawn in with a random walk type
 *
 *  You may specify a property on the entityclasses.xml to specify a range. if this property does not exist, the following range
 *  is used:
 *  
 *  { 1, 2, 2, 3, 4, 5, 6, 7, 8 }
 *  
 * Usage XML:
 * 
 *      <!-- enforce a specific walk type range for an entity class -->
 *      <property name="RandomWalkTypes" value="2,3,4,5,6,7" />
 *
 */
public class SphereII_Transmogrifier
{
    private static readonly string AdvFeatureClass = "AdvancedZombieFeatures";
    private static readonly string Feature = "RandomWalk";

    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("CopyPropertiesFromEntityClass")]
    public class SphereII_EntityAlive_CopyPropertiesFromEntityClass
    {
        public static void Postfix(EntityAlive __instance, ref int ___walkType)
        {
            // Check if this feature is enabled.
            if (Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
            {
                if ((___walkType != 4 && ___walkType != 8 ) && __instance is EntityZombie)
                {

                    // Distribution of Walk Types in an array. Adjust the numbers as you want for distribution. The 9 in the default int[9] indicates how many walk types you've specified.
                    int[] numbers = new int[9] { 1, 2, 2, 3, 4, 5, 6, 7,7 };

                    System.Random random = new System.Random();

                    // Randomly generates a number between 0 and the maximum number of elements in the numbers.
                    int randomNumber = random.Next(0, numbers.Length);

                    // return the randomly selected walk type
                    ___walkType = numbers[randomNumber];
                    AdvLogging.DisplayLog(AdvFeatureClass, " Random Walk Type: " + ___walkType);

                }

                EntityClass entityClass = __instance.EntityClass;
                if (entityClass.Properties.Values.ContainsKey("RandomWalkTypes"))
                {
                    List<int> Ranges = new List<int>();

                    foreach (string text in entityClass.Properties.Values["RandomWalkTypes"].Split(new char[] { ',' }))
                        Ranges.Add(StringParsers.ParseSInt32(text));

                    System.Random random = new System.Random();
                    int randomIndex = random.Next(0, Ranges.Count);
                    ___walkType = Ranges[randomIndex];
                    AdvLogging.DisplayLog(AdvFeatureClass, " Random Walk Type: " + ___walkType);


                }

            }

        }

    }

}

