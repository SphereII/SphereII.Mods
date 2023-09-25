using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FarmPlotManager
{
    private static FarmPlotManager _instance = null;
    private Dictionary<Vector3i, FarmPlotData> _farmPlots = new Dictionary<Vector3i, FarmPlotData>();
    private readonly GameRandom _random;

    public static FarmPlotManager Instance
    {
        get
        {
            if (_instance != null) return _instance;
            Log.Out("Initialing Farm Plot Manager.");
            _instance = new FarmPlotManager();
            return _instance;
        }
    }

    private FarmPlotManager()
    {
        _random = GameManager.Instance.World.GetGameRandom();
    }

    public void Add(Vector3i position)
    {
        if (_farmPlots.ContainsKey(position))
            return;
        _farmPlots.Add(position, new FarmPlotData(position));
    }


    public void Remove(Vector3i position)
    {
        if (_farmPlots.ContainsKey(position))
            _farmPlots.Remove(position);
    }

    public FarmPlotData Get(Vector3i position)
    {
        if (_farmPlots.ContainsKey(position))
            return _farmPlots[position];

        var lowerPosition = position + Vector3i.down;
        return _farmPlots.ContainsKey(lowerPosition) ? _farmPlots[lowerPosition] : null;
    }

    public FarmPlotData GetRandomCloseEntry(Vector3i position, float range = 50)
    {
        var close = GetCloseEntry(position, range);
        return close.Count > 0 ? close[_random.RandomRange(0, close.Count)] : null;
    }

    public FarmPlotData GetFarmPlotsNearby(Vector3i position, bool needWater = true)
    {
        var range = 1;
        for (var x = -range; x <= range; x++)
        {
            for (var z = -range; z <= range; z++)
            {
                for (var y = position.y - 2; y <= position.y + 2; y++)
                {
                    var blockPos = new Vector3i(position.x + x, y, position.z + z);
                    if (!_farmPlots.ContainsKey(blockPos)) continue;
                    if (_farmPlots[blockPos].Visited) continue;
                    if (needWater)
                    {
                        if (_farmPlots[blockPos].HasWater()) return _farmPlots[blockPos];
                        continue;
                    }

                    return _farmPlots[blockPos];
                }
            }
        }

        return null;
    }

    public FarmPlotData GetFarmPlotsNearbyWithPlants(Vector3i position)
    {
        var range = 1;
        for (var x = -range; x <= range; x++)
        {
            for (var z = -range; z <= range; z++)
            {
                for (var y = position.y - 2; y <= position.y + 2; y++)
                {
                    var blockPos = new Vector3i(position.x + x, y, position.z + z);
                    if (_farmPlots.ContainsKey(blockPos) && _farmPlots[blockPos].HasPlant() &&
                        _farmPlots[blockPos].Visited == false)
                        return _farmPlots[blockPos];
                }
            }
        }

        return null;
    }

    public FarmPlotData GetClosesUnmaintainedWithPlants(Vector3i position, float range = 50)
    {
        var distance = range * range;
        FarmPlotData farmData = null;
        foreach (var entry in GetCloseFarmPlots(position, range))
        {
            if (entry.Visited) continue;
            if (entry.IsEmpty()) continue;
            if (!entry.HasWater()) continue;

            var distance2 = Vector3.Distance(position, entry.GetBlockPos());
            if (distance < distance2) continue;
            distance = distance2;
            farmData = entry;
        }

        return farmData;
    }

    public FarmPlotData GetClosesUnmaintained(Vector3i position, float range = 50)
    {
        var distance = range * range;
        FarmPlotData farmData = null;
        foreach (var entry in GetCloseFarmPlots(position, range))
        {
            if (entry.Visited) continue;
            if (entry.IsEmpty()) continue;
            if (!entry.HasWater()) continue;

            var distance2 = Vector3.Distance(position, entry.GetBlockPos());
            if (distance < distance2) continue;
            distance = distance2;
            farmData = entry;
        }

        return farmData;
    }

    public List<FarmPlotData> GetCloseEntry(Vector3i position, float range = 50)
    {
        var close = new List<FarmPlotData>();
        foreach (var entry in _farmPlots)
        {
            var distance = Vector3.Distance(position, entry.Key);
            if (distance < range)
                close.Add(entry.Value);
        }

        return close;
    }

    public void ResetPlantsInRange(Vector3i position, float range = 50)
    {
        foreach (var entry in _farmPlots)
        {
            var distance = Vector3.Distance(position, entry.Key);
            if (distance < range)
                entry.Value.Reset();
        }
    }

    public bool AllPlotsVisited()
    {
        foreach (var entry in _farmPlots)
        {
            if (entry.Value.Visited == false)
                return false;
        }

        return true;
    }

    public bool AllPlotsVisited(Vector3i position, float range = 50)
    {
        foreach (var entry in _farmPlots)
        {
            var distance = Vector3.Distance(position, entry.Key);
            if (distance > range) continue;
            if (entry.Value.Visited == false)
                return false;
        }

        return true;
    }

    public void ResetAllPlants()
    {
        foreach (var entry in _farmPlots)
            entry.Value.Reset();
    }

    public List<Vector3> GetClosePositions(Vector3i position, float range = 50, bool ignoreEmpty = false)
    {
        var counter = 0;
        var close = new List<Vector3>();
        foreach (var entry in _farmPlots)
        {
            counter++;
            var distance = Vector3.Distance(position, entry.Key);
            if (distance < range && entry.Value.Visited == false)
            {
                // If it doesn't have a plant, ignore it.
                if (ignoreEmpty && entry.Value.IsEmpty()) continue;

                // if it doesn't have water, ignore it.
                if (!entry.Value.HasWater()) continue;
                close.Add(entry.Key);
            }
            //if (distance < range && entry.Value.Visited == false)
            //    close.Add(entry.Key);
        }

        return close;
    }

    public FarmPlotData GetClosesFarmPlotsWilted(Vector3i position, float range = 50)
    {
        var counter = 0;
        var close = new List<FarmPlotData>();
        foreach (var entry in _farmPlots)
        {
            counter++;
            var distance = Vector3.Distance(position, entry.Key);
            if (distance < range)
            {
                if (entry.Value.IsDeadPlant())
                    return entry.Value;
            }
        }

        return null;
    }

    public List<FarmPlotData> GetCloseFarmPlotsWilted(Vector3i position, float range = 50)
    {
        var counter = 0;
        var close = new List<FarmPlotData>();
        foreach (var entry in _farmPlots)
        {
            counter++;
            var distance = Vector3.Distance(position, entry.Key);
            if (distance < range)
            {
                if (entry.Value.IsDeadPlant())
                    close.Add(entry.Value);
            }
        }

        return close;
    }

    public List<FarmPlotData> GetCloseFarmPlots(Vector3i position, float range = 50, bool ignoreEmpty = false)
    {
        var counter = 0;
        var close = new List<FarmPlotData>();
        foreach (var entry in _farmPlots)
        {
            counter++;
            var distance = Vector3.Distance(position, entry.Key);
            if (distance < range && entry.Value.Visited == false)
            {
                // If it doesn't have a plant, ignore it.
                if (ignoreEmpty && entry.Value.IsEmpty()) continue;

                // if it doesn't have water, ignore it.
                if (!entry.Value.HasWater()) continue;

                close.Add(entry.Value);
            }
        }

        return close;
    }
}