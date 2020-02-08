using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class SphereCache
{
    public static Dictionary<int, List<Vector3>> PathingCache = new Dictionary<int, List<Vector3>>();
    public static Dictionary<int, Vector3i> DoorCache = new Dictionary<int,Vector3i>();

    public static System.Random random = new System.Random(DateTime.Now.GetHashCode());

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
    public static bool blDisplayLog = true;
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
            return;
        }
        DisplayLog(EntityID, " Pathing Vector not in Cache...");
    }
    #endregion

}

