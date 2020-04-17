using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

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

