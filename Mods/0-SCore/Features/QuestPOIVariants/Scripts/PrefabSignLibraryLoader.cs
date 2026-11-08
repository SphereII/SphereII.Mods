using System.Collections.Generic;

/// <summary>
/// Keeps the sign libraries of POI variant prefabs loaded and sent to clients.
///
/// A prefab's signs live in "&lt;prefab&gt;_signs.xml" and are registered under the prefab's name only
/// when the prefab file is loaded. Sign libraries are not saved with the world: on startup they come
/// back only because every prefab in prefabs.xml is loaded. A variant (cabin_01_bandit) is not in
/// prefabs.xml, so after a restart its library is missing while the world still holds its sign
/// canvases, and every canvas logs "references sign ..., which has no data" and renders blank.
///
/// Clients also receive sign data only once, when they join. A library loaded later (when a quest
/// swaps a POI to its variant) never reaches players who are already connected.
///
/// EnsureLibrary fixes both: it loads a missing prefab library on demand and pushes its signs to
/// connected clients. Server only; each library is handled once per world.
/// </summary>
public static class PrefabSignLibraryLoader
{
    private const string LogPrefix = "[SCore] PrefabSignLibraryLoader: ";

    // Library ids already loaded / synced (or found to have no file) for the current world.
    private static readonly HashSet<string> Handled = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
    private static World _world;

    /// <summary>
    /// Makes sure the sign library named after a prefab is registered on the server and sent to
    /// connected clients. Safe to call repeatedly; does nothing for non-prefab libraries.
    /// </summary>
    public static void EnsureLibrary(string libraryId)
    {
        if (string.IsNullOrEmpty(libraryId)) return;
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;

        var world = GameManager.Instance.World;
        if (world == null) return;
        if (_world != world)
        {
            _world = world;
            Handled.Clear();
        }

        if (!Handled.Add(libraryId)) return;

        // Default, internal, user and player libraries are not prefab libraries.
        if (libraryId == "[D]" || SignDataManager.localLibraryIds.Contains(libraryId)) return;
        if (SignDataManager.IsPlayerLibraryId(libraryId)) return;

        var manager = SignDataManager.Instance;
        if (!manager.TryGetSignLibrary(libraryId, out var library))
        {
            library = LoadFromPrefab(libraryId);
            if (library == null) return;
            if (!manager.TryRegisterLibrary(libraryId, library))
                manager.TryGetSignLibrary(libraryId, out library);
            Log.Out($"{LogPrefix}loaded {library.signs.Count} sign(s) for prefab {libraryId}");
        }

        // Pushes each sign to connected clients and refreshes any canvas already showing it locally.
        foreach (var guid in new List<System.Guid>(library.signs.Keys))
            manager.MarkSignDirty(new GlobalSignId(libraryId, guid));
    }

    private static SignLibrary LoadFromPrefab(string prefabName)
    {
        var location = PathAbstractions.PrefabsSearchPaths.GetLocation(prefabName);
        if (location.Type == PathAbstractions.EAbstractedLocationType.None) return null;
        if (!SdFile.Exists(location.FullPathNoExtension + "_signs.xml"))
        {
            Log.Warning($"{LogPrefix}prefab {prefabName} has sign canvases but no {prefabName}_signs.xml next to it; those signs can't be shown.");
            return null;
        }

        try
        {
            var library = new SignLibrary();
            library.ReadXml(new XmlFile(location.Folder, location.FileNameNoExtension + "_signs"));
            return library;
        }
        catch (System.Exception ex)
        {
            Log.Error($"{LogPrefix}failed to read {prefabName}_signs.xml: {ex}");
            return null;
        }
    }
}
