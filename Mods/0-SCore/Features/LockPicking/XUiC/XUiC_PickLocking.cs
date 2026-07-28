// Simple XUI screen to enable the lock picking to have a window-style pop up.

public class XUiC_PickLocking : XUiController {
    public static string ID = "";
    private Vector3i blockPos;
    private BlockValue currentBlock;
    private SphereLocks Lock;

    // Reference to our current locked container
    private ILockable LockedItem;

    // TEFeatureLockPickable (chests, cars, etc.) has no owner concept and doesn't implement
    // ILockable, so it's tracked separately. Its own success handling (DowngradeToUnlockedVariant +
    // lockPickSuccessEvent) runs instead of ILockable.SetLocked(false).
    private TEFeatureLockPickable LockedFeature;

    public override void Init() {
        Lock = new SphereLocks();
        ID = windowGroup.Id;
        base.Init();
    }

    public override void Update(float _dt) {
        base.Update(_dt);
        if (LockedItem == null && LockedFeature == null)
            return;

        // Check if the lock is open
        if (Lock.IsLockOpened())
        {
            if (LockedItem != null)
            {
                LockedItem.SetLocked(false);
            }
            else
            {
                LockedFeature.unlockCompletion = 1f;
                LockedFeature.DowngradeToUnlockedVariant(blockPos);
                if (!string.IsNullOrEmpty(LockedFeature.lockPickSuccessEvent))
                {
                    GameEventManager.Current.HandleAction(LockedFeature.lockPickSuccessEvent, null,
                        xui.playerUI.entityPlayer, false, blockPos, "", "", false, true, "", null, null);
                }
            }
            OnClose();
        }
    }

    // Set the container reference so we can unlock it.
    public static void Open(LocalPlayerUI playerUi, ILockable lockedItem, BlockValue blockValue, Vector3i blockPos) {
        // Configure the lock pick
        var pickLocking = playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_PickLocking>();
        pickLocking.LockedItem = lockedItem;
        pickLocking.LockedFeature = null;
        pickLocking.currentBlock = blockValue;
        pickLocking.blockPos = blockPos;
        playerUi.windowManager.Open(ID, true);
    }

    // Overload for TEFeatureLockPickable (chests, cars, etc.), which isn't ILockable.
    public static void Open(LocalPlayerUI playerUi, TEFeatureLockPickable lockedFeature, BlockValue blockValue, Vector3i blockPos) {
        var pickLocking = playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_PickLocking>();
        pickLocking.LockedItem = null;
        pickLocking.LockedFeature = lockedFeature;
        pickLocking.currentBlock = blockValue;
        pickLocking.blockPos = blockPos;
        playerUi.windowManager.Open(ID, true);
    }

    // Set the player reference and display the lock.
    public override void OnOpen() {
        EntityPlayer player = xui.playerUI.entityPlayer;
        base.OnOpen();
        Lock = new SphereLocks();

        // Pass the Player reference to the lock before we enable.
        Lock.Init(currentBlock, blockPos);
        Lock.SetPlayer(player);
        Lock.Enable();
        if (!ThreadManager.IsMainThread()) return;
        xui.playerUI.entityPlayer.PlayOneShot("open_sign");
    }

    // windowManager.Close(ID) below re-enters this method: GUIWindowManager.Close ->
    // XUiWindowGroup.OnClose -> Controller.OnClose. On that second pass LockedFeature is already
    // null, so the success handling below would run again and downgrade the block a second time -
    // for a cop car that replaced the picked-lock bonus variant with cntPoliceCar01AlarmLocked.
    private bool closing;

    public override void OnClose() {
        if (closing)
        {
            base.OnClose();
            return;
        }

        closing = true;
        try
        {
            if (Lock.IsLockOpened())
            {
                // adding trigger
                OnLootContainerPicked.onLootContainerPicked(currentBlock);

                // TEFeatureLockPickable already swapped the block itself via DowngradeToUnlockedVariant
                // in Update() above - don't also run the generic Block-level downgrade below.
                if (LockedFeature == null)
                {
                    // Block.LockpickDowngradeBlock is dead in V2 - the field still exists but nothing
                    // parses it from XML anymore, so it is always air. Only DowngradeBlock is left.
                    var blockValue = currentBlock.Block.DowngradeBlock;
                    if (!blockValue.isair)
                    {
                        blockValue = BlockPlaceholderMap.Instance.Replace(blockValue,
                            GameManager.Instance.World.GetGameRandom(), blockPos.x, blockPos.z, false);
                        blockValue.rotation = currentBlock.rotation;
                        blockValue.meta = currentBlock.meta;
                        GameManager.Instance.World.SetBlockRPC(new BlockValueRef(blockPos), blockValue, blockValue.Block.Density);
                    }
                }
            }

            Lock.Disable();
            LockedItem = null;
            LockedFeature = null;
            base.OnClose();
            xui.playerUI.windowManager.Close(ID);
            if (!ThreadManager.IsMainThread()) return;
            xui.playerUI.entityPlayer.PlayOneShot("close_sign");
        }
        finally
        {
            closing = false;
        }
    }

}