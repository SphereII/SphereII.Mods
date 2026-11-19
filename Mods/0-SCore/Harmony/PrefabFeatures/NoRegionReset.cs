using System;
using System.Collections.Generic;
using HarmonyLib;

namespace Harmony.PrefabFeatures
{
    /**
     * SCoreNoRegionReset
     *
     * Protects individual POIs from the timed region reset (and the admin regionreset command's protected modes),
     * without having to flag them as a TraderArea.
     *
     * Add to the POI's own prefab XML (next to its .tts):
     *     <property name="SCoreNoRegionReset" value="True" />
     *
     * Each prefab reads only its own XML; there is no inheritance between e.g. cabin.xml and cabin_bandit.xml.
     *
     * Every chunk the POI occupies is given ChunkProtectionLevel.Trader, the same protection the game gives trader
     * compounds, so the reset skips it. Unlike TraderArea, this has no gameplay side effects (block protection,
     * waypoints, closing hours), works for single-chunk POIs, and covers every chunk regardless of grouping mode.
     */
    public class NoRegionReset
    {
        private const string PropertyName = "SCoreNoRegionReset";
        private const ChunkProtectionLevel Protection = ChunkProtectionLevel.Trader;

        // Prefab names (file name without extension) whose XML has SCoreNoRegionReset=true.
        // Keyed by name rather than stored on the Prefab, because rotated prefab clones don't copy 'properties'.
        private static readonly HashSet<string> ProtectedPrefabNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly object NamesLock = new object();

        private static readonly List<PrefabInstance> PrefabBuffer = new List<PrefabInstance>();
        private static int _lastLoggedCount = -1;

        private static bool IsProtected(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName)) return false;
            lock (NamesLock)
            {
                return ProtectedPrefabNames.Contains(prefabName);
            }
        }

        // Record the property when a prefab's XML is read.
        [HarmonyPatch(typeof(Prefab))]
        [HarmonyPatch(nameof(Prefab.LoadXMLData))]
        public class PrefabLoadXMLData
        {
            public static void Postfix(Prefab __instance, PathAbstractions.AbstractedLocation _location)
            {
                var name = _location.FileNameNoExtension;
                if (string.IsNullOrEmpty(name)) return;

                var enabled = false;
                __instance.properties?.ParseBool(PropertyName, ref enabled);

                lock (NamesLock)
                {
                    if (enabled)
                        ProtectedPrefabNames.Add(name);
                    else
                        ProtectedPrefabNames.Remove(name);
                }
            }
        }

        // Add protection after vanilla has computed it. Callers already hold the chunksInSaveDir lock.
        [HarmonyPatch(typeof(RegionFileManager))]
        [HarmonyPatch(nameof(RegionFileManager.UpdateChunkProtectionLevels))]
        public class RegionFileManagerUpdateChunkProtectionLevels
        {
            public static void Postfix(RegionFileManager __instance, EnumResetUnprotectedChunksGroupingMode _groupingMode)
            {
                lock (NamesLock)
                {
                    if (ProtectedPrefabNames.Count == 0) return;
                }

                var decorator = GameManager.Instance?.World?.ChunkCache?.ChunkProvider?.GetDynamicPrefabDecorator();
                if (decorator == null) return;

                var protectedPois = 0;
                lock (PrefabBuffer)
                {
                    PrefabBuffer.Clear();
                    decorator.GetAllPrefabs(PrefabBuffer);

                    foreach (var prefabInstance in PrefabBuffer)
                    {
                        if (prefabInstance?.prefab == null) continue;
                        if (!IsProtected(prefabInstance.prefab.PrefabName)) continue;

                        protectedPois++;
                        foreach (var chunkKey in prefabInstance.GetOccupiedChunks())
                            Protect(__instance, chunkKey, _groupingMode);
                    }

                    PrefabBuffer.Clear();
                }

                if (protectedPois != _lastLoggedCount)
                {
                    _lastLoggedCount = protectedPois;
                    Log.Out($"[SCore] {PropertyName}: protecting {protectedPois} POI(s) from region reset.");
                }
            }

            private static void Protect(RegionFileManager rfm, long chunkKey, EnumResetUnprotectedChunksGroupingMode groupingMode)
            {
                AddLevel(rfm.chunkProtectionLevels, chunkKey);

                // In GroupedPOIs mode the game resets chunk groups as a unit and copies the group's protection to every
                // member, so protect the whole group to avoid partially resetting overlapping POIs.
                if (groupingMode != EnumResetUnprotectedChunksGroupingMode.GroupedPOIs) return;
                if (!rfm.chunkGroups.TryGetGroup(chunkKey, out var group)) return;

                rfm.groupProtectionLevels.TryGetValue(group, out var groupLevel);
                if ((groupLevel & Protection) != 0) return;

                rfm.groupProtectionLevels[group] = groupLevel | Protection;
                foreach (var key in group.Keys)
                    AddLevel(rfm.chunkProtectionLevels, key);
            }

            private static void AddLevel(Dictionary<long, ChunkProtectionLevel> levels, long chunkKey)
            {
                levels.TryGetValue(chunkKey, out var level);
                levels[chunkKey] = level | Protection;
            }
        }
    }
}
