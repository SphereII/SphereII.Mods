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
                Log.Out("Command: " + _commandName);
                if (_commandName != "TEFeatureLockPickable:pick")
                    return true;

                if (_blockValue.ischild) return true;
                var tileEntity = _world.GetTileEntity(_blockPos) as TileEntityComposite;
                var lockable = tileEntity?.GetFeature<TEFeatureLockable>();

                if (lockable != null)
                {
                    if (!lockable.IsLocked() &&
                        lockable.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
                    {
                        __instance.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
                        return false;
                    }

                    // Check if the player has lock picks.
                    var item = ItemClass.GetItem("resourceLockPick");
                    var playerUI = _player?.PlayerUI;
                    if (playerUI == null)
                        return false;
                    var playerInventory = playerUI.xui.PlayerInventory;
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

                // TEFeatureLockPickable (chests, cars, etc.) has no owner concept - it doesn't
                // implement ILockable. Route it through SCore's own pick minigame too, using its
                // own configured lock-pick item; the minigame itself handles pick breaking.
                var lockPickable = tileEntity?.GetFeature<TEFeatureLockPickable>();
                if (lockPickable == null)
                    return false;
                if (!lockPickable.NeedsLockpicking())
                    return true;

                var pickItem = !string.IsNullOrEmpty(lockPickable.lockPickItem)
                    ? ItemClass.GetItem(lockPickable.lockPickItem)
                    : ItemClass.GetItem("resourceLockPick");

                var lockPickPlayerUI = _player?.PlayerUI;
                if (lockPickPlayerUI == null)
                    return false;
                var lockPickInventory = lockPickPlayerUI.xui.PlayerInventory;
                if (lockPickInventory.GetItemCount(pickItem) == 0)
                {
                    lockPickPlayerUI.xui.CollectedItemList.AddItemStack(new ItemStack(pickItem, 0), true);
                    GameManager.ShowTooltip(_player, Localization.Get("ttLockpickMissing"));
                    return false;
                }

                XUiC_PickLocking.Open(lockPickPlayerUI, lockPickable, _blockValue, _blockPos);
                return false;
            }
        }
    }
}