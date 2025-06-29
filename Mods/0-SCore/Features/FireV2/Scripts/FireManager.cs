using UnityEngine;
using System;
using Audio;

public class FireManager : MonoBehaviour
{
    private IFireHandler _fireHandler;
    private ISmokeHandler _smokeHandler;
    private IFireNetworkManager _networkManager;
    private ILightManager _lightManager;
    private FireConfig _config;
    private FireEvents _events;

    public FireEvents Events {
        get { return _events; }
    }

    public bool Enabled => _config.Enabled;
    public static FireManager Instance { get; private set; }

    public void Init()
    {
        _config = FireConfig.LoadFromXml();
        if (!_config.Enabled)
        {
            Debug.Log("FireManager is disabled.");
            return;
        }
        _events = new FireEvents();
        _fireHandler = new FireHandler(_events, _config);
        _smokeHandler = new SmokeHandler(_events, _config);     

        _networkManager = new FireNetworkManager();
        _lightManager = new LightManager();
        InitializeSystem();
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ForceStop()
    {
        
        _fireHandler?.ForceStop();
        CancelInvoke();
        Save();
    }
    private void InitializeSystem()
    {
        InvokeRepeating(nameof(UpdateSystems), _config.CheckInterval, _config.CheckInterval);
        
        // Subscribe to events
        _events.OnFireStarted += HandleFireStarted;
        _events.OnFireExtinguished += HandleFireExtinguished;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        _events.OnFireStarted -= HandleFireStarted;
        _events.OnFireExtinguished -= HandleFireExtinguished;
    }

    private void UpdateSystems()
    {
        try
        {
            _fireHandler.UpdateFires();
            _smokeHandler.CheckSmokePositions();
            _lightManager.UpdateLights();
            CheckForPlayer();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in FireManager.UpdateSystems: {ex.Message}");
        }
    }

    public void CheckForPlayer()
    {
        if (GameManager.Instance.World == null) return;
        if (GameManager.Instance.World.Players == null) return;

        var fireSound = _config.FireSound.ToLower();
        foreach (var player in GameManager.Instance.World.Players.list)
        {
            var position = new Vector3i(player.position);
         
            if (Instance.IsPositionCloseToFire(position, 10))
            {
                // Check if we are already looping on the player.
                if (Manager.loopingOnEntity.TryGetValue(player.entityId, out var value))
                {
                    if (value.ContainsKey(fireSound))
                        continue;
                }

                Manager.PlayInsidePlayerHead(fireSound, player.entityId, isLooping: true);
                continue;
            }

            Manager.StopLoopInsidePlayerHead(fireSound, player.entityId);
        }
    }
    public void AddFire(Vector3i blockPos, int entityId = -1)
    {
        try
        {
            if ( _fireHandler.IsBurning(blockPos)) return;
            if (_fireHandler.IsFlammable(blockPos))
            {
                _fireHandler.AddFire(blockPos, entityId);
                _events.RaiseFireStarted(blockPos, entityId);
                _networkManager.SyncAddFire(blockPos, entityId);

            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error adding fire at position {blockPos}: {ex.Message}");
        }
    }

    public void SyncAddFire(Vector3i blockPos, int entityId = -1)
    {
        AddFire(blockPos, entityId);
        _networkManager.SyncAddFire(blockPos, entityId);
    }

    public void ClearFire(Vector3i blockPos)
    {
        if (!Enabled) return;
  
        try
        {
            _fireHandler.RemoveFire(blockPos, -1, false);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error Remove fire at position {blockPos}: {ex.Message}");
        }
        
    }
    public void ExtinguishFire(Vector3i blockPos, int entityId = -1)
    {
        if (!Enabled) return;
  
        try
        {
            _fireHandler.RemoveFire(blockPos, entityId);
            _smokeHandler.AddSmoke(blockPos);
            _events.RaiseFireExtinguished(blockPos, entityId);
            _networkManager.SyncExtinguishFire(blockPos, entityId);

        }
        catch (Exception ex)
        {
            Debug.LogError($"Error extinguishing fire at position {blockPos}: {ex.Message}");
        }
    }

    public void SyncExtinguishFire(Vector3i blockPos, int entityId = -1)
    {
        ExtinguishFire(blockPos, entityId);
        _networkManager.SyncExtinguishFire(blockPos, entityId);
    }

    public void ClearPosition(Vector3i blockPos)
    {
        _fireHandler.RemoveFire(blockPos);
        _networkManager.SyncExtinguishFire(blockPos, -1);
    }

    private void HandleFireStarted(Vector3i position, int entityId)
    {
        // Handle any additional logic needed when fire starts
        _lightManager.AddLight(position);
    }

    private void HandleFireExtinguished(Vector3i position, int entityId)
    {
        // Handle any additional logic needed when fire is extinguished
        _lightManager.RemoveLight(position);
    }

    public bool IsPositionBurning(Vector3i position)
    {
        return _fireHandler.IsBurning(position);
    }

    public void Save()
    {
        _fireHandler?.SaveState();
    }

    public void Load()
    {
        _fireHandler?.LoadState();
    }

    public void InvokeOnBlockDestroyed(int count)
    {
        _events.RaiseOnBlockDestroyedCount(count);
    }

    public void InvokeFireUpdate(int count)
    {
        _events.RaiseFireUpdate(count);

    }

    public bool IsPositionCloseToFire(Vector3i vector3I, int maxRange)
    {
        return _fireHandler.IsPositionCloseToFire(vector3I, maxRange);
    }

    public void Reset()
    {
        _fireHandler.Reset();
    }

    public bool IsBurning(Vector3i blockPosition)
    {
        return _config.Enabled && _fireHandler.IsBurning(blockPosition);
    }

    public int CloseFires(Vector3i position, int maxRange)
    {
        return _fireHandler.CloseFires(position, maxRange);
    }
}