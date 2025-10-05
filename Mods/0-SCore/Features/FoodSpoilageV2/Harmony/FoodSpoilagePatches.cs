using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SphereII.FoodSpoilage.HarmonyPatches
{
    /// <summary>
    /// Harmony patches for implementing the Food Spoilage system.
    /// Includes patches for ItemValue cloning and the core spoilage logic in XUiC_ItemStack.Update.
    /// </summary>
    public class FoodSpoilagePatches
    {
        // --- ItemValue Clone Patch ---
        [HarmonyPatch(typeof(ItemValue))]
        [HarmonyPatch("Clone")]
        public class ItemValueClonePatch
        {
            private static void Postfix(ref ItemValue __result, ItemValue __instance)
            {
                // Check if the core feature and the alternate value usage are enabled
                if (!SpoilageConfig.IsFoodSpoilageEnabled || !SpoilageConfig.UseAlternateItemValue)
                    return;

                // Check if the item instance itself is spoilable
                if (__instance?.ItemClass == null ||
                    !__instance.ItemClass.Properties.GetBool(SpoilageConstants.PropSpoilable))
                    return;

                // If metadata exists, perform a deep clone
                if (__instance.Metadata != null)
                {
                    // Create a new dictionary for the clone to avoid reference sharing.
                    __result.Metadata = new Dictionary<string, TypedMetadataValue>();
                    foreach (var kvp in __instance.Metadata)
                    {
                        // Clone the metadata value itself before adding.
                        __result.SetMetadata(kvp.Key, kvp.Value.Clone());
                    }
                }
            }
        }

        // --- ItemStack Update Patch (Core Spoilage Logic) ---
        [HarmonyPatch(typeof(XUiC_ItemStack))]
        [HarmonyPatch("Update")]
        public class FoodSpoilageXUiCItemStackUpdatePatch
        {
            #region Helper Methods

            /// <summary>
            /// Checks if the ItemStack should be skipped for spoilage processing.
            /// </summary>
            private static bool ShouldSkipSpoilage(XUiC_ItemStack instance)
            {
                if (!SpoilageConfig.IsFoodSpoilageEnabled) return true;
                if (instance == null) return true;
                if (instance.StackLocation == XUiC_ItemStack.StackLocationTypes.Creative) return true; // Skip creative

                var itemStack = instance.ItemStack;
                if (itemStack == null || itemStack.IsEmpty()) return true; // Skip empty/null stacks

                // If locked in drag/drop, it might be temporary, skip for stability
                if (instance.IsLocked && instance.IsDragAndDrop) return true;

                var itemClass = itemStack.itemValue?.ItemClass;
                // Skip if item has no class or is not marked as Spoilable
                return itemClass == null || !itemClass.Properties.GetBool(SpoilageConstants.PropSpoilable);
            }

            /// <summary>
            /// Gets spoilage-related properties from the ItemClass.
            /// </summary>
            private static void GetSpoilageProperties(ItemClass itemClass, out float spoilageMax,
                out float spoilagePerTick, out int tickPerLoss, out string spoiledItemName, out bool isFreshnessOnly,
                out bool useStackSpoil)
            {
                spoilageMax = itemClass.Properties.GetFloat(SpoilageConstants.PropSpoilageMax);
                spoilagePerTick = itemClass.Properties.GetFloat(SpoilageConstants.PropSpoilagePerTick);
                isFreshnessOnly = itemClass.Properties.GetBool(SpoilageConstants.PropFreshnessOnly);
                useStackSpoil = SpoilageConfig.IsGlobalFullStackSpoilEnabled ||
                                itemClass.Properties.GetBool(SpoilageConstants.PropFullStackSpoil);

                // Use item-specific TickPerLoss if available, otherwise global
                if (itemClass.Properties.Contains(SpoilageConstants.PropTickPerLoss))
                    tickPerLoss = itemClass.Properties.GetInt(SpoilageConstants.PropTickPerLoss);
                else
                    tickPerLoss = SpoilageConfig.GlobalTickPerLoss;

                // Use item-specific SpoiledItem if available, otherwise global
                if (itemClass.Properties.Contains(SpoilageConstants.PropSpoiledItem))
                    spoiledItemName = itemClass.Properties.GetString(SpoilageConstants.PropSpoiledItem);
                else
                    spoiledItemName = SpoilageConfig.GlobalSpoiledItem;

                // Use default if config or property is empty/invalid
                if (string.IsNullOrEmpty(spoiledItemName))
                    spoiledItemName = SpoilageConstants.DefaultSpoiledItem;

                // Basic validation / defaults
                if (spoilageMax <= 0) spoilageMax = 1000f; // Default max if not set or invalid
                if (spoilagePerTick <= 0) spoilagePerTick = 1f; // Default per tick if not set or invalid
                if (tickPerLoss <= 0) tickPerLoss = 500; // Default tick loss if not set or invalid
            }


            /// <summary>
            /// Calculates the spoilage modifier based on the item stack's location.
            /// </summary>
            private static float CalculateContainerModifier(XUiC_ItemStack instance, out string storageTypeDebug)
            {
                float modifier = 0f;
                storageTypeDebug = "Unknown";

                switch (instance.StackLocation)
                {
                    case XUiC_ItemStack.StackLocationTypes.ToolBelt:
                        modifier = SpoilageConfig.ToolbeltModifier;
                        storageTypeDebug = $"Toolbelt ({modifier})";
                        break;
                    case XUiC_ItemStack.StackLocationTypes.Backpack:
                        modifier = SpoilageConfig.BackpackModifier;
                        storageTypeDebug = $"Backpack ({modifier})";
                        break;
                    case XUiC_ItemStack.StackLocationTypes.LootContainer:
                        modifier = SpoilageConfig.ContainerModifier; // Base container modifier
                        storageTypeDebug = $"Container ({modifier})";
                        var container = instance.xui?.lootContainer;
                        if (container != null)
                        {
                            var blockValue = GameManager.Instance?.World?.GetBlock(container.ToWorldPos());
                            if (blockValue != null && blockValue.Value.Block != null)
                            {
                                var block = blockValue.Value.Block;
                                storageTypeDebug += $" [{Localization.Get(block.GetBlockName())}]";
                                // Apply preservation bonus if present
                                if (block.Properties.Contains(SpoilageConstants.PropPreserveBonus))
                                {
                                    float preserveBonus =
                                        block.Properties.GetFloat(SpoilageConstants.PropPreserveBonus);
                                    modifier -= preserveBonus; // Subtract bonus (negative modifier means less spoilage)
                                    storageTypeDebug += $" PreserveBonus (-{preserveBonus})";

                                    // Check for total preservation (-99 indicates no spoilage)
                                    if (preserveBonus == -99f)
                                    {
                                        modifier = -9999f; // Special value to indicate full preservation
                                        storageTypeDebug += " [Total Preservation]";
                                    }
                                }
                            }
                        }

                        break;
                    // Handle other cases explicitly or fall through to default
                    case XUiC_ItemStack.StackLocationTypes.Vehicle:
                    case XUiC_ItemStack.StackLocationTypes.Workstation:
                    case XUiC_ItemStack.StackLocationTypes.DewCollector:
                    case XUiC_ItemStack.StackLocationTypes.Equipment: // Equipment might warrant its own modifier?
                    case XUiC_ItemStack.StackLocationTypes.Merge: // Merge shouldn't really hold items long term?
                    default: // Generic Container as default
                        modifier = SpoilageConfig.ContainerModifier;
                        storageTypeDebug = $"Generic/Other ({modifier})";
                        break;
                    case XUiC_ItemStack.StackLocationTypes.Creative: // Already handled in ShouldSkip, but safe fallback
                        return -9999f; // Indicate no spoilage
                }

                return modifier;
            }

            /// <summary>
            /// Calculates the effective spoilage per tick, considering base rate, modifiers, and minimums.
            /// </summary>
            private static float CalculateEffectiveSpoilage(float baseSpoilagePerTick, float containerModifier)
            {
                // Check for total preservation signal
                if (containerModifier <= -9999f) return 0f;

                float effectiveSpoilage = baseSpoilagePerTick + containerModifier;

                // Ensure spoilage doesn't go below the configured minimum, unless preserved
                if (effectiveSpoilage < SpoilageConfig.MinimumSpoilage &&
                    containerModifier > -9000f) // Avoid overriding total preservation
                {
                    effectiveSpoilage = SpoilageConfig.MinimumSpoilage;
                }

                // Ensure spoilage doesn't become negative (unless preserved)
                if (effectiveSpoilage < 0 && containerModifier > -9000f)
                {
                    effectiveSpoilage = 0;
                }

                return effectiveSpoilage;
            }

            /// <summary>
            /// Calculates how many spoilage ticks have been missed since the last check.
            /// </summary>
            private static int CalculateMissedSpoilageMultiplier(int nextTickDue, ulong currentWorldTime,
                int tickPerLoss)
            {
                if (tickPerLoss <= 0) return 1; // Avoid division by zero

                // Convert to consistent types (long should be sufficient)
                long currentTimeInt = ToSafeInt(currentWorldTime);
                long nextTickDueInt = nextTickDue; // Already int

                if (currentTimeInt < nextTickDueInt) return 0; // Not due yet

                long ticksPassed = currentTimeInt - nextTickDueInt;
                int multiplier = (int)(ticksPassed / tickPerLoss) + 1; // +1 because the current tick also counts

                return Math.Max(1, multiplier); // Must apply at least one tick's worth
            }

            /// <summary>
            /// Processes the item stack, reducing count or converting to spoiled item if necessary.
            /// Returns the remaining spoilage amount after processing.
            /// </summary>
            private static float ProcessSpoiledItemStack(ref XUiC_ItemStack instance, float currentSpoilage,
                float spoilageMax, string spoiledItemName, bool useStackSpoil)
            {
                if (spoilageMax <= 0) return currentSpoilage; // Avoid division by zero or infinite loops

                while (currentSpoilage >= spoilageMax)
                {
                    currentSpoilage -= spoilageMax; // Reduce spoilage by one item's worth

                    // Get the spoiled item only if it's not "None"
                    ItemValue spoiledItemValue = null;
                    if (!spoiledItemName.Equals(SpoilageConstants.NoneString, StringComparison.OrdinalIgnoreCase))
                    {
                        spoiledItemValue = ItemClass.GetItem(spoiledItemName, false);
                    }

                    int countToRemove = 1;
                    if (useStackSpoil)
                    {
                        countToRemove = instance.ItemStack.count; // Spoil the whole stack at once
                        AdvLogging.DisplayLog(SpoilageConstants.AdvFeatureClass,
                            $"{instance.ItemStack.itemValue.ItemClass.GetItemName()}: Full stack ({countToRemove}) spoiling to {spoiledItemName}");
                    }

                    // Handle adding the spoiled item (if one exists)
                    if (spoiledItemValue != null && !spoiledItemValue.IsEmpty())
                    {
                        int countToAdd = countToRemove; // Add same amount as removed/spoiled
                        ItemStack spoiledItemStack = new ItemStack(spoiledItemValue, countToAdd);

                        // Attempt to add to player inventory or drop
                        var player = instance.xui?.playerUI?.entityPlayer;
                        if (player != null && !instance.xui.PlayerInventory.AddItem(spoiledItemStack, true))
                        {
                            player.world.gameManager.ItemDropServer(spoiledItemStack, player.GetPosition(),
                                Vector3.zero, -1, 60f, false);
                        }
                    }

                    // Remove the spoiled item(s) from the stack
                    if (instance.ItemStack.count > countToRemove)
                    {
                        AdvLogging.DisplayLog(SpoilageConstants.AdvFeatureClass,
                            $"{instance.ItemStack.itemValue.ItemClass.GetItemName()}: Reducing stack by {countToRemove}. Remaining: {instance.ItemStack.count - countToRemove}");
                        instance.ItemStack.count -= countToRemove;
                    }
                    else
                    {
                        AdvLogging.DisplayLog(SpoilageConstants.AdvFeatureClass,
                            $"{instance.ItemStack.itemValue.ItemClass.GetItemName()}: Stack depleted. Removing.");
                        instance.ItemStack = ItemStack.Empty.Clone(); // Set to empty stack
                        currentSpoilage = 0; // No more spoilage left if stack is gone
                        break; // Exit loop, stack is gone
                    }
                }


                return Math.Max(0, currentSpoilage); // Ensure spoilage doesn't go negative
            }

            /// <summary>
            /// Updates the Freshness metadata and visual indicators (Durability bar).
            /// </summary>
            private static void UpdateDisplayMetadata(XUiC_ItemStack instance, float currentSpoilage, float spoilageMax)
            {
                if (spoilageMax <= 0) spoilageMax = 1f; // Prevent division by zero

                float percentFresh = 1f - Mathf.Clamp01(currentSpoilage / spoilageMax);
                instance.ItemStack.itemValue.SetMetadata(SpoilageConstants.MetaFreshness, percentFresh,
                    TypedMetadataValue.TypeTag.Float);

                // --- Update Durability Bar Visuals ---
                var isFreshnessOnly = instance.itemClass.Properties.GetBool(SpoilageConstants.PropFreshnessOnly);
                bool isFresh = IsFreshEnough(instance.ItemStack.itemValue); // Check if freshness > threshold

                var maxQuality = QualityUtils.GetMaxQuality();
                // Get Quality Tier Color based on freshness percentage (0-7)
                int tierColor = maxQuality - (int)Math.Round(maxQuality * (currentSpoilage /
                                                       spoilageMax)); // Inverted: 0% fresh = tier 0, 100% fresh = tier 7
                tierColor = Mathf.Clamp(tierColor, 0, maxQuality); // Clamp to valid range

                // Allow item property to override calculated color
                var itemClass = instance.ItemStack.itemValue?.ItemClass;
                if (itemClass != null && itemClass.Properties.Contains(SpoilageConstants.PropQualityTierColor))
                {
                    tierColor = itemClass.Properties.GetInt(SpoilageConstants.PropQualityTierColor);
                    tierColor = Mathf.Clamp(tierColor, 0, maxQuality); // Clamp override too
                }

                // Update Durability Sprite
                var durabilityController = instance.GetChildById("durability");
                if (durabilityController?.ViewComponent is XUiV_Sprite durabilitySprite)
                {
                    durabilitySprite.IsVisible = !isFreshnessOnly || isFresh; 
                    durabilitySprite.Color = QualityInfo.GetQualityColor(tierColor);
                    durabilitySprite.Fill = percentFresh;
                }

                // Update Durability Background Sprite
                var backgroundController = instance.GetChildById("durabilityBackground");
                if (backgroundController?.ViewComponent is XUiV_Sprite backgroundSprite)
                {
                    backgroundSprite.IsVisible = !isFreshnessOnly || isFresh; 
                }
            }

            /// <summary>
            /// Checks if the item's freshness is above a minimum threshold (e.g., 0.1f).
            /// </summary>
            public static bool IsFreshEnough(ItemValue itemValue)
            {
                if (itemValue == null || !itemValue.HasMetadata(SpoilageConstants.MetaFreshness))
                    return true; // Assume fresh if no data
                if (itemValue.GetMetadata(SpoilageConstants.MetaFreshness) is float freshness)
                {
                    // Consider fresh if > 10% (adjust threshold as needed)
                    return freshness > 0.1f;
                }

                return true; // Default to fresh if metadata is wrong type
            }

            /// <summary>
            /// Calculates the next world time tick when spoilage should be checked.
            /// </summary>
            private static int CalculateNextSpoilageTick(ulong currentWorldTime, int ticksPerLoss)
            {
                if (ticksPerLoss <= 0) ticksPerLoss = 1; // Prevent adding zero or negative
                ulong nextTickActual = currentWorldTime + (ulong)Math.Abs(ticksPerLoss);
                return ToSafeInt(nextTickActual);
            }

            /// <summary>
            /// Gets the next spoilage tick stored in item metadata. Returns -1 if not found.
            /// </summary>
            private static int GetNextSpoilageTick(ItemValue itemValue)
            {
                if (itemValue != null && itemValue.GetMetadata(SpoilageConstants.MetaNextSpoilageTick) is int nextTick)
                {
                    return nextTick;
                }

                return -1;
            }

            /// <summary>
            /// Sets the next spoilage tick in item metadata.
            /// </summary>
            private static void SetNextSpoilageTick(ItemValue itemValue, int nextTick)
            {
                itemValue?.SetMetadata(SpoilageConstants.MetaNextSpoilageTick, nextTick,
                    TypedMetadataValue.TypeTag.Integer);
            }

            /// <summary>
            /// Gets the current spoilage amount stored in item metadata. Returns 0f if not found (starts fresh).
            /// </summary>
            private static float GetCurrentSpoilage(ItemValue itemValue)
            {
                if (itemValue != null &&
                    itemValue.GetMetadata(SpoilageConstants.MetaSpoilageAmount) is float spoilageAmount)
                {
                    return spoilageAmount;
                }

                // Initialize spoilage if not present
                itemValue?.SetMetadata(SpoilageConstants.MetaSpoilageAmount, 0f, TypedMetadataValue.TypeTag.Float);
                return 0f;
            }

            /// <summary>
            /// Sets the current spoilage amount in item metadata.
            /// </summary>
            private static void SetCurrentSpoilage(ItemValue itemValue, float spoilageAmount)
            {
                itemValue?.SetMetadata(SpoilageConstants.MetaSpoilageAmount, Mathf.Max(0, spoilageAmount),
                    TypedMetadataValue.TypeTag.Float);
            }


            /// <summary>
            /// Converts an unsigned long to a signed int safely by discarding high-order bits.
            /// This avoids OverflowException from Convert.ToInt32 or casting when the ulong exceeds int.MaxValue.
            /// Used for world time ticks where absolute precision beyond int range might not be critical
            /// for this mod's purpose, but wrapping could occur if game runs for extremely long times.
            /// </summary>
            private static int ToSafeInt(ulong uLong)
            {
                // Using unchecked context allows overflow without exception, effectively taking lower 32 bits.
                unchecked
                {
                    return (int)uLong;
                }
                // Alternate: Bitwise AND preserves lower bits up to int.MaxValue
                // return (int)(uLong & int.MaxValue);
            }

            #endregion

            // --- Main Prefix Logic ---
            public static bool Prefix(XUiC_ItemStack __instance)
            {
                try // Add a try-catch block for safety during frequent updates
                {
                    // 1. Initial Checks & Skip Conditions
                    if (ShouldSkipSpoilage(__instance)) return true; // Continue original method if skipped

                    var itemClass = __instance.ItemStack.itemValue.ItemClass;

                    // 2. Get Spoilage Parameters
                    GetSpoilageProperties(itemClass, out float spoilageMax, out float baseSpoilagePerTick,
                        out int tickPerLoss, out string spoiledItemName, out bool isFreshnessOnly,
                        out bool useStackSpoil);

                    // 3. Check Spoilage Timing
                    ulong currentWorldTime = GameManager.Instance.World.GetWorldTime();

                    int nextTickDue = GetNextSpoilageTick(__instance.ItemStack.itemValue);
                    if (nextTickDue <= 0) // Initialize if first time
                    {
                        nextTickDue = CalculateNextSpoilageTick(currentWorldTime, tickPerLoss);
                        SetNextSpoilageTick(__instance.ItemStack.itemValue, nextTickDue);
                    }

                    // If not yet time to spoil, just update display and exit prefix (run Postfix potentially)
                    if (ToSafeInt(currentWorldTime) < nextTickDue)
                    {
                        return true; // Allow original update to run if needed
                    }

                    // 4. Calculate Spoilage to Apply
                    string storageTypeDebug; // For logging
                    float containerModifier = CalculateContainerModifier(__instance, out storageTypeDebug);

                    // Check for total preservation (-9999f signal)
                    if (containerModifier <= -9999f)
                    {
                        // Reset next tick to effectively pause spoilage while preserved
                        SetNextSpoilageTick(__instance.ItemStack.itemValue, CalculateNextSpoilageTick(currentWorldTime, tickPerLoss));
                       // UpdateDisplayMetadata(__instance, GetCurrentSpoilage(__instance.ItemStack.itemValue), spoilageMax); // Update display
                        return true; // Skip applying spoilage
                    }

                    float effectiveSpoilagePerTick = CalculateEffectiveSpoilage(baseSpoilagePerTick, containerModifier);
                    int missedTicksMultiplier =
                        CalculateMissedSpoilageMultiplier(nextTickDue, currentWorldTime, tickPerLoss);
                    float totalSpoilageToAdd = effectiveSpoilagePerTick * missedTicksMultiplier;

                    // 5. Apply Spoilage & Update State
                    float currentSpoilage = GetCurrentSpoilage(__instance.ItemStack.itemValue);
                    currentSpoilage += totalSpoilageToAdd;

                    // 6. Process Spoilage (Item Conversion / Removal)
                    // Only process conversion if not 'FreshnessOnly'
                    if (!isFreshnessOnly)
                    {
                        currentSpoilage = ProcessSpoiledItemStack(ref __instance, currentSpoilage, spoilageMax,
                            spoiledItemName, useStackSpoil);
                    }
                    // If the stack became empty, ProcessSpoiledItemStack returns 0 and clears the instance stack.
                    if (__instance.ItemStack.IsEmpty())
                    {
                        return true;
                    }
                    // 7. Update Metadata (Spoilage amount and next tick)
                    SetCurrentSpoilage(__instance.ItemStack.itemValue, currentSpoilage);
                    int nextSpoilageTickScheduled = CalculateNextSpoilageTick(currentWorldTime, tickPerLoss);
                    SetNextSpoilageTick(__instance.ItemStack.itemValue, nextSpoilageTickScheduled);

                    // Force UI refresh to show changes
                     __instance.ForceRefreshItemStack();

                    // --- Logging ---
                    if (AdvLogging.LogEnabled(SpoilageConstants.AdvFeatureClass)) // Check if logging is enabled
                    {
                        string logMsg =
                            $"FoodSpoilage Tick: [{itemClass.GetItemName()}] Count: {__instance.ItemStack.count} Loc: {storageTypeDebug} Mod: {containerModifier} EffRate: {effectiveSpoilagePerTick} MissedTicks: {missedTicksMultiplier} SpoilToAdd: {totalSpoilageToAdd} NewTotal: {currentSpoilage}/{spoilageMax} NextTick: {nextSpoilageTickScheduled}";
                        AdvLogging.DisplayLog(SpoilageConstants.AdvFeatureClass, logMsg);
                    }
                }
                catch (Exception ex)
                {
                    // Log exception to prevent UI lockups if something goes wrong
                    Log.Error(
                        $"Exception in FoodSpoilageXUiCItemStackUpdatePatch.Prefix: {ex.Message}\n{ex.StackTrace}");
                }

                // Returning true allows the original Update method to run IF needed,
                // but most spoilage UI updates are handled via ForceRefreshItemStack in UpdateDisplayMetadata.
                // If issues arise with other Update behaviors, this might need to be `false`. Test carefully.
                return true;
            }

            // Postfix might still be useful for ensuring the display is correct *after*
            // the original Update runs, though the Prefix now handles most visual updates.
            // Kept minimal for now.
            public static void Postfix(XUiC_ItemStack __instance)
            {
                // Minimal check: Ensure display reflects current state after all updates.
                if (ShouldSkipSpoilage(__instance)) return;

                try
                {
                    var itemValue = __instance.ItemStack?.itemValue;
                    if (itemValue == null || itemValue.IsEmpty()) return;

                    var itemClass = itemValue.ItemClass;
                    if (itemClass == null) return;


                    float spoilageMax = itemClass.Properties.GetFloat(SpoilageConstants.PropSpoilageMax);
                    if (spoilageMax <= 0) spoilageMax = 1000f;
                    float currentSpoilage = GetCurrentSpoilage(itemValue);
                    UpdateDisplayMetadata(__instance, currentSpoilage, spoilageMax);
                }
                catch (Exception ex)
                {
                    Log.Error(
                        $"Exception in FoodSpoilageXUiCItemStackUpdatePatch.Postfix: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }
    }
}