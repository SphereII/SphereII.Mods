// Simple XUI screen to enable the lock picking to have a window-style pop up.

public class XUiC_PickLocking : XUiController
{
    public static string ID = "";
    private Vector3i blockPos;
    private BlockValue currentBlock;
    private SphereLocks Lock;

    // Reference to our current locked container
    private ILockable LockedItem;

    public override void Init()
    {
        Lock = new SphereLocks();
        ID = windowGroup.ID;
        base.Init();
    }

    public override void Update(float _dt)
    {
        base.Update(_dt);
        if (LockedItem == null)
            return;

        // Check if the lock is open
        if (Lock.IsLockOpened())
        {
            LockedItem.SetLocked(false);
            OnClose();
        }
    }

    // Set the container reference so we can unlock it.
    public static void Open(LocalPlayerUI playerUi, ILockable lockedItem, BlockValue blockValue, Vector3i blockPos)
    {
        // Configure the lock pick
        playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_PickLocking>().LockedItem = lockedItem;
        playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_PickLocking>().currentBlock = blockValue;
        playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_PickLocking>().blockPos = blockPos;
        playerUi.windowManager.Open(ID, true);
    }

    // Set the player reference and display the lock.
    public override void OnOpen()
    {
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

    public override void OnClose()
    {
        if (Lock.IsLockOpened())
        {
            if (!currentBlock.Block.DowngradeBlock.isair)
            {
                var blockValue2 = currentBlock.Block.DowngradeBlock;
                blockValue2 = BlockPlaceholderMap.Instance.Replace(blockValue2, GameManager.Instance.World.GetGameRandom(), blockPos.x, blockPos.z, false);
                blockValue2.rotation = currentBlock.rotation;
                blockValue2.meta = currentBlock.meta;
                GameManager.Instance.World.SetBlockRPC(0, blockPos, blockValue2, blockValue2.Block.Density);
            }
        }
        Lock.Disable();
        LockedItem = null;
        base.OnClose();
        xui.playerUI.windowManager.Close(ID);
        if (!ThreadManager.IsMainThread()) return; 
        xui.playerUI.entityPlayer.PlayOneShot("close_sign");
    }
}