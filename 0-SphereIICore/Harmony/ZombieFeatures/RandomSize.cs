using DMT;
using Harmony;
using System;
using System.IO;
using UnityEngine;

public class SphereII_RandomSize
{
    private static string AdvFeatureClass = "AdvancedZombieFeatures";
    private static string Feature = "RandomSize";

    public static class RandomSizeHelper
    {
        public static bool AllowedRandomSize(EntityAlive entity)
        {
            bool bRandomSize = false;

            if (entity is EntityZombie)
                bRandomSize = true;

            EntityClass entityClass = EntityClass.list[entity.entityClass];
            if (entityClass.Properties.Values.ContainsKey("RandomSize"))
                bRandomSize = StringParsers.ParseBool(entityClass.Properties.Values["RandomSize"], 0, -1, true);

            return bRandomSize;
        }
    }

    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("CopyPropertiesFromEntityClass")]
    public class SphereII_EntityAlive_CopyPropertiesFromEntityClass
    {
       
        public static void Postfix(EntityAlive __instance)
        {
            // Check if this feature is enabled.
            if (Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
            {
                if (RandomSizeHelper.AllowedRandomSize(__instance))
                {
                    // This is the distributed random heigh multiplier. Add or adjust values as you see fit. By default, it's just a small adjustment.
                    float[] numbers = new float[9] { 0.7f, 0.8f, 0.9f, 0.9f, 1.0f, 1.0f, 1.0f, 1.1f, 1.2f };

                    System.Random random = new System.Random();
                    int randomIndex = random.Next(0, numbers.Length);
                    float flScale = numbers[randomIndex];

                    AdvLogging.DisplayLog(AdvFeatureClass, " Random Size: " + flScale);
                    // scale down the zombies, or upscale them
                    __instance.gameObject.transform.localScale = new Vector3(flScale, flScale, flScale);
                }
            }
        }

    }

    // Read Helper to make sure the size of the zombies are distributed properly
    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("Read")]
    public class SphereII_EntityAlive_Read
    {
        public static void Postfix(EntityAlive __instance, BinaryReader _br)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return;

            try
            {
                if (RandomSizeHelper.AllowedRandomSize(__instance))
                {

                    float flScale = _br.ReadSingle();
                    __instance.gameObject.transform.localScale = new Vector3(flScale, flScale, flScale);
                }
            }
            catch (Exception)
            {

            }



        }
    }
    // Write Helper to make sure the size of the zombies are distributed properly
    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("Write")]
    public class SphereII_EntityAlive_Write
    {
        public static void Postfix(EntityAlive __instance, BinaryWriter _bw)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return;
            try
            {
                if (RandomSizeHelper.AllowedRandomSize(__instance))
                {
                    float flScale = __instance.gameObject.transform.localScale.x;
                    _bw.Write(flScale);
                }

            }
            catch (Exception)
            {

            }

        }

    }
}

