using DMT;
using Harmony;
using System;
using System.IO;
using UnityEngine;

public class SphereII_TileEntities
{
    [HarmonyPatch(typeof(TileEntity))]
    [HarmonyPatch("Instantiate")]
    public class SphereII_TileEntity_Instantiate
    {

        static TileEntity Postfix(TileEntity __result, TileEntityType type, Chunk _chunk)
        {
            if (__result == null)
            {
                switch (type)
                {
                    case TileEntityType.PoweredWorkstationSDX:
                        return new TileEntityPoweredWorkstationSDX(_chunk);
                }
            }
            return __result;
        }
    }

    [HarmonyPatch(typeof(GameManager))]
    [HarmonyPatch("TELockServer")]
    public class SphereII_GameManager_TELockServer
    {
        static void Postfix(GameManager __instance, int _clrIdx, Vector3i _blockPos, int _lootEntityId, int _entityIdThatOpenedIt)
        {
            if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
                return;

            LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(__instance.World.GetEntity(_entityIdThatOpenedIt) as EntityPlayerLocal);
            TileEntity tileEntity;
            if (_lootEntityId == -1)
            {
                tileEntity = __instance.World.GetTileEntity(_clrIdx, _blockPos);
            }
            else
            {
                tileEntity = __instance.World.GetTileEntity(_lootEntityId);
            }
            if (tileEntity == null)
            {
                return;
            }

            if (tileEntity is TileEntityPowered)
            {
                if (uiforPlayer != null)
                {
                    BlockValue block = GameManager.Instance.World.GetBlock(tileEntity.ToWorldPos());
                    string blockName = Block.list[block.type].GetBlockName();
                    WorkstationData workstationData = CraftingManager.GetWorkstationData(blockName);
                    if (workstationData != null)
                    {
                        string text = (workstationData.WorkstationWindow != "") ? workstationData.WorkstationWindow : string.Format("workstation_{0}", blockName);
                        if (uiforPlayer.windowManager.Contains(text))
                        {
                            ((XUiC_WorkstationWindowGroup)((XUiWindowGroup)uiforPlayer.windowManager.GetWindow(text)).Controller).SetTileEntity(tileEntity as TileEntityWorkstation);
                            uiforPlayer.windowManager.Open(text, true, false, true);
                            return;
                        }
                    }
                }
            }
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageTELock>().Setup(NetPackageTELock.TELockType.AccessClient, _clrIdx, _blockPos, _lootEntityId, _entityIdThatOpenedIt), true, -1, -1, -1, -1);
        }
    }
}
