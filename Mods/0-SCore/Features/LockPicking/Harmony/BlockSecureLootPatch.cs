using HarmonyLib;
using Platform;

namespace Features.LockPicking
{
    public class BlockSecureLootPatch
    {
        private static readonly string AdvFeatureClass = "AdvancedLockpicking";
        private static readonly string Feature = "AdvancedLocks";


        [HarmonyPatch(typeof(BlockCompositeTileEntity))]
        [HarmonyPatch("OnBlockActivated")]
        [HarmonyPatch(new[]
        {
            typeof(string), typeof(WorldBase), typeof(Vector3i), typeof(BlockValue), typeof(EntityPlayerLocal)
        })]
        public class BlockSecureLootOnBlockActivated
        {
            public static bool Prefix(ref Block __instance, string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player
               ,BlockActivationCommand[] ___cmds)
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
                var lockable = (_world.GetTileEntity(_blockPos) as TileEntityComposite)?.GetFeature<TEFeatureLockable>();
                if (lockable == null) return false;

                if (!lockable.IsLocked() &&
                    lockable.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
                {
                    __instance.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
                    return false;
                }

                // Check if the player has lock picks.
                var playerUI = _player?.PlayerUI;
                if (playerUI == null)
                    return false;
                var playerInventory = playerUI.xui.PlayerInventory;
                var item = ItemClass.GetItem("resourceLockPick");
                if (playerInventory.GetItemCount(item) == 0)
                {
                    playerUI.xui.CollectedItemList.AddItemStack(new ItemStack(item, 0), true);
                    GameManager.ShowTooltip(_player, Localization.Get("ttLockpickMissing"));
                    return false;
                }

                lockable.SetLocked(true);
                XUiC_PickLocking.Open(playerUI, lockable, _blockValue, _blockPos);
                return false;
            }
        }
    }
}