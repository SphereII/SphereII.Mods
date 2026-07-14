using HarmonyLib;
using SCore.Features.RemoteCrafting.Scripts;

namespace Features.RemoteCrafting {
    public class DropBoxToContainersPatches {
        private const string AdvFeatureClass = "AdvancedRecipes";
        private const string Feature = "ReadFromContainers";

        [HarmonyPatch(typeof(XUiC_LootContainer))]
        [HarmonyPatch(nameof(XUiC_LootContainer.OnClose))]
        public class XUiCLootContainerOnClose {
            public static bool Prefix(XUiC_LootContainer __instance) {

                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                if (__instance.localTileEntity == null)
                {
                    return true;
                }
                var composite = __instance.localTileEntity as TEFeatureStorage;
                if (composite == null)
                {
                    return true;
                }

                var pos = composite.ToWorldPos();
                var blockValue = GameManager.Instance.World.GetBlock(pos);
                var block = blockValue.Block;
                var hasDropBoxProperty = block.Properties.Values.TryGetValue("DropBox", out var dropBoxStr) &&
                                         StringParsers.TryParseBool(dropBoxStr, out var dropBoxBool) && dropBoxBool;
                if (block is not BlockDropBoxContainer && !hasDropBoxProperty)
                {
                    return true;
                }

                var strDistance = Configuration.GetPropertyValue(AdvFeatureClass, "Distance");
                var distance = 30f;
                if (!string.IsNullOrEmpty(strDistance))
                    distance = StringParsers.ParseFloat(strDistance);

                var primaryPlayer = __instance.xui.playerUI.entityPlayer;
                if (primaryPlayer == null)
                    return true;

                if (__instance.localTileEntity.items == null)
                {
                    return true;
                }

                var items = __instance.localTileEntity.items;
                var tileEntities = RemoteCraftingUtils.GetTileEntities(primaryPlayer, distance, false);
                for (var i = 0; i < items.Length; i++)
                {
                    if (items[i].IsEmpty()) continue;

                    if (RemoteCraftingUtils.AddToNearbyContainer(primaryPlayer, items[i], tileEntities))
                    {
                        items[i] = ItemStack.Empty.Clone();
                        continue;
                    }

                    __instance.localTileEntity.UpdateSlot(i, items[i]);
                }

                __instance.localTileEntity.SetModified();
                return true;
            }
        }
    }
}