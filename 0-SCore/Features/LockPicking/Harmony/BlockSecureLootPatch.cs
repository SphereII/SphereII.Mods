using HarmonyLib;
using Platform;

namespace Features.LockPicking
{
    public class BlockSecureLootPatch
    {
        private static readonly string AdvFeatureClass = "AdvancedLockpicking";
        private static readonly string Feature = "AdvancedLocks";


        [HarmonyPatch(typeof(BlockSecureLoot))]
        [HarmonyPatch("OnBlockActivated")]
        [HarmonyPatch(new[]
        {
            typeof(string), typeof(WorldBase), typeof(int), typeof(Vector3i), typeof(BlockValue), typeof(EntityPlayerLocal)
        })]
        public class BlockSecureLootOnBlockActivated
        {
            public static bool Prefix(ref Block __instance, string _commandName, WorldBase _world, int _cIdx,
                Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player
                , string ___lockPickItem, BlockActivationCommand[] ___cmds)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                if (!LockPickingUtils.CheckForMiniGame(_player))
                {
                    return true;
                }
                if (_commandName != "pick")
                    return true;

                if (_blockValue.ischild) return true;
                if (_world.GetTileEntity(_cIdx, _blockPos) is not TileEntitySecureLootContainer tileEntitySecureLootContainer) return false;

                if (!tileEntitySecureLootContainer.IsLocked() &&
                    tileEntitySecureLootContainer.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
                {
                    __instance.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player as EntityPlayerLocal);
                    return false;
                }

                // Check if the player has lock picks.
                var playerUI = (_player as EntityPlayerLocal)?.PlayerUI;
                if (playerUI == null)
                    return false;
                var playerInventory = playerUI.xui.PlayerInventory;
                var item = ItemClass.GetItem("resourceLockPick");
                if (playerInventory.GetItemCount(item) == 0)
                {
                    playerUI.xui.CollectedItemList.AddItemStack(new ItemStack(item, 0), true);
                    GameManager.ShowTooltip((EntityPlayerLocal) _player, Localization.Get("ttLockpickMissing"));
                    return false;
                }

                tileEntitySecureLootContainer.SetLocked(true);
                XUiC_PickLocking.Open(playerUI, tileEntitySecureLootContainer, _blockValue, _blockPos);
                return false;
            }
        }
    }
}