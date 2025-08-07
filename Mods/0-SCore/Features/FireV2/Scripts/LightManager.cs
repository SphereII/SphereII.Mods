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
        
        var invalidPositions = _lightPositions
            .Where(kvp => kvp.Value == null)
            .Select(kvp => kvp.Key)
            .ToList();
        
        foreach (var pos in invalidPositions)
        {
            _lightPositions.Remove(pos);
        }
    }

    private void UpdateFadingLights()
    {
        var completedLights = new HashSet<Light>();

        foreach (var light in _fadingLights)
        {
            if (light == null) continue;

            light.intensity -= Time.deltaTime * FadeOutSpeed;

            if (light.intensity <= 0f)
            {
                completedLights.Add(light);
                Object.Destroy(light.gameObject);
            }
        }

        _fadingLights.ExceptWith(completedLights);
    }

    private void ManageLightLimit()
    {
        if (_activeLights.Count <= MaxLights)
            return;

        var lightsToRemove = _activeLights
            .OrderBy(l => Random.value)
            .Take(_activeLights.Count - MaxLights)
            .ToList();

        foreach (var light in lightsToRemove)
        {
            _activeLights.Remove(light);
            _fadingLights.Add(light);
            
            // Remove from positions dictionary
            var positionKey = _lightPositions
                .FirstOrDefault(kvp => kvp.Value == light).Key;
            
            if (positionKey != default)
            {
                _lightPositions.Remove(positionKey);
            }
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
