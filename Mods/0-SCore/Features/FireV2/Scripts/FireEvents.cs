using System;
using UnityEngine;

public class FireEvents
{
    // Basic fire event delegates
    public delegate void FireEventHandler(Vector3i position, int entityId);
    public delegate void FireUpdateEventHandler(int fireCount);
    public delegate void BlockDestroyedEventHandler(Vector3i position, BlockValue blockValue);
    public delegate void BlockDestroyedCountEventHandler( int count);

    // Core fire events
    public event FireEventHandler OnFireStarted;
    public event FireEventHandler OnFireExtinguished;
    public event FireEventHandler OnFireSpread;
    public event FireUpdateEventHandler OnFireUpdate;
    public event BlockDestroyedEventHandler OnBlockDestroyed;
public event    BlockDestroyedCountEventHandler OnBlockDestroyedCount;

    // Additional events for game systems
    public event Action<Vector3i> OnSmokeStarted;
    public event Action<Vector3i> OnSmokeEnded;
    public event Action<Vector3i> OnLightAdded;
    public event Action<Vector3i> OnLightRemoved;

    // Event raising methods with null checks and error handling
    public void RaiseFireStarted(Vector3i position, int entityId)
    {
        try
        {
            OnFireStarted?.Invoke(position, entityId);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error raising FireStarted event: {ex.Message}");
        }
    }

    public void RaiseFireExtinguished(Vector3i position, int entityId)
    {
        try
        {
            OnFireExtinguished?.Invoke(position, entityId);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error raising FireExtinguished event: {ex.Message}");
        }
    }

    public void RaiseFireSpread(Vector3i position, int entityId)
    {
        try
        {
            OnFireSpread?.Invoke(position, entityId);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error raising FireSpread event: {ex.Message}");
        }
    }

    public void RaiseFireUpdate(int fireCount)
    {
        try
        {
            OnFireUpdate?.Invoke(fireCount);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error raising FireUpdate event: {ex.Message}");
        }
    }
    
    public void RaiseOnBlockDestroyedCount(int fireCount)
    {
        try
        {
            OnBlockDestroyedCount?.Invoke(fireCount);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error raising OnBlockDestroyedCount event: {ex.Message}");
        }
    }

    
    public void RaiseBlockDestroyed(Vector3i position, BlockValue blockValue)
    {
        try
        {
            OnBlockDestroyed?.Invoke(position, blockValue);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error raising BlockDestroyed event: {ex.Message}");
        }
    }

    public void RaiseSmokeStarted(Vector3i position)
    {
        try
        {
            OnSmokeStarted?.Invoke(position);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error raising SmokeStarted event: {ex.Message}");
        }
    }

    public void RaiseSmokeEnded(Vector3i position)
    {
        try
        {
            OnSmokeEnded?.Invoke(position);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error raising SmokeEnded event: {ex.Message}");
        }
    }

    public void RaiseLightAdded(Vector3i position)
    {
        try
        {
            OnLightAdded?.Invoke(position);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error raising LightAdded event: {ex.Message}");
        }
    }

    public void RaiseLightRemoved(Vector3i position)
    {
        try
        {
            OnLightRemoved?.Invoke(position);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error raising LightRemoved event: {ex.Message}");
        }
    }
}