using DMT;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SphereII_Spook
{
    private static readonly string AdvFeatureClass = "Theme";
    private static readonly string Feature = "Spook";


    // Constant Blood Moon
    [HarmonyPatch(typeof(SkyManager))]
    [HarmonyPatch("BloodMoon")]
    public class SphereII_CaveProject_SkyManager_BloodMoon
    {
        public static bool Prefix(ref bool __result)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            SkyManager.SetSunIntensity(0.3f);
            __result = true;
            return false;
        }
    }


    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("OnEntityDeath")]
    public class SphereII_Spook_OnEntityDeath
    {
        public static void Postfix(EntityAlive __instance)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return;

            // Spawn location
            Vector3i blockPos;
            blockPos.x = (int)__instance.position.x;
            blockPos.y = (int)__instance.position.y - 1 ;
            blockPos.z = (int)__instance.position.z;

            // skip if the decal isn't there.
            BlockValue block = GameManager.Instance.World.GetBlock(blockPos);
            if (block.hasdecal)
                return;

            // blood decals
            int[] decals = new int[] { 1, 4, 8 };
            block.hasdecal = true;
            block.decalface = BlockFace.Top;
            int index = UnityEngine.Random.Range(0, decals.Length);
            block.decaltex = (byte)decals[index];

            Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(blockPos);
            if (chunk != null)
                __instance.world.SetBlockRPC(blockPos, block);
        }
    }
}