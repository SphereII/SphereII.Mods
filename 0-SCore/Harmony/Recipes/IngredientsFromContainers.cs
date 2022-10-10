using HarmonyLib;
using Platform;
using Platform.EOS;
using System.Collections.Generic;
using System.Linq;
using UAI;
using UnityEngine;

namespace SCore.Harmony.Recipes
{
    public class EnhancedRecipeLists
    {
        private static readonly string AdvFeatureClass = "AdvancedRecipes";
        private static readonly string Feature = "ReadFromContainers";

        public static List<TileEntity> GetTileEntities(EntityAlive player)
        {
            var distance = 30f;
            var strDistance = Configuration.GetPropertyValue(AdvFeatureClass, "Distance");
            if (!string.IsNullOrEmpty(strDistance))
                distance = StringParsers.ParseFloat(strDistance);
            var tileEntities = new List<TileEntity>();
            tileEntities = GetTileEntities(player, distance);
            return tileEntities;
        }
        public static List<TileEntity> GetTileEntities(EntityAlive player, float distance)
        {
            var disabledsender = Configuration.GetPropertyValue(AdvFeatureClass, "disablesender").Split(',');
            var nottoWorkstation = Configuration.GetPropertyValue(AdvFeatureClass, "nottoWorkstation");
            var bindtoWorkstation = Configuration.GetPropertyValue(AdvFeatureClass, "bindtoWorkstation");
            var tileEntities = new List<TileEntity>();
            var _targetTypes = "Loot, SecureLoot, SecureLootSigned";
            var paths = SCoreUtils.ScanForTileEntities(player, _targetTypes, true);
            foreach (var path in paths)
            {
                var distanceToLeader = Vector3.Distance(player.position, path);
                if (distanceToLeader < distance)
                {
                    var tileEntity = player.world.GetTileEntity(0, new Vector3i(path));
                    if (tileEntity == null) continue;
                    // Check if Broadcastmanager is running if running check if lootcontainer is in Broadcastmanager dictionary
                    // note: Broadcastmanager.Instance.Check()) throws nullref if Broadcastmanager is not running.
                    // works because Hasinstance is being checked first in the or.
                    if (!Broadcastmanager.HasInstance || Broadcastmanager.Instance.Check(tileEntity.ToWorldPos()))
                    {
                        switch (tileEntity.GetTileEntityType())
                        {
                            case TileEntityType.Loot:
                                var lootTileEntity = tileEntity as TileEntityLootContainer;
                                if (lootTileEntity == null) break;
                                // if sending disabled skip container
                                if (disabledsender[0] != null) if (disableSender(disabledsender, tileEntity)) break;
                                if (!string.IsNullOrEmpty(nottoWorkstation)) if (notToWorkstation(nottoWorkstation, player, tileEntity)) goto default;
                                if (!string.IsNullOrEmpty(bindtoWorkstation))
                                {
                                    if (bindToWorkstation(bindtoWorkstation, player, tileEntity)) tileEntities.Add(tileEntity);
                                    else goto default;
                                }
                                else tileEntities.Add(tileEntity);
                                break;
                            case TileEntityType.SecureLootSigned:
                            case TileEntityType.SecureLoot:
                                var secureTileEntity = tileEntity as TileEntitySecureLootContainer;
                                if (secureTileEntity == null) break;
                                if (secureTileEntity.IsLocked() && !secureTileEntity.LocalPlayerIsOwner()) break;
                                // if sending disabled skip container
                                if (disabledsender[0] != null) if (disableSender(disabledsender, tileEntity)) break;
                                if (!string.IsNullOrEmpty(nottoWorkstation)) if (notToWorkstation(nottoWorkstation, player, tileEntity)) goto default;
                                if (!string.IsNullOrEmpty(bindtoWorkstation))
                                {
                                    if (bindToWorkstation(bindtoWorkstation, player, tileEntity)) tileEntities.Add(tileEntity);
                                    else goto default;
                                }
                                else tileEntities.Add(tileEntity);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            return tileEntities;
        }
        public static bool disableSender(string[] value, TileEntity tileEntity)
        {
            var lootTileEntity = tileEntity as TileEntityLootContainer;
            bool invertdisable = bool.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "Invertdisable"));
            if (!invertdisable)
            {
                if (value.Any(x => x.Trim() == lootTileEntity.lootListName)) return true;
            }
            else if (!value.Any(x => x.Trim() == lootTileEntity.lootListName)) return true;

            return false;
        }
        public static bool bindToWorkstation(string value, EntityAlive player, TileEntity tileEntity)
        {
            bool result = false;
            var playerLocal = player as EntityPlayerLocal;
            var lootTileEntity = tileEntity as TileEntityLootContainer;
            // bind storage to workstation
            if (value.Split(';').Where(x => x.Split(':')[0].Split(',').Any(ws => ws.Trim() == playerLocal.PlayerUI.xui.currentWorkstation))
                .Any(x => x.Split(':')[1].Split(',').Any(y => y == lootTileEntity.lootListName))) result = true;
            // bind storage to other workstations if allowed
            if (!value.Split(';').Any(x => x.Split(':')[0].Split(',').Any(ws => ws.Trim() == playerLocal.PlayerUI.xui.currentWorkstation))
                && !bool.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "enforcebindtoWorkstation")))
            {
                if (value.Split(';').Any(x => x.Split(':')[1].Split(',').Any(y => y == lootTileEntity.lootListName))) result = false;
                else result = true;
            }
            return result;
        }
        public static bool notToWorkstation(string value, EntityAlive player, TileEntity tileEntity)
        {
            bool result = false;
            var playerLocal = player as EntityPlayerLocal;
            var lootTileEntity = tileEntity as TileEntityLootContainer;
            foreach (var bind in value.Split(';'))
            {
                var workstation = bind.Split(':')[0].Split(',');
                var disablebinding = bind.Split(':')[1].Split(',');
                if ((workstation.Any(ws => ws.Trim() == playerLocal.PlayerUI.xui.currentWorkstation)) && (disablebinding.Any(x => x.Trim() == lootTileEntity.lootListName))) result = true;
            }
            return result;
        }
        public static List<ItemStack> SearchNearbyContainers(EntityAlive player)
        {
            var _items = new List<ItemStack>();
            var tileEntities = EnhancedRecipeLists.GetTileEntities(player);
            foreach (var tileEntity in tileEntities)
            {
                var lootTileEntity = tileEntity as TileEntityLootContainer;
                if (lootTileEntity == null) continue;
                _items.AddRange(lootTileEntity.GetItems());
            }
            return _items;
        }
        public static List<ItemStack> SearchNearbyContainers(EntityAlive player, ItemValue itemValue)
        {
            var _item = new List<ItemStack>();
            var _items = new List<ItemStack>();
            var tileEntities = EnhancedRecipeLists.GetTileEntities(player);
            foreach (var tileEntity in tileEntities)
            {
                var lootTileEntity = tileEntity as TileEntityLootContainer;
                if (lootTileEntity == null) continue;
                _item.AddRange(lootTileEntity.GetItems());
            }
            for (int i = 0; i < _item.Count; i++)
            {
                if ((!_item[i].itemValue.HasModSlots || !_item[i].itemValue.HasMods()) && _item[i].itemValue.type == itemValue.type)
                {
                    _items.Add(_item[i]);
                }
            }
            return _items;
        }

        public static List<ItemStack> SearchNearbyContainers(EntityAlive player, ItemValue itemValue, float distance)
        {
            var _item = new List<ItemStack>();
            var _items = new List<ItemStack>();
            var tileEntities = EnhancedRecipeLists.GetTileEntities(player, distance);
            foreach (var tileEntity in tileEntities)
            {
                var lootTileEntity = tileEntity as TileEntityLootContainer;
                if (lootTileEntity == null) continue;
                _item.AddRange(lootTileEntity.GetItems());
            }
            for (int i = 0; i < _item.Count; i++)
            {
                if ((!_item[i].itemValue.HasModSlots || !_item[i].itemValue.HasMods()) && _item[i].itemValue.type == itemValue.type)
                {
                    _items.Add(_item[i]);
                }
            }
            return _items;
        }

        [HarmonyPatch(typeof(XUiC_RecipeList))]
        [HarmonyPatch("BuildRecipeInfosList")]
        public class BuildRecipeInfosList
        {
            public static bool Prefix(XUiC_RecipeList __instance, ref List<ItemStack> _items)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;
                var player = __instance.xui.playerUI.entityPlayer;

                _items.AddRange(EnhancedRecipeLists.SearchNearbyContainers(player));
                return true;
            }
        }

        [HarmonyPatch(typeof(XUiM_PlayerInventory))]
        [HarmonyPatch("GetAllItemStacks")]
        public class GetAllItemStacks
        {
            public static void Postfix(ref List<ItemStack> __result, EntityPlayerLocal ___localPlayer)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;
                __result.AddRange(EnhancedRecipeLists.SearchNearbyContainers(___localPlayer));
            }
        }

        //[HarmonyPatch(typeof(XUiM_PlayerInventory))]
        //[HarmonyPatch("GetItemCount")]
        //[HarmonyPatch(new[] { typeof(ItemValue) })]
        //public class GetItemCount
        //{
        //    public static void Postfix(ref int __result, EntityPlayerLocal ___localPlayer, ItemValue _itemValue)
        //    {
        //        // Check if this feature is enabled.
        //        if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
        //            return;

        //        foreach (var item in EnhancedRecipeLists.SearchNearbyContainers(___localPlayer))
        //        {
        //            if (item == null) continue;
        //            if (item.IsEmpty()) continue;
        //            if (item.itemValue == null) continue;
        //            if ((!item.itemValue.HasModSlots || !item.itemValue.HasMods()) && item.itemValue.type == _itemValue.type)
        //                __result += item.count;

        //        }

        //    }
        //}

        // replaces getitemcount
        // mostly a copy of the original code.
        [HarmonyPatch(typeof(XUiC_IngredientEntry))]
        [HarmonyPatch("GetBindingValue")]
        public class GetBindingValue
        {
            public static bool Prefix(XUiC_IngredientEntry __instance, ref bool __result, ref string value, string bindingName, CachedStringFormatter<int> ___needcountFormatter, CachedStringFormatter<int> ___havecountFormatter, bool ___materialBased, ItemStack ___ingredient, string ___material, XUiC_RecipeCraftCount ___craftCountControl, CachedStringFormatterXuiRgbaColor ___itemicontintcolorFormatter)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;
                bool flag = ___ingredient != null;
                switch (bindingName)
                {
                    case "haveneedcount":
                        {
                            string text = (flag ? ___needcountFormatter.Format(___ingredient.count * ___craftCountControl.Count) : "");
                            int value1 = 0;
                            XUiC_WorkstationMaterialInputGrid childByType = __instance.WindowGroup.Controller.GetChildByType<XUiC_WorkstationMaterialInputGrid>();
                            if (childByType != null)
                            {
                                if (___materialBased)
                                {
                                    value = (flag ? (___havecountFormatter.Format(childByType.GetWeight(___material)) + "/" + text) : "");
                                }
                                else
                                {
                                    value = (flag ? (___havecountFormatter.Format(__instance.xui.PlayerInventory.GetItemCount(___ingredient.itemValue)) + "/" + text) : "");
                                }
                            }
                            else
                            {
                                XUiC_WorkstationInputGrid childByType2 = __instance.WindowGroup.Controller.GetChildByType<XUiC_WorkstationInputGrid>();
                                if (childByType2 != null)
                                {
                                    value = (flag ? (___havecountFormatter.Format(childByType2.GetItemCount(___ingredient.itemValue)) + "/" + text) : "");
                                }
                                else
                                {
                                    value = (flag ? (___havecountFormatter.Format(__instance.xui.PlayerInventory.GetItemCount(___ingredient.itemValue)) + "/" + text) : "");
                                    if (flag)
                                    {
                                        // add items from lootcontainers
                                        value1 = __instance.xui.PlayerInventory.GetItemCount(___ingredient.itemValue);
                                        ItemStack[] array = SearchNearbyContainers(__instance.xui.playerUI.entityPlayer, ___ingredient.itemValue).ToArray();
                                        for (int k = 0; k < array.Length; k++)
                                        {
                                            if (array[k] != null && array[k].itemValue.type != 0 && ___ingredient.itemValue.type == array[k].itemValue.type)
                                            {
                                                value1 += array[k].count;
                                            }
                                        }
                                        value = (flag ? (___havecountFormatter.Format(value1) + "/" + text) : "");
                                    }
                                }
                            }
                            __result = true;
                            return false;
                        }
                    default:
                        return true;
                }
            }
        }

        [HarmonyPatch(typeof(XUiM_PlayerInventory))]
        [HarmonyPatch("HasItems")]
        public class HasItems
        {
            public static bool Postfix(bool __result, IList<ItemStack> _itemStacks, EntityPlayerLocal ___localPlayer, int _multiplier)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return __result;

                if (__result == true) return __result;

                var totalCount = 0;
                //var tileEntities = EnhancedRecipeLists.GetTileEntities(___localPlayer);

                foreach (var itemStack in _itemStacks)
                {
                    int num = itemStack.count * _multiplier;
                    // check player inventory
                    var slots = ___localPlayer.bag.GetSlots();
                    totalCount = totalCount + slots
                        .Where(x => x.itemValue.ItemClass == itemStack.itemValue.ItemClass)
                        .Sum(y => y.count);
                    // check container
                    totalCount = totalCount + SearchNearbyContainers(___localPlayer, itemStack.itemValue).Sum(y => y.count);
                    if (totalCount >= num) return true;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(XUiM_PlayerInventory))]
        [HarmonyPatch("RemoveItems")]
        public class RemoveItems
        {
            public static bool Prefix(IList<ItemStack> _itemStacks, EntityPlayerLocal ___localPlayer, int _multiplier)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                var tileEntities = EnhancedRecipeLists.GetTileEntities(___localPlayer);
                foreach (var itemStack in _itemStacks)
                {
                    // counter quantity needed from item
                    int q = itemStack.count * _multiplier;
                    //check player inventory for materials and reduce counter
                    var slots = ___localPlayer.bag.GetSlots();
                    q = q - slots
                        .Where(x => x.itemValue.ItemClass == itemStack.itemValue.ItemClass)
                        .Sum(y => y.count);

                    // check storage boxes
                    foreach (var tileEntity in tileEntities)
                    {
                        if (q <= 0) break;
                        var lootTileEntity = tileEntity as TileEntityLootContainer;
                        if (lootTileEntity == null) continue;

                        // If there's no items in this container, skip.
                        if (!lootTileEntity.HasItem(itemStack.itemValue)) continue;

                        int num = itemStack.count * _multiplier;
                        if (lootTileEntity == null) break;
                        for (int y = 0; y < lootTileEntity.items.Length; y++)
                        {
                            var item = lootTileEntity.items[y];
                            if (item.IsEmpty()) continue;
                            if (item.itemValue.ItemClass == itemStack.itemValue.ItemClass)
                            {
                                // If we can completely satisfy the result, let's do that.
                                if (item.count >= q)
                                {
                                    item.count -= q;
                                    q = 0;
                                }
                                else
                                {
                                    // Otherwise, let's just count down until we meet the requirement.
                                    while (q >= 0)
                                    {
                                        item.count--;
                                        q--;
                                        if (item.count <= 0)
                                            break;
                                    }
                                }

                                //Update the slot on the container, and do the Setmodified(), so that the dedis can get updated.
                                if (item.count < 1)
                                    lootTileEntity.UpdateSlot(y, ItemStack.Empty.Clone());
                                else
                                    lootTileEntity.UpdateSlot(y, item);
                                lootTileEntity.SetModified();
                            }
                        }
                    }
                }
                return true;
            }
        }

        // Code from OCB7D2D/OcbPinRecipes
        // Patch world unload to cleanup and save on exit
        [HarmonyPatch(typeof(World))]
        [HarmonyPatch("UnloadWorld")]
        public class World_UnloadWorld
        {
            static void Postfix()
            {
                if (!Broadcastmanager.HasInstance) return;
                Broadcastmanager.Cleanup();
            }
        }
    }

    // Code from Laydor slightly modified
    public class XUiC_BroadcastButton : XUiController
    {
        private static readonly string AdvFeatureClass = "AdvancedRecipes";

        XUiV_Button button;

        public override void Init()
        {
            base.Init();

            button = viewComponent as XUiV_Button;

            OnPress += Grab_OnPress;
        }

        public override void Update(float _dt)
        {
            base.Update(_dt);
            if (IsDirty)
            {
                IsDirty = false;
                SetupButton();
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();
            //if debug enabled show lootList name of container
            if (Configuration.CheckFeatureStatus(AdvFeatureClass, "Debug"))
                if (xui.lootContainer != null) Log.Out("Current Container name: " + xui.lootContainer.lootListName);
            IsDirty = true;
        }

        private void Grab_OnPress(XUiController _sender, int _mouseButton)
        {
            //Check if Broadcastmanager is running
            if (!Broadcastmanager.HasInstance) return;

            if (Broadcastmanager.Instance.Check(xui.lootContainer.ToWorldPos()))
            {
                //Unselect button
                button.Selected = true;
                // Remove from Broadcastmanager dictionary
                Broadcastmanager.Instance.remove(xui.lootContainer.ToWorldPos());
            }
            else
            {
                //Select button
                button.Selected = false;
                // Add to Broadcastmanager dictionary
                Broadcastmanager.Instance.add(xui.lootContainer.ToWorldPos());
            }
        }

        private void SetupButton()
        {

            //Unselect button and disable it
            button.Enabled = false;
            button.Selected = false;
            button.IsVisible = false;

            var disabledsender = Configuration.GetPropertyValue(AdvFeatureClass, "disablesender").Split(',');
            var bindToWorkstation = Configuration.GetPropertyValue(AdvFeatureClass, "bindtoWorkstation").Split(';');
            if (xui.lootContainer != null && Broadcastmanager.HasInstance)
            {
                if (disabledsender[0] != null)
                    if (EnhancedRecipeLists.disableSender(disabledsender, xui.lootContainer))
                    {
                        return;
                    }
                if (!string.IsNullOrEmpty(bindToWorkstation[0]))
                {
                    int counter = 0;
                    foreach (var bind in bindToWorkstation)
                    {
                        var bindings = bind.Split(':')[1].Split(',');
                        if ((bindings.Any(x => x.Trim() == xui.lootContainer.lootListName))) counter++;
                    }
                    if (counter == 0 && bool.Parse(Configuration.GetPropertyValue(AdvFeatureClass, "enforcebindtoWorkstation")))
                    {
                        return;
                    }
                }
                //Enable button and set if button is selected
                button.IsVisible = true;
                button.Enabled = true;
                button.Selected = !Broadcastmanager.Instance.Check(xui.lootContainer.ToWorldPos());
            }
        }
    }
}