using Audio;
using Platform;
using System;
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

// The sign-based portal: its name is the sign text, which PortalManager reads to link portal pairs.
//
// This used to extend BlockSign, which the game abandoned in 3.0 - it creates no tile entity, so
// GetTileEntity(...) as TileEntityComposite was always null here. OnBlockActivated returned false
// before reaching any command, and the SetText(location) in OnBlockAdded silently did nothing, so
// portals registered with PortalManager under an empty name and never linked.
//
// Lock/unlock/keypad and the sign editor now come from the TEFeatureLockable and TEFeatureSignable
// features declared in PortalBlocks.xml, which own their own permission checks. Only the
// portalActivate command is SCore's.
public class BlockPortal2 : BlockCompositeTileEntity
{
    private const string PortalActivateCommand = "portalActivate";
    private const string EditCommand = "edit";

    private string buffCooldown = "buffTeleportCooldown";
    private int delay = 1000;
    private string location;
    private bool display = false;
    private string buffActivate = "";
    private string displayBuff = "";

    private readonly BlockActivationCommand portalActivateCmd =
        new BlockActivationCommand(PortalActivateCommand, "pen", true, true);
    private BlockActivationCommand[] portalCmds;

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

    private static TEFeatureLockable GetLockable(WorldBase _world, Vector3i _blockPos)
    {
        return (_world.GetTileEntity(_blockPos) as TileEntityComposite)?.GetFeature<TEFeatureLockable>();
    }

    // A locked portal still teleports for the owner and anyone on its ACL.
    private static bool IsUnlockedFor(TEFeatureLockable _lockable)
    {
        if (GameManager.Instance.IsEditMode()) return true;
        if (_lockable == null || !_lockable.IsLocked()) return true;
        return _lockable.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier);
    }

    public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
        if (_blockValue.ischild) return;
        PortalManager.Instance.RemovePosition(_blockPos);
    }

    public override void OnBlockLoaded(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockLoaded(_world, _blockPos, _blockValue);
        if (_blockValue.ischild) return;
        PortalManager.Instance.AddPosition(_blockPos);
    }

    public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
    {
        base.OnBlockAdded(world, _chunk, _blockPos, _blockValue, _addedByPlayer);

        // Only the parent cell owns the tile entity, and PortalManager keys off the parent anyway.
        if (_blockValue.ischild) return;
        if (string.IsNullOrEmpty(location)) return;

        // Set text first so AddPosition reads the correct name from the tile entity.
        var teComposite = world.GetTileEntity(_blockPos) as TileEntityComposite;
        teComposite?.GetFeature<TEFeatureSignable>()?.SetText(location);
        PortalManager.Instance.AddPosition(_blockPos, location);
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

    public override bool OnBlockActivated(string commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
    {
        if (_blockValue.ischild)
        {
            Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
            return OnBlockActivated(commandName, _world, parentPos, _world.GetBlock(parentPos), _player);
        }

        if (commandName == PortalActivateCommand)
        {
            if (IsUnlockedFor(GetLockable(_world, _blockPos)))
                TeleportPlayer(_player, _blockPos);
            return false;
        }

        // A hard-coded Location fixes the portal name at placement, so the sign editor stays shut.
        if (commandName == EditCommand && !string.IsNullOrEmpty(location))
        {
            Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
            return false;
        }

        // edit / lock / unlock / keypad are all feature-owned from here.
        return base.OnBlockActivated(commandName, _world, _blockPos, _blockValue, _player);
    }

    public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        // portalActivate is always offered, so don't defer to the feature commands' enabled state.
        return true;
    }

    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        var featureCmds = base.GetBlockActivationCommands(_world, _blockValue, _blockPos, _entityFocusing);

        if (portalCmds == null || portalCmds.Length != featureCmds.Length + 1)
            portalCmds = new BlockActivationCommand[featureCmds.Length + 1];

        // BlockActivationCommand is a struct, so these are copies - toggling one below cannot
        // corrupt the array the base class caches and reuses.
        Array.Copy(featureCmds, portalCmds, featureCmds.Length);
        portalCmds[featureCmds.Length] = portalActivateCmd;

        if (!string.IsNullOrEmpty(location))
            for (var i = 0; i < featureCmds.Length; i++)
                if (portalCmds[i].text == EditCommand)
                    portalCmds[i].enabled = false;

        return portalCmds;
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

    public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
    {
        if (_ebcd == null) return;

        // Suppress the sign text mesh (the portal model has none) while still letting the base
        // class build and register the composite tile entity.
        _ebcd.bHasTransform = false;
        base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _ebcd);
        _ebcd.bHasTransform = true;
    }

    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
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
