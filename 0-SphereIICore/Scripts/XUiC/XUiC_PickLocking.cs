using Lockpicking;
using System.Collections;
using UnityEngine;


// Simple XUI screen to enable the lock picking to have a window-style pop up.
public class XUiC_PickLocking : XUiController
{
    public static string ID = "";
    SphereII_Locks Lock;

    // Reference to our current locked container
    ILockable LockedItem;
    BlockValue currentBlock;
    Vector3i blockPos;

    public override void Init()
    {
        Lock = new SphereII_Locks();
        XUiC_PickLocking.ID = windowGroup.ID;
        base.Init();
        //Lock.Init();
    }

    public override void Update(float _dt)
    {
        base.Update(_dt);
        if (this.LockedItem == null)
            return;

        // Check if the lock is open
        if (Lock.IsLockOpened())
        {
                this.LockedItem.SetLocked(false);
                OnClose();
        }
    }

    // Set the container reference so we can unlock it.
    public static void Open(LocalPlayerUI _playerUi, ILockable _lockedItem, BlockValue _blockValue, Vector3i _blockPos)
    {
        // Configure the lock pick
        _playerUi.xui.FindWindowGroupByName(XUiC_PickLocking.ID).GetChildByType<XUiC_PickLocking>().LockedItem = _lockedItem;
        _playerUi.xui.FindWindowGroupByName(XUiC_PickLocking.ID).GetChildByType<XUiC_PickLocking>().currentBlock = _blockValue;
        _playerUi.xui.FindWindowGroupByName(XUiC_PickLocking.ID).GetChildByType<XUiC_PickLocking>().blockPos = _blockPos;
        _playerUi.windowManager.Open(XUiC_PickLocking.ID, true, false, true);
    }

    // Set the player reference and display the lock.
    public override void OnOpen()
    {
        EntityPlayer player = base.xui.playerUI.entityPlayer;
        base.OnOpen();
        Lock = new SphereII_Locks();

        // Pass the Player reference to the lock before we enable.
        Lock.Init(currentBlock, blockPos);
        Lock.SetPlayer(player);
        Lock.Enable();


        base.xui.playerUI.entityPlayer.PlayOneShot("open_sign", false);
    }

    public override void OnClose()
    {
        if (Lock.IsLockOpened() )
        {
            Block.list[currentBlock.type].OnBlockActivated(GameManager.Instance.World, 0, blockPos, currentBlock, base.xui.playerUI.entityPlayer as EntityAlive);
        }
        Lock.Disable();
        this.LockedItem = null;
        base.OnClose();
        base.xui.playerUI.windowManager.Close(XUiC_PickLocking.ID);
        base.xui.playerUI.entityPlayer.PlayOneShot("close_sign", false);

    }
}
