using HarmonyLib;
using Platform;

//OnBlockActivated(int _indexInBlockActivationCommands, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)

namespace Features.LockPicking
{
    public abstract class BlockDoorSecurePatch
    {
        private static readonly string AdvFeatureClass = "AdvancedLockpicking";
        private static readonly string Feature = "AdvancedLocks";

    
        [HarmonyPatch(typeof(BlockCompositeTileEntity))]
        [HarmonyPatch("OnBlockActivated")]
        [HarmonyPatch(new[] { typeof(string), typeof(WorldBase),typeof(Vector3i), typeof(BlockValue), typeof(EntityPlayerLocal) })]
        public class BlockDoorSecureOnBlockActivated
        {
            public static bool Prefix(ref Block __instance, string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                if (!LockPickingUtils.CheckForMiniGame(_player))
                {
                    return true;
                }

                if (_blockValue.ischild) return true;
                var lockable = (_world.GetTileEntity(_blockPos) as TileEntityComposite)?.GetFeature<TEFeatureLockable>();
                if (lockable == null)
                    return true;

                if (!lockable.IsLocked() || lockable.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
                {
                    return true;
                }

                var pickable = true;
                // If it has a pickable property and its set to false, don't let them pick it.
                if (__instance.Properties.Contains("Pickable"))
                    StringParsers.TryParseBool(__instance.Properties.Values["Pickable"], out pickable);

                if (pickable == false) return true;

                // Door has an owner, don't allow picking.
                if (lockable.IsLocked() && lockable.GetOwner() != null)
                    return true;

                if (!lockable.IsLocked()) return true;
                // 1 == try to open locked door.
                // Newer game versions prefix the activation command with the feature name (e.g. "TEFeatureDoor:open").
                if (_commandName != "TEFeatureDoor:open") return true;


                // Check if the player has lock picks.
                var playerUI = _player?.PlayerUI;
                if (playerUI == null)
                    return true;
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