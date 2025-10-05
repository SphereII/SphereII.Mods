using System;
using System.Collections.Generic;
using Audio;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.ItemDegradation.Utils
{
    public class ItemDegradationHelpers
    {
        public const string AdvFeatureClass = "ItemDegradation";
        
        public const string DegradationPerUse = "DegradationPerUse";
        public const string DegradationMaxUse = "DegradationMaxUse";

        public static int GetDurabilityForQuality(int quality, int minQuality, int maxQuality, int minDurability, int maxDurability)
        {
            // Clamp the quality input
            quality = Math.Max(minQuality, Math.Min(maxQuality, quality));

            float qualityStep = (float)(quality - minQuality) / (maxQuality - minQuality);
            float durabilityRange = maxDurability - minDurability;

            return (int)Math.Round(minDurability + (qualityStep * durabilityRange));
        }

        private static int GetValue(ItemValue mod, string property)
        {
            // 1. Check dynamic metadata first for override values.
            if (mod.GetMetadata(property) is int value)
            {
                return value;
            }

            if (mod.ItemClass?.Properties == null) return -1;
            
            // 2. Fall back to static item class properties.
            if (!mod.ItemClass.Properties.Contains(property)) return -1;

            var propValue = mod.ItemClass.Properties.GetString(property);
            if (string.IsNullOrEmpty(propValue)) return -1;

            // If it's a single integer, return it directly.
            if (!propValue.Contains(',')) return StringParsers.ParseSInt32(propValue);

            // If it's a comma-separated range, perform the quality check.
            var qualityRangeString = mod.ItemClass.Properties.Params1[property];
            if (string.IsNullOrEmpty(qualityRangeString)) return -1;

            var qualityRange = StringParsers.ParseVector2i(qualityRangeString);
            var durabilityRange = StringParsers.ParseVector2i(propValue);

            var maxDurability = GetDurabilityForQuality(mod.Quality, qualityRange.x, qualityRange.y, durabilityRange.x, durabilityRange.y);
            mod.SetMetadata(property, maxDurability, TypedMetadataValue.TypeTag.Integer);
            return maxDurability;

            // Return a default value if the property is not found anywhere.
        }

        public static int GetDegradationPerUse(ItemValue mod)
        {
            var value = GetValue(mod, DegradationPerUse);
            return value >= 0 ? value : 0;
        }

        public static int GetMaxUseTimes(ItemValue mod)
        {
            var value = GetValue(mod, DegradationMaxUse);
            return value >= 0 ? value : 0;
        }

        public static float GetPercentUsed(ItemValue mod)
        {
            var maxUse = GetMaxUseTimes(mod);
            return mod.UseTimes / maxUse;
        }

        public static bool CanDegrade(ItemValue mod)
        {
            if (mod == null) return false;
            if (mod.IsEmpty()) return false;
            if (!mod.ItemClass.ShowQualityBar) return false;
            //if (!mod.HasQuality) return false;
            if (GetMaxUseTimes(mod) <= 1) return false;
            return true;
        }

        public static bool CanDegrade(BlockValue blockValue)
        {
           return blockValue.Block.Properties.Contains(DegradationPerUse);
        }
        public static bool IsDegraded(ItemValue mod)
        {
            if (!CanDegrade(mod)) return false;
            return mod.UseTimes >= GetMaxUseTimes(mod);
        }

        public static void CheckBlockForDegradation(BlockValue blockValue, Vector3i position, float ticks)
        {
            //    <property name="DegradationPerUse" value="1"/>
            if (!blockValue.Block.Properties.Contains(DegradationPerUse)) return;
            var perUse = blockValue.Block.Properties.GetInt(DegradationPerUse);
            if (perUse < 0)
                perUse = 1;

            var minEffect = BlockEffectsManager.GetBlockEffect(blockValue.Block.GetBlockName());
            if (minEffect == null)
            {
             //   blockValue.Block.DamageBlock(GameManager.Instance.World, 0, position, blockValue, perUse * (int)ticks, -1);
                return;
            }
            
            var minEventParams = new MinEventParams {
                BlockValue = blockValue,
                Self = GameManager.Instance.World.GetPrimaryPlayer(),
                Position =  position,
                StartPosition = new Vector3(ticks,perUse)
            };
            minEffect.FireEvent((MinEventTypes)SCoreMinEventTypes.onSelfItemDegrade, minEventParams);
         }

        public static void CheckModification(ItemValue mod, EntityAlive player, int degradeOveride = 0)
        {
            if (!CanDegrade(mod)) return;
            if (IsDegraded(mod))
            {
                DeactivateItem(mod, player);
                if (mod.ItemClass.MaxUseTimesBreaksAfter.Value)
                {
                    if ( player != null)
                        Manager.BroadcastPlay(player, "itembreak");
                    mod = ItemValue.None;
                }

                return;
            }
            
            mod.UseTimes += GetDegradationPerUse(mod);
            // Allow an over-ride
            mod.UseTimes += degradeOveride;

            // Do another IsDegraded check to see if we need to trigger any events.
            if (!IsDegraded(mod)) return;
            
            // Make sure it doesn't get silly and reset it to the max use time.
            mod.UseTimes = GetMaxUseTimes(mod);
            
            DeactivateItem(mod, player);

      
        }

        private static void DeactivateItem(ItemValue mod, EntityAlive player)
        {
            if (mod.ItemClass.HasTrigger(MinEventTypes.onSelfItemActivate) && mod.Activated != 0)
            {
                player.MinEventContext.ItemValue = mod;
                mod.FireEvent(MinEventTypes.onSelfItemDeactivate, player != null ? player.MinEventContext : null);
                mod.Activated = 0;
            }
        }

        public static void CheckModificationOnItem(ItemValue[] items, EntityAlive player)
        {
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i]?.ItemClass == null) continue;
                //CheckModification(items[i], player);
                CheckModsForDegradation(items[i], player);
             
            }
        }
        
        public static void CheckModsForDegradation(ItemValue mod, EntityAlive player)
        {
                OnSelfItemDegrade.CheckForDegradation(mod, player);
        }

        public static void CheckToolsForDegradation(TileEntityWorkstation instance, global::Recipe recipe)
        {
            if (instance.bUserAccessing || instance.queue.Length == 0 || (instance.isModuleUsed[3] && !instance.isBurning)) return;
            if (instance.Tools == null) return;
            ItemStack[] slots = instance.Tools;
            for (var i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsEmpty()) continue;
                var itemValue = slots[i].itemValue;
                if (itemValue.type != recipe.craftingToolType) continue;
                if (!CanDegrade(itemValue)) continue;
                if (IsDegraded(slots[i].itemValue))
                {
                    instance.IsBurning = false;
                    instance.ResetTickTime();
                    return;
                }
                OnSelfItemDegrade.CheckForDegradation(slots[i]);
            }
        }

        public static List<ItemValue> FindAllItemValues(EntityAlive entityAlive, string itemName, string tagsString)
        {
            List<ItemValue> itemValues = new List<ItemValue>();

            // Early exit if both criteria are empty, no search needed.
            if (string.IsNullOrEmpty(itemName) && string.IsNullOrEmpty(tagsString))
            {
                return itemValues;
            }

            // Parse tags once if provided.
            FastTags<TagGroup.Global> tags = FastTags<TagGroup.Global>.none;
            if (!string.IsNullOrEmpty(tagsString))
            {
                tags = FastTags<TagGroup.Global>.Parse(tagsString);
            }

            // Find items in bag
            FindItemValues(entityAlive.bag.GetSlots(), itemName, tags, itemValues);
            // Find items in inventory
            FindItemValues(entityAlive.inventory.GetSlots(), itemName, tags, itemValues);
            // Find items in equipment
            FindItemValues(entityAlive.equipment.m_slots, itemName, tags, itemValues);

            return itemValues;
        }
      
        public static void FindItemValues(ItemValue[] rawItemValues, string itemName, FastTags<TagGroup.Global> tags, List<ItemValue> resultsList)
        {
            if (rawItemValues == null || rawItemValues.Length == 0)
            {
                return;
            }

            foreach (var itemValue in rawItemValues)
            {
                // Skip if the itemValue itself is null or empty
                if (itemValue == null || itemValue.IsEmpty())
                {
                    continue;
                }

                // Check if the itemValue matches the criteria
                if (IsItemMatch(itemValue, itemName, tags))
                {
                    resultsList.Add(itemValue);
                }
            }
        }
        
        public static void FindItemValues(ItemStack[] itemStacks, string itemName, FastTags<TagGroup.Global> tags, List<ItemValue> resultsList)
        {
            if (itemStacks == null || itemStacks.Length == 0)
            {
                return;
            }

            foreach (var itemStack in itemStacks)
            {
                // Skip if the stack itself is null, empty, or its contained itemValue is null
                if (itemStack == null || itemStack.IsEmpty() || itemStack.itemValue == null)
                {
                    continue;
                }

                // Check if the itemValue matches the criteria
                if (IsItemMatch(itemStack.itemValue, itemName, tags))
                {
                    resultsList.Add(itemStack.itemValue);
                }
            }
        }

        private static bool IsItemMatch(ItemValue itemValue, string itemNameCsv, FastTags<TagGroup.Global> tags)
        {
            // 1. Basic validation for the item itself
            if (itemValue == null || itemValue.IsEmpty() || itemValue.ItemClass == null)
            {
                return false;
            }

            // 2. Check for item name matches (if itemNameCsv is provided)
            bool nameMatches = false;
            if (!string.IsNullOrEmpty(itemNameCsv))
            {
                string currentItemName = itemValue.ItemClass.GetItemName();
                // Split the CSV string and iterate. Using StringSplitOptions.RemoveEmptyEntries
                // prevents empty strings if there are consecutive commas or leading/trailing commas.
                string[] namesToMatch = itemNameCsv.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);

                foreach (string nameEntry in namesToMatch)
                {
                    // Trim whitespace from the entry for accurate comparison
                    if (currentItemName.Equals(nameEntry.Trim(), System.StringComparison.Ordinal)) // Or OrdinalIgnoreCase if desired
                    {
                        nameMatches = true;
                        break; // Found a match, no need to check other names
                    }
                }
            }
        
            // 3. Check for tag matches (if tags are provided)
            bool tagsMatch = !tags.IsEmpty && itemValue.ItemClass.HasAnyTags(tags);

            // 4. Return true if either names match OR tags match
            return nameMatches || tagsMatch;
        }
    }
}