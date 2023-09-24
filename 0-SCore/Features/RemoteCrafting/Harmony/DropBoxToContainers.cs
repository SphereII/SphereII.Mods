using HarmonyLib;
using SCore.Features.RemoteCrafting.Scripts;
using UnityEngine;

namespace Features.RemoteCrafting
{
    public class DropBoxToContainersPatches
    {
        private const string AdvFeatureClass = "AdvancedRecipes";
        private const string Feature = "ReadFromContainers";
        
        [HarmonyPatch(typeof(XUiC_LootContainer))]
        [HarmonyPatch("OnClose")]
        public class XUiCLootContainerOnClose
        {
            public static bool Prefix(XUiC_LootContainer __instance, BlockValue ___blockValue, TileEntityLootContainer ___localTileEntity)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                if (!___blockValue.Block.Properties.Values.ContainsKey("DropBox")) return true;
                if (___localTileEntity == null) return true;
                StringParsers.TryParseBool(___blockValue.Block.Properties.Values["DropBox"], out var isDropBox);
                if (!isDropBox) return true;
                
                var strDistance = Configuration.GetPropertyValue(AdvFeatureClass, "Distance");
                var distance = 30f;
                if (!string.IsNullOrEmpty(strDistance))
                    distance = StringParsers.ParseFloat(strDistance);
                var primaryPlayer = __instance.xui.playerUI.entityPlayer;
                var items = ___localTileEntity.GetItems();
                for (var i = 0; i < items.Length; i++)
                {
                    if ( items[i].IsEmpty()) continue;
                    // If we successfully added, clear the stack.
                    if (RemoteCraftingUtils.AddToNearbyContainer(primaryPlayer, items[i], distance))
                    {
                        Debug.Log($"Removing {items[i].itemValue.ItemClass.GetItemName()}");
                        //itemStack.Clear();
                        items[i] = ItemStack.Empty.Clone();
                    }
                    ___localTileEntity.UpdateSlot(i, items[i]);
                        
                }
___localTileEntity.SetModified();
                return true;
            }
        }
    }
}