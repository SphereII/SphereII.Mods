using HarmonyLib;
using Platform;

//OnBlockActivated(int _indexInBlockActivationCommands, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)

namespace Features.LockPicking
{
    public abstract class BlockDoorSecurePatch
    {
        private static readonly string AdvFeatureClass = "AdvancedLockpicking";
        private static readonly string Feature = "AdvancedLocks";

    
        [HarmonyPatch(typeof(BlockDoorSecure))]
        [HarmonyPatch("OnBlockActivated")]
        [HarmonyPatch(new[] { typeof(string), typeof(WorldBase), typeof(int), typeof(Vector3i), typeof(BlockValue), typeof(EntityPlayer) })]
        public class BlockDoorSecureOnBlockActivated
        {
            public static bool Prefix(ref Block __instance, string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, global::EntityAlive _player)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                // If they have a controller, skip the mini game
                if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
                    return true;

                if (_blockValue.ischild) return true;
                var tileEntitySecureDoor = (TileEntitySecureDoor)_world.GetTileEntity(_cIdx, _blockPos);
                if (tileEntitySecureDoor == null)
                    return true;

                if (!tileEntitySecureDoor.IsLocked() || tileEntitySecureDoor.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
                {
                    return true;
                    // __instance.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
                    // return false;
                }

                var pickable = true;
                // If it has a pickable property and its set to false, don't let them pick it.
                if (__instance.Properties.Contains("Pickable"))
                    StringParsers.TryParseBool(__instance.Properties.Values["Pickable"], out pickable);

                if (pickable == false) return true;

                // Door has an owner, don't allow picking.
                if (tileEntitySecureDoor.IsLocked() && tileEntitySecureDoor.GetOwner() != null)
                    return true;

                if (!tileEntitySecureDoor.IsLocked()) return true;
                // 1 == try to open locked door.
                if (_commandName != "open") return true;
                
                // Check if the player has lock picks.
                var playerUI = (_player as EntityPlayerLocal)?.PlayerUI;
                if (playerUI == null)
                    return true;
                var playerInventory = playerUI.xui.PlayerInventory;
                var item = ItemClass.GetItem("resourceLockPick");
                if (playerInventory.GetItemCount(item) == 0)
                {
                    playerUI.xui.CollectedItemList.AddItemStack(new ItemStack(item, 0), true);
                    GameManager.ShowTooltip((EntityPlayerLocal) _player, Localization.Get("ttLockpickMissing"));
                    return false;
                }

                tileEntitySecureDoor.SetLocked(true);
                XUiC_PickLocking.Open(playerUI, tileEntitySecureDoor, _blockValue, _blockPos);
                return false;
            }
        }


   
    }
}