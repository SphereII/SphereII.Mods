using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Audio;
using Debug = UnityEngine.Debug;

public class FireManager : MonoBehaviour
{
    private IFireHandler _fireHandler;
    private ISmokeHandler _smokeHandler;
    private IFireNetworkManager _networkManager;
    private ILightManager _lightManager;
    private FireConfig _config;
    private FireEvents _events;
    private Coroutine _updateFires;
    private bool _isUpdateCycleRunning; // New flag to track if a cycle is in progress

    public FireEvents Events
    {
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
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            // Initial call to start the repeating cycle.
            // Subsequent calls will be scheduled at the end of the coroutine.
            Invoke(nameof(StartUpdateCycle), _config.CheckInterval);
        }

        // Subscribe to events
        _events.OnFireStarted += HandleFireStarted;
        _events.OnFireExtinguished += HandleFireExtinguished;
    }

    private void OnDestroy()
    {
        if (_events == null) return;
        // Unsubscribe from events
        _events.OnFireStarted -= HandleFireStarted;
        _events.OnFireExtinguished -= HandleFireExtinguished;
    }

    // New method to start the update cycle coroutine
    private void StartUpdateCycle()
    {
        // Don't process fire if there's no players.
        var world = GameManager.Instance.World;
        if (world?.Players == null || world.Players.Count == 0)
        {
            // If no players, reschedule for later.
            Invoke(nameof(StartUpdateCycle), _config.CheckInterval);
            return;
        }

        // Only start a new coroutine if one isn't already running.
        if (!_isUpdateCycleRunning)
        {
            StartCoroutine(UpdateSystemsRoutine());
        }
    }
  
    // This coroutine now encapsulates the entire update cycle.
    private IEnumerator UpdateSystemsRoutine()
    {
        _isUpdateCycleRunning = true;
        var stopwatch = new Stopwatch();
        stopwatch.Restart();

        // Perform the updates for smoke and lights first.
        _smokeHandler.CheckSmokePositions();
        _lightManager.UpdateLights();

        // Run the main fire processing coroutine.
        if (_fireHandler.IsAnyFireBurning())
        {
            _updateFires = StartCoroutine(UpdateFiresRoutine());
            yield return _updateFires; // Wait for the fire processing to fully complete.
        }

        stopwatch.Stop();

        // Log the time taken for the whole process.
        _fireHandler.DisplayStatus(stopwatch.Elapsed.TotalSeconds);

        // Reset the running flag and schedule the next cycle.
        _isUpdateCycleRunning = false;
        Invoke(nameof(StartUpdateCycle), _config.CheckInterval);
    }
    
    // The UpdateFiresRoutine can be called as a sub-coroutine.
    private IEnumerator UpdateFiresRoutine()
    {
        do
        {
            _fireHandler.UpdateFires(); // This is the batch-based processing
            yield return null;
        } while (_fireHandler.IsProcessing());

        var pendingChanges = _fireHandler.GetPendingList();
        if (pendingChanges.Count > 0)
        {
            GameManager.Instance.World.SetBlocksRPC(pendingChanges);
            List<Vector3i> firePositions = new List<Vector3i>();
            foreach (var change in pendingChanges)
            {
                firePositions.Add(change.pos);
            }
            _networkManager.SyncAddFireBatch(firePositions);
        }

        var removal = _fireHandler.GetRemovalBlocks();
        if (removal.Count > 0)
        {
            _networkManager.SyncRemoveFireBatch(removal);
        }
  
        //_fireHandler.FinalizeBatchProcessing();
        _updateFires = null;
    }


    private Dictionary<int, Vector3i> playerSoundPositions = new Dictionary<int, Vector3i>();

    public void CheckForPlayer(Entity player)
    {
        if (player is not EntityPlayer) return;
        var position = new Vector3i(player.position);
        if (!_fireHandler.IsAnyFireBurning())
        {
            // Stops the sound if it's a different position
            StopSoundAtPosition(player, position);
            return;
        }

        if (!Instance.IsPositionCloseToFire(position, 10))
        {
            StopSoundAtPosition(player, position);
            return;
        }

        var fireSound = _config.FireSound.ToLower();

        // Let the Manager handle if it's a new play or an update to an existing one.
        Manager.BroadcastPlay(position, fireSound, player.entityId);

        // Stops the sound if it's a different position
        StopSoundAtPosition(player, position);

        // Update the position whether it's new or existing.
        playerSoundPositions[player.entityId] = position;
    }

    private void StopSoundAtPosition(Entity player, Vector3i checkPosition)
    {
        var fireSound = _config.FireSound.ToLower();
        if (!playerSoundPositions.TryGetValue(player.entityId, out var oldSoundPosition)) return;

        if (checkPosition == oldSoundPosition) return;
        Manager.BroadcastStop(oldSoundPosition, fireSound);
        playerSoundPositions.Remove(player.entityId); // Crucial: remove when no longer playing
    }

    public void CheckForPlayer()
    {
        if (GameManager.Instance.World == null) return;
        if (GameManager.Instance.World.Players == null) return;

        foreach (var player in GameManager.Instance.World.Players.list)
        {
            CheckForPlayer(player);
        }
    }

    public void AddFire(Vector3i blockPos, int entityId = -1)
    {
        try
        {
            if (_fireHandler.IsBurning(blockPos)) return;
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

    public void ExtinguishFire(Vector3i blockPos, int entityId = -1, bool netSync = true)
    {
        if (!Enabled) return;

        try
        {
            _fireHandler.RemoveFire(blockPos, entityId);
            _smokeHandler.AddSmoke(blockPos);
            _events.RaiseFireExtinguished(blockPos, entityId);
            if (netSync)
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
    }

    public void ClearPosition(Vector3i blockPos)
    {
        _fireHandler.RemoveFire(blockPos);
        _networkManager.SyncExtinguishFire(blockPos, -1);
    }

    private void HandleFireStarted(Vector3i position, int entityId)
    {
        // Handle any additional logic needed when fire starts
        // _lightManager.AddLight(position);
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
        if (!_fireHandler.IsAnyFireBurning()) return false;
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