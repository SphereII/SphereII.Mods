using Audio;
using Platform;
using UnityEngine;

internal class BlockPathFinding : BlockPlayerSign
{
    private readonly BlockActivationCommand[] cmds =
    {
        new BlockActivationCommand("edit", "pen", false),
        new BlockActivationCommand("lock", "lock", false),
        new BlockActivationCommand("unlock", "unlock", false),
        new BlockActivationCommand("keypad", "keypad", false),
        new BlockActivationCommand("take", "hand", false)
    };

    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        var tileEntitySign = (TileEntitySign)_world.GetTileEntity(_clrIdx, _blockPos);
        if (tileEntitySign == null) return new BlockActivationCommand[0];

        var internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
        var playerData = _world.GetGameManager().GetPersistentPlayerList().GetPlayerData(tileEntitySign.GetOwner());
        var flag = tileEntitySign.LocalPlayerIsOwner();
        var flag2 = !tileEntitySign.LocalPlayerIsOwner() && (playerData != null && playerData.ACL != null) && playerData.ACL.Contains(internalLocalUserIdentifier);
        this.cmds[0].enabled = (tileEntitySign.IsUserAllowed(internalLocalUserIdentifier) || flag); 
        this.cmds[1].enabled = (!tileEntitySign.IsLocked() && (flag || flag2));
        this.cmds[2].enabled = (tileEntitySign.IsLocked() && flag);
        this.cmds[3].enabled = ((!tileEntitySign.IsUserAllowed(internalLocalUserIdentifier) && tileEntitySign.HasPassword() && tileEntitySign.IsLocked()) || flag);
        this.cmds[4].enabled = ((!tileEntitySign.IsUserAllowed(internalLocalUserIdentifier) && tileEntitySign.HasPassword() && tileEntitySign.IsLocked()) || flag);

        return cmds;
    }


    public override bool OnBlockActivated(string commandName, WorldBase _world, int _cIdx,
        Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        if (_blockValue.ischild)
        {
            var parentPos = list[_blockValue.type].multiBlockPos.GetParentPos(_blockPos, _blockValue);
            var block = _world.GetBlock(parentPos);
            return OnBlockActivated(commandName, _world, _cIdx, parentPos, block, _player);
        }

        var tileEntitySign = _world.GetTileEntity(_cIdx, _blockPos) as TileEntitySign;
        if (tileEntitySign == null) return false;

        var internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
        switch (commandName)
        {
            case "edit":
                if (GameManager.Instance.IsEditMode() || !tileEntitySign.IsLocked() || tileEntitySign.IsUserAllowed(internalLocalUserIdentifier))
                    return OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
                return false;
            case "lock":
                tileEntitySign.SetLocked(true);
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locking");
                GameManager.ShowTooltip(_player as EntityPlayerLocal, "containerLocked");
                return true;
            case "unlock":
                tileEntitySign.SetLocked(false);
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
                GameManager.ShowTooltip(_player as EntityPlayerLocal, "containerUnlocked");
                return true;
            case "keypad":
                XUiC_KeypadWindow.Open(LocalPlayerUI.GetUIForPlayer(_player as EntityPlayerLocal), tileEntitySign);
                return true;
            case "take":
                var uiforPlayer = LocalPlayerUI.GetUIForPlayer(_player as EntityPlayerLocal);
                var itemStack = new ItemStack(_blockValue.ToItemValue(), 1);
                if (!uiforPlayer.xui.PlayerInventory.AddItem(itemStack)) uiforPlayer.xui.PlayerInventory.DropItem(itemStack);
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