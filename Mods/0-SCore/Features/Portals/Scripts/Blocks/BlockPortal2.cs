using Audio;
using Platform;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;


/*
 * Property name value buffRequiresBuffName but if "" then not required.

You could have a hidden true portal name like $playernamePortalOne but displays as PortalOne

Feature request: destination blocks.  No teleporting capabilities, just used as a destination.

Give buff value equals property that gives a buff when used. Then I can specify different visual scenes while transporting

*/
public class BlockPortal2 : BlockPlayerSign
{
    private string buffCooldown = "buffTeleportCooldown";
    private int delay = 1000;
    private string location;
    private bool display = false;
    private string buffActivate = "";
    private string displayBuff = "";

    public override void Init()
    {
        // Touch the singleton early so its GameStartDone handler is registered before
        // the event fires. Block.Init() runs during block registration, well before
        // GameStartDone, guaranteeing the save file will be loaded on startup.
        _ = PortalManager.Instance;

        if (Properties.Values.ContainsKey("CooldownBuff"))
            buffCooldown = Properties.Values["CooldownBuff"];
        if (Properties.Values.ContainsKey("ActivateBuff"))
            buffActivate = Properties.Values["ActivateBuff"];
        if (Properties.Values.ContainsKey("Delay"))
            delay = StringParsers.ParseSInt32(Properties.Values["Delay"]);
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
        if (_blockValue.ischild) return;
        PortalManager.Instance.RemovePosition(_blockPos);
    }

    public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
        if (_blockValue.ischild) return;
        PortalManager.Instance.AddPosition(_blockPos);
    }

    public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
    {
        base.OnBlockAdded(world, _chunk, _blockPos, _blockValue, _addedByPlayer);
        if (!string.IsNullOrEmpty(location))
        {
            // Set text first so AddPosition reads the correct name from the tile entity.
            var tileEntitySign = world.GetTileEntity(0, _blockPos) as TileEntitySign;
            tileEntitySign?.SetText(location);
            PortalManager.Instance.AddPosition(_blockPos, location);
        }
    }

    // --- Teleportation ---

    public bool CanUseTeleport(EntityAlive player, Vector3i blockPos)
    {
        if (string.IsNullOrEmpty(buffActivate)) return true;
        if (player.Buffs.HasBuff(buffActivate)) return true;

        var msg = Localization.Get("xuiPortalDenied");
        if (string.IsNullOrEmpty(msg)) return false;

        Manager.BroadcastPlayByLocalPlayer(blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
        GameManager.ShowTooltip(player as EntityPlayerLocal, msg, string.Empty, "ui_denied", null);
        return false;
    }

    public void TeleportPlayer(EntityAlive player, Vector3i blockPos)
    {
        if (!CanUseTeleport(player, blockPos)) return;
        if (player.Buffs.HasBuff(buffCooldown)) return;
        player.Buffs.AddBuff(buffCooldown);

        // Resolve the destination parent position on the main thread before the Task delay.
        var destBase = PortalManager.Instance.GetDestination(blockPos);
        if (destBase == Vector3i.zero) return;

        // Force-load the destination chunk (true = require loaded) so the player
        // doesn't fall through unloaded terrain on arrival.
        var destObserver = GameManager.Instance.AddChunkObserver(destBase, true, 2, -1);

        // Capture the main thread SynchronizationContext so SetPosition is called safely.
        var ctx = SynchronizationContext.Current;
        Task.Delay(delay).ContinueWith(t =>
            ctx.Post(_ =>
            {
                GameManager.Instance.RemoveChunkObserver(destObserver);

                // Center the player inside the portal's horizontal footprint.
                // destBase is the parent (corner) position; for a 3x3x3 portal dim.x/2 = 1, dim.z/2 = 1.
                var spawnPos = destBase;
                var destBlock = GameManager.Instance.World.GetBlock(destBase);
                if (destBlock.Block.isMultiBlock)
                {
                    var dim = destBlock.Block.multiBlockPos.dim;
                    spawnPos = new Vector3i(destBase.x + dim.x / 2, destBase.y + 1, destBase.z + dim.z / 2);
                }
                else
                {
                    spawnPos = new Vector3i(destBase.x, destBase.y + 1, destBase.z);
                }

                player.SetPosition(spawnPos);
            }, null));
    }

    // --- Activation ---

    public override bool OnBlockActivated(string commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
    {
        if (_blockValue.ischild)
        {
            Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
            return OnBlockActivated(commandName, _world, _cIdx, parentPos, _world.GetBlock(parentPos), _player);
        }
        var tileEntitySign = _world.GetTileEntity(_cIdx, _blockPos) as TileEntitySign;
        if (tileEntitySign == null) return false;

        switch (commandName)
        {
            case "portalActivate":
                if (GameManager.Instance.IsEditMode() || !tileEntitySign.IsLocked() || tileEntitySign.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
                    TeleportPlayer(_player, _blockPos);
                return false;
            case "edit":
                if (GameManager.Instance.IsEditMode() || !tileEntitySign.IsLocked() || tileEntitySign.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
                {
                    if (string.IsNullOrEmpty(location))
                        return OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
                }
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
                return false;
            case "lock":
                tileEntitySign.SetLocked(true);
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locking");
                GameManager.ShowTooltip(_player, "containerLocked");
                return true;
            case "unlock":
                tileEntitySign.SetLocked(false);
                Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
                GameManager.ShowTooltip(_player, "containerUnlocked");
                return true;
            case "keypad":
                if (string.IsNullOrEmpty(location))
                    XUiC_KeypadWindow.Open(LocalPlayerUI.GetUIForPlayer(_player), tileEntitySign);
                return true;
            default:
                return false;
        }
    }

    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        var tileEntitySign = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntitySign;
        if (tileEntitySign == null) return new BlockActivationCommand[0];

        PlatformUserIdentifierAbs localUser = PlatformManager.InternalLocalUserIdentifier;
        PersistentPlayerData playerData = _world.GetGameManager().GetPersistentPlayerList().GetPlayerData(tileEntitySign.GetOwner());
        bool isOwner = tileEntitySign.LocalPlayerIsOwner();
        bool isACL = !isOwner && playerData?.ACL != null && playerData.ACL.Contains(localUser);

        cmds[0].enabled = true;
        cmds[1].enabled = string.IsNullOrEmpty(location);
        cmds[2].enabled = !tileEntitySign.IsLocked() && (isOwner || isACL);
        cmds[3].enabled = tileEntitySign.IsLocked() && isOwner;
        cmds[4].enabled = (!tileEntitySign.IsUserAllowed(localUser) && tileEntitySign.HasPassword() && tileEntitySign.IsLocked()) || isOwner;
        return cmds;
    }

    // --- Visuals ---

    public void ToggleAnimator(Vector3i blockPos)
    {
        // Null-safe chunk fetch — chunk may not be loaded yet
        var ebcd = GameManager.Instance.World.GetChunkFromWorldPos(blockPos)?.GetBlockEntity(blockPos);
        if (ebcd == null || ebcd.transform == null) return;

        var animator = ebcd.transform.GetComponentInChildren<Animator>();
        if (animator == null) return;

        bool isOn = PortalManager.Instance.IsLinked(blockPos);
        animator.SetBool("portalOn", isOn);
        animator.SetBool("portalOff", !isOn);
    }

    public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
    {
        if (_ebcd == null) return;
        _ebcd.bHasTransform = false;
        base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
        _ebcd.bHasTransform = true;
    }

    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        if (!display) return "";

        if (!string.IsNullOrEmpty(displayBuff) && !_entityFocusing.Buffs.HasBuff(displayBuff))
            return $"{Localization.Get("teleportto")}...";

        ToggleAnimator(_blockPos);

        if (PortalManager.Instance.IsLinked(_blockPos))
            return $"{Localization.Get("teleportto")} {PortalManager.Instance.GetDestinationName(_blockPos)}";

        return Localization.Get("portal_configure");
    }
}
