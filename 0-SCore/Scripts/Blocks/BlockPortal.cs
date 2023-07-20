using Audio;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BlockPortal: BlockPoweredPortal
{

}
/*
 * Property name value buffRequiresBuffName but if "" then not required.

You could have a hidden true portal name like $playernamePortalOne but displays as PortalOne

Feature request: destination blocks.  No teleporting capabilities, just used as a destination.

Give buff value equals property that gives a buff when used. Then I can specify different visual scenes whole transporting 

*/
public class BlockPortal2 : BlockPlayerSign
{

    private string buffCooldown = "buffTeleportCooldown";
    private int delay = 1000;
    private string location;
    private bool display = false;

    private string displayBuff = "";
    public override void Init()
    {
        if (Properties.Values.ContainsKey("CooldownBuff"))
            buffCooldown = Properties.Values["CooldownBuff"];

        if (Properties.Values.ContainsKey("Delay"))
        {
            var delayString = Properties.Values["Delay"];
            delay = StringParsers.ParseSInt32(delayString);
        }

        if (Properties.Values.ContainsKey("Location"))
            location = Properties.Values["Location"];

        if (Properties.Values.ContainsKey("Display"))
             display = StringParsers.ParseBool(Properties.Values["Display"]);

        if (Properties.Values.ContainsKey("DisplayBuff"))
            displayBuff = Properties.Values["DisplayBuff"];


        base.Init();
    }

    private BlockActivationCommand[] cmds = new BlockActivationCommand[]
{
       new BlockActivationCommand("portalActivate", "pen", true, true),

        new BlockActivationCommand("edit", "pen", false, false),
        new BlockActivationCommand("lock", "lock", false, false),
        new BlockActivationCommand("unlock", "unlock", false, false),
        new BlockActivationCommand("keypad", "keypad", false, false)

};

    public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
        PortalManager.Instance.RemovePosition(_blockPos);
    }
    public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
        PortalManager.Instance.AddPosition(_blockPos);
    }

    public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
        PortalManager.Instance.AddPosition(_blockPos);
        if (string.IsNullOrEmpty(location)) return;

        TileEntitySign tileEntitySign = world.GetTileEntity(0, _blockPos) as TileEntitySign;
        if (tileEntitySign == null) return;

        tileEntitySign.SetText(location);
    }

 
    //public override bool OnEntityCollidedWithBlock(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, Entity _entity)
    //{
    //    TeleportPlayer(_entity as EntityAlive, _blockPos);
    //    return base.OnEntityCollidedWithBlock(_world, _clrIdx, _blockPos, _blockValue, _entity);
    //}
    //public override void OnEntityWalking(WorldBase _world, int _x, int _y, int _z, BlockValue _blockValue, Entity entity)
    //{
    //    TeleportPlayer(entity as EntityAlive, new Vector3i(_x, _y, _z));
    //    base.OnEntityWalking(_world, _x, _y, _z, _blockValue, entity);
    //}
    public void TeleportPlayer(EntityAlive _player, Vector3i _blockPos)
    {
        if (_player.Buffs.HasBuff(buffCooldown)) return;
        _player.Buffs.AddBuff(buffCooldown);

        Task task = Task.Delay(delay)
             .ContinueWith(t => Teleport(_player, _blockPos));

    }
    
    private void Teleport(EntityAlive _player, Vector3i _blockPos)
    {
        var destination = PortalManager.Instance.GetDestination(_blockPos);
        if (destination != Vector3i.zero)
            _player.SetPosition(destination + Vector3i.up);
    }

    public override bool OnBlockActivated(string commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        if (_blockValue.ischild)
        {
            Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
            BlockValue block = _world.GetBlock(parentPos);
            return this.OnBlockActivated(commandName, _world, _cIdx, parentPos, block, _player);
        }
        TileEntitySign tileEntitySign = _world.GetTileEntity(_cIdx, _blockPos) as TileEntitySign;
        if (tileEntitySign == null)
        {
            return false;
        }
        switch (commandName)
        {
            case "portalActivate":
                if (GameManager.Instance.IsEditMode() || !tileEntitySign.IsLocked() || tileEntitySign.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
                {
                    TeleportPlayer(_player, _blockPos);
                }
                return false;
            case "edit":
                if (GameManager.Instance.IsEditMode() || !tileEntitySign.IsLocked() || tileEntitySign.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
                {
                    if (string.IsNullOrEmpty(location))
                        return this.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
                }
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
                if ( string.IsNullOrEmpty(location))
                    XUiC_KeypadWindow.Open(LocalPlayerUI.GetUIForPlayer(_player as EntityPlayerLocal), tileEntitySign);
                return true;
            default:
                return false;
        }
    }
    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        TileEntitySign tileEntitySign = (TileEntitySign)_world.GetTileEntity(_clrIdx, _blockPos);
        if (tileEntitySign == null)
        {
            return new BlockActivationCommand[0];
        }
        PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
        PersistentPlayerData playerData = _world.GetGameManager().GetPersistentPlayerList().GetPlayerData(tileEntitySign.GetOwner());
        bool flag = tileEntitySign.LocalPlayerIsOwner();
        bool flag2 = !tileEntitySign.LocalPlayerIsOwner() && (playerData != null && playerData.ACL != null) && playerData.ACL.Contains(internalLocalUserIdentifier);
        this.cmds[0].enabled = true;
        this.cmds[1].enabled = string.IsNullOrEmpty(location);
        this.cmds[2].enabled = (!tileEntitySign.IsLocked() && (flag || flag2));
        this.cmds[3].enabled = (tileEntitySign.IsLocked() && flag);
        this.cmds[4].enabled = ((!tileEntitySign.IsUserAllowed(internalLocalUserIdentifier) && tileEntitySign.HasPassword() && tileEntitySign.IsLocked()) || flag);
        return this.cmds;
    }

    public void ToggleAnimator(Vector3i blockPos)
    {
        var _ebcd = GameManager.Instance.World.GetChunkFromWorldPos(blockPos).GetBlockEntity(blockPos);
        if (_ebcd == null || _ebcd.transform == null)
            return;

        var isOn = false;
        var animator = _ebcd.transform.GetComponentInChildren<Animator>();
        if (animator == null) return;
        if (PortalManager.Instance.IsLinked(blockPos))
            isOn = true;
        else
            isOn = false;

        animator.SetBool("portalOn", isOn);
        animator.SetBool("portalOff", !isOn);

    }
    public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
    {
        if (_ebcd == null)
            return;

        // Hide the sign, so its not visible. Without this, it errors out.
        _ebcd.bHasTransform = false;

        base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);

        //TileEntitySign tileEntitySign = (TileEntitySign)_world.GetTileEntity(_cIdx, _blockPos);
        //if (tileEntitySign != null )
        //{
        //    var text = tileEntitySign.GetText();
        //    PortalManager.Instance.AddPosition(_blockPos, text);
        //}
        
            // Re-show the transform. This won't have a visual effect, but fixes when you pick up the block, the outline of the block persists.
            _ebcd.bHasTransform = true;
    }
    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        if (display == false) return "";

        if ( !string.IsNullOrEmpty(displayBuff))
        {
            if (_entityFocusing.Buffs.HasBuff(displayBuff) == false) return $"{Localization.Get("teleportto")}...";
        }
        var text = "";

        PortalManager.Instance.AddPosition(_blockPos);
        ToggleAnimator(_blockPos);

        if ( PortalManager.Instance.IsLinked(_blockPos))
        {
            var destination = PortalManager.Instance.GetDestinationName(_blockPos);
            return $"{Localization.Get("teleportto")} {destination}";
        }
        return $"{Localization.Get("portal_configure")} {text}";
    }
}