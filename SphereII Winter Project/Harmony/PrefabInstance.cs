using DMT;
using Harmony;
using System;
using UnityEngine;

/*
   public class PrefabInstance
   {
	   public PrefabInstance(int _id, string _filename, Vector3i _position, byte _rotation, Prefab _bad, int _standaloneBlockSize)
       {
       }
   }
*/
public class SphereII_WinterProject
{
    public class SphereII_WinterProject_Init : IHarmony
    {
        public void Start()
        {
            var harmony = HarmonyInstance.Create(GetType().ToString());

            // Navezgane only - Since it's pregenerated, it uses a different prefabs loading, with preset locations. This will adjust the prefabs for only navezgane.
            var original = typeof(PrefabInstance).GetConstructor(new Type[] { typeof(int), typeof(string), typeof(Vector3i), typeof(byte), typeof(Prefab), typeof(int) });
            var prefix = typeof(SphereII_WinterProject_PrefabInstance).GetMethod("PrefabInstance_Prefix");
            harmony.Patch(original, new HarmonyMethod(prefix));

            // Random Gen
            harmony.PatchAll();
        }
    }



    // Navezgane only - Since it's pregenerated, it uses a different prefabs loading, with preset locations. This will adjust the prefabs for only navezgane.
    public class SphereII_WinterProject_PrefabInstance
    {
        public static bool PrefabInstance_Prefix(ref PrefabInstance __instance, ref Prefab _bad)
        {

            // No longer needed, but left as an example
            return true;
        }
    }

    [HarmonyPatch(typeof(Prefab))]
    [HarmonyPatch("loadBlockData")]
    public class SphereII_WinterProject_LoadBlockData
    {
        public static bool Postfix(bool __result, ref Prefab __instance)
        {
            if (__result)
            {
                if (__instance.size.y < 10)
                {
                    Debug.Log("\n**************");
                    Debug.Log("Winter Project Prefab Filter : " + __instance.filename + " yOffset: " + __instance.yOffset + " Size: " + __instance.size.ToString());
                    Debug.Log("Disabling POI that is too short. Expect the next line to be a WRN about it. Ignore it. ");
                    return false;
                }
                __instance.yOffset -= 8;
                __instance.bTraderArea = false;
                __instance.bExcludeDistantPOIMesh = true;
                __instance.bCopyAirBlocks = true;
            }
            return __result;
        }

    }

    [HarmonyPatch(typeof(Prefab))]
    [HarmonyPatch("CopyBlocksIntoChunkNoEntities")]
    public class SphereII_WinterProject_Prefab_Prefix_CopyBlocksIntoChunkNoEntities
    {
        public static bool Prefix(ref Prefab __instance, ref Vector3i _prefabTargetPos)
        {
            // If they are pre-generated Winter Project worlds, don't apply this. They'd have already been applied.
            if (GamePrefs.GetString(EnumGamePrefs.GameWorld).ToLower().Contains("winter project"))
                return true;

            _prefabTargetPos.y -= 8;
            __instance.bTraderArea = false;
            __instance.bExcludeDistantPOIMesh = true;
            __instance.bCopyAirBlocks = true;
            return true;

        }

    }

    [HarmonyPatch(typeof(Prefab))]
    [HarmonyPatch("CopyIntoLocal")]
    public class SphereII_WinterProject_Prefab_Prefix
    {
        public static bool Prefix(Prefab __instance, ref Vector3i _destinationPos, ChunkCluster _cluster, QuestTags _questTags)
        {
            // If they are pre-generated Winter Project worlds, don't apply this. They'd have already been applied.
            if (GamePrefs.GetString(EnumGamePrefs.GameWorld).ToLower().Contains("winter project"))
                return true;

            _destinationPos.y -= 8;
            return true;
        }

    }
    [HarmonyPatch(typeof(Prefab))]
    [HarmonyPatch("CopyIntoLocal")]
    public class SphereII_WinterProject_Prefab
    {
        public static void Postfix(Prefab __instance, Vector3i _destinationPos, ChunkCluster _cluster, QuestTags _questTags)
        {
            WinterModPrefab.SetSnowPrefab(__instance, _cluster, _destinationPos, _questTags);
        }

    }

    [HarmonyPatch(typeof(Prefab))]
    [HarmonyPatch("LoadXMLData")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(string) })]
    public class SphereII_WinterProject_Prefab_LoadXMLData
    {
        public static void Postfix(bool __result, ref Prefab __instance)
        {
            if (__result)
            {
                // Distant POI Y Offset: this is what makes the prefabs look like they are on top of the snow, but then drop down.
                __instance.distantPOIYOffset -= 8;
                __instance.bExcludeDistantPOIMesh = true;
            }
        }

    }

    [HarmonyPatch(typeof(PrefabInstance))]
    [HarmonyPatch("CopyIntoChunk")]
    public class SphereII_WinterProject_PrefabInstance_CopyIntoChunk
    {
        public static void Postfix(PrefabInstance __instance, Chunk _chunk)
        {
            WinterModPrefab.SetSnowChunk(_chunk, __instance.boundingBoxPosition, __instance.boundingBoxSize);
        }
    }


}
