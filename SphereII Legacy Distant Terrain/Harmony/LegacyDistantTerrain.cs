using System.Collections.Generic;
using System.Reflection;
using DMT;
using HarmonyLib;
using UnityEngine;

// Re-enables Legacy Distant Terrain for low end machines.
class SphereII_LegacyDistantTerrain
{
    public class SphereII_LegacyDistantTerrain_Init : IHarmony
    {
        public void Start()
        {
            Debug.Log(" Loading Patch: " + this.GetType().ToString());
            var harmony = new Harmony(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }


    [HarmonyPatch(typeof(GameOptionsManager))]
    [HarmonyPatch("ApplyTerrainOptions")]
    public class SphereII_GameOptionsManager_ApplyTerrainOptions
    {
        public static bool Prefix()
        {
            Debug.Log(" SphereII Legacy Distant Terrain: Forcing Low Terrain Textures for FPS Boost..." );
            Shader.EnableKeyword("GAME_TERRAINLOWQ");
            Shader.DisableKeyword("_MAX3LAYER");
            Shader.EnableKeyword("_MAX2LAYER");

            return false;

        }
    }
    [HarmonyPatch(typeof(GameManager))]
    [HarmonyPatch("IsSplatMapAvailable")]
    public class SphereII_GameManager_SplatMap
    {
        public static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(WorldEnvironment))]
    [HarmonyPatch("Cleanup")]
    public class SphereII_WorldEnvironment_Cleanup
    {

        public static void Postfix(WorldEnvironment __instance, ChunkCluster.OnChunkVisibleDelegate ___chunkClusterVisibleDelegate)
        {
            if (DistantTerrain.Instance != null && !GameManager.IsSplatMapAvailable())
            {
                GameManager.Instance.World.ChunkClusters[0].OnChunkVisibleDelegates -= ___chunkClusterVisibleDelegate;
                DistantTerrain.Instance.Cleanup();
                DistantTerrain.Instance = null;
            }
        }
    }
    //Missing Code from A17.4, brought forward for A18.

    [HarmonyPatch(typeof(WorldEnvironment))]
    [HarmonyPatch("OnChunkDisplayed")]
    public class SphereII_WorldEnvironment_OnChunkDisplayed
    {

        public static bool Prefix(long _key, bool _bDisplayed)
        {
 
            if (DistantTerrain.Instance == null)
                return true;

            ChunkCluster chunkCluster = GameManager.Instance.World.ChunkClusters[0];
            if (chunkCluster == null)
                return true;

            if (!GameManager.IsSplatMapAvailable())
            {
                Chunk chunkSync = chunkCluster.GetChunkSync(_key);
                if (_bDisplayed && chunkSync != null && chunkSync.NeedsOnlyCollisionMesh)
                    return true;

                DistantTerrain.Instance.ActivateChunk(WorldChunkCache.extractX(_key), WorldChunkCache.extractZ(_key), !_bDisplayed);
                return false;
            }
            return true;
        }
    }
    // Missing Code from A17.4, brought forward for A18. 
    [HarmonyPatch(typeof(WorldEnvironment))]
    [HarmonyPatch("Update")]
    public class SphereII_VoxelMeshTerrain_Update
    {
        public static bool Prefix(WorldEnvironment __instance, ref bool ___bTerrainActived, EntityPlayer ___localPlayer)
        {
            if (!GameManager.IsDedicatedServer && DistantTerrain.Instance != null && !GameManager.IsSplatMapAvailable())
            {
                if (DistantTerrain.Instance.IsTerrainReady)
                {
                    if (!___bTerrainActived)
                    {
                        ___bTerrainActived = true;
                        DistantTerrain.Instance.SetTerrainVisible(true);
                        List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
                        for (int i = 0; i < chunkArrayCopySync.Count; i++)
                        {
                            if (chunkArrayCopySync[i].IsDisplayed)
                            {
                                DistantTerrain.Instance.ActivateChunk(chunkArrayCopySync[i].X, chunkArrayCopySync[i].Z, false);
                            }
                        }
                    }
                }
                else
                {
                    ___bTerrainActived = false;
                }
                if (___localPlayer)
                    DistantTerrain.Instance.UpdateTerrain(___localPlayer.GetPosition());
            }
            return true;
        }
    }

    // Missing Code from A17.4, brought forward for A18.
    [HarmonyPatch(typeof(WorldEnvironment))]
    [HarmonyPatch("createDistantTerrain")]
    public class SphereII_WorldEnvironment_CreateDistantTerrain
    {
        public static float terrainHeightFuncAllOtherWorlds(float x, float y, float z)
        {
            float poiheightOverride = GameManager.Instance.World.ChunkCache.ChunkProvider.GetPOIHeightOverride((int)x, (int)z);
            if (poiheightOverride != 0f)
            {
                return poiheightOverride - 0.5f;
            }
            if (GameManager.Instance.World.ChunkCache.ChunkProvider.GetTerrainGenerator() == null)
                return poiheightOverride;
            return GameManager.Instance.World.ChunkCache.ChunkProvider.GetTerrainGenerator().GetTerrainHeightAt((int)x, (int)z);
        }

        public static void Postfix(WorldEnvironment __instance, World ___world)
        {
            
            if(GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Empty" || GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Playtesting")
            {
                if(DistantTerrain.Instance != null)
                {
                    DistantTerrain.Instance.Cleanup();
                    DistantTerrain.Instance = null;
                }
                return;
            }
            if (!GameManager.IsDedicatedServer && !GameManager.IsSplatMapAvailable())
            {
                Debug.Log("Creating Legacy Distant Terrain");
                if (DistantTerrain.Instance == null && !GameManager.Instance.World.ChunkClusters[0].IsFixedSize)
                {
                    DistantTerrain.cShiftHiResChunks = new Vector3(0f, 0.5f, 0f);
                    DistantTerrain.Instance = new DistantTerrain();
                    DistantTerrain.Instance.Init();
                }
                DistantTerrainConstants.SeaLevel = 0f;
                DistantTerrain.Instance.Configure(new DelegateGetTerrainHeight(terrainHeightFuncAllOtherWorlds), GameManager.Instance.World.wcd, 0f);
                if (DistantTerrain.Instance != null)
                {
                    DistantTerrain.Instance.SetTerrainVisible(true);
                }
            }
        }
    }
}

