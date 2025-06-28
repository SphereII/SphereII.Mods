using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SmokeHandler : ISmokeHandler
{
    private readonly Dictionary<Vector3i, float> _smokeTimers = new Dictionary<Vector3i, float>();
    private readonly FireEvents _events;
    private readonly FireConfig _config;

    public SmokeHandler(FireEvents events, FireConfig config)
    {
        _events = events ?? throw new ArgumentNullException(nameof(events));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public void AddSmoke(Vector3i position)
    {
        if (!_config.Enabled || _smokeTimers.ContainsKey(position))
            return;

        var block = GameManager.Instance.World.GetBlock(position);
        if (block.isair)
            return;

        _smokeTimers[position] = Time.time + _config.SmokeTime;
        
        // Get the appropriate smoke particle
        var smokeParticle = GetSmokeParticle(position);
        
        // Add particle effect
        BlockUtilitiesSDX.addParticlesCentered(smokeParticle, position);
        
        // Raise event
        _events.RaiseSmokeStarted(position);
    }

    public void RemoveSmoke(Vector3i position)
    {
        if (!_smokeTimers.ContainsKey(position))
            return;

        _smokeTimers.Remove(position);
        BlockUtilitiesSDX.removeParticles(position);
        
        // Raise event
        _events.RaiseSmokeEnded(position);
    }

    public void CheckSmokePositions()
    {
        var currentTime = Time.time;
        var expiredPositions = _smokeTimers
            .Where(kv => currentTime > kv.Value)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var position in expiredPositions)
        {
            RemoveSmoke(position);
        }
    }

    public bool HasSmoke(Vector3i position)
    {
        return _smokeTimers.ContainsKey(position);
    }

    private string GetSmokeParticle(Vector3i position)
    {
        var block = GameManager.Instance.World.GetBlock(position);
        var smokeParticle = _config.SmokeParticle;

        // Check block properties for custom smoke particle
        if (block.Block.Properties.Contains("SmokeParticle"))
        {
            smokeParticle = block.Block.Properties.GetString("SmokeParticle");
        }
        // Check block material properties for custom smoke particle
        else if (block.Block.blockMaterial.Properties.Contains("SmokeParticle"))
        {
            smokeParticle = block.Block.blockMaterial.Properties.GetString("SmokeParticle");
        }

        // Check for random smoke particle configuration
        var randomSmoke = _config.SmokeParticle?.Split(',');
        if (randomSmoke != null && randomSmoke.Length > 0)
        {
            var randomIndex = GameManager.Instance.World.GetGameRandom().RandomRange(0, randomSmoke.Length);
            smokeParticle = randomSmoke[randomIndex];
        }

        return smokeParticle;
    }

    public void SaveState()
    {
        // Implementation depends on your save system
        // Example:
        var saveData = new Dictionary<string, object>
        {
            ["smokeTimers"] = _smokeTimers
        };
        
        // Save to disk or network
    }

    public void LoadState()
    {
        // Implementation depends on your save system
        // Example:
        _smokeTimers.Clear();
        
        // Load from disk or network and populate the dictionary
        // Restart particle effects for existing smoke positions
        foreach (var position in _smokeTimers.Keys)
        {
            BlockUtilitiesSDX.addParticlesCentered(GetSmokeParticle(position), position);
        }
    }

    public void Clear()
    {
        foreach (var position in _smokeTimers.Keys.ToList())
        {
            RemoveSmoke(position);
        }
        _smokeTimers.Clear();
    }
}
