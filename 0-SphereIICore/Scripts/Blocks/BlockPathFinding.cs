using Audio;
using UnityEngine;

class BlockPathFinding : BlockPlayerSign
{
    private readonly BlockActivationCommand[] cmds = new BlockActivationCommand[]
{
        new BlockActivationCommand("edit", "pen", false),
        new BlockActivationCommand("lock", "lock", false),
        new BlockActivationCommand("unlock", "unlock", false),
        new BlockActivationCommand("keypad", "keypad", false),
        new BlockActivationCommand("take", "hand", false)
};

    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        TileEntitySign tileEntitySign = (TileEntitySign)_world.GetTileEntity(_clrIdx, _blockPos);
        if (tileEntitySign == null)
        {
            return new BlockActivationCommand[0];
        }
        string @string = GamePrefs.GetString(EnumGamePrefs.PlayerId);
        PersistentPlayerData playerData = _world.GetGameManager().GetPersistentPlayerList().GetPlayerData(tileEntitySign.GetOwner());

        bool flag = !tileEntitySign.IsOwner(@string) && (playerData != null && playerData.ACL != null) && playerData.ACL.Contains(@string);
        cmds[0].enabled = true;
        cmds[1].enabled = (!tileEntitySign.IsLocked() && (tileEntitySign.IsOwner(@string) || flag));
        cmds[2].enabled = (tileEntitySign.IsLocked() && tileEntitySign.IsOwner(@string));
        cmds[3].enabled = ((!tileEntitySign.IsUserAllowed(@string) && tileEntitySign.HasPassword() && tileEntitySign.IsLocked()) || tileEntitySign.IsOwner(@string));
        cmds[4].enabled = ((!tileEntitySign.IsUserAllowed(@string) && tileEntitySign.HasPassword() && tileEntitySign.IsLocked()) || tileEntitySign.IsOwner(@string));

        if (_world.IsEditor() || _entityFocusing.IsGodMode.Value)
        {
            return cmds;
        }

        if ( tileEntitySign.IsOwner(@string) || tileEntitySign.IsUserAllowed(@string))
            return cmds;

        return new BlockActivationCommand[0];
    }


    public override bool OnBlockActivated(int _indexInBlockActivationCommands, WorldBase _world, int _cIdx,
        Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        if (_blockValue.ischild)
        {
            Vector3i parentPos = Block.list[_blockValue.type].multiBlockPos.GetParentPos(_blockPos, _blockValue);
            BlockValue block = _world.GetBlock(parentPos);
            return OnBlockActivated(_indexInBlockActivationCommands, _world, _cIdx, parentPos, block, _player);
        }
        TileEntitySign tileEntitySign = _world.GetTileEntity(_cIdx, _blockPos) as TileEntitySign;
        if (tileEntitySign == null)
        {
            return false;
        }
        switch (_indexInBlockActivationCommands)
        {
            case 0:
                if (GameManager.Instance.IsEditMode() || !tileEntitySign.IsLocked() || tileEntitySign.IsUserAllowed(GamePrefs.GetString(EnumGamePrefs.PlayerId)))
                {
                    return OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
                }
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
                return false;
            case 1:
                tileEntitySign.SetLocked(true);
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locking");
                GameManager.ShowTooltip(_player as EntityPlayerLocal, "containerLocked");
                return true;
            case 2:
                tileEntitySign.SetLocked(false);
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
                GameManager.ShowTooltip(_player as EntityPlayerLocal, "containerUnlocked");
                return true;
            case 3:
                XUiC_KeypadWindow.Open(LocalPlayerUI.GetUIForPlayer(_player as EntityPlayerLocal), tileEntitySign);
                return true;
            case 4:
                LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_player as EntityPlayerLocal);
                ItemStack itemStack = new ItemStack(_blockValue.ToItemValue(), 1);
                if (!uiforPlayer.xui.PlayerInventory.AddItem(itemStack))
                {
                    uiforPlayer.xui.PlayerInventory.DropItem(itemStack);
                }
                _world.SetBlockRPC(_cIdx, _blockPos, BlockValue.Air);

                return true;
            default:
                return false;
        }
    }


    public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
    {
        if (_ebcd == null)
            return;

        // Hide the sign, so its not visible. Without this, it errors out.
        _ebcd.bHasTransform = false;
        base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);

        // Re-show the transform. This won't have a visual effect, but fixes when you pick up the block, the outline of the block persists.
        _ebcd.bHasTransform = true;

    }

}

