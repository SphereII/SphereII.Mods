using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightManager : ILightManager
{
    private const int MaxLights = 32;
    private const float FadeOutSpeed = 1.0f;
    private const float DefaultIntensity = 2.0f;
    private const float DefaultRange = 10f;

    private readonly HashSet<Light> _activeLights = new HashSet<Light>();
    private readonly HashSet<Light> _fadingLights = new HashSet<Light>();
    private readonly Dictionary<Vector3i, Light> _lightPositions = new Dictionary<Vector3i, Light>();

    // Reusable buffers to avoid per-frame allocations.
    private readonly List<Vector3i> _invalidPositionBuffer = new List<Vector3i>();
    private readonly List<Light> _completedFadeBuffer = new List<Light>();
    private readonly List<Vector3i> _lightCullBuffer = new List<Vector3i>();

    public void UpdateLights()
    {
        RemoveInvalidLights();
        UpdateFadingLights();
        ManageLightLimit();
    }

    public void AddLight(Vector3i position)
    {
        
        if (_lightPositions.ContainsKey(position))
            return;

        var lightGameObject = new GameObject("FireLight");
        lightGameObject.transform.position = position + new Vector3(0.5f, 0.5f, 0.5f); // Center in block

        var light = lightGameObject.AddComponent<Light>();
        ConfigureLight(light);

        _activeLights.Add(light);
        _lightPositions[position] = light;

        ManageLightLimit();
    }

    public void RemoveLight(Vector3i position)
    {
        if (!_lightPositions.TryGetValue(position, out var light))
            return;

        _lightPositions.Remove(position);
        _activeLights.Remove(light);
        
        if (light != null)
        {
            _fadingLights.Add(light);
        }
    }

    private void ConfigureLight(Light light)
    {
        light.type = LightType.Point;
        light.color = new Color(1f, 0.6f, 0.2f); // Warm fire color
        light.intensity = DefaultIntensity;
        light.range = DefaultRange;
        light.shadows = LightShadows.Soft;
    }

    private void RemoveInvalidLights()
    {
        _activeLights.RemoveWhere(light => light == null);
        _fadingLights.RemoveWhere(light => light == null);

        _invalidPositionBuffer.Clear();
        foreach (var kvp in _lightPositions)
        {
            if (kvp.Value == null)
                _invalidPositionBuffer.Add(kvp.Key);
        }
        foreach (var pos in _invalidPositionBuffer)
            _lightPositions.Remove(pos);
    }

    private void UpdateFadingLights()
    {
        _completedFadeBuffer.Clear();
        foreach (var light in _fadingLights)
        {
            if (light == null) continue;
            light.intensity -= Time.deltaTime * FadeOutSpeed;
            if (light.intensity <= 0f)
            {
                _completedFadeBuffer.Add(light);
                Object.Destroy(light.gameObject);
            }
        }
        foreach (var light in _completedFadeBuffer)
            _fadingLights.Remove(light);
    }

    private void ManageLightLimit()
    {
        if (_activeLights.Count <= MaxLights)
            return;

        var excess = _activeLights.Count - MaxLights;
        _lightCullBuffer.Clear();
        foreach (var kvp in _lightPositions)
        {
            if (_lightCullBuffer.Count >= excess) break;
            _lightCullBuffer.Add(kvp.Key);
        }
        foreach (var pos in _lightCullBuffer)
        {
            if (!_lightPositions.TryGetValue(pos, out var light)) continue;
            _lightPositions.Remove(pos);
            _activeLights.Remove(light);
            _fadingLights.Add(light);
        }
    }

    public void Clear()
    {
        foreach (var light in _activeLights.Concat(_fadingLights))
        {
            if (light != null)
            {
                Object.Destroy(light.gameObject);
            }
        }

        _activeLights.Clear();
        _fadingLights.Clear();
        _lightPositions.Clear();
    }

    public bool HasLight(Vector3i position)
    {
        return _lightPositions.ContainsKey(position);
    }

    public int GetActiveLightCount()
    {
        return _activeLights.Count;
    }

    public int GetFadingLightCount()
    {
        return _fadingLights.Count;
    }
}
