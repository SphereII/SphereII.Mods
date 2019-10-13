using DMT;
using Harmony;
using System;
using System.IO;
using UnityEngine;

public class SphereII_Transmogrifier
{
    public class SphereII_Transmogrifier_Init : IHarmony
    {
        public void Start()
        {
            Debug.Log(" Loading Patch: " + GetType().ToString());
            var harmony = HarmonyInstance.Create(GetType().ToString());
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("GetWalkType")]
    public class SphereII_EntityAlive_GetWalkType
    {
        public static int Postfix(int __result, EntityAlive __instance)
        {
            // Don't adjust crawlers and non-Zombies.
            if ((__result != 4) && __instance is EntityZombie)
            {
                // Distribution of Walk Types in an array. Adjust the numbers as you want for distribution. The 9 in the default int[9] indicates how many walk types you've specified.
                int[] numbers = new int[9] { 1, 2, 2, 3, 4, 5, 6, 7, 8 };

                System.Random random = new System.Random();

                // Randomly generates a number between 0 and the maximum number of elements in the numbers.
                int randomNumber = random.Next(0, numbers.Length);

                // return the randomly selected walk type
                __result = numbers[randomNumber];

            }

            return __result;

        }

    }

 
    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("CopyPropertiesFromEntityClass")]
    public class SphereII_EntityAlive_CopyPropertiesFromEntityClass
    {
        public static void Postfix(EntityAlive __instance)
        {
            EntityClass entityClass = EntityClass.list[__instance.entityClass];
            if (entityClass.Properties.Values.ContainsKey("RandomSize"))
            {
                bool blRandomSize = false;
                bool.TryParse(entityClass.Properties.Values["RandomSize"], out blRandomSize);
                if (blRandomSize)
                {
                    // This is the distributed random heigh multiplier. Add or adjust values as you see fit. By default, it's just a small adjustment.
                    float[] numbers = new float[9] { 0.7f, 0.8f, 0.9f, 0.9f, 1.0f, 1.0f, 1.0f, 1.1f, 1.2f };

                    System.Random random = new System.Random();
                    int randomIndex = random.Next(0, numbers.Length);
                    float flScale = numbers[randomIndex];

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
            try
            {

                float flScale = _br.ReadSingle();
                __instance.gameObject.transform.localScale = new Vector3(flScale, flScale, flScale);
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
            try
            {

                float flScale = __instance.gameObject.transform.localScale.x;
                _bw.Write(flScale);
            }
            catch (Exception)
            {

            }

        }

    }
    // Give a damage boost to headshots
    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("DamageEntity")]
    public class SphereII_EntityAlive_DamageEntity
    {
        public static bool Prefix(EntityAlive __instance, ref DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale)
        {
            // Apply a damage boost if there'sa  head shot.
            if (__instance is EntityZombie)
            {
                bool blHeadShotsMatter = false;
                EntityClass entityClass = EntityClass.list[__instance.entityClass];
                bool.TryParse(entityClass.Properties.Values["HeadShots"], out blHeadShotsMatter);

                if (blHeadShotsMatter)
                {
                    EnumBodyPartHit bodyPart = _damageSource.GetEntityDamageBodyPart(__instance);
                    if (bodyPart == EnumBodyPartHit.Head)
                    {
                        // Apply a damage multiplier for the head shot, and bump the dismember bonus for the head shot
                        // This will allow the heads to go explode off, which according to legend, if the only want to truly kill a zombie.
                        _damageSource.DamageMultiplier = 1f;

                        _damageSource.DismemberChance = 0.08f;
                    }
                    // Reducing the damage to the torso will prevent the entity from being killed by torso shots, while also maintaining de-limbing.
                    else if (bodyPart == EnumBodyPartHit.Torso)
                    {
                        _damageSource.DamageMultiplier = 0.1f;
                    }

                }
            }
            return true;
        }
    }


}
