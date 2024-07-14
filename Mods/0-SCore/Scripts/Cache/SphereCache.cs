using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WorldGenerationEngineFinal;
using Random = System.Random;

public static class SphereCache
{
    public static Dictionary<int, List<Vector3>> PathingCache = new Dictionary<int, List<Vector3>>();
    public static Dictionary<int, Vector3i> DoorCache = new Dictionary<int, Vector3i>();
    public static Dictionary<int, Vector3> LastBlock = new Dictionary<int, Vector3>();

    public static Random random = new Random(DateTime.Now.GetHashCode());

    public static List<Vector3i> caveChunks = new List<Vector3i>(); //static list somewhere
    public static List<Vector3i> caveEntrances = new List<Vector3i>();

    public static FastNoise fastNoise;
    public static List<string> POIs = new List<string>();
    public static List<string> DeepCavePrefabs = new List<string>();

    public static List<Vector3i> spawnPoints = new List<Vector3i>(); //static list somewhere

    public static HashSet<Vector2> caveMap = new HashSet<Vector2>();
    public static Dictionary<int, Entity> LeaderCache = new Dictionary<int, Entity>();

    public static Dictionary<int, Vector3i> BreakingBlockCache = new Dictionary<int, Vector3i>();
    public static Vector3i[] FindRandomPoints(int count)
    {
        Vector3i _minSize;
        Vector3i _maxSize;
        GameManager.Instance.World.GetWorldExtent(out _minSize, out _maxSize);
        var rand = GameManager.Instance.World.GetGameRandom();
        rand.SetSeed(GameManager.Instance.World.Seed);

        var positions = new Vector3i[count];
        for (var i = 0; i < count; i++)
        {

            // Be sure we are within the bounds of the world.
            var x = (int)Math.Round(Utils.Fastfloor(GameManager.Instance.World.GetGameRandom().RandomRange(_minSize.x + 16, _maxSize.x - 16)) / 16.0) * 16;
            var z = (int)Math.Round(Utils.Fastfloor(GameManager.Instance.World.GetGameRandom().RandomRange(_minSize.z + 16, _maxSize.z - 16)) / 16.0) * 16;
            positions[i].x = x;
            positions[i].z = z;
        }

        return positions;
    }

    public static FastNoise GetFastNoise(Chunk chunk)
    {
        var AdvFeatureClass = "CaveConfiguration";

        if (fastNoise != null)
        {
            if (chunk != null)
                fastNoise.SetSeed(chunk.GetHashCode());
            else
                fastNoise.SetSeed(0);

            return fastNoise;
        }


        fastNoise = new FastNoise();

        // Read in the available POIs
        foreach (var poi in Configuration.GetPropertyValue(AdvFeatureClass, "CavePOIs").Split(','))
            POIs.Add(poi);

        foreach (var poi in Configuration.GetPropertyValue(AdvFeatureClass, "DeepCavePrefabs").Split(','))
            DeepCavePrefabs.Add(poi);

        var Octaves = int.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "Octaves"));
        var Lacunarity = float.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "Lacunarity"));
        var Gain = float.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "Gain"));
        var Frequency = float.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "Frequency"));

        var fractalType = EnumUtils.Parse<FastNoise.FractalType>(Configuration.GetPropertyValue(AdvFeatureClass, "FractalType"));
        var noiseType = EnumUtils.Parse<FastNoise.NoiseType>(Configuration.GetPropertyValue(AdvFeatureClass, "NoiseType"));

        fastNoise.SetFractalType(fractalType);
        fastNoise.SetNoiseType(noiseType);
        fastNoise.SetFractalOctaves(Octaves);
        fastNoise.SetFractalLacunarity(Lacunarity);
        fastNoise.SetFractalGain(Gain);
        fastNoise.SetFrequency(Frequency);

        if (chunk != null)
            fastNoise.SetSeed(chunk.GetHashCode());
        else
            fastNoise.SetSeed(0);

        return fastNoise;
    }

    public static void GenerateCaveChunks(int CaveEntrances = 2)
    {
        var AdvFeatureClass = "CaveConfiguration";

        if (caveChunks.Count == 0)
        {
            var configurationType = Configuration.GetPropertyValue(AdvFeatureClass, "GenerationType");
            switch (configurationType)
            {
                case "Legacy":
                    break;
                case "Sebastian":
                    //Log.Out("Initializing Sebastian Cave System...");
                    //var counter = 0;
                    //var prefabs = GameManager.Instance.GetDynamicPrefabDecorator().allPrefabs;
                    //// garage_02,remnant_oldwest_06,cemetery_01,abandoned_house_01,house_burnt_06,vacant_lot_01,mp_waste_bldg_05_grey,oldwest_coal_factory,diner_03
                    //var prefabFilter = Configuration.GetPropertyValue(AdvFeatureClass, "PrefabSister").Split(',').ToList();
                    //foreach (var sister in prefabFilter)
                    //{
                    //   foreach( var individualInstance in prefabs.FindAll(instance => instance.name.Contains(sister)))
                    //   {
                    //       var pos = individualInstance.boundingBoxPosition;
                    //       var size = 200;
                    //       counter++;
                    //       Log.Out($"Generating Cave at {pos} of size {size}...");
                    //       Sebastian.GenerateCave(pos, size, size);
                    //   }
                    //}
                    //Log.Out($"Cave System Generation Complete: {counter} Caves Generated.");
                    //// Add an empty vector for the caveChunks so we don't re-generate.
                    caveChunks.Add(new Vector3i(0,0,0));
                    return;
                case "HeightMap":
                  //  HeightMapTunneler.Init();
                    break;
                default:
                    break;
            }

            var MaxCount = int.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "CaveCluster"));
            var ClusterSize = int.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "CavesClusterSize"));

            var display = "Searching for " + MaxCount + " Cave Clusters. Each Cave Cluster will include " + ClusterSize + " chunks...";
            AdvLogging.DisplayLog(AdvFeatureClass, display);

            var RandomCavePoints = FindRandomPoints(MaxCount);
            foreach (var randomChunkPosition in RandomCavePoints)
            {
                var caveRadius = ClusterSize;

                for (var x = 0; x < CaveEntrances; x++)
                {
                    //find a random x/z inside the bounds of the cave
                    var entranceX = Utils.Fastfloor(GameManager.Instance.World.GetGameRandom().RandomRange(randomChunkPosition.x, randomChunkPosition.x + caveRadius * 16));
                    var entranceZ = Utils.Fastfloor(GameManager.Instance.World.GetGameRandom().RandomRange(randomChunkPosition.z, randomChunkPosition.z + caveRadius * 16));
                    var entrance = new Vector3i(entranceX, x, entranceZ);
                    caveEntrances.Add(new Vector3i(entranceX, 1, entranceZ));
                    display = "Cave Spawn Area: " + randomChunkPosition + " Entrance: " + new Vector3i(entranceX, 0, entranceZ);
                    AdvLogging.DisplayLog(AdvFeatureClass, display);
                    


                }

                for (var cX = 0; cX < caveRadius; cX++)
                {
                    for (var cZ = 0; cZ < caveRadius; cZ++)
                    {
                        var cave = randomChunkPosition + new Vector3i(cX * 16, 0, cZ * 16);
                        caveChunks.Add(new Vector3i(cave));
                    }
                }
            }
        }
    }

    public static void AddDoor(int EntityID, Vector3i doorPos)
    {
        if (DoorCache.ContainsKey(EntityID))
        {
            DisplayLog(EntityID, " Cache Hit. Replacing Door Vector.");
            DoorCache[EntityID] = doorPos;
        }
        else
        {
            DisplayLog(EntityID, " No Door Cache Exists. Adding for " + EntityID);
            DoorCache.Add(EntityID, doorPos);
        }
    }

    public static Vector3i GetDoor(int EntityID)
    {
        if (DoorCache.ContainsKey(EntityID))
        {
            DisplayLog(EntityID, " Cache Hit. returning Door Position: " + DoorCache[EntityID]);
            return DoorCache[EntityID];
        }

        //  DisplayLog(EntityID, " No Door Record.");
        return Vector3i.zero;
    }

    public static void RemoveDoor(int EntityID, Vector3i doorPos)
    {
        if (DoorCache.ContainsKey(EntityID))
            DoorCache.Remove(EntityID);
    }

    
    #region PathingCache

    public static bool blDisplayLog = false;

    public static void DisplayLog(int EntityID, string Message)
    {
        if (blDisplayLog)
            Debug.Log("SphereCache: " + EntityID + " : " + Message);
    }


    public static void AddPaths(int EntityID, List<Vector3> Paths)
    {
        if (PathingCache.ContainsKey(EntityID))
        {
            DisplayLog(EntityID, " Cache Hit. Updating Paths. New Length: " + Paths.Count);
            PathingCache[EntityID] = Paths;
        }
        else
        {
            DisplayLog(EntityID, " No Cache Entry, Adding Paths. New Length: " + Paths.Count);
            PathingCache.Add(EntityID, Paths);
        }
    }

    public static List<Vector3> GetPaths(int EntityID)
    {
        if (PathingCache.ContainsKey(EntityID))
        {
            DisplayLog(EntityID, " Cache Hit. Returning Paths: " + PathingCache[EntityID].Count);
            return PathingCache[EntityID];
        }

        DisplayLog(EntityID, " No Paths for this Entity.");
        return null;
    }

    public static void RemovePaths(int EntityID)
    {
        if (PathingCache.ContainsKey(EntityID))
        {
            DisplayLog(EntityID, "Removing Entity from Cache");
            PathingCache.Remove(EntityID);
            return;
        }

        DisplayLog(EntityID, " Entity is not in the cache.");
    }


    public static Vector3 GetRandomPath(int EntityID)
    {
        if (PathingCache.ContainsKey(EntityID))
        {
            DisplayLog(EntityID, " Getting Random Cache from pool of " + PathingCache[EntityID].Count);
            return PathingCache[EntityID][random.Next(PathingCache[EntityID].Count)];
        }

        return Vector3.zero;
    }

    public static void RemovePath(int EntityID, Vector3 vector)
    {
        if (PathingCache.ContainsKey(EntityID))
        {
            DisplayLog(EntityID, " Pathing to " + vector + ". Removing from cache.");
            PathingCache[EntityID].Remove(vector);

            if (PathingCache[EntityID].Count == 0)
                PathingCache.Remove(EntityID);
            return;
        }

        DisplayLog(EntityID, " Pathing Vector not in Cache...");
    }

    #endregion
}