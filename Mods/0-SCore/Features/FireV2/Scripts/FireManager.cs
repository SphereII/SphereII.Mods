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
    private readonly Stopwatch _stopwatch = new Stopwatch();
    private static bool _joinHookRegistered;
    // How often a client expires its own smoke and "recently extinguished" entries.
    private const float ClientMaintenanceInterval = 5f;
private string _fireSoundLower;

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

        _fireSoundLower = _config.FireSound.ToLower();
        _events = new FireEvents();
        _networkManager = new FireNetworkManager();
        _fireHandler = new FireHandler(_events, _config, _networkManager);
        _smokeHandler = new SmokeHandler(_events, _config);
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
        var isServer = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;

        // Init runs once per game on the same component, so drop anything left from the last one.
        CancelInvoke();
        StopAllCoroutines();
        _isUpdateCycleRunning = false;

        if (isServer)
        {
            // Initial call to start the repeating cycle.
            // Subsequent calls will be scheduled at the end of the coroutine.
            Invoke(nameof(StartUpdateCycle), _config.CheckInterval);

            // The handler is static and registered once; it works on whichever instance is current.
            if (!_joinHookRegistered)
            {
                ModEvents.PlayerSpawnedInWorld.RegisterHandler(SyncFiresToJoiningPlayer);
                _joinHookRegistered = true;
            }
        }
        else
        {
            // The server's update cycle does not run here, so smoke and the extinguished list
            // need their own timer or they never expire on a client.
            InvokeRepeating(nameof(ClientMaintenance), ClientMaintenanceInterval, ClientMaintenanceInterval);
        }

        // Subscribe to events
        _events.OnFireStarted += HandleFireStarted;
        _events.OnFireExtinguished += HandleFireExtinguished;

        // Only the server owns the saved fire state.
        if (isServer && _config.FirePersists)
            Load();
    }

    private void ClientMaintenance()
    {
        if (GameManager.Instance.World == null) return;
        _smokeHandler.CheckSmokePositions();
        _fireHandler.UpdateExtinguishedPositions();
    }

    /// <summary>Server: sends everything that is burning to a player who has just joined.</summary>
    private static void SyncFiresToJoiningPlayer(ref ModEvents.SPlayerSpawnedInWorldData data)
    {
        var manager = Instance;
        if (manager == null || manager._config == null || !manager._config.Enabled) return;
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;
        if (data.ClientInfo == null) return;
        // Only on arrival. Respawning after death or teleporting does not lose the fire map.
        if (data.RespawnType != RespawnType.JoinMultiplayer && data.RespawnType != RespawnType.EnterMultiplayer) return;

        var positions = manager._fireHandler.GetBurningPositions();
        if (positions.Count == 0) return;

        manager._networkManager.SendFiresToClient(data.ClientInfo, positions);
        if (manager._config.Logging)
            Log.Out($"[FireManager] Sent {positions.Count} fires to joining player {data.ClientInfo.entityId}");
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
        _stopwatch.Restart();

        // Perform the updates for smoke and lights first.
        _smokeHandler.CheckSmokePositions();
        _lightManager.UpdateLights();

        // These have to run even when nothing is burning: otherwise a block that was put out stays
        // unburnable until some other fire starts, and saved fires never come back.
        _fireHandler.UpdateExtinguishedPositions();
        _fireHandler.RestorePendingFires();

        // Run the main fire processing coroutine.
        if (_fireHandler.IsAnyFireBurning())
        {
            _updateFires = StartCoroutine(UpdateFiresRoutine());
            yield return _updateFires; // Wait for the fire processing to fully complete.
        }

        _stopwatch.Stop();

        // Log the time taken for the whole process.
        _fireHandler.DisplayStatus(_stopwatch.Elapsed.TotalSeconds);

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

        // The handler applies the block changes and sends the removals when its last batch is done.
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

        var fireSound = _fireSoundLower;

        // Let the Manager handle if it's a new play or an update to an existing one.
        Manager.BroadcastPlay(position, fireSound, player.entityId);

        // Stops the sound if it's a different position
        StopSoundAtPosition(player, position);

        // Update the position whether it's new or existing.
        playerSoundPositions[player.entityId] = position;
    }

    private void StopSoundAtPosition(Entity player, Vector3i checkPosition)
    {
        var fireSound = _fireSoundLower;
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

    /// <summary>
    /// Starts a fire here if the block can burn. On the server this tells every client; on a client
    /// it shows the fire straight away and asks the server, which decides.
    /// </summary>
    public void AddFire(Vector3i blockPos, int entityId = -1)
    {
        try
        {
            if (_fireHandler.IsBurning(blockPos)) return;
            if (!_fireHandler.IsFlammable(blockPos)) return;

            _fireHandler.AddFire(blockPos, entityId);
            _networkManager.SyncAddFire(blockPos, entityId);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error adding fire at position {blockPos}: {ex.Message}");
        }
    }

    /// <summary>
    /// Client: applies a fire the server has already decided on. It is not re-checked against this
    /// machine's idea of what can burn, and nothing is sent back.
    /// </summary>
    public void AddFireFromServer(Vector3i blockPos, int entityId = -1)
    {
        if (!Enabled) return;

        try
        {
            _fireHandler.AddFire(blockPos, entityId, true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error adding fire from server at position {blockPos}: {ex.Message}");
        }
    }

    public void SyncAddFire(Vector3i blockPos, int entityId = -1)
    {
        AddFire(blockPos, entityId);
    }

    public void ClearFire(Vector3i blockPos)
    {
        if (!Enabled) return;

        try
        {
            var wasBurning = _fireHandler.IsBurning(blockPos);
            _fireHandler.RemoveFire(blockPos, -1, false);

            // A client that does not have this chunk loaded never sees the block change, so the
            // server tells it directly.
            if (wasBurning && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
                _networkManager.SyncRemoveFire(blockPos);
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
            if (!_fireHandler.IsBurning(blockPos)) return;

            _fireHandler.RemoveFire(blockPos, entityId);
            _smokeHandler.AddSmoke(blockPos);
           // _events.RaiseFireExtinguished(blockPos, entityId);
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

    /// <summary>
    /// Puts out every fire within <paramref name="range"/> blocks (a cube) of <paramref name="center"/>.
    /// A client clears the fires it knows about straight away, then asks the server, which checks its
    /// own fire map and tells every client what went out. The client's copy of the fire map can miss
    /// fires (for example, fires in chunks it had not loaded yet), so the server always decides.
    /// </summary>
    public void ExtinguishArea(Vector3i center, int range, int entityId = -1)
    {
        if (!Enabled) return;

        try
        {
            range = Math.Max(0, range);
            var burning = new List<Vector3i>();
            for (var x = -range; x <= range; x++)
            for (var y = -range; y <= range; y++)
            for (var z = -range; z <= range; z++)
            {
                var position = new Vector3i(center.x + x, center.y + y, center.z + z);
                if (_fireHandler.IsBurning(position))
                    burning.Add(position);
            }

            var removed = ExtinguishLocal(burning, entityId);

            if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
                _networkManager.SyncExtinguishFireBatch(removed, entityId);
            else
                _networkManager.SyncExtinguishArea(center, range, entityId);

            if (_config.Logging)
                Log.Out($"[FireManager] ExtinguishArea {center} range {range}: extinguished {removed.Count} fires (server: {SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer})");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error extinguishing fires around {center}: {ex.Message}");
        }
    }

    /// <summary>
    /// Puts out fires at a list of positions. A client clears what it knows about and sends the whole
    /// list to the server; the server clears what it has and tells every client what went out.
    /// </summary>
    public void ExtinguishFires(List<Vector3i> positions, int entityId = -1)
    {
        if (!Enabled || positions == null || positions.Count == 0) return;

        try
        {
            var removed = ExtinguishLocal(positions, entityId);
            if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
                _networkManager.SyncExtinguishFireBatch(removed, entityId);
            else
                _networkManager.SyncExtinguishFireBatch(positions, entityId);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error extinguishing {positions.Count} fires: {ex.Message}");
        }
    }

    /// <summary>Removes the burning positions from this machine's fire map, adding smoke. Returns what was removed.</summary>
    private List<Vector3i> ExtinguishLocal(List<Vector3i> positions, int entityId)
    {
        var removed = new List<Vector3i>();
        foreach (var position in positions)
        {
            if (!_fireHandler.IsBurning(position)) continue;
            _fireHandler.RemoveFire(position, entityId);
            _smokeHandler.AddSmoke(position);
            removed.Add(position);
        }

        return removed;
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