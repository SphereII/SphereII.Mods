using Audio;
using HarmonyLib;
using System;
using UnityEngine;

//OnBlockActivated(int _indexInBlockActivationCommands, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)

public class SphereII_Blocks_BlockSecureLoot
{

    private static readonly string AdvFeatureClass = "AdvancedLockpicking";
    private static readonly string Feature = "AdvancedLocks";

    [HarmonyPatch(typeof(BlockSecureLoot))]
    [HarmonyPatch("OnBlockActivated")]
    [HarmonyPatch(new Type[] { typeof(int), typeof(WorldBase), typeof(int), typeof(Vector3i), typeof(BlockValue), typeof(EntityPlayer) })]

    public class SphereII_Block_Init
    {
        public static bool Prefix(ref Block __instance, int _indexInBlockActivationCommands, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player
            , string ___lockPickItem)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            if (_blockValue.ischild)
            {
                return true;
            }
            TileEntitySecureLootContainer tileEntitySecureLootContainer = _world.GetTileEntity(_cIdx, _blockPos) as TileEntitySecureLootContainer;
            if (tileEntitySecureLootContainer == null)
            {
                return false;
            }

            if (tileEntitySecureLootContainer.IsLocked() && tileEntitySecureLootContainer.IsUserAllowed(GamePrefs.GetString(EnumGamePrefs.PlayerId)))
            {
                __instance.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
                return false;
            }
            switch (_indexInBlockActivationCommands)
            {
                case 4:
                    {
                        // Check if the player has lock picks.
                        LocalPlayerUI playerUI = (_player as EntityPlayerLocal).PlayerUI;
                        XUiM_PlayerInventory playerInventory = playerUI.xui.PlayerInventory;
                        ItemValue item = ItemClass.GetItem("resourceLockPick", false);
                        if (playerInventory.GetItemCount(item) == 0)
                        {
                            playerUI.xui.CollectedItemList.AddItemStack(new ItemStack(item, 0), true);
                            GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("ttLockpickMissing"));
                            return false;
                        }

                        tileEntitySecureLootContainer.SetLocked(true);
                        XUiC_PickLocking.Open(playerUI, tileEntitySecureLootContainer, _blockValue, _blockPos);
                        return false;

                    }
                default:
                    return true;
            }

            return true;

        }

    }
    [HarmonyPatch(typeof(BlockDoorSecure))]
    [HarmonyPatch("OnBlockActivated")]
    [HarmonyPatch(new Type[] { typeof(int), typeof(WorldBase), typeof(int), typeof(Vector3i), typeof(BlockValue), typeof(EntityPlayer) })]

    public class SphereII_Block_Init_Door
    {
        public static bool Prefix(ref Block __instance, int _indexInBlockActivationCommands, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            if (_blockValue.ischild)
            {
                return true;
            }
            TileEntitySecureDoor tileEntitySecureDoor = (TileEntitySecureDoor)_world.GetTileEntity(_cIdx, _blockPos);
            if (tileEntitySecureDoor == null)
                return true;


            if (!tileEntitySecureDoor.IsLocked() || tileEntitySecureDoor.IsUserAllowed(GamePrefs.GetString(EnumGamePrefs.PlayerId)))
            {
                __instance.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
                return false;
            }


            if (tileEntitySecureDoor.IsLocked() && tileEntitySecureDoor.IsUserAllowed(GamePrefs.GetString(EnumGamePrefs.PlayerId)))
            {
                __instance.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
                return false;
            }
            if (tileEntitySecureDoor.IsLocked())
            {
                // 1 == try to open locked door.
                if (_indexInBlockActivationCommands == 1)
                {
                    // Check if the player has lock picks.
                    LocalPlayerUI playerUI = (_player as EntityPlayerLocal).PlayerUI;
                    XUiM_PlayerInventory playerInventory = playerUI.xui.PlayerInventory;
                    ItemValue item = ItemClass.GetItem("resourceLockPick", false);
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


    [HarmonyPatch(typeof(TileEntityLootContainer))]
    [HarmonyPatch("Reset")]
    public class SphereII_TileEntityReset
    {

        public static bool IsSupposedToBeLocked(Vector3i position)
        {
            // Detect which prefab we are at.
            var prefabInstance = GameManager.Instance.GetDynamicPrefabDecorator()?.GetPrefabAtPosition(position.ToVector3());
            if (prefabInstance == null)
                return false;

            for (var i = 0; i < prefabInstance.prefab.size.x; i++)
            {
                for (var j = 0; j < prefabInstance.prefab.size.z; j++)
                {
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
                }
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