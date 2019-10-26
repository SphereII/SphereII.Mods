using DMT;
using Harmony;
using System;
using System.IO;
using UnityEngine;

public class SphereII_Transmogrifier
{
    private static string AdvFeatureClass = "AdvancedZombieFeatures";
    private static string Feature = "RandomWalk";

    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("CopyPropertiesFromEntityClass")]
    public class SphereII_EntityAlive_CopyPropertiesFromEntityClass
    {
        public static void Postfix(EntityAlive __instance, ref int ___walkType)
        {
            // Check if this feature is enabled.
            if (Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
            {
                if ((___walkType != 4) && __instance is EntityZombie)
                {
                    // Distribution of Walk Types in an array. Adjust the numbers as you want for distribution. The 9 in the default int[9] indicates how many walk types you've specified.
                    int[] numbers = new int[9] { 1, 2, 2, 3, 4, 5, 6, 7, 8 };

                    System.Random random = new System.Random();

                    // Randomly generates a number between 0 and the maximum number of elements in the numbers.
                    int randomNumber = random.Next(0, numbers.Length);

                    // return the randomly selected walk type
                    ___walkType = numbers[randomNumber];
                    AdvLogging.DisplayLog(AdvFeatureClass, " Random Walk Type: " + ___walkType);

                }

            }

        }

    }

}

