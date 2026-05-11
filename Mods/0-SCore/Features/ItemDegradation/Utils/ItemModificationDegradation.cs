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

        public static int GetDurabilityForQuality(int quality, int minQuality, int maxQuality,
                                                    int minDurability, int maxDurability,
                                                    string curve = "linear")
        {
            quality = Math.Max(minQuality, Math.Min(maxQuality, quality));

            float t = (maxQuality == minQuality) ? 1f
                      : (float)(quality - minQuality) / (maxQuality - minQuality);

            float curvedT = curve switch
            {
                "quadratic" => t * t,
                "cubic"     => t * t * t,
                _           => t,   // linear (default)
            };

            return (int)Math.Round(minDurability + curvedT * (maxDurability - minDurability));
        }

        private static int GetValue(ItemValue mod, string property)
        {
            if (mod.ItemClass?.Properties == null) return -1;
            if (!mod.ItemClass.Properties.Contains(property)) return -1;

            var propValue = mod.ItemClass.Properties.GetString(property);
            if (string.IsNullOrEmpty(propValue)) return -1;

            // Single integer — no quality scaling.
            if (!propValue.Contains(',')) return StringParsers.ParseSInt32(propValue);

            // Comma-separated range requires a param1 quality range (e.g. param1="1,6").
            // Use TryGetValue: item_modifiers may not populate Params1 for appended
            // properties, which previously caused a silent -1 return → GetMaxUseTimes=0
            // → CanDegrade=false → no SCore degradation fired at all.
            if (!mod.ItemClass.Properties.Params1.TryGetValue(property, out var qualityRangeString) ||
                string.IsNullOrEmpty(qualityRangeString))
            {
                Log.Warning($"[SCore] ItemDegradation: '{mod.ItemClass.GetItemName()}' has a " +
                            $"comma-separated '{property}' but no param1 quality range. " +
                            $"Falling back to the minimum value. Add param1=\"minQuality,maxQuality\" to fix this.");
                return StringParsers.ParseSInt32(propValue.Substring(0, propValue.IndexOf(',')));
            }

            var qualityRange    = StringParsers.ParseVector2i(qualityRangeString);
            var durabilityRange = StringParsers.ParseVector2i(propValue);
            var curve           = mod.ItemClass.Properties.Contains("DegradationCurve")
                                  ? mod.ItemClass.Properties.GetString("DegradationCurve")
                                  : "linear";

            return GetDurabilityForQuality(mod.Quality, qualityRange.x, qualityRange.y,
                                           durabilityRange.x, durabilityRange.y, curve);
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
            if (maxUse <= 0) return 0f;
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
                // When MaxUseTimesBreaksAfter is true, the break sound and slot removal are
                // handled by the caller (MinEventActionRoutineUpdate.CheckItemValue) which
                // has the array index needed to null out the Modifications slot. Nothing to
                // do here beyond the DeactivateItem call above.

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