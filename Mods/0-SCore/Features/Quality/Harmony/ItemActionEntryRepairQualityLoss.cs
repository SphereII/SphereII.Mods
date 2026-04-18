using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Quality.Harmony
{
    /// <summary>
    /// Replaces vanilla's flat -1 quality-per-repair with a configurable percentage loss.
    ///
    /// Global default (blocks.xml AdvancedItemFeatures):
    ///   <property name="RepairQualityLoss" value="10"/>   <!-- all items lose 10 % per repair -->
    ///
    /// Per-item override (items.xml / item property block):
    ///   <property name="RepairQualityLoss" value="5"/>    <!-- this item loses only 5 %       -->
    ///   <property name="RepairQualityLoss" value="0"/>    <!-- this item loses no quality      -->
    ///
    /// The per-item value takes priority over the global default.  If neither is set the
    /// vanilla -1 flat reduction is used unchanged.
    ///
    /// Values are whole-number percentages (0–100).  Loss is rounded up (Ceil) and the
    /// result is clamped to QualityUtils.GetMinQuality().
    /// </summary>
    [HarmonyPatch(typeof(ItemActionEntryRepair))]
    [HarmonyPatch(nameof(ItemActionEntryRepair.OnActivated))]
    public class ItemActionEntryRepairQualityLoss
    {
        private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
        private static readonly string GlobalProperty  = "RepairQualityLoss";
        private static readonly string ItemProperty    = "RepairQualityLoss";
        private const           string MetaKey         = "SCoreRepairOrigQuality";

        // Static fallback for when ItemValue.Clone() does not copy metadata.
        // Only valid between OnActivated and the matching RepairItem event fire.
        private static int _pendingOriginalQuality = -1;

        /// <summary>
        /// Returns the loss percent (0–100) to apply, or -1 if no source is configured (vanilla fallback).
        ///
        /// Priority (highest → lowest):
        ///   1. Player cvar  "RepairQualityLoss"  — stored as a fraction: 0.05 = 5%, 0.10 = 10%
        ///                                          0 = unset; negative (e.g. -1) = no loss
        ///   2. Per-item property  RepairQualityLoss  in items.xml  (whole-number %, e.g. "5")
        ///   3. Global property  RepairQualityLoss  in AdvancedItemFeatures (blocks.xml)  (same format)
        /// </summary>
        private static float GetLossPercent(ItemValue itemValue)
        {
            // 1. Player cvar (highest priority — buffs/quests/progression can override).
            //    Stored as a fraction (0.05 = 5%). 0 = unset. Negative = no loss, clamped to 0.
            var player = GameManager.Instance?.World?.GetPrimaryPlayer();
            if (player != null && player.Buffs.HasCustomVar("RepairQualityLoss"))
            {
                float cvarVal = player.Buffs.GetCustomVar("RepairQualityLoss");
                if (cvarVal != 0f)
                    return Mathf.Max(0f, cvarVal * 100f);
            }

            // 2. Per-item override.
            if (itemValue.ItemClass.Properties.Contains(ItemProperty))
            {
                var str = itemValue.ItemClass.Properties.GetStringValue(ItemProperty);
                if (float.TryParse(str, out float itemLoss))
                    return itemLoss;
            }

            // 3. Global default in AdvancedItemFeatures.
            var globalStr = Configuration.GetPropertyValue(AdvFeatureClass, GlobalProperty);
            if (!string.IsNullOrEmpty(globalStr) && float.TryParse(globalStr, out float globalLoss))
                return globalLoss;

            return -1f;
        }

        public static void Prefix(ItemActionEntryRepair __instance)
        {
            var itemValue = ItemClassUtils.GetItemValue(__instance.ItemController);
            if (itemValue == null) return;
            if (GetLossPercent(itemValue) < 0f) return;

            // Snapshot quality BEFORE vanilla's repair queue applies the -1 reduction.
            int originalQuality = (int)itemValue.Quality;
            _pendingOriginalQuality = originalQuality;

            // Also write to metadata so the value travels with the cloned ItemValue that
            // AddRepairItemToQueue stores internally (works if Clone copies metadata).
            itemValue.SetMetadata(MetaKey, originalQuality, TypedMetadataValue.TypeTag.Integer);

            // Ensure exactly one subscription so rapid back-to-back repairs don't stack.
            QuestEventManager.Current.RepairItem -= OnRepairItem;
            QuestEventManager.Current.RepairItem += OnRepairItem;
        }

        private static void OnRepairItem(ItemValue itemValue)
        {
            QuestEventManager.Current.RepairItem -= OnRepairItem;

            float lossPercent = GetLossPercent(itemValue);
            if (lossPercent < 0f) return;

            // Resolve original quality: metadata is preferred (survives Clone), static
            // field is the fallback for engines where Clone drops metadata.
            int originalQuality;
            var raw = itemValue.GetMetadata(MetaKey);
            if (raw is int metaQuality)
                originalQuality = metaQuality;
            else if (_pendingOriginalQuality >= 0)
                originalQuality = _pendingOriginalQuality;
            else
                return;

            _pendingOriginalQuality = -1;

            float lossFraction = lossPercent / 100f;
            int qualityLoss    = Mathf.CeilToInt(originalQuality * lossFraction);
            int newQuality     = Mathf.Max(QualityUtils.GetMinQuality(), originalQuality - qualityLoss);
            itemValue.Quality  = (ushort)newQuality;
        }
    }
}
