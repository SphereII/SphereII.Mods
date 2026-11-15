using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

/// <summary>
/// Keeps every machine's view of which POIs are swapped to a variant in step with the server.
///
/// The swap itself (QuestPOIVariantManager.PrepareForReset) only happens on the server. Without this,
/// clients kept the original prefab on the POI, and after a restart the server did too, so the POI's
/// name, tier and tags described the original while the world held the variant's blocks.
///
/// - Server: each swap is saved to SCore_POIVariants.xml in the save folder and applied again when the
///   game starts. Each swap is sent to connected clients, and the full list to each player that spawns.
/// - Client: applies what the server sends (QuestPOIVariantManager.ApplySwap). The variant prefab must
///   be installed on the client, as it is for any modlet with prefabs.
///
/// POIs are matched by their bounding box position, which doesn't change when a POI is swapped and is
/// the same on the server and on clients.
/// </summary>
public static class QuestPOIVariantSync
{
    private const string LogPrefix = "[SCore] QuestPOIVariants: ";
    private const string SaveFile = "SCore_POIVariants.xml";

    // POI position -> the variant prefab it's on. POIs on their original prefab aren't listed.
    private static readonly Dictionary<Vector3i, string> Swaps = new Dictionary<Vector3i, string>();
    private static World _world;

    private static bool IsServer => SingletonMonoBehaviour<ConnectionManager>.Instance?.IsServer == true;

    public static void Init()
    {
        ModEvents.GameStartDone.RegisterHandler(OnGameStartDone);
        ModEvents.PlayerSpawnedInWorld.RegisterHandler(OnPlayerSpawnedInWorld);
    }

    /// <summary>Server: the POI was just swapped to variantName (null or empty: back to its original).</summary>
    public static void RecordSwap(PrefabInstance prefabInstance, string variantName)
    {
        if (prefabInstance == null || !IsServer) return;
        RefreshWorld();

        var position = prefabInstance.boundingBoxPosition;
        if (string.IsNullOrEmpty(variantName))
        {
            if (!Swaps.Remove(position)) return;
        }
        else
        {
            if (Swaps.TryGetValue(position, out var current) && current == variantName) return;
            Swaps[position] = variantName;
        }

        Save();
        var single = new Dictionary<Vector3i, string> { { position, variantName ?? string.Empty } };
        SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePOIVariantSDX>().Setup(single));
    }

    /// <summary>Client: apply swaps sent by the server. An empty name means the POI is back on its original.</summary>
    public static void ApplyFromServer(Dictionary<Vector3i, string> swaps)
    {
        if (swaps == null || IsServer) return;
        foreach (var entry in swaps)
        {
            var prefabInstance = FindPOI(entry.Key);
            if (prefabInstance == null)
            {
                Log.Warning($"{LogPrefix}server swapped the POI at {entry.Key}, but there's no POI there on this client");
                continue;
            }

            if (!QuestPOIVariantManager.ApplySwap(prefabInstance, entry.Value))
                Log.Warning($"{LogPrefix}could not switch {prefabInstance.name} at {entry.Key} to {entry.Value}; is the prefab installed on this client?");
        }
    }

    private static void OnGameStartDone(ref ModEvents.SGameStartDoneData data)
    {
        if (!IsServer) return;
        Load();
    }

    private static void OnPlayerSpawnedInWorld(ref ModEvents.SPlayerSpawnedInWorldData data)
    {
        if (!IsServer || data.ClientInfo == null) return;
        RefreshWorld();
        if (Swaps.Count == 0) return;
        data.ClientInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePOIVariantSDX>().Setup(Swaps));
    }

    private static PrefabInstance FindPOI(Vector3i position)
    {
        var decorator = GameManager.Instance.GetDynamicPrefabDecorator();
        if (decorator == null) return null;
        foreach (var prefabInstance in decorator.allPrefabs)
            if (prefabInstance.boundingBoxPosition == position)
                return prefabInstance;
        return null;
    }

    private static void RefreshWorld()
    {
        if (GameManager.Instance.World == _world) return;
        // A different world without GameStartDone (shouldn't happen): start from its save.
        Load();
    }

    // -------------------------------------------------------------------------
    // Saving
    // -------------------------------------------------------------------------

    private static string SavePath => $"{GameIO.GetSaveGameDir()}/{SaveFile}";

    private static void Load()
    {
        _world = GameManager.Instance.World;
        Swaps.Clear();

        var path = SavePath;
        if (!File.Exists(path)) return;

        try
        {
            var doc = new XmlDocument();
            doc.Load(path);
            foreach (XmlNode node in doc.SelectNodes("/poi_variants/poi"))
            {
                if (!(node is XmlElement e)) continue;
                var variant = e.GetAttribute("variant");
                if (string.IsNullOrEmpty(variant)) continue;
                Swaps[StringParsers.ParseVector3i(e.GetAttribute("position"))] = variant;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"{LogPrefix}could not read {path}: {ex.Message}");
            return;
        }

        var applied = 0;
        foreach (var entry in new List<KeyValuePair<Vector3i, string>>(Swaps))
        {
            var prefabInstance = FindPOI(entry.Key);
            if (prefabInstance == null)
            {
                Log.Warning($"{LogPrefix}no POI at {entry.Key} in this world; forgetting its swap to {entry.Value}");
                Swaps.Remove(entry.Key);
                continue;
            }

            if (!QuestPOIVariantManager.ApplySwap(prefabInstance, entry.Value))
            {
                // Keep the entry: the variant may be back once its modlet is installed again.
                Log.Warning($"{LogPrefix}could not restore {prefabInstance.name} at {entry.Key} as {entry.Value}");
                continue;
            }

            PrefabSignLibraryLoader.EnsureLibrary(entry.Value);
            applied++;
        }

        Log.Out($"{LogPrefix}restored {applied} POI variant swap(s)");
    }

    private static void Save()
    {
        if (!IsServer) return;
        var dir = GameIO.GetSaveGameDir();
        if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;

        try
        {
            var doc = new XmlDocument();
            var root = doc.CreateElement("poi_variants");
            doc.AppendChild(root);
            foreach (var entry in Swaps)
            {
                var e = doc.CreateElement("poi");
                e.SetAttribute("position", $"{entry.Key.x},{entry.Key.y},{entry.Key.z}");
                e.SetAttribute("variant", entry.Value);
                root.AppendChild(e);
            }

            // Write to a temp file first so a crash mid-write can't lose the list.
            var path = SavePath;
            var temp = path + ".tmp";
            doc.Save(temp);
            if (File.Exists(path)) File.Delete(path);
            File.Move(temp, path);
        }
        catch (Exception ex)
        {
            Log.Error($"{LogPrefix}could not save POI variant swaps: {ex.Message}");
        }
    }
}

/// <summary>Server → client: POIs swapped to a variant (an empty name: back on the original prefab).</summary>
public class NetPackagePOIVariantSDX : NetPackage
{
    private Dictionary<Vector3i, string> swaps = new Dictionary<Vector3i, string>();

    public NetPackagePOIVariantSDX Setup(Dictionary<Vector3i, string> _swaps)
    {
        swaps = new Dictionary<Vector3i, string>(_swaps);
        return this;
    }

    public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        if (_world == null) return;
        QuestPOIVariantSync.ApplyFromServer(swaps);
    }

    public override void read(PooledBinaryReader _br)
    {
        swaps = new Dictionary<Vector3i, string>();
        var count = _br.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            var position = StreamUtils.ReadVector3i(_br);
            swaps[position] = _br.ReadString();
        }
    }

    public override void write(PooledBinaryWriter _bw)
    {
        base.write(_bw);
        _bw.Write(swaps.Count);
        foreach (var entry in swaps)
        {
            StreamUtils.Write(_bw, entry.Key);
            _bw.Write(entry.Value ?? string.Empty);
        }
    }
}
