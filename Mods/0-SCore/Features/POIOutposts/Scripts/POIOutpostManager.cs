using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

/// <summary>
/// Server side of POI outposts: a quest's ConvertPOISDX action marks its POI to be rebuilt,
/// permanently, from a variant prefab (for example cabin_01 as cabin_01_outpost), with a settler
/// NPC stationed in it.
/// <para>
/// Outposts are saved with the world (SCore_POIOutposts.xml in the save folder). Each one is:
/// </para>
/// <list type="bullet">
///   <item>rebuilt once all of its chunks are loaded and no player is near it, so it can be marked
///   while the player is away (at turn-in) and is rebuilt out of sight;</item>
///   <item>hidden from every quest's POI search, so no quest can reset it back;</item>
///   <item>always rebuilt from the outpost prefab if something else resets it anyway;</item>
///   <item>given its settler after the rebuild, and a new one if the settler is gone, unless
///   settler_respawn is false.</item>
/// </list>
/// </summary>
public static class POIOutpostManager
{
    private const string LogPrefix = "[SCore] POIOutposts: ";
    private const string SaveFile = "SCore_POIOutposts.xml";

    /// <summary>Cvar set to 1 on every settler, so it can be found again after a restart.</summary>
    public const string SettlerCVar = "POIOutpostSettler";

    // How often loaded outposts are checked.
    private const float TickSeconds = 2f;

    // A POI isn't rebuilt, and its settler isn't respawned, while a player is this close to it.
    private const int PlayerMargin = 10;

    // Chunks must be loaded this long before the rebuild, and before a missing settler counts as
    // missing (entities load with their chunks, but not always on the same frame).
    private const float RebuildDelay = 3f;
    private const float SettlerCheckDelay = 10f;

    public class Outpost
    {
        public Vector3i Position; // the POI's boundingBoxPosition
        public string PoiName = string.Empty;
        public string Suffix = string.Empty;
        public string QuestId = string.Empty;
        public string QuestTags = string.Empty;
        public string Settler = string.Empty;
        public string SettlerBlock = string.Empty;
        public string SettlerBuff = string.Empty;
        public bool Respawn = true;
        public bool Built;
        public bool SettlerSpawned;
        public int SettlerId = -1;

        // Runtime only.
        internal PrefabInstance PrefabInstance;
        internal float LoadedSince = -1f;
        internal bool Busy;
        internal bool SettlerFailed; // the settler couldn't be created; don't retry every tick
    }

    private static readonly Dictionary<Vector3i, Outpost> Outposts = new Dictionary<Vector3i, Outpost>();
    private static World _world;
    private static float _nextTick;

    public static void Init()
    {
        ModEvents.GameStartDone.RegisterHandler(OnGameStartDone);
        ModEvents.GameUpdate.RegisterHandler(OnGameUpdate);
    }

    private static bool IsServer => SingletonMonoBehaviour<ConnectionManager>.Instance?.IsServer == true;

    // -------------------------------------------------------------------------
    // Queries
    // -------------------------------------------------------------------------

    /// <summary>True if the POI is an outpost, or is waiting to become one.</summary>
    public static bool IsOutpost(PrefabInstance prefabInstance)
    {
        return prefabInstance != null && Outposts.Count > 0 && IsServer
               && Outposts.ContainsKey(prefabInstance.boundingBoxPosition);
    }

    /// <summary>The variant suffix the POI must always be rebuilt from, or null if it isn't an outpost.</summary>
    public static string GetSuffix(PrefabInstance prefabInstance)
    {
        if (prefabInstance == null || Outposts.Count == 0 || !IsServer) return null;
        return Outposts.TryGetValue(prefabInstance.boundingBoxPosition, out var outpost) ? outpost.Suffix : null;
    }

    /// <summary>
    /// The list without outposts. Returns the same list when it has none, so the game's cached
    /// lists are only copied when they must be.
    /// </summary>
    public static List<PrefabInstance> WithoutOutposts(List<PrefabInstance> prefabInstances)
    {
        if (prefabInstances == null || Outposts.Count == 0 || !IsServer) return prefabInstances;
        if (!prefabInstances.Exists(IsOutpost)) return prefabInstances;
        return prefabInstances.FindAll(p => !IsOutpost(p));
    }

    public static IEnumerable<Outpost> All => Outposts.Values;

    // -------------------------------------------------------------------------
    // Conversion requests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Marks the POI at poiPosition to become an outpost. Called by ConvertPOISDX on the server,
    /// directly or through NetPackagePOIOutpostSDX. The rebuild itself happens later, in the update.
    /// </summary>
    public static void RequestConversion(Vector3 poiPosition, Outpost request)
    {
        if (!IsServer || request == null) return;
        RefreshWorld();

        var prefabInstance = GameManager.Instance.GetDynamicPrefabDecorator()?.GetPrefabAtPosition(poiPosition);
        if (prefabInstance == null)
        {
            Log.Warning($"{LogPrefix}quest {request.QuestId}: no POI at {poiPosition}; nothing to convert");
            return;
        }

        if (Outposts.TryGetValue(prefabInstance.boundingBoxPosition, out var existing))
        {
            // Every party member's quest runs the action; the first one wins.
            Log.Out($"{LogPrefix}{prefabInstance.name} is already an outpost ({existing.Suffix}); ignoring quest {request.QuestId}");
            return;
        }

        if (string.IsNullOrEmpty(request.Suffix))
        {
            Log.Error($"{LogPrefix}quest {request.QuestId}: ConvertPOISDX has no poi_variant_suffix");
            return;
        }

        QuestPOIVariantManager.RegisterSuffix(request.Suffix);
        if (!QuestPOIVariantManager.HasVariant(prefabInstance, request.Suffix))
        {
            Log.Error($"{LogPrefix}quest {request.QuestId}: {prefabInstance.name} has no usable {request.Suffix} variant (missing, or a different footprint); not converting it");
            return;
        }

        request.Position = prefabInstance.boundingBoxPosition;
        request.PoiName = QuestPOIVariantManager.GetOriginalName(prefabInstance);
        request.PrefabInstance = prefabInstance;
        request.Built = false;
        request.SettlerSpawned = false;
        request.SettlerId = -1;
        Outposts[request.Position] = request;
        Save();
        Log.Out($"{LogPrefix}{prefabInstance.name} at {request.Position} will become an outpost ({request.PoiName}{request.Suffix}) once it's loaded and no player is near it");
    }

    /// <summary>Forgets an outpost: quests can pick the POI again, and it keeps its blocks until one resets it.</summary>
    public static bool Remove(Vector3i position)
    {
        if (!Outposts.Remove(position)) return false;
        Save();
        return true;
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    private static void OnGameUpdate(ref ModEvents.SGameUpdateData data)
    {
        if (Outposts.Count == 0 || Time.time < _nextTick) return;
        _nextTick = Time.time + TickSeconds;
        if (!IsServer || GameManager.Instance.World == null) return;
        RefreshWorld();

        foreach (var outpost in Outposts.Values)
        {
            try
            {
                Process(GameManager.Instance.World, outpost);
            }
            catch (Exception ex)
            {
                Log.Error($"{LogPrefix}{outpost.PoiName} at {outpost.Position}: {ex}");
            }
        }
    }

    private static void Process(World world, Outpost outpost)
    {
        if (outpost.Busy) return;

        var prefabInstance = GetPrefabInstance(outpost);
        if (prefabInstance == null) return;

        if (!AreChunksLoaded(world, prefabInstance))
        {
            outpost.LoadedSince = -1f;
            return;
        }

        if (outpost.LoadedSince < 0f) outpost.LoadedSince = Time.time;
        var loadedFor = Time.time - outpost.LoadedSince;

        if (!outpost.Built)
        {
            if (loadedFor < RebuildDelay) return;
            if (IsPlayerNear(world, prefabInstance)) return;
            if (RescueCaptiveManager.IsResetting(prefabInstance)) return;
            // Another quest is still running here (its rally point locked the POI).
            if (prefabInstance.lockInstance != null && prefabInstance.lockInstance.IsLocked) return;

            outpost.Busy = true;
            GameManager.Instance.StartCoroutine(Rebuild(world, outpost, prefabInstance));
            return;
        }

        if (string.IsNullOrEmpty(outpost.Settler) || outpost.SettlerFailed || loadedFor < SettlerCheckDelay) return;

        var settler = FindSettler(world, outpost, prefabInstance);
        if (settler != null)
        {
            outpost.SettlerId = settler.entityId;
            // Stay runs the Guard task, which walks back to GuardPosition; unset, that's (0,0,0).
            if (settler is IEntityOrderReceiverSDX orderReceiver && orderReceiver.GuardPosition == Vector3.zero)
            {
                orderReceiver.GuardPosition = settler.position;
                orderReceiver.GuardLookPosition = settler.position + settler.GetForwardVector();
            }
            return;
        }

        if (outpost.SettlerSpawned && !outpost.Respawn) return;
        if (IsPlayerNear(world, prefabInstance)) return;
        SpawnSettler(world, outpost, prefabInstance);
    }

    private static IEnumerator Rebuild(World world, Outpost outpost, PrefabInstance prefabInstance)
    {
        try
        {
            QuestPOIVariantManager.RegisterSuffix(outpost.Suffix);
            var questClass = string.IsNullOrEmpty(outpost.QuestId) ? null : QuestClass.GetQuest(outpost.QuestId);
            var questTags = string.IsNullOrEmpty(outpost.QuestTags)
                ? FastTags<TagGroup.Global>.none
                : FastTags<TagGroup.Global>.Parse(outpost.QuestTags);

            Log.Out($"{LogPrefix}{prefabInstance.name} at {outpost.Position}: rebuilding as {outpost.PoiName}{outpost.Suffix}");

            // QuestPOIVariantManager.PrepareForReset asks GetSuffix for this POI, so the rebuild uses
            // the outpost prefab whatever the quest class says.
            yield return world.ResetPOIS(new List<PrefabInstance> { prefabInstance }, questTags, -1, null, questClass);

            outpost.Built = true;
            Save();
            Log.Out($"{LogPrefix}{prefabInstance.name} at {outpost.Position} is now an outpost");
        }
        finally
        {
            outpost.Busy = false;
        }
    }

    private static void SpawnSettler(World world, Outpost outpost, PrefabInstance prefabInstance)
    {
        var entityClass = RescueCaptiveManager.GetEntityClass(world, outpost.Settler, out var error);
        if (entityClass == -1)
        {
            Log.Error($"{LogPrefix}{prefabInstance.name}: settler: {error}");
            outpost.SettlerFailed = true;
            return;
        }

        var position = RescueCaptiveManager.FindSpawnPosition(world, prefabInstance, outpost.SettlerBlock, out var source);
        var rotation = new Vector3(0f, world.GetGameRandom().RandomRange(0, 360), 0f);
        if (!(EntityFactory.CreateEntity(entityClass, position, rotation) is EntityAlive settler))
        {
            Log.Error($"{LogPrefix}{prefabInstance.name}: could not create settler entity class {entityClass} for '{outpost.Settler}'");
            outpost.SettlerFailed = true;
            return;
        }

        // StaticSpawner keeps the settler from despawning while no player is near.
        settler.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
        world.SpawnEntityInWorld(settler);

        // Cvars on server-owned entities aren't sent to clients unless forced.
        settler.Buffs.SetCustomVar(SettlerCVar, 1f, true, CVarOperation.set, true);
        if (settler is IEntityOrderReceiverSDX orderReceiver)
        {
            orderReceiver.GuardPosition = position;
            orderReceiver.GuardLookPosition = position + settler.GetForwardVector();
        }
        EntityUtilities.SetCurrentOrder(settler.entityId, EntityUtilities.Orders.Stay);
        if (!string.IsNullOrEmpty(outpost.SettlerBuff))
            settler.Buffs.AddBuff(outpost.SettlerBuff);

        var respawned = outpost.SettlerSpawned;
        outpost.SettlerId = settler.entityId;
        outpost.SettlerSpawned = true;
        Save();
        Log.Out($"{LogPrefix}{prefabInstance.name}: {(respawned ? "respawned" : "spawned")} settler {settler.EntityName} ({settler.entityId}) at {position} ({source})");
    }

    // The settler by ID, or (after a restart, or if the ID was reused) any living entity in the
    // outpost with the settler cvar.
    private static EntityAlive FindSettler(World world, Outpost outpost, PrefabInstance prefabInstance)
    {
        var bounds = GetBounds(prefabInstance, 4);

        if (outpost.SettlerId > 0 && world.GetEntity(outpost.SettlerId) is EntityAlive known && !known.IsDead()
            && known.Buffs.GetCustomVar(SettlerCVar) > 0f && bounds.Contains(known.position))
            return known;

        var found = new List<Entity>();
        world.GetEntitiesInBounds(typeof(EntityAlive), bounds, found);
        foreach (var entity in found)
        {
            if (entity is EntityAlive alive && !alive.IsDead() && !(alive is EntityPlayer)
                && alive.Buffs.GetCustomVar(SettlerCVar) > 0f)
                return alive;
        }

        return null;
    }

    private static bool IsPlayerNear(World world, PrefabInstance prefabInstance)
    {
        var bounds = GetBounds(prefabInstance, PlayerMargin);
        foreach (var player in world.Players.list)
            if (player != null && bounds.Contains(player.position)) return true;
        return false;
    }

    // The POI's box, grown by margin on every side and covering the full world height.
    private static Bounds GetBounds(PrefabInstance prefabInstance, int margin)
    {
        var min = prefabInstance.boundingBoxPosition.ToVector3();
        var size = prefabInstance.boundingBoxSize.ToVector3();
        var center = new Vector3(min.x + size.x / 2f, 128f, min.z + size.z / 2f);
        return new Bounds(center, new Vector3(size.x + margin * 2, 512f, size.z + margin * 2));
    }

    // ResetPOIS fails on a chunk that isn't loaded.
    private static bool AreChunksLoaded(World world, PrefabInstance prefabInstance)
    {
        var min = prefabInstance.boundingBoxPosition;
        var size = prefabInstance.boundingBoxSize;
        for (var cx = World.toChunkXZ(min.x); cx <= World.toChunkXZ(min.x + size.x - 1); cx++)
        for (var cz = World.toChunkXZ(min.z); cz <= World.toChunkXZ(min.z + size.z - 1); cz++)
        {
            if (world.GetChunkSync(cx, cz) == null) return false;
        }

        return true;
    }

    private static PrefabInstance GetPrefabInstance(Outpost outpost)
    {
        if (outpost.PrefabInstance != null) return outpost.PrefabInstance;

        var decorator = GameManager.Instance.GetDynamicPrefabDecorator();
        if (decorator == null) return null;
        foreach (var prefabInstance in decorator.allPrefabs)
        {
            if (prefabInstance.boundingBoxPosition != outpost.Position) continue;
            outpost.PrefabInstance = prefabInstance;
            return prefabInstance;
        }

        Log.Warning($"{LogPrefix}no POI at {outpost.Position} ({outpost.PoiName}) in this world; ignoring that outpost");
        outpost.Busy = true; // never process it again this session
        return null;
    }

    // -------------------------------------------------------------------------
    // Saving
    // -------------------------------------------------------------------------

    private static void OnGameStartDone(ref ModEvents.SGameStartDoneData data)
    {
        if (!IsServer) return;
        Load();
    }

    private static void RefreshWorld()
    {
        var world = GameManager.Instance.World;
        if (world == _world) return;
        // A different world without GameStartDone (shouldn't happen): start from its save.
        Load();
    }

    private static string SavePath => $"{GameIO.GetSaveGameDir()}/{SaveFile}";

    private static void Load()
    {
        _world = GameManager.Instance.World;
        Outposts.Clear();

        var path = SavePath;
        if (!File.Exists(path)) return;

        try
        {
            var doc = new XmlDocument();
            doc.Load(path);
            foreach (XmlNode node in doc.SelectNodes("/outposts/outpost"))
            {
                if (!(node is XmlElement e)) continue;
                var outpost = new Outpost
                {
                    Position = StringParsers.ParseVector3i(e.GetAttribute("position")),
                    PoiName = e.GetAttribute("poi"),
                    Suffix = e.GetAttribute("suffix"),
                    QuestId = e.GetAttribute("quest"),
                    QuestTags = e.GetAttribute("quest_tags"),
                    Settler = e.GetAttribute("settler"),
                    SettlerBlock = e.GetAttribute("settler_block"),
                    SettlerBuff = e.GetAttribute("settler_buff"),
                    Respawn = !e.HasAttribute("respawn") || StringParsers.ParseBool(e.GetAttribute("respawn")),
                    Built = e.HasAttribute("built") && StringParsers.ParseBool(e.GetAttribute("built")),
                    SettlerSpawned = e.HasAttribute("settler_spawned") && StringParsers.ParseBool(e.GetAttribute("settler_spawned")),
                    SettlerId = e.HasAttribute("settler_id") ? StringParsers.ParseSInt32(e.GetAttribute("settler_id")) : -1
                };
                if (string.IsNullOrEmpty(outpost.Suffix)) continue;
                Outposts[outpost.Position] = outpost;
                QuestPOIVariantManager.RegisterSuffix(outpost.Suffix);
            }

            Log.Out($"{LogPrefix}loaded {Outposts.Count} outpost(s)");
        }
        catch (Exception ex)
        {
            Log.Error($"{LogPrefix}could not read {path}: {ex.Message}");
        }
    }

    private static void Save()
    {
        if (!IsServer) return;
        var dir = GameIO.GetSaveGameDir();
        if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;

        try
        {
            var doc = new XmlDocument();
            var root = doc.CreateElement("outposts");
            doc.AppendChild(root);
            foreach (var outpost in Outposts.Values)
            {
                var e = doc.CreateElement("outpost");
                e.SetAttribute("position", $"{outpost.Position.x},{outpost.Position.y},{outpost.Position.z}");
                e.SetAttribute("poi", outpost.PoiName ?? string.Empty);
                e.SetAttribute("suffix", outpost.Suffix ?? string.Empty);
                e.SetAttribute("quest", outpost.QuestId ?? string.Empty);
                e.SetAttribute("quest_tags", outpost.QuestTags ?? string.Empty);
                e.SetAttribute("settler", outpost.Settler ?? string.Empty);
                e.SetAttribute("settler_block", outpost.SettlerBlock ?? string.Empty);
                e.SetAttribute("settler_buff", outpost.SettlerBuff ?? string.Empty);
                e.SetAttribute("respawn", outpost.Respawn ? "true" : "false");
                e.SetAttribute("built", outpost.Built ? "true" : "false");
                e.SetAttribute("settler_spawned", outpost.SettlerSpawned ? "true" : "false");
                e.SetAttribute("settler_id", outpost.SettlerId.ToString());
                root.AppendChild(e);
            }

            // Write to a temp file first so a crash mid-write can't lose the outposts.
            var path = SavePath;
            var temp = path + ".tmp";
            doc.Save(temp);
            if (File.Exists(path)) File.Delete(path);
            File.Move(temp, path);
        }
        catch (Exception ex)
        {
            Log.Error($"{LogPrefix}could not save outposts: {ex.Message}");
        }
    }
}
