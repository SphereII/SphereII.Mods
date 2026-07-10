using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Quality.Harmony
{
    public class QualityDebugSuite
    {
        private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
        private static readonly string Feature = "CustomQualityLevels";

        // STEP 1: UI Input
        [HarmonyPatch(typeof(XUiC_CraftingQueue))]
        [HarmonyPatch(nameof(XUiC_CraftingQueue.AddRecipeToCraftAtIndex))]
        public class DebugStep1_UI
        {
            public static void Prefix(int lastQuality, global::Recipe _recipe)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;
                string name = _recipe != null ? _recipe.GetName() : "null";
                Log.Out($"[QualityDebug] Step 1 (UI Input): Recipe '{name}' queued with lastQuality: {lastQuality}");
            }
        }

        // STEP 2: Packing Verification (After Sync)
        [HarmonyPatch(typeof(XUiC_WorkstationWindowGroup))]
        [HarmonyPatch("syncTEfromUI")]
        public class DebugStep2_Sync
        {
            public static void Postfix(XUiC_WorkstationWindowGroup __instance)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;

                var te = __instance.WorkstationData.TileEntity;
                if (te == null || te.Queue == null) return;

                for (int i = 0; i < te.Queue.Length; i++)
                {
                    var item = te.Queue[i];
                    if (item != null && item.Recipe != null && item.Multiplier > 0)
                    {
                        // Check if it is packed (EntityID > 65535 usually means packed)
                        bool isPacked = (item.StartingEntityId >> 16) != 0;
                        int unpackedQuality = (item.StartingEntityId >> 16) & 0xFFFF;
                        int unpackedID = item.StartingEntityId & 0xFFFF;

                        Log.Out($"[QualityDebug] Step 2 (Sync Complete): Queue[{i}] - " +
                                $"StoredQuality(Byte): {item.Quality} | " +
                                $"StartingEntityId: {item.StartingEntityId} | " +
                                $"IsPacked: {isPacked} | " +
                                $"UnpackedQual: {unpackedQuality} | " +
                                $"UnpackedID: {unpackedID}");
                    }
                }
            }
        }

        // STEP 3: Pre-Craft Verification (Is data available before consumption?)
        [HarmonyPatch(typeof(TileEntityWorkstation))]
        [HarmonyPatch("HandleRecipeQueue")]
        public class DebugStep3_PreCraft
        {
            public static void Prefix(TileEntityWorkstation __instance)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;

                if (__instance.Queue == null) return;

                // Only log if something is actually crafting to avoid spam
                var item = __instance.Queue.LastOrDefault(); // Workstations process from the end of the array typically, or index 0 depending on logic.
                // Based on source: recipeQueueItem = this.queue[this.queue.Length - 1];

                // Let's check the active one (last one usually in vanilla code provided)
                if (__instance.Queue.Length > 0)
                {
                    var activeItem = __instance.Queue[__instance.Queue.Length - 1];

                    // We only care if it's actively crafting and about to finish
                    if (activeItem != null && activeItem.IsCrafting && activeItem.CraftingTimeLeft < 0.2f && activeItem.Multiplier > 0)
                    {
                        int packedQuality = (activeItem.StartingEntityId >> 16) & 0xFFFF;
                        Log.Out($"[QualityDebug] Step 3 (Crafting Tick): About to finish '{activeItem.Recipe.GetName()}'. " +
                                $"StoredEntityID: {activeItem.StartingEntityId}. " +
                                $"Potential Unpacked Quality: {packedQuality}. " +
                                $"Byte Quality: {activeItem.Quality}");
                    }
                }
            }
        }

        // STEP 4: Final Output Verification
        [HarmonyPatch(typeof(TileEntityWorkstation))]
        [HarmonyPatch(nameof(TileEntityWorkstation.AddCraftComplete))]
        public class DebugStep4_Result
        {
            public static void Prefix(ItemValue itemCrafted)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;

                if (itemCrafted != null)
                {
                    Log.Out($"[QualityDebug] Step 4 (Result): AddCraftComplete received Item with Quality: {itemCrafted.Quality}");
                }
            }
        }
    }
}
