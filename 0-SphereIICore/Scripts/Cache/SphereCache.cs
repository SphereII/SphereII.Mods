using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public static class SphereCache
{
    public static Dictionary<int, List<Vector3>> PathingCache = new Dictionary<int, List<Vector3>>();
    public static Dictionary<int, Vector3i> DoorCache = new Dictionary<int, Vector3i>();
    public static Dictionary<int, Vector3> LastBlock = new Dictionary<int, Vector3>();

    public static System.Random random = new System.Random(DateTime.Now.GetHashCode());

    public static List<Vector3i> caveChunks = new List<Vector3i>(); //static list somewhere
    public static List<Vector3i> caveEntrances = new List<Vector3i>();
    public static Vector3i[] FindRandomPoints(int count)
    {
        Vector3i _minSize;
        Vector3i _maxSize;
        GameManager.Instance.World.GetWorldExtent(out _minSize, out _maxSize);
        GameRandom rand = GameManager.Instance.World.GetGameRandom();
        rand.SetSeed(GameManager.Instance.World.Seed);

        Vector3i[] positions = new Vector3i[count];
        for (int i = 0; i < count; i++)
        {
            // Be sure we are within the bounds of the world.
            int x = (int)Math.Round(Utils.Fastfloor(GameManager.Instance.World.GetGameRandom().RandomRange(_minSize.x + 16, _maxSize.x  - 16)) / 16.0) * 16;
            int z = (int)Math.Round(Utils.Fastfloor(GameManager.Instance.World.GetGameRandom().RandomRange(_minSize.z + 16, _maxSize.z  - 16)) / 16.0) * 16;
            positions[i].x = x;
            positions[i].z = z;
        }

        return positions;
    }
    public static void GenerateCaveChunks(int CaveEntrances = 2)
    {
        string AdvFeatureClass = "CaveConfiguration";

        if (caveChunks.Count == 0)
        {
            int MaxCount = int.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "CaveCluster"));
            int ClusterSize = int.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "CavesClusterSize"));

            String display = "Searching for " + MaxCount + " Cave Clusters. Each Cave Cluster will include " + ClusterSize + " chunks...";
            AdvLogging.DisplayLog(AdvFeatureClass, display);
           
            Vector3i[] RandomCavePoints = FindRandomPoints(MaxCount);
            //for (int x = 0; x < 10; x++)
            //{
            for (int i = 0; i < RandomCavePoints.Length; i++)
            {
                Vector3i randomChunkPosition = RandomCavePoints[i]; //vector3i world pos
                var caveRadius = ClusterSize;

                for (int x = 0; x < CaveEntrances; x++ )
                {
                    //find a random x/z inside the bounds of the cave
                    int entranceX = Utils.Fastfloor(GameManager.Instance.World.GetGameRandom().RandomRange(randomChunkPosition.x, randomChunkPosition.x + (caveRadius * 16)));
                    int entranceZ = Utils.Fastfloor(GameManager.Instance.World.GetGameRandom().RandomRange(randomChunkPosition.z, randomChunkPosition.z + (caveRadius * 16)));
                    Vector3i entrance = new Vector3i(entranceX, x, entranceZ);
                    caveEntrances.Add(new Vector3i(entranceX, x, entranceZ));
                    display = "Cave Spawn Area: " + randomChunkPosition + " Entrance: " + new Vector3i(entranceX, 0, entranceZ);
                    AdvLogging.DisplayLog(AdvFeatureClass, display);
                    Debug.Log(display);
                }

                for (var cX = 0; cX < caveRadius; cX++)
                {
                    for (var cZ = 0; cZ < caveRadius; cZ++)
                    {
                        Vector3i cave = randomChunkPosition + new Vector3i(cX * 16, 0, cZ * 16);
                        caveChunks.Add(randomChunkPosition + new Vector3i(cX * 16, 0, cZ * 16));

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
    public static void DisplayLog(int EntityID, String Message)
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

