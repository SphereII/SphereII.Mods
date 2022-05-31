using HarmonyLib;
using Platform;

//OnBlockActivated(int _indexInBlockActivationCommands, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)

namespace Harmony.Blocks
{
    public class SCoreBlocksBlockSecureLoot
    {
        private static readonly string AdvFeatureClass = "AdvancedLockpicking";
        private static readonly string Feature = "AdvancedLocks";

        [HarmonyPatch(typeof(BlockSecureLoot))]
        [HarmonyPatch("OnBlockActivated")]
        [HarmonyPatch(new[] { typeof(int), typeof(WorldBase), typeof(int), typeof(Vector3i), typeof(BlockValue), typeof(EntityPlayer) })]
        public class Init
        {
            public static bool Prefix(ref Block __instance, int _indexInBlockActivationCommands, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, global::EntityAlive _player
                , string ___lockPickItem, BlockActivationCommand[] ___cmds)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                var command = ___cmds[_indexInBlockActivationCommands];
                if (command.text != "pick")
                    return true;


                if (_blockValue.ischild) return true;
                if (!(_world.GetTileEntity(_cIdx, _blockPos) is TileEntitySecureLootContainer tileEntitySecureLootContainer)) return false;

                if (!tileEntitySecureLootContainer.IsLocked() && tileEntitySecureLootContainer.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
                {
                    __instance.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
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
                    GameManager.ShowTooltip((EntityPlayerLocal)_player, Localization.Get("ttLockpickMissing"));
                    return false;
                }

                tileEntitySecureLootContainer.SetLocked(true);
                XUiC_PickLocking.Open(playerUI, tileEntitySecureLootContainer, _blockValue, _blockPos);
                return false;

            }
        }

        [HarmonyPatch(typeof(BlockDoorSecure))]
        [HarmonyPatch("OnBlockActivated")]
        [HarmonyPatch(new[] { typeof(int), typeof(WorldBase), typeof(int), typeof(Vector3i), typeof(BlockValue), typeof(EntityPlayer) })]
        public class InitDoor
        {
            public static bool Prefix(ref Block __instance, int _indexInBlockActivationCommands, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, global::EntityAlive _player)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                if (_blockValue.ischild) return true;
                var tileEntitySecureDoor = (TileEntitySecureDoor)_world.GetTileEntity(_cIdx, _blockPos);
                if (tileEntitySecureDoor == null)
                    return true;

                if (!tileEntitySecureDoor.IsLocked() || tileEntitySecureDoor.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
                    return true;

                if (tileEntitySecureDoor.IsLocked() && tileEntitySecureDoor.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
                    return true;


                var pickable = true;
                // If it has a pickable property and its set to false, don't let them pick it.
                if (__instance.Properties.Contains("Pickable"))
                    StringParsers.TryParseBool(__instance.Properties.Values["Pickable"], out pickable);

                if (pickable == false) return true;

                // Door has an owner, don't allow picking.
                if (tileEntitySecureDoor.IsLocked() && tileEntitySecureDoor.GetOwner() != null)
                    return true;

                if (tileEntitySecureDoor.IsLocked())
                {
                    // 1 == try to open locked door.
                    if (_indexInBlockActivationCommands == 1)
                    {
                        // Check if the player has lock picks.
                        var playerUI = (_player as EntityPlayerLocal)?.PlayerUI;
                        if (playerUI == null)
                            return true;
                        var playerInventory = playerUI.xui.PlayerInventory;
                        var item = ItemClass.GetItem("resourceLockPick");
                        if (playerInventory.GetItemCount(item) == 0)
                        {
                            playerUI.xui.CollectedItemList.AddItemStack(new ItemStack(item, 0), true);
                            GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("ttLockpickMissing"));
                            return false;
                        }

                        tileEntitySecureDoor.SetLocked(true);
                        XUiC_PickLocking.Open(playerUI, tileEntitySecureDoor, _blockValue, _blockPos);
                        return false;
                    }
                }
                return true;
            }
        }


        // This resets for the quest activations
        [HarmonyPatch(typeof(TileEntityLootContainer))]
        [HarmonyPatch("Reset")]
        public class SCoreTileEntityReset
        {
            private static bool IsSupposedToBeLocked(Vector3i position)
            {
                // Detect which prefab we are at.
                var prefabInstance = GameManager.Instance.GetDynamicPrefabDecorator()?.GetPrefabAtPosition(position.ToVector3());
                if (prefabInstance == null)
                    return false;

                for (var i = 0; i < prefabInstance.prefab.size.x; i++)
                    for (var j = 0; j < prefabInstance.prefab.size.z; j++)
                        for (var k = 0; k < prefabInstance.prefab.size.y; k++)
                        {
                            // Find the world position of this block then check if it matches our current position.
                            var num = i + prefabInstance.boundingBoxPosition.x;
                            var num2 = j + prefabInstance.boundingBoxPosition.z;
                            var num7 = World.toBlockY(k + prefabInstance.boundingBoxPosition.y);
                            var localPosition = new Vector3i(num, num7, num2);
                            if (localPosition != position)
                                continue;

                            // Grab the blockValue from the original prefab reference, then use its meta value.
                            var blockValue = prefabInstance.prefab.GetBlock(i, k, j);
                            if (blockValue.ischild)
                                continue;

                            return (blockValue.meta & 4) > 0;
                        }

                return false;
            }

            public static bool Prefix(TileEntityLootContainer __instance)
            {
                if (__instance.bPlayerStorage || __instance.bPlayerBackpack)
                    return true;

                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, "QuestFullReset"))
                    return true;

                switch (__instance)
                {
                    case TileEntitySecure tileEntity when tileEntity.IsLocked():
                        return true;
                    case TileEntitySecure tileEntity:
                        {
                            if (IsSupposedToBeLocked(__instance.ToWorldPos())) // Check the meta of the original blockvalue.
                                tileEntity.SetLocked(true);
                            return true;
                        }
                    case TileEntitySecureLootContainer secureLootContainer when secureLootContainer.IsLocked():
                        return true;
                    case TileEntitySecureLootContainer secureLootContainer: // All non-player secure containers are locked by default.
                        {
                            secureLootContainer.SetLocked(true);
                            return true;
                        }
                    default:
                        return true;
                }
            }
        }
    }
}