using HarmonyLib;
using SCore.Features.RemoteCrafting.Scripts;

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
                foreach (var itemStack in ___localTileEntity.GetItems())
                {
                    if ( itemStack.IsEmpty()) continue;
                    // If we successfully added, clear the stack.
                    if (RemoteCraftingUtils.AddToNearbyContainer(primaryPlayer, itemStack, distance))
                        itemStack.Clear();
                }

                return true;
            }
        }
    }
}