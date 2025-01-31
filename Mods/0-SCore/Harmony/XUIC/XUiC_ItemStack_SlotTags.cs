using System;
using System.Collections.Generic;
using System.Reflection;
using Audio;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace SCore.Harmony.TileEntities
{
    public class CheckItemsForContainers
    {
        private static bool IsStackAllowedInContainer(XUiC_ItemStack itemStack)
        {
            if (itemStack == null) return true;
            switch (itemStack.StackLocation)
            {
                case XUiC_ItemStack.StackLocationTypes.Backpack:
                case XUiC_ItemStack.StackLocationTypes.ToolBelt:

                    return true;
            }

            var currentStack = itemStack.xui.dragAndDrop?.CurrentStack;
            if (currentStack == null || currentStack.IsEmpty()) return true;

            // Only run on loot containers and their slots.
            if (itemStack.xui.lootContainer == null) return true;
            if (itemStack.StackLocation != XUiC_ItemStack.StackLocationTypes.LootContainer) return true;
            var blockValue = GameManager.Instance.World.GetBlock(itemStack.xui.lootContainer.ToWorldPos());
            var block = blockValue.Block;
            return CanPlaceItemInContainerViaTags(block, currentStack, true);
        }

        private static bool CheckItemsForContainer(ItemStack itemStack)
        {
            if (!itemStack.itemValue.HasMetadata("NoStorage"))
            {
                return true;
            }

            var noStorageString = itemStack.itemValue.GetMetadata("NoStorage")?.ToString();
            if (string.IsNullOrEmpty(noStorageString))
            {
                return true;
            }

            var noStorageId = StringParsers.ParseSInt32(noStorageString);
            if (noStorageId > 0)
            {
                DisplayToolTip(null, itemStack);
                return false;
            }

            return true;
        }

        private static bool CanPlaceItemInContainerViaTags(Block block, ItemStack itemStack, bool showToolTip = false)
        {
            if (CheckItemsForContainer(itemStack) == false) return false;

            if (block == null) return true;

            // If the tags don't exist, skip all the checks.
            if (!block.Properties.Contains("AllowTags") && !block.Properties.Contains("DisallowTags")) return true;

            var all = FastTags<TagGroup.Global>.Parse("all");
            if (block.Properties.Contains("AllowTags"))
            {
                var allowedTags = FastTags<TagGroup.Global>.Parse(block.Properties.GetString("AllowTags"));
                if (allowedTags.Test_AnySet(all)) return true;
                if (itemStack.itemValue.ItemClass.HasAnyTags(allowedTags)) return true;
            }

            if (block.Properties.Contains("DisallowTags"))
            {
                var blockedTags = FastTags<TagGroup.Global>.Parse(block.Properties.GetString("DisallowTags"));
                if (!itemStack.itemValue.ItemClass.HasAnyTags(blockedTags)) return true;
            }


            if (!showToolTip) return false;
            DisplayToolTip(block, itemStack);
            return false;
        }

        private static void DisplayToolTip(Block block, ItemStack itemStack)
        {
            var message = Localization.Get("ItemCannotBePlaced");
            if (block != null)
            {
                if (block.Properties.Contains("DisallowedKey"))
                {
                    message = Localization.Get(block.Properties.GetString("DisallowedKey"));
                }
            }

            if (itemStack.itemValue.ItemClass.Properties.Contains("DisallowedKey"))
            {
                message = Localization.Get(itemStack.itemValue.ItemClass.Properties.GetString("DisallowedKey"));
            }

            Manager.PlayInsidePlayerHead("ui_denied");
            var primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
            XUiC_PopupToolTip.ClearTooltips(primaryPlayer.playerUI.xui);
            GameManager.ShowTooltip(primaryPlayer, message);
        }

        // For clicking and sending objects to the toolbelt/backpack/loot container
        public class TEFeatureStoragePatch
        {
            [HarmonyPatch(typeof(TEFeatureStorage))]
            [HarmonyPatch("TryStackItem")]
            public class TEFeatureStorageTryStackItem
            {
                public static bool Prefix(TEFeatureStorage __instance, ItemStack _itemStack)
                {
                    if (__instance is ITileEntityLootable tileEntityLootable)
                    {
                        if (tileEntityLootable.GetChunk() != null)
                            return CanPlaceItemInContainerViaTags(tileEntityLootable.blockValue.Block, _itemStack);
                        return CanPlaceItemInContainerViaTags(null, _itemStack);
                    }

                    return true;
                }
            }

            [HarmonyPatch(typeof(TEFeatureStorage))]
            [HarmonyPatch("AddItem")]
            public class TEFeatureStorageAddItem
            {
                public static bool Prefix(TEFeatureStorage __instance, ItemStack _itemStack)
                {
                    if (__instance is ITileEntityLootable tileEntityLootable)
                    {
                        return CanPlaceItemInContainerViaTags(tileEntityLootable.blockValue.Block, _itemStack);
                    }

                    return true;
                }
            }
        }

        public class TileEntityContainerStoragePatch
        {
            [HarmonyPatch(typeof(TileEntityLootContainer))]
            [HarmonyPatch("TryStackItem")]
            public class TEFeatureStorageTryStackItem
            {
                public static bool Prefix(TileEntityLootContainer __instance, ItemStack _itemStack)
                {
                    if (__instance is ITileEntityLootable tileEntityLootable)
                    {
                        if (tileEntityLootable.GetChunk() != null)
                            return CanPlaceItemInContainerViaTags(tileEntityLootable.blockValue.Block, _itemStack);
                        return CanPlaceItemInContainerViaTags(null, _itemStack);
                    }

                    return true;
                }
            }

            [HarmonyPatch(typeof(TileEntityLootContainer))]
            [HarmonyPatch("AddItem")]
            public class TEFeatureStorageAddItem
            {
                public static bool Prefix(TileEntityLootContainer __instance, ItemStack _item)
                {
                    if (__instance is ITileEntityLootable tileEntityLootable)
                    {
                        if ( tileEntityLootable.GetChunk() != null )
                            return CanPlaceItemInContainerViaTags(tileEntityLootable.blockValue.Block, _item);
                        return CanPlaceItemInContainerViaTags(null, _item);
                    }

                    return true;
                }
            }
        }

        // For other methods, such as automatic stashing, dragging and dropping, etc.
        [HarmonyPatch]
        public class XUiC_ItemStack_SlotTags
        {
            [HarmonyTargetMethod]
            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return typeof(XUiC_ItemStack).GetMethod("CanSwap");
                yield return typeof(XUiC_ItemStack).GetMethod("ForceSetItemStack");
                yield return typeof(XUiC_ItemStack).GetMethod("HandleDropOne");
                yield return typeof(XUiC_ItemStack).GetMethod("HandleMoveToPreferredLocation");
                yield return typeof(XUiC_ItemStack).GetMethod("HandlePartialStackPickup");
                yield return typeof(XUiC_ItemStack).GetMethod("HandleStackSwap");
                yield return typeof(XUiC_ItemStack).GetMethod("SwapItem");
            }

            public static bool Prefix(XUiC_ItemStack __instance)
            {
                return IsStackAllowedInContainer(__instance);
            }
        }

        public class XUiM_LootContainerItemStackPatch
        {
            [HarmonyPatch(typeof(XUiM_LootContainer))]
            [HarmonyPatch("AddItem")]
            public class XUiMLootContainerItemStackPatchAddItem
            {
                public static bool Prefix(ItemStack _itemStack, XUi _xui)
                {
                    if (_xui.lootContainer == null) return true;
                    var blockValue = GameManager.Instance.World.GetBlock(_xui.lootContainer.ToWorldPos());
                    var block = blockValue.Block;
                    return CanPlaceItemInContainerViaTags(block, _itemStack, true);
                }
            }
        }

        // public class TileEntityLootContainerItemStackPatch {
        //     [HarmonyPatch(typeof(TileEntityLootContainer))]
        //     [HarmonyPatch("AddItem")]
        //     public class XTileEntityLootContainerItemStackPatchItemStackPatchAddItem {
        //         public static bool Prefix(TileEntityLootContainer __instance, ItemStack _item) {
        //             var block = __instance?.blockValue.Block;
        //             if ( block == null ) return true;
        //             return CanPlaceItemInContainerViaTags(block, _item, true);
        //         }
        //     }
        // }

        public class XUICLootContainerCheckItemsForContainers
        {
            [HarmonyPatch(typeof(XUiC_LootWindow))]
            [HarmonyPatch("SetTileEntityChest")]
            public class XUiCLootWindowSetTileEntityChest
            {
                public static void Postfix(XUiC_LootWindow __instance, string _lootContainerName)
                {
                    if (__instance.te == null) return;
                    var blockValue = GameManager.Instance.World.GetBlock(__instance.te.ToWorldPos());
                    if (blockValue.isair) return;
                    var block = blockValue.Block;
                    if (block.Properties.Contains("AllowTags"))
                    {
                        var display = block.Properties.GetString("AllowTags");
                        __instance.lootContainerName = $"{_lootContainerName} ( Tag Limited: {display} )";
                        __instance.RefreshBindings(true);
                        return;
                    }

                    if (block.Properties.Contains("DisallowTags"))
                    {
                        var display = block.Properties.GetString("DisallowTags");
                        __instance.lootContainerName = $"{_lootContainerName} ( Blocked Tags: {display} )";
                        __instance.RefreshBindings(true);
                    }
                }
            }
        }
    }
}