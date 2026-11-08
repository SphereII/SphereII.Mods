using System;
using System.Collections.Generic;

/// <summary>
/// Lets a quest rebuild its target POI from a variant prefab when the rally point resets it.
/// A quest opts in with the property "poi_variant_suffix": for a POI named cabin_01 and a suffix of
/// "_bandit", the POI is rebuilt from cabin_01_bandit.
///
/// The variant must have the same footprint (x and z) as the original, but may be taller or shorter.
/// It is built at the original's base height; the variant's own YOffset is not used.
///
/// The swap is never reverted on its own. The POI keeps the variant's blocks until another quest resets
/// it; a quest without the suffix rebuilds the original prefab.
///
/// A POI outpost (POIOutposts, ConvertPOISDX) is always rebuilt from its outpost variant, whatever
/// the quest asks for; outposts are saved by POIOutpostManager.
///
/// Nothing else is saved. After a restart the POI list (prefabs.xml) points at the original prefab again,
/// while the world may still hold a variant's blocks and volumes. So every reset of a POI that has a
/// variant works over the tallest possible column (the original or any of its variants):
/// - all sleeper and trigger volumes inside that column are removed first, so the rebuild recreates
///   them from whichever prefab is being built;
/// - blocks above the prefab being built are cleared, so a taller variant doesn't leave its top
///   floors standing on a shorter rebuild.
/// </summary>
public static class QuestPOIVariantManager
{
    public const string PropVariantSuffix = "poi_variant_suffix";
    private const string LogPrefix = "[SCore] QuestPOIVariants: ";

    // PrefabInstance.id -> the prefab it was loaded with, for POIs currently swapped to a variant.
    private static readonly Dictionary<int, Prefab> OriginalPrefabs = new Dictionary<int, Prefab>();

    // Base prefab name -> tallest height among the original and its variants, or 0 if it has no variants.
    private static readonly Dictionary<string, int> MaxHeights = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    // Prefab name -> whether a prefab file with that name exists.
    private static readonly Dictionary<string, bool> PrefabExists = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

    // "<base name><suffix>" -> whether that variant exists and fits the original's footprint.
    private static readonly Dictionary<string, bool> HasVariantCache = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

    private static readonly List<string> KnownSuffixes = new List<string>();

    // Suffixes used by something other than a quest's poi_variant_suffix (ConvertPOISDX, outposts).
    private static readonly List<string> ExtraSuffixes = new List<string>();
    private static World _world;
    private static int _questCountAtScan = -1;

    /// <summary>
    /// Called before World.ResetPOIS rebuilds the given POIs. Swaps each POI's prefab to the
    /// requested variant (or back to its original), clears the old volumes, and sizes the POI for it.
    /// </summary>
    public static void PrepareForReset(World world, List<PrefabInstance> prefabInstances, QuestClass questClass)
    {
        if (world == null || prefabInstances == null) return;
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;

        RefreshState(world);
        if (KnownSuffixes.Count == 0) return;

        var questSuffix = GetSuffix(questClass);

        foreach (var prefabInstance in prefabInstances)
        {
            if (prefabInstance?.prefab == null) continue;

            // An outpost is always rebuilt as an outpost.
            var suffix = POIOutpostManager.GetSuffix(prefabInstance) ?? questSuffix;

            var original = OriginalPrefabs.TryGetValue(prefabInstance.id, out var stored) ? stored : prefabInstance.prefab;
            var baseName = original.PrefabName;
            var maxHeight = GetMaxHeight(baseName, original, prefabInstance);
            if (maxHeight <= 0) continue; // no variants for this POI

            var target = original;
            if (!string.IsNullOrEmpty(suffix))
                target = LoadVariant(baseName + suffix, original, prefabInstance) ?? original;

            // The world may hold another variant's blocks and volumes even if prefabInstance.prefab says
            // otherwise (e.g. after a restart), so always clean the full column before rebuilding.
            PurgeVolumes(world, prefabInstance, original.size, maxHeight);
            if (target.size.y < maxHeight)
                ClearAbove(world, prefabInstance, original.size, target.size.y, maxHeight);

            // The rebuild, sleeper volume ownership and tile entity resets all use the POI's box.
            prefabInstance.boundingBoxSize = target.size;

            if (target == prefabInstance.prefab) continue;

            if (target == original)
                OriginalPrefabs.Remove(prefabInstance.id);
            else
                OriginalPrefabs[prefabInstance.id] = original;

            Log.Out($"{LogPrefix}{prefabInstance.name} at {prefabInstance.boundingBoxPosition}: rebuilding from {target.PrefabName} (size {target.size})");
            prefabInstance.prefab = target;

            // Loading the variant registered its sign library on the server only; send it to connected clients.
            if (target != original)
                PrefabSignLibraryLoader.EnsureLibrary(target.PrefabName);
        }
    }

    /// <summary>
    /// True if this POI has a usable variant for the suffix: a prefab named after the POI's original
    /// prefab plus the suffix, with the same footprint. POIs currently swapped to a variant still count.
    /// Used by quest objectives to pick only POIs that can be swapped.
    /// </summary>
    public static bool HasVariant(PrefabInstance prefabInstance, string suffix)
    {
        if (prefabInstance?.prefab == null || string.IsNullOrEmpty(suffix)) return false;
        var world = GameManager.Instance.World;
        if (world == null) return false;

        RefreshState(world);
        var original = OriginalPrefabs.TryGetValue(prefabInstance.id, out var stored) ? stored : prefabInstance.prefab;
        var variantName = original.PrefabName + suffix;
        if (HasVariantCache.TryGetValue(variantName, out var has)) return has;

        // The original and the variant are compared at the same rotation, so the result holds for every rotation.
        has = LoadVariant(variantName, original, prefabInstance) != null;
        HasVariantCache[variantName] = has;
        return has;
    }

    /// <summary>
    /// Adds a variant suffix that isn't set on any quest (a ConvertPOISDX action's, for example), so
    /// resets size and clean POIs for its variants too.
    /// </summary>
    public static void RegisterSuffix(string suffix)
    {
        suffix = suffix?.Trim();
        if (string.IsNullOrEmpty(suffix) || ExtraSuffixes.Contains(suffix)) return;
        ExtraSuffixes.Add(suffix);
        _questCountAtScan = -1; // rescan
    }

    /// <summary>The name of the prefab the POI was loaded with, even while it's swapped to a variant.</summary>
    public static string GetOriginalName(PrefabInstance prefabInstance)
    {
        if (prefabInstance?.prefab == null) return string.Empty;
        return OriginalPrefabs.TryGetValue(prefabInstance.id, out var stored) ? stored.PrefabName : prefabInstance.prefab.PrefabName;
    }

    public static string GetSuffix(QuestClass questClass)
    {
        var properties = questClass?.Properties;
        if (properties == null || !properties.Contains(PropVariantSuffix)) return null;
        return properties.GetString(PropVariantSuffix)?.Trim();
    }

    // Clears caches when the world changes, and rescans quests for suffixes when the quest list changes.
    private static void RefreshState(World world)
    {
        if (_world != world)
        {
            _world = world;
            OriginalPrefabs.Clear();
            MaxHeights.Clear();
            PrefabExists.Clear();
            HasVariantCache.Clear();
            _questCountAtScan = -1;
        }

        if (_questCountAtScan == QuestClass.s_Quests.Count) return;
        _questCountAtScan = QuestClass.s_Quests.Count;

        KnownSuffixes.Clear();
        MaxHeights.Clear();
        foreach (var questClass in QuestClass.s_Quests.Values)
        {
            var suffix = GetSuffix(questClass);
            if (!string.IsNullOrEmpty(suffix) && !KnownSuffixes.Contains(suffix))
                KnownSuffixes.Add(suffix);
        }

        foreach (var suffix in ExtraSuffixes)
            if (!KnownSuffixes.Contains(suffix))
                KnownSuffixes.Add(suffix);
    }

    // Tallest height among the original and its usable variants; 0 if there are none.
    private static int GetMaxHeight(string baseName, Prefab original, PrefabInstance prefabInstance)
    {
        if (string.IsNullOrEmpty(baseName)) return 0;
        if (MaxHeights.TryGetValue(baseName, out var maxHeight)) return maxHeight;

        maxHeight = 0;
        foreach (var suffix in KnownSuffixes)
        {
            var variant = LoadVariant(baseName + suffix, original, prefabInstance);
            if (variant == null) continue;
            maxHeight = Math.Max(Math.Max(maxHeight, original.size.y), variant.size.y);
        }

        MaxHeights[baseName] = maxHeight;
        return maxHeight;
    }

    private static bool DoesPrefabExist(string name)
    {
        if (PrefabExists.TryGetValue(name, out var exists)) return exists;
        exists = PathAbstractions.PrefabsSearchPaths.GetLocation(name).Type != PathAbstractions.EAbstractedLocationType.None;
        PrefabExists[name] = exists;
        return exists;
    }

    private static Prefab LoadVariant(string variantName, Prefab original, PrefabInstance prefabInstance)
    {
        if (!DoesPrefabExist(variantName)) return null;

        // The cache is shared and keyed by name + rotation, so ask for the POI's rotation and never rotate it here.
        var prefab = GameManager.Instance.GetDynamicPrefabDecorator()?.prefabCache?.GetPrefabRotated(variantName, prefabInstance.rotation);
        if (prefab == null)
        {
            Log.Warning($"{LogPrefix}Could not load variant prefab {variantName}.");
            return null;
        }

        if (prefab.size.x != original.size.x || prefab.size.z != original.size.z)
        {
            Log.Warning($"{LogPrefix}Variant {variantName} is size {prefab.size} but {original.PrefabName} is {original.size}. The footprint (x and z) must match; using the original.");
            return null;
        }

        if (prefab.yOffset != original.yOffset)
            Log.Warning($"{LogPrefix}Variant {variantName} has YOffset {prefab.yOffset} but {original.PrefabName} has {original.yOffset}. The variant is built at the original's height.");

        return prefab;
    }

    // Removes every sleeper and trigger volume inside the POI's column (up to the tallest variant),
    // despawning anything they spawned. Without this, volumes with the same bounds would be reused (keeping
    // the old spawn group), and volumes the new prefab doesn't have would keep spawning inside it.
    // Volumes are found by area rather than through the POI's own lists, because after a restart volumes
    // above the original's height aren't attached to the POI.
    private static void PurgeVolumes(World world, PrefabInstance prefabInstance, Vector3i footprint, int maxHeight)
    {
        var min = prefabInstance.boundingBoxPosition;
        var max = min + new Vector3i(footprint.x, maxHeight, footprint.z);

        world.RemoveSleeperVolumesFor(prefabInstance);
        world.RemoveTriggerVolumesFor(prefabInstance);
        prefabInstance.sleeperVolumes.Clear();
        prefabInstance.triggerVolumes.Clear();

        var sleeperIds = new HashSet<int>();
        lock (world.sleeperVolumes)
        {
            foreach (var pair in world.sleeperVolumes)
            {
                if (IsInside(pair.Value.BoxMin, pair.Value.BoxMax, min, max))
                    sleeperIds.Add(pair.Key);
            }

            foreach (var id in sleeperIds)
            {
                var volume = world.sleeperVolumes[id];
                volume.DespawnAndReset(world);
                world.sleeperVolumeMap.Remove(new VolumeKey(volume));
                world.sleeperVolumes.Remove(id);
            }
        }

        var triggerIds = new HashSet<int>();
        lock (world.triggerVolumes)
        {
            foreach (var pair in world.triggerVolumes)
            {
                if (IsInside(pair.Value.BoxMin, pair.Value.BoxMax, min, max))
                    triggerIds.Add(pair.Key);
            }

            foreach (var id in triggerIds)
            {
                world.triggerVolumeMap.Remove(new VolumeKey(world.triggerVolumes[id]));
                world.triggerVolumes.Remove(id);
            }
        }

        if (sleeperIds.Count == 0 && triggerIds.Count == 0) return;

        // Chunks keep volume ids in capped lists (255). Stale ids are skipped by the game, but prune them
        // from loaded chunks so repeated resets can't fill those lists up.
        var padding = SleeperVolume.chunkPadding;
        var chunkMin = min - padding;
        var chunkMax = max + padding;
        for (var cx = World.toChunkXZ(chunkMin.x); cx <= World.toChunkXZ(chunkMax.x); cx++)
        {
            for (var cz = World.toChunkXZ(chunkMin.z); cz <= World.toChunkXZ(chunkMax.z); cz++)
            {
                if (world.GetChunkSync(cx, cz) is not Chunk chunk) continue;
                chunk.sleeperVolumes.RemoveAll(sleeperIds.Contains);
                chunk.triggerVolumes.RemoveAll(triggerIds.Contains);
            }
        }
    }

    private static bool IsInside(Vector3i boxMin, Vector3i boxMax, Vector3i min, Vector3i max)
    {
        return boxMin.x >= min.x && boxMin.y >= min.y && boxMin.z >= min.z
               && boxMax.x <= max.x && boxMax.y <= max.y && boxMax.z <= max.z;
    }

    // Sets every non-air block above the prefab being built (up to the tallest variant) to air, so a
    // taller variant's upper floors don't survive a shorter rebuild. The rebuild that follows regenerates
    // these chunks and resends them to clients.
    private static void ClearAbove(World world, PrefabInstance prefabInstance, Vector3i footprint, int fromHeight, int toHeight)
    {
        var basePos = prefabInstance.boundingBoxPosition;
        var yStart = Math.Max(0, basePos.y + fromHeight);
        var yEnd = Math.Min(255, basePos.y + toHeight - 1);
        if (yEnd < yStart) return;

        var cleared = 0;
        for (var x = basePos.x; x < basePos.x + footprint.x; x++)
        {
            for (var z = basePos.z; z < basePos.z + footprint.z; z++)
            {
                if (world.GetChunkFromWorldPos(x, 0, z) is not Chunk chunk) continue;
                var lx = World.toBlockXZ(x);
                var lz = World.toBlockXZ(z);
                for (var y = yStart; y <= yEnd; y++)
                {
                    if (chunk.GetBlock(lx, y, lz).isair) continue;
                    chunk.SetBlock(world, lx, y, lz, BlockValue.Air, true, true, true, true);
                    chunk.SetDensity(lx, y, lz, MarchingCubes.DensityAir);
                    cleared++;
                }
            }
        }

        if (cleared > 0)
            Log.Out($"{LogPrefix}{prefabInstance.name}: cleared {cleared} blocks above height {fromHeight}");
    }
}
