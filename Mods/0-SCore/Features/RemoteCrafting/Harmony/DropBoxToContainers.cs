using HarmonyLib;
using SCore.Features.RemoteCrafting.Scripts;

namespace Features.RemoteCrafting {
    public class DropBoxToContainersPatches {
        private const string AdvFeatureClass = "AdvancedRecipes";
        private const string Feature = "ReadFromContainers";

        private static bool IsServer => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;

        // Distribute when the box closes. Single player and a P2P host's own boxes do it here,
        // where the local copy is the authoritative one.
        [HarmonyPatch(typeof(XUiC_LootContainer))]
        [HarmonyPatch(nameof(XUiC_LootContainer.OnClose))]
        public class XUiCLootContainerOnClose {
            public static bool Prefix(XUiC_LootContainer __instance) {

                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                // A remote client must not distribute: it would write into containers it has not
                // locked and the server would also sort the box, duplicating every item. The server
                // distributes when the client's lock is released (see TEFeatureStorageOnUnlockedServer).
                if (!IsServer)
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
                var block = GameManager.Instance.World.GetBlock(pos).Block;
                if (!RemoteCraftingUtils.IsDropBoxEnabled(block))
                {
                    Log.Out($"[DropBox] OnClose: block at {pos} ({block.GetBlockName()}) is not a drop box, skipping.");
                    return true;
                }

                var distance = RemoteCraftingUtils.GetDropBoxDistance();
                var primaryPlayer = __instance.xui.playerUI.entityPlayer;
                Log.Out($"[DropBox] OnClose: distributing from {block.GetBlockName()} at {pos}, player={primaryPlayer?.EntityName ?? "null"}, distance={distance}");

                if (__instance.localTileEntity.ItemGrid.items == null)
                {
                    Log.Out("[DropBox] OnClose: items array is null, skipping.");
                    return true;
                }

                var items = __instance.localTileEntity.ItemGrid.items;
                for (var i = 0; i < items.Length; i++)
                {
                    if (items[i].IsEmpty()) continue;
                    var itemName = items[i].itemValue?.ItemClass?.GetItemName() ?? "unknown";
                    Log.Out($"[DropBox] OnClose: trying to distribute slot {i}: {itemName} x{items[i].count}");

                    if (RemoteCraftingUtils.AddToNearbyContainer(primaryPlayer, items[i], distance))
                    {
                        Log.Out($"[DropBox] OnClose: distributed {itemName} successfully, clearing slot {i}.");
                        RemoteCraftingUtils.ClearSlot(items, i);
                        continue;
                    }

                    Log.Out($"[DropBox] OnClose: no container accepted {itemName}, leaving in slot {i}.");
                }

                __instance.localTileEntity.SetModified();
                return true;
            }
        }

        // Dedicated server / remote clients: distribute on the server once the client's lock is
        // released. The client's final box contents arrive before its unlock request (same channel),
        // so this sorts exactly what the player left in the box, once.
        [HarmonyPatch(typeof(TEFeatureStorage))]
        [HarmonyPatch(nameof(TEFeatureStorage.OnUnlockedServer))]
        public class TEFeatureStorageOnUnlockedServer {
            public static void Postfix(TEFeatureStorage __instance, int _unlockingPlayerId) {
                if (!IsServer) return;
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;

                var world = GameManager.Instance.World;
                if (world == null) return;

                // The host's own close already distributed in the OnClose prefix.
                if (_unlockingPlayerId == world.GetPrimaryPlayerId()) return;

                var box = __instance.Parent;
                if (box == null) return;

                // OnUnlockedServer may have just removed the tile entity.
                var pos = __instance.ToWorldPos();
                if (world.GetTileEntity(pos) != box) return;
                if (!RemoteCraftingUtils.IsDropBoxEnabled(world.GetBlock(pos).Block)) return;

                RemoteCraftingUtils.DistributeDropBox(box, __instance, RemoteCraftingUtils.GetDropBoxDistance());
            }
        }
    }
}
