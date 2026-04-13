using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Quality.Harmony
{
    [HarmonyPatch(typeof(XUiC_WorkstationWindowGroup))]
    [HarmonyPatch("syncTEfromUI")]
    public class XUiCWorkstationWindowGroupSyncTEFromUI
    {
        private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
        private static readonly string Feature = "CustomQualityLevels";

        // Persists packed (quality << 16 | entityId) values across syncTEfromUI calls,
        // including the final teardown call that fires when the window closes.
        // Key: tile entity world position.  Value: slot index → packed int.
        // Shared with TileEntityWorkstationAddCraftComplete so it can clear entries on completion.
        internal static readonly Dictionary<Vector3i, Dictionary<int, int>> QualityCache
            = new Dictionary<Vector3i, Dictionary<int, int>>();

        public static void Postfix(XUiC_WorkstationWindowGroup __instance)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;

            var tileEntity = __instance.WorkstationData.TileEntity;
            var craftingQueue = __instance.craftingQueue;
            if (tileEntity == null || craftingQueue == null) return;

            var teQueue = tileEntity.Queue;
            if (teQueue == null) return;

            var tePos = tileEntity.ToWorldPos();
            bool isModified = false;

            // ── Phase 1: refresh cache from live UI ────────────────────────────────
            // Only runs when the UI still has valid recipe data (window is open and
            // the player has selected recipes).  This writes the user's chosen quality
            // into the cache keyed by tile entity position + queue slot.
            var recipesToCraft = craftingQueue.GetRecipesToCraft();
            if (recipesToCraft != null)
            {
                int count = Math.Min(recipesToCraft.Length, teQueue.Length);
                for (int i = 0; i < count; i++)
                {
                    var uiRecipe = recipesToCraft[i];
                    if (uiRecipe?.GetRecipe() == null) continue;

                    int realQuality   = uiRecipe.OutputQuality;
                    int playerEntityId = uiRecipe.StartingEntityId;

                    // Quality ≤ 255 fits in vanilla's byte field — no packing needed.
                    if (realQuality <= 255) continue;

                    int packedValue = ((realQuality & 0xFFFF) << 16) | (playerEntityId & 0xFFFF);

                    if (!QualityCache.TryGetValue(tePos, out var slotCache))
                        QualityCache[tePos] = slotCache = new Dictionary<int, int>();

                    slotCache[i] = packedValue;
                }
            }

            // ── Phase 2: re-assert cached values ──────────────────────────────────
            // Runs unconditionally, including during window teardown.  This overwrites
            // whatever the game's syncTEfromUI just wrote into StartingEntityId with
            // the preserved packed value, keeping the quality intact even after the
            // player closes the workstation window.
            if (!QualityCache.TryGetValue(tePos, out var cache)) return;

            var slotsToRemove = new List<int>();
            foreach (var entry in cache)
            {
                int slot        = entry.Key;
                int packedValue = entry.Value;
                if (slot >= teQueue.Length) { slotsToRemove.Add(slot); continue; }

                var teItem = teQueue[slot];
                if (teItem == null || teItem.Recipe == null || teItem.Multiplier <= 0)
                {
                    // Slot is empty — craft finished or was cancelled; drop the entry.
                    slotsToRemove.Add(slot);
                    continue;
                }

                if (teItem.StartingEntityId != packedValue)
                {
                    teItem.StartingEntityId = packedValue;
                    isModified = true;
                }
            }

            foreach (int slot in slotsToRemove)
                cache.Remove(slot);

            if (cache.Count == 0)
                QualityCache.Remove(tePos);

            if (isModified)
                tileEntity.setModified();
        }
    }
}
