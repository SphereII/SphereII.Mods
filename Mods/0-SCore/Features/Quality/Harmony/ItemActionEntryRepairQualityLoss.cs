using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Quality.Harmony
{
    /// <summary>
    /// Replaces vanilla's flat -1 quality-per-repair with a configurable loss.
    ///
    /// Flat levels (takes priority over percentage). Whole-number levels, stored as integer:
    ///   Player cvar  RepairQualityLossLevels  (highest priority — set via buffs/progression)
    ///   <property name="RepairQualityLossLevels" value="2"/>  <!-- per-item or global XML -->
    ///
    /// Percentage-based. Whole-number percent, cvar stored as fraction (0.10 = 10%):
    ///   Player cvar  RepairQualityLoss  (highest priority — set via buffs/progression)
    ///   <property name="RepairQualityLoss" value="10"/>  <!-- per-item or global XML -->
    ///   <property name="RepairQualityLoss" value="0"/>   <!-- no quality loss -->
    ///
    /// Both support per-item (items.xml) and global (AdvancedItemFeatures in blocks.xml) values.
    /// Per-item takes priority over global. If nothing is configured, vanilla -1 flat reduction is used.
    /// </summary>
    [HarmonyPatch(typeof(ItemActionEntryRepair))]
    [HarmonyPatch(nameof(ItemActionEntryRepair.OnActivated))]
    public class ItemActionEntryRepairQualityLoss
    {
        private static readonly string AdvFeatureClass   = "AdvancedItemFeatures";
        private static readonly string PercentProperty   = "RepairQualityLoss";
        private static readonly string FlatLevelsProperty = "RepairQualityLossLevels";
        private const           string MetaKey           = "SCoreRepairOrigQuality";

        // Queue tracks one pre-repair quality per queued repair, in FIFO order.
        // Used as fallback when ItemValue.Clone() does not copy metadata.
        private static readonly Queue<int> _pendingOriginalQualities = new Queue<int>();

        /// <summary>
        /// Returns flat quality levels to lose per repair, or -1 if not configured.
        /// Priority: player cvar → per-item XML → global XML.
        /// </summary>
        private static int GetLossLevels(ItemValue itemValue)
        {
            // Player cvar (highest priority). Stored as a whole integer: 2 = lose 2 levels. 0 = unset.
            var player = GameManager.Instance?.World?.GetPrimaryPlayer();
            if (player != null && player.Buffs.HasCustomVar(FlatLevelsProperty))
            {
                int cvarVal = (int)player.Buffs.GetCustomVar(FlatLevelsProperty);
                if (cvarVal != 0)
                    return Mathf.Max(0, cvarVal);
            }

            if (itemValue.ItemClass.Properties.Contains(FlatLevelsProperty))
            {
                var str = itemValue.ItemClass.Properties.GetStringValue(FlatLevelsProperty);
                if (int.TryParse(str, out int itemLevels))
                    return itemLevels;
            }

            var globalStr = Configuration.GetPropertyValue(AdvFeatureClass, FlatLevelsProperty);
            if (!string.IsNullOrEmpty(globalStr) && int.TryParse(globalStr, out int globalLevels))
                return globalLevels;

            return -1;
        }

        /// <summary>
        /// Returns the loss percent (0–100) to apply, or -1 if no source is configured (vanilla fallback).
        /// Priority: player cvar → per-item XML → global XML.
        /// </summary>
        private static float GetLossPercent(ItemValue itemValue)
        {
            // Player cvar (highest priority). Stored as a fraction: 0.05 = 5%. 0 = unset.
            var player = GameManager.Instance?.World?.GetPrimaryPlayer();
            if (player != null && player.Buffs.HasCustomVar(PercentProperty))
            {
                float cvarVal = player.Buffs.GetCustomVar(PercentProperty);
                if (cvarVal != 0f)
                    return Mathf.Max(0f, cvarVal * 100f);
            }

            if (itemValue.ItemClass.Properties.Contains(PercentProperty))
            {
                var str = itemValue.ItemClass.Properties.GetStringValue(PercentProperty);
                if (float.TryParse(str, out float itemLoss))
                    return itemLoss;
            }

            var globalStr = Configuration.GetPropertyValue(AdvFeatureClass, PercentProperty);
            if (!string.IsNullOrEmpty(globalStr) && float.TryParse(globalStr, out float globalLoss))
                return globalLoss;

            return -1f;
        }

        private static bool HasAnyLossConfigured(ItemValue itemValue)
        {
            return GetLossLevels(itemValue) >= 0 || GetLossPercent(itemValue) >= 0f;
        }

        public static void Prefix(ItemActionEntryRepair __instance)
        {
            var itemValue = ItemClassUtils.GetItemValue(__instance.ItemController);
            if (itemValue == null) return;
            if (!HasAnyLossConfigured(itemValue)) return;

            // Snapshot quality BEFORE vanilla's repair queue applies the -1 reduction.
            int originalQuality = (int)itemValue.Quality;
            _pendingOriginalQualities.Enqueue(originalQuality);

            // Also write to metadata so the value travels with the cloned ItemValue that
            // AddRepairItemToQueue stores internally (works if Clone copies metadata).
            itemValue.SetMetadata(MetaKey, originalQuality, TypedMetadataValue.TypeTag.Integer);

            // Allow multiple subscriptions — one per queued repair — so simultaneous repairs
            // each get their own OnRepairItem call. The old "-= then +=" pattern was a bug:
            // it ensured only one handler, so the second of two queued repairs never fired.
            QuestEventManager.Current.RepairItem += OnRepairItem;
        }

        private static void OnRepairItem(ItemValue itemValue)
        {
            QuestEventManager.Current.RepairItem -= OnRepairItem;

            // Resolve original quality: metadata is preferred (item-specific, survives Clone),
            // queue is the fallback in FIFO order matching the repair queue.
            int originalQuality = -1;
            var raw = itemValue.GetMetadata(MetaKey);
            if (raw is int metaQuality)
            {
                originalQuality = metaQuality;
                if (_pendingOriginalQualities.Count > 0)
                    _pendingOriginalQualities.Dequeue();
            }
            else if (_pendingOriginalQualities.Count > 0)
            {
                originalQuality = _pendingOriginalQualities.Dequeue();
            }

            if (originalQuality < 0) return;

            // Flat levels take priority over percentage.
            int lossLevels = GetLossLevels(itemValue);
            if (lossLevels >= 0)
            {
                int newQualityFlat = Mathf.Max(QualityUtils.GetMinQuality(), originalQuality - lossLevels);
                itemValue.Quality  = (ushort)newQualityFlat;
                return;
            }

            float lossPercent = GetLossPercent(itemValue);
            if (lossPercent < 0f) return;

            float lossFraction = lossPercent / 100f;
            int qualityLoss    = Mathf.CeilToInt(originalQuality * lossFraction);
            int newQuality     = Mathf.Max(QualityUtils.GetMinQuality(), originalQuality - qualityLoss);
            itemValue.Quality  = (ushort)newQuality;
        }
    }
}
