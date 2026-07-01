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
                    Log.Out("[DropBox] OnClose: localTileEntity is null, skipping.");
                    return true;
                }
                var composite = __instance.localTileEntity as TEFeatureStorage;
                if (composite == null)
                {
                    Log.Out("[DropBox] OnClose: localTileEntity is not TEFeatureStorage, skipping.");
                    return true;
                }

                var pos = composite.ToWorldPos();
                var blockValue = GameManager.Instance.World.GetBlock(pos);
                var block = blockValue.Block;
                var hasDropBoxProperty = block.Properties.Values.TryGetValue("DropBox", out var dropBoxStr) &&
                                         StringParsers.TryParseBool(dropBoxStr, out var dropBoxBool) && dropBoxBool;
                if (block is not BlockDropBoxContainer && !hasDropBoxProperty)
                {
                    Log.Out($"[DropBox] OnClose: block at {pos} ({block.GetBlockName()}) is not a drop box, skipping.");
                    return true;
                }

                var strDistance = Configuration.GetPropertyValue(AdvFeatureClass, "Distance");
                var distance = 30f;
                if (!string.IsNullOrEmpty(strDistance))
                    distance = StringParsers.ParseFloat(strDistance);

                var primaryPlayer = __instance.xui.playerUI.entityPlayer;
                Log.Out($"[DropBox] OnClose: distributing from {block.GetBlockName()} at {pos}, player={primaryPlayer?.EntityName ?? "null"}, distance={distance}, IsServer={SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer}");

                if (__instance.localTileEntity.items == null)
                {
                    Log.Out("[DropBox] OnClose: items array is null, skipping.");
                    return true;
                }

                var items = __instance.localTileEntity.items;
                for (var i = 0; i < items.Length; i++)
                {
                    if (items[i].IsEmpty()) continue;
                    var itemName = items[i].itemValue?.ItemClass?.GetItemName() ?? "unknown";
                    Log.Out($"[DropBox] OnClose: trying to distribute slot {i}: {itemName} x{items[i].count}");

                    if (RemoteCraftingUtils.AddToNearbyContainer(primaryPlayer, items[i], distance))
                    {
                        Log.Out($"[DropBox] OnClose: distributed {itemName} successfully, clearing slot {i}.");
                        items[i] = ItemStack.Empty.Clone();
                        continue;
                    }

                    Log.Out($"[DropBox] OnClose: no container accepted {itemName}, leaving in slot {i}.");
                    __instance.localTileEntity.UpdateSlot(i, items[i]);
                }

                __instance.localTileEntity.SetModified();
                return true;
            }
        }
    }
}