using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

/**
 * SphereII_EntityAlive_Patches
 * 
 * This class includes a Harmony patch to allow EntityAlive's to save and read their faction ID, allowing persistence across game loads.
 * 
 */
class SphereII_EntityAlive_Patches
{
    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("Write")]
    public class SphereII_EntityAlive_Write
    {
        public static void Postfix(EntityAlive __instance, BinaryWriter _bw)
        {
            _bw.Write(__instance.factionId);
        }
    }

    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("Read")]
    public class SphereII_EntityAlive_Read
    {
        public static void Postfix(ref EntityAlive __instance, BinaryReader _br)
        {
            try
            {
                __instance.factionId = _br.ReadByte();
            }
            catch (Exception ex)
            {
                // Fail safe for first load up
            }
        }
    }


}

