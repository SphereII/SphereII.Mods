using System;
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
            if (!mod.HasQuality) return false;
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

        public static void CheckModification(ItemValue mod, EntityAlive player)
        {
            if (!CanDegrade(mod)) return;
            if (IsDegraded(mod)) return;
            mod.UseTimes += GetDegradationPerUse(mod);
            if (mod.UseTimes < GetMaxUseTimes(mod)) return;

            if ( player != null)
                Manager.BroadcastPlay(player, "itembreak");
            if (mod.ItemClass.HasTrigger(MinEventTypes.onSelfItemActivate) && mod.Activated != 0)
            {
                player.MinEventContext.ItemValue = mod;
                mod.FireEvent(MinEventTypes.onSelfItemDeactivate, player != null ? player.MinEventContext : null);
                mod.Activated = 0;
            }

            if (mod.ItemClass.MaxUseTimesBreaksAfter.Value)
            {
                mod = ItemValue.None;
            }
        }

        public static void CheckModificationOnItem(ItemValue[] items, EntityAlive player)
        {
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i]?.ItemClass == null) continue;
                CheckModification(items[i], player);
            }
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


     
    }
}