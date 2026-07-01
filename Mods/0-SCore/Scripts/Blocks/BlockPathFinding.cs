using Audio;
using Platform;
using UnityEngine;

internal class BlockPathFinding : BlockSign {
    private readonly BlockActivationCommand[] cmds = {
        new BlockActivationCommand("edit", "pen", false),
        new BlockActivationCommand("lock", "lock", false),
        new BlockActivationCommand("unlock", "unlock", false),
        new BlockActivationCommand("keypad", "keypad", false),
        new BlockActivationCommand("take", "hand", false)
    };

    private static TEFeatureLockable GetLockable(WorldBase _world, Vector3i _blockPos)
    {
        return (_world.GetTileEntity(_blockPos) as TileEntityComposite)?.GetFeature<TEFeatureLockable>();
    }

    // Do a pre-check on permissions to remove the ghost "Press <e> to interact" when there's no options.
    public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos,
        EntityAlive _entityFocusing) {
        var lockable = GetLockable(_world, _blockPos);
        if (lockable == null) return false;

        if (_world.IsEditor()) return true;

        var internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
        var isOwner = lockable.LocalPlayerIsOwner();
        return lockable.IsUserAllowed(internalLocalUserIdentifier) || isOwner;
    }

    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing) {
        var lockable = GetLockable(_world, _blockPos);
        if (lockable == null) return new BlockActivationCommand[0];

        var internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
        var isOwner = lockable.LocalPlayerIsOwner();

        cmds[0].enabled = _world.IsEditor() || lockable.IsUserAllowed(internalLocalUserIdentifier) || isOwner;
        cmds[1].enabled = !lockable.IsLocked() && isOwner;
        cmds[2].enabled = lockable.IsLocked() && isOwner;
        cmds[3].enabled = lockable.IsUserAllowed(internalLocalUserIdentifier) || isOwner;
        cmds[4].enabled = lockable.IsUserAllowed(internalLocalUserIdentifier) || isOwner;

        return cmds;
    }


    public override bool OnBlockActivated(string commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player) {
        if (_blockValue.ischild)
        {
            var parentPos = list[_blockValue.type].multiBlockPos.GetParentPos(_blockPos, _blockValue);
            var block = _world.GetBlock(parentPos);
            return OnBlockActivated(commandName, _world, parentPos, block, _player);
        }

        var lockable = GetLockable(_world, _blockPos);
        if (lockable == null) return false;

        var internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
        switch (commandName)
        {
            case "edit":
                if (GameManager.Instance.IsEditMode() || !lockable.IsLocked() ||
                    lockable.IsUserAllowed(internalLocalUserIdentifier))
                    return OnBlockActivated(_world, _blockPos, _blockValue, _player);
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
                return false;
            case "lock":
                lockable.SetLocked(true);
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locking");
                GameManager.ShowTooltip(_player as EntityPlayerLocal, "containerLocked");
                return true;
            case "unlock":
                lockable.SetLocked(false);
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
                GameManager.ShowTooltip(_player as EntityPlayerLocal, "containerUnlocked");
                return true;
            case "keypad":
                XUiC_KeypadWindow.Open(LocalPlayerUI.GetUIForPlayer(_player as EntityPlayerLocal), lockable);
                return true;
            case "take":
                var uiforPlayer = LocalPlayerUI.GetUIForPlayer(_player as EntityPlayerLocal);
                var itemStack = new ItemStack(_blockValue.ToItemValue(), 1);
                if (!uiforPlayer.xui.PlayerInventory.AddItem(itemStack))
                    uiforPlayer.xui.PlayerInventory.DropItem(itemStack);
                _world.SetBlockRPC(_blockPos, BlockValue.Air);

                return true;
            default:
                return false;
        }
    }


    private void UpdateVisible(BlockEntityData _ebcd) {
        _ebcd.transform.gameObject.SetActive(false);
    }

    public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd) {
        if (_ebcd == null)
            return;
        base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _ebcd);
        UpdateVisible(_ebcd);
    }
}
