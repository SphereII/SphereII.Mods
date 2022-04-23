using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FarmPlotManager
{
    private static FarmPlotManager instance = null;
    private Dictionary<Vector3i, FarmPlotData> FarmPlots = new Dictionary<Vector3i, FarmPlotData>();
    private GameRandom random;

    public static FarmPlotManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new FarmPlotManager();
            }
            return instance;
        }
    }

    public FarmPlotManager()
    {
        random = GameManager.Instance.World.GetGameRandom();
    }
    public void Add(Vector3i position)
    {
        if (FarmPlots.ContainsKey(position))
            return;
        FarmPlots.Add(position, new FarmPlotData(position));    
    }

    public void Remove(Vector3i position)
    {
        if ( FarmPlots.ContainsKey(position))
            FarmPlots.Remove(position);
    }

    public FarmPlotData Get(Vector3i position)
    {
        if ( FarmPlots.ContainsKey(position))
            return FarmPlots[position];
        return null; 
    }
    public FarmPlotData GetRandomCloseEntry(Vector3i position, float range = 50)
    {
        var close = GetCloseEntry(position, range);
        if (close.Count > 0)
            return close[random.RandomRange(0, close.Count)];
        return null;
    }

    public FarmPlotData GetFarmPlotsNearby( Vector3i position)
    {
        foreach( var neighbor in Vector3i.AllDirections)
        {
            var blockPos = position + neighbor;
            if (FarmPlots.ContainsKey(blockPos) && FarmPlots[blockPos].Visited == false)
                return FarmPlots[blockPos];
        }
        return null;
    }

    public FarmPlotData GetFarmPlotsNearbyWithPlants(Vector3i position)
    {
        foreach (var neighbor in Vector3i.AllDirections)
        {
            var blockPos = position + neighbor;
            if (FarmPlots.ContainsKey(blockPos) && FarmPlots[blockPos].HasPlant() && FarmPlots[blockPos].Visited == false)
                return FarmPlots[blockPos];
        }
        return null;
    }
    public FarmPlotData GetClosesUnmaintainedWithPlants(Vector3i position, float range = 50)
    {
        float distance = range * range;
        FarmPlotData farmData = null;
        foreach (var entry in GetCloseFarmPlots(position, range))
        {
            if (entry.HasPlant() == false) continue;
            if (entry.Visited) continue;

            var distance2 = Vector3.Distance(position, entry.GetBlockPos());
            if (distance > distance2)
            {
                distance = distance2;
                farmData = entry;
            }
        }
        if (farmData == null)
            ResetPlantsInRange(position, range);
        return farmData;
    }
    public FarmPlotData GetClosesUnmaintained(Vector3i position, float range = 50)
    {
        float distance = range * range;
        FarmPlotData farmData =null;
        foreach (var entry in GetCloseFarmPlots(position, range))
        {
            if (entry.Visited) continue;

            var distance2 = Vector3.Distance(position, entry.GetBlockPos());
            if ( distance > distance2)
            {
                distance = distance2;
                farmData = entry;
            }
        }
        if (farmData == null)
            ResetPlantsInRange(position, range);
        return farmData;
    }
    public List<FarmPlotData> GetCloseEntry(Vector3i position, float range = 50)
    {
        var close = new List<FarmPlotData>();
        foreach (var entry in FarmPlots)
        {
            var distance = Vector3.Distance(position, entry.Key);
            if (distance < 50)
                close.Add(entry.Value);
        }
        return close;
    }

    public void ResetPlantsInRange( Vector3i position, float range = 50)
    {
        foreach (var entry in FarmPlots)
        {
            var distance = Vector3.Distance(position, entry.Key);
            if (distance < range)
                entry.Value.Reset();
        }
    }
    public void ResetAllPlants()
    {
        foreach(var entry in FarmPlots)
            entry.Value.Reset();
    }
    public List<Vector3> GetClosePositions(Vector3i position, float range = 50)
    {
        var counter = 0;
        var close = new List<Vector3>();
        foreach (var entry in FarmPlots)
        {
            counter++;
            var distance = Vector3.Distance(position, entry.Key);
            if (distance < range && entry.Value.Visited == false)
                close.Add(entry.Key);
        }

        if (counter > 0 && close.Count == 0)
            ResetPlantsInRange(position, range);
        return close;
    }

    public List<FarmPlotData> GetCloseFarmPlots(Vector3i position, float range = 50)
    {
        var counter = 0;
        var close = new List<FarmPlotData>();
        foreach (var entry in FarmPlots)
        {
            counter++;
            var distance = Vector3.Distance(position, entry.Key);
            if (distance < range && entry.Value.Visited == false)
                close.Add(entry.Value);
        }

        if (counter > 0 && close.Count == 0)
            ResetPlantsInRange(position, range);
        return close;
    }
}

