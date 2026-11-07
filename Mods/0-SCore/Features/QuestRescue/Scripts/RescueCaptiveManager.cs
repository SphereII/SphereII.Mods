using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Server side of the rescue quest: spawns one captive per POI, hands its entity ID to every
/// quester that asks, and removes it again if the quest is dropped before the captive is freed.
/// <para>
/// Each party member runs their own copy of the objective, so the captive is keyed by the POI
/// rather than by quest. Nothing here is saved; after a restart the objective sends the ID it
/// stored in its quest, and the captive is re-registered from that.
/// </para>
/// </summary>
public static class RescueCaptiveManager
{
    private const string LogPrefix = "[SCore] QuestRescue: ";

    /// <summary>Cvar set to 1 on every captive. The free dialog action checks it.</summary>
    public const string CaptiveCVar = "RescueCaptive";

    /// <summary>
    /// Cvar set to 1 on a captive while an escort objective is running. The NPCModRescueEscort AI
    /// package keys off it to keep the captive following even with enemies around.
    /// </summary>
    public const string EscortCVar = "RescueEscort";

    /// <summary>
    /// After this many requests the captive is spawned even if the POI was never locked, so a
    /// quest without a RallyPoint still works. The objective sends one request every 3 seconds.
    /// </summary>
    private const int UnlockedSpawnAttempts = 10;

    // POI (PrefabInstance.id) -> captive entity ID.
    private static readonly Dictionary<int, int> Captives = new Dictionary<int, int>();

    // Captive entity ID -> captive_buff to remove when freed. Re-filled after a restart when the
    // objective asks for its captive again.
    private static readonly Dictionary<int, string> CaptiveBuffs = new Dictionary<int, string>();

    // Captives that were dismissed by an escort with on_complete="leave" and are waiting to be removed.
    private static readonly HashSet<int> Leaving = new HashSet<int>();

    // POIs whose blocks are being rebuilt by World.ResetPOIS right now.
    private static readonly HashSet<int> ResettingPOIs = new HashSet<int>();

    private static World _world;

    public static void MarkResetStarted(List<PrefabInstance> prefabInstances)
    {
        RefreshWorld();
        foreach (var prefabInstance in prefabInstances)
            if (prefabInstance != null) ResettingPOIs.Add(prefabInstance.id);
    }

    /// <summary>True while World.ResetPOIS is rebuilding the POI.</summary>
    public static bool IsResetting(PrefabInstance prefabInstance)
    {
        RefreshWorld();
        return prefabInstance != null && ResettingPOIs.Contains(prefabInstance.id);
    }

    public static void MarkResetFinished(List<PrefabInstance> prefabInstances)
    {
        foreach (var prefabInstance in prefabInstances)
            if (prefabInstance != null) ResettingPOIs.Remove(prefabInstance.id);
    }

    /// <summary>
    /// Returns the POI's captive, spawning it if needed. Returns -1 when the POI isn't ready yet
    /// (still being rebuilt, not locked by the rally point, or its chunks aren't loaded); the
    /// objective asks again later.
    /// </summary>
    public static int RequestCaptive(Vector3 poiPosition, string entityGroup, string spawnBlocks,
        string captiveBuff, int knownCaptiveId, int attempt, out Vector3 captivePosition)
    {
        captivePosition = Vector3.zero;
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return -1;
        RefreshWorld();

        var world = GameManager.Instance.World;
        var prefabInstance = GameManager.Instance.GetDynamicPrefabDecorator()?.GetPrefabAtPosition(poiPosition);
        if (prefabInstance == null)
        {
            Log.Warning($"{LogPrefix}no POI at {poiPosition}");
            return -1;
        }

        // The quest already has a captive (e.g. after a restart).
        if (IsUsable(world, knownCaptiveId, out var known))
        {
            Captives[prefabInstance.id] = knownCaptiveId;
            CaptiveBuffs[knownCaptiveId] = captiveBuff;
            captivePosition = known.position;
            return knownCaptiveId;
        }

        // Another party member's objective already spawned one.
        if (Captives.TryGetValue(prefabInstance.id, out var existingId))
        {
            if (IsUsable(world, existingId, out var existing))
            {
                captivePosition = existing.position;
                return existingId;
            }

            Captives.Remove(prefabInstance.id);
        }

        if (!IsReadyForSpawn(world, prefabInstance, attempt)) return -1;

        var entityClass = GetEntityClass(world, entityGroup, out var classError);
        if (entityClass == -1)
        {
            Log.Error($"{LogPrefix}{prefabInstance.name}: {classError}");
            return -1;
        }

        var position = FindSpawnPosition(world, prefabInstance, spawnBlocks, out var source);
        var rotation = new Vector3(0f, world.GetGameRandom().RandomRange(0, 360), 0f);
        if (!(EntityFactory.CreateEntity(entityClass, position, rotation) is EntityAlive captive))
        {
            Log.Error($"{LogPrefix}could not create entity class {entityClass} for '{entityGroup}'");
            return -1;
        }

        // StaticSpawner keeps the captive from despawning while no player is near.
        captive.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
        world.SpawnEntityInWorld(captive);

        // Cvars on server-owned entities aren't sent to clients unless forced.
        captive.Buffs.SetCustomVar(CaptiveCVar, 1f, true, CVarOperation.set, true);
        // Stay runs the Guard task, which walks back to GuardPosition. Unset, that is (0,0,0), and
        // the captive would set off across the map.
        if (captive is IEntityOrderReceiverSDX orderReceiver)
        {
            orderReceiver.GuardPosition = position;
            orderReceiver.GuardLookPosition = position + captive.GetForwardVector();
        }
        EntityUtilities.SetCurrentOrder(captive.entityId, EntityUtilities.Orders.Stay);
        if (!string.IsNullOrEmpty(captiveBuff))
            captive.Buffs.AddBuff(captiveBuff);

        Captives[prefabInstance.id] = captive.entityId;
        CaptiveBuffs[captive.entityId] = captiveBuff;
        captivePosition = position;
        Log.Out($"{LogPrefix}{prefabInstance.name}: spawned captive {captive.EntityName} ({captive.entityId}) at {position} ({source})");
        return captive.entityId;
    }

    /// <summary>
    /// Called when a quest with a captive fails or is abandoned. The captive is removed only if it
    /// was never freed and no one else is still on the quest for this POI.
    /// </summary>
    public static void ReleaseCaptive(Vector3 poiPosition, int captiveId)
    {
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;

        var world = GameManager.Instance.World;
        var prefabInstance = GameManager.Instance.GetDynamicPrefabDecorator()?.GetPrefabAtPosition(poiPosition);
        if (prefabInstance?.lockInstance != null && prefabInstance.lockInstance.IsLocked)
            return; // a party member is still on it

        if (!(world.GetEntity(captiveId) is EntityAlive captive)) return;
        if (captive.Buffs.GetCustomVar(CaptiveCVar) <= 0f) return; // not one of ours
        if (IsFreed(captive)) return;

        if (prefabInstance != null) Captives.Remove(prefabInstance.id);
        CaptiveBuffs.Remove(captiveId);
        Log.Out($"{LogPrefix}removing captive {captive.EntityName} ({captiveId}); the quest ended before it was freed");
        world.RemoveEntity(captiveId, EnumRemoveEntityReason.Despawned);
    }

    /// <summary>
    /// Server half of freeing a captive. The dialog action has already set the leader on the
    /// freeing player's machine (that is the direction cvars sync in); this repeats it here so the
    /// server's leader cache and despawn exemption are set, removes the captive buff, and tells
    /// every quester the captive is free.
    /// </summary>
    public static void FreeCaptive(int captiveId, int playerId)
    {
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;

        var world = GameManager.Instance.World;
        if (!(world.GetEntity(captiveId) is EntityAlive captive) || captive.IsDead()) return;
        if (captive.Buffs.GetCustomVar(CaptiveCVar) <= 0f) return; // not one of ours
        if (!(world.GetEntity(playerId) is EntityPlayer)) return;

        EntityUtilities.SetLeaderAndOwner(captiveId, playerId);

        if (CaptiveBuffs.TryGetValue(captiveId, out var buff))
        {
            if (!string.IsNullOrEmpty(buff)) captive.Buffs.RemoveBuff(buff);
            CaptiveBuffs.Remove(captiveId);
        }

        foreach (var pair in Captives)
        {
            if (pair.Value != captiveId) continue;
            Captives.Remove(pair.Key);
            break;
        }

        Log.Out($"{LogPrefix}captive {captive.EntityName} ({captiveId}) freed by player {playerId}");
        NotifyFreed(captiveId);
    }

    /// <summary>
    /// Turns the escort flag on or off. AI runs on the server, so this is where it has to be set.
    /// </summary>
    public static void SetEscort(int captiveId, bool escorting)
    {
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;
        if (!(GameManager.Instance.World.GetEntity(captiveId) is EntityAlive captive) || captive.IsDead()) return;
        if (captive.Buffs.GetCustomVar(CaptiveCVar) <= 0f) return; // not one of ours

        var value = escorting ? 1f : 0f;
        if (captive.Buffs.GetCustomVar(EscortCVar) == value) return;
        captive.Buffs.SetCustomVar(EscortCVar, value, true, CVarOperation.set, true);
        Log.Out($"{LogPrefix}captive {captive.EntityName} ({captiveId}) escort {(escorting ? "started" : "ended")}");
    }

    /// <summary>
    /// Escort on_complete="leave": the captive stops following its leader (the same steps as the
    /// Dismiss command) and wanders, then is removed after <paramref name="delaySeconds"/> unless
    /// someone hires it again first. Party members each send this; only the first one counts.
    /// </summary>
    public static void LeaveAfterEscort(int captiveId, float delaySeconds)
    {
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;

        var world = GameManager.Instance.World;
        if (!(world.GetEntity(captiveId) is EntityAlive captive) || captive.IsDead()) return;
        if (captive.Buffs.GetCustomVar(CaptiveCVar) <= 0f) return; // not one of ours, or already left
        if (!Leaving.Add(captiveId)) return;

        // No longer a captive: the free response stays hidden and Release leaves it alone.
        captive.Buffs.SetCustomVar(CaptiveCVar, 0f, true, CVarOperation.set, true);
        captive.Buffs.SetCustomVar(EscortCVar, 0f, true, CVarOperation.set, true);

        var leaderId = (int)captive.Buffs.GetCustomVar("Leader");
        if (leaderId <= 0) leaderId = (int)captive.Buffs.GetCustomVar("Owner");
        if (world.GetEntity(leaderId) is EntityPlayer leader)
            EntityUtilities.ExecuteCMD(captiveId, "Dismiss", leader);

        // Clients keep their own copy of these cvars; make sure they see the dismissal too.
        captive.Buffs.SetCustomVar("Leader", 0f, true, CVarOperation.set, true);
        captive.Buffs.SetCustomVar("Owner", 0f, true, CVarOperation.set, true);
        SphereCache.LeaderCache.Remove(captiveId);

        Log.Out($"{LogPrefix}captive {captive.EntityName} ({captiveId}) is leaving; removing it in {delaySeconds:0.#}s");
        GameManager.Instance.StartCoroutine(RemoveAfterDelay(captiveId, delaySeconds));
    }

    /// <summary>
    /// Asks the server to send the captive away (LeaveAfterEscort), from either side. Used by the
    /// quest objectives on the quest owner's machine.
    /// </summary>
    public static void RequestLeave(int playerId, int captiveId, float delaySeconds)
    {
        if (captiveId <= 0) return;

        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            LeaveAfterEscort(captiveId, delaySeconds);
        else
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                NetPackageManager.GetPackage<NetPackageRescueCaptiveSDX>().SetupLeave(
                    playerId, captiveId, Mathf.RoundToInt(delaySeconds)));
    }

    private static IEnumerator RemoveAfterDelay(int captiveId, float delaySeconds)
    {
        if (delaySeconds > 0f)
            yield return new WaitForSeconds(delaySeconds);

        Leaving.Remove(captiveId);
        var world = GameManager.Instance.World;
        if (world == null || !(world.GetEntity(captiveId) is EntityAlive captive) || captive.IsDead()) yield break;

        // A player hired it again while it was leaving: let them keep it.
        if (IsFreed(captive))
        {
            Log.Out($"{LogPrefix}{captive.EntityName} ({captiveId}) was hired again; not removing it");
            yield break;
        }

        Log.Out($"{LogPrefix}removing {captive.EntityName} ({captiveId}); it left after the escort");
        world.RemoveEntity(captiveId, EnumRemoveEntityReason.Despawned);
    }

    /// <summary>
    /// Tells every client that the captive is free, so the objective completes for all party
    /// members, not only the player who freed it.
    /// </summary>
    public static void NotifyFreed(int captiveId)
    {
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;

        ObjectiveRescueNPCSDX.OnCaptiveFreed(captiveId);
        SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
            NetPackageManager.GetPackage<NetPackageRescueCaptiveSDX>().SetupFreed(captiveId));
    }

    public static bool IsFreed(EntityAlive captive)
    {
        return captive.Buffs.GetCustomVar("Leader") > 0f || captive.Buffs.GetCustomVar("Owner") > 0f;
    }

    // -------------------------------------------------------------------------

    private static bool IsUsable(World world, int entityId, out EntityAlive entity)
    {
        entity = entityId > 0 ? world.GetEntity(entityId) as EntityAlive : null;
        return entity != null && !entity.IsDead();
    }

    private static bool IsReadyForSpawn(World world, PrefabInstance prefabInstance, int attempt)
    {
        // Spawning during the rebuild could bury the captive in blocks that are still being placed.
        if (ResettingPOIs.Contains(prefabInstance.id)) return false;

        // A party member's objective can start before the rally point's reset does. The POI is
        // locked right before the reset starts, so wait for the lock.
        var locked = prefabInstance.lockInstance != null && prefabInstance.lockInstance.IsLocked;
        if (!locked)
        {
            if (attempt < UnlockedSpawnAttempts) return false;
            if (attempt == UnlockedSpawnAttempts)
                Log.Warning($"{LogPrefix}{prefabInstance.name} was never locked by a rally point; spawning the captive anyway");
        }

        // Block searches return air in unloaded chunks.
        var min = prefabInstance.boundingBoxPosition;
        var size = prefabInstance.boundingBoxSize;
        for (var x = min.x; x < min.x + size.x + 16; x += 16)
        for (var z = min.z; z < min.z + size.z + 16; z += 16)
        {
            var cx = World.toChunkXZ(Mathf.Min(x, min.x + size.x - 1));
            var cz = World.toChunkXZ(Mathf.Min(z, min.z + size.z - 1));
            if (world.GetChunkSync(cx, cz) == null) return false;
        }

        return true;
    }

    /// <summary>
    /// The captive's entity class: a weighted pick from the group, or the class itself. Spawn
    /// groups usually pad themselves with "none" entries (class id 0) so the biome spawner often
    /// spawns nothing; a captive must always spawn, so only real classes are picked.
    /// </summary>
    internal static int GetEntityClass(World world, string name, out string error)
    {
        error = null;
        if (string.IsNullOrEmpty(name))
        {
            error = "entity_group is empty";
            return -1;
        }

        if (EntityGroups.list.TryGetValue(name, out var group))
        {
            var total = 0f;
            foreach (var entry in group)
                if (entry.prob > 0f && EntityClass.list.ContainsKey(entry.entityClassId)) total += entry.prob;

            if (total <= 0f)
            {
                error = $"entity group '{name}' has no spawnable entity classes (only 'none' entries?)";
                return -1;
            }

            var roll = world.GetGameRandom().RandomFloat * total;
            var last = -1;
            foreach (var entry in group)
            {
                if (entry.prob <= 0f || !EntityClass.list.ContainsKey(entry.entityClassId)) continue;
                last = entry.entityClassId;
                roll -= entry.prob;
                if (roll <= 0f) return last;
            }

            return last; // rounding
        }

        var hash = EntityClass.FromString(name);
        if (EntityClass.list.ContainsKey(hash)) return hash;

        error = $"'{name}' is not an entity group or entity class";
        return -1;
    }

    /// <summary>
    /// Where the captive goes, in order: a random marker block from spawn_block inside the POI, a
    /// random sleeper spawn point in the POI, or the POI's center at ground level.
    /// </summary>
    internal static Vector3 FindSpawnPosition(World world, PrefabInstance prefabInstance, string spawnBlocks, out string source)
    {
        if (!string.IsNullOrEmpty(spawnBlocks))
        {
            var markers = FindMarkerBlocks(world, prefabInstance, spawnBlocks);
            if (markers.Count > 0)
            {
                var marker = markers[world.GetGameRandom().RandomRange(markers.Count)];
                var block = world.GetBlock(marker).Block;
                // Stand on top of a solid marker (a bed, a crate); stand inside a walk-through one.
                var y = block.IsCollideMovement ? marker.y + 1 : marker.y;
                source = $"marker {block.GetBlockName()}";
                return new Vector3(marker.x + 0.5f, y, marker.z + 0.5f);
            }

            Log.Warning($"{LogPrefix}{prefabInstance.name} has no '{spawnBlocks}' block; using a sleeper spawn point");
        }

        var spawnPoints = new List<Vector3i>();
        var volumes = prefabInstance.prefab.SleeperVolumeList;
        for (var i = 0; i < volumes.Count; i++)
        {
            var psv = volumes[i];
            var start = prefabInstance.boundingBoxPosition + psv.startPos;
            var index = world.FindSleeperVolume(start, start + psv.size);
            if (index == -1) continue;
            foreach (var point in world.GetSleeperVolume(index).spawnPointList)
                spawnPoints.Add(point.pos);
        }

        if (spawnPoints.Count > 0)
        {
            var point = spawnPoints[world.GetGameRandom().RandomRange(spawnPoints.Count)];
            source = "sleeper spawn point";
            return new Vector3(point.x + 0.5f, point.y, point.z + 0.5f);
        }

        var center = prefabInstance.boundingBoxPosition + new Vector3i(prefabInstance.boundingBoxSize.x / 2, 0, prefabInstance.boundingBoxSize.z / 2);
        source = "POI center";
        return new Vector3(center.x + 0.5f, world.GetHeight(center.x, center.z) + 1, center.z + 0.5f);
    }

    private static List<Vector3i> FindMarkerBlocks(World world, PrefabInstance prefabInstance, string spawnBlocks)
    {
        var names = new HashSet<string>();
        foreach (var name in spawnBlocks.Split(','))
            if (!string.IsNullOrWhiteSpace(name)) names.Add(name.Trim());

        var found = new List<Vector3i>();
        var min = prefabInstance.boundingBoxPosition;
        var size = prefabInstance.boundingBoxSize;
        for (var x = min.x; x < min.x + size.x; x++)
        for (var z = min.z; z < min.z + size.z; z++)
        for (var y = min.y; y < min.y + size.y; y++)
        {
            var pos = new Vector3i(x, y, z);
            var blockValue = world.GetBlock(pos);
            if (blockValue.isair || blockValue.ischild) continue;
            if (names.Contains(blockValue.Block.GetBlockName())) found.Add(pos);
        }

        return found;
    }

    /// <summary>The state belongs to one world; drop it when a new one loads.</summary>
    private static void RefreshWorld()
    {
        var world = GameManager.Instance.World;
        if (world == _world) return;
        _world = world;
        Captives.Clear();
        CaptiveBuffs.Clear();
        Leaving.Clear();
        ResettingPOIs.Clear();
    }
}
