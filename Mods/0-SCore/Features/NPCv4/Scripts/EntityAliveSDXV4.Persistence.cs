using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class EntityAliveSDXV4
{
    // =========================================================================
    // Persistence (Save / Load)
    // =========================================================================

    public override void Write(BinaryWriter _bw, bool bNetworkWrite)
    {
        base.Write(_bw, bNetworkWrite);

        // SDX section: component-framed so one failing component is dropped whole instead of
        // half-written, and so the record can shed components (journal first, loot last) to
        // stay under EntityCreationData's 64KB record limit — an oversized record corrupts the
        // whole chunk on its next load.
        var section = new SdxEntityPersistence.Section("EntityAliveSDXV4");

        section.Add(SdxEntityPersistence.ComponentIdentity, "identity", SdxEntityPersistence.NeverDrop, bw => {
            bw.Write(_strMyName);
            bw.Write(GuardPosition.ToString());
            bw.Write(factionId);
            bw.Write(GuardLookPosition.ToString());
        });

        section.Add(SdxEntityPersistence.ComponentQuestJournal, "quest journal", 0, bw => questJournal.Write(bw));

        // ── Patrol coordinates — string.Join avoids the O(n²) concatenation of the original
        section.Add(SdxEntityPersistence.ComponentPatrol, "patrol", SdxEntityPersistence.NeverDrop,
            bw => bw.Write(string.Join(";", PatrolCoordinates)));

        section.Add(SdxEntityPersistence.ComponentBuffsProgression, "buffs/progression", 1, bw => {
            Buffs.Write(bw);
            bw.Write(Progression != null);
            if (Progression != null) Progression.Write(bw);
        });

        section.Add(SdxEntityPersistence.ComponentWeapon, "weapon", SdxEntityPersistence.NeverDrop,
            bw => bw.Write(inventory.holdingItem != null ? inventory.holdingItem.GetItemName() : ""));

        section.Add(SdxEntityPersistence.ComponentLoot, "loot", 2, bw => {
            if (HarvestManager.Has(entityId))
            {
                bw.Write(true);
                GameUtils.WriteItemStack(bw, HarvestManager.GetOrCreate(entityId).GetItems());
            }
            else if (lootContainer != null)
            {
                bw.Write(true);
                GameUtils.WriteItemStack(bw, lootContainer.GetItems());
            }
            else
            {
                bw.Write(false);
            }
        });

        section.WriteTo(_bw);
    }

    public override void Read(byte _version, BinaryReader _br)
    {
        base.Read(_version, _br);

        // Defaults first: a component that is absent (dropped on write or skipped on a failed
        // read) leaves these in a sane state instead of stale or garbage values.
        questJournal = new QuestJournal();
        _patrolCoordinatesLegacy = new List<Vector3>();

        if (!SdxEntityPersistence.TryReadSection(_br, "EntityAliveSDXV4", ReadSdxComponent))
            ReadLegacy(_br);
    }

    private void ReadSdxComponent(byte id, PooledBinaryReader br)
    {
        switch (id)
        {
            case SdxEntityPersistence.ComponentIdentity:
                _strMyName = br.ReadString();
                _cachedDisplayNameKey = null; // invalidate display-name cache

                // Components may not be ready yet during Read (called before PostInit in some
                // paths). Store into legacy fields; PostInit will flush them into the patrol
                // component.
                _guardPositionLegacy     = ModGeneralUtilities.StringToVector3(br.ReadString());
                factionId                = br.ReadByte();
                _guardLookPositionLegacy = ModGeneralUtilities.StringToVector3(br.ReadString());
                break;

            case SdxEntityPersistence.ComponentQuestJournal:
                questJournal = new QuestJournal();
                questJournal.Read(br);
                break;

            case SdxEntityPersistence.ComponentPatrol:
                _patrolCoordinatesLegacy = new List<Vector3>();
                var strPatrol = br.ReadString();
                if (!string.IsNullOrEmpty(strPatrol))
                {
                    foreach (var pt in strPatrol.Split(';'))
                    {
                        var v = ModGeneralUtilities.StringToVector3(pt);
                        if (v != Vector3.zero) _patrolCoordinatesLegacy.Add(v);
                    }
                }
                break;

            case SdxEntityPersistence.ComponentBuffsProgression:
                Buffs.Read(br);
                if (br.ReadBoolean())
                    Progression.Read(br, this);
                break;

            case SdxEntityPersistence.ComponentWeapon:
                _currentWeapon = br.ReadString();
                if (!string.IsNullOrEmpty(_currentWeapon))
                    UpdateWeapon(_currentWeapon);
                break;

            case SdxEntityPersistence.ComponentLoot:
                if (br.ReadBoolean())
                {
                    var items = GameUtils.ReadItemStack(br);
                    if (items != null)
                    {
                        var hc = HarvestManager.GetOrCreate(entityId);
                        for (int i = 0; i < items.Length && i < hc.items.Length; i++)
                            hc.items[i] = items[i];
                    }
                }
                break;

            // Unknown component ids (from a newer SCore) are skipped by the framing.
        }
    }

    // Pre-framing record layout; still hit for saves written before this format existed.
    private void ReadLegacy(BinaryReader _br)
    {
        _strMyName         = _br.ReadString();
        _cachedDisplayNameKey = null; // invalidate display-name cache

        // Components may not be ready yet during Read (called before PostInit in some paths).
        // Store into legacy fields; PostInit will flush them into the patrol component.
        _guardPositionLegacy     = ModGeneralUtilities.StringToVector3(_br.ReadString());
        factionId                = _br.ReadByte();
        _guardLookPositionLegacy = ModGeneralUtilities.StringToVector3(_br.ReadString());

        questJournal = new QuestJournal();
        questJournal.Read(_br as PooledBinaryReader);

        _patrolCoordinatesLegacy = new List<Vector3>();
        var strPatrol = _br.ReadString();
        if (!string.IsNullOrEmpty(strPatrol))
        {
            foreach (var pt in strPatrol.Split(';'))
            {
                var v = ModGeneralUtilities.StringToVector3(pt);
                if (v != Vector3.zero) _patrolCoordinatesLegacy.Add(v);
            }
        }

        try
        {
            Buffs.Read(_br);
            if (Progression != null) Progression.Read(_br, this);
        }
        catch (Exception ex) { Log.Error($"EntityAliveSDXV4.Read Failed to load Buffs/Progression: {ex.Message}"); }

        _currentWeapon = _br.ReadString();
        if (!string.IsNullOrEmpty(_currentWeapon))
            UpdateWeapon(_currentWeapon);

        if (_br.ReadBoolean())
        {
            var items = GameUtils.ReadItemStack(_br);
            if (items != null)
            {
                var hc = HarvestManager.GetOrCreate(entityId);
                for (int i = 0; i < items.Length && i < hc.items.Length; i++)
                    hc.items[i] = items[i];
            }
        }
    }

    public void WriteSyncData(BinaryWriter _bw, ushort syncFlags)
    {
        if (lootContainer == null) return;
        var slots = lootContainer.items;
        _bw.Write((byte)slots.Length);
        for (int i = 0; i < slots.Length; i++)
            slots[i].Write(_bw);
        _bw.Write(_currentWeapon);
        _bw.Write(EntityName);
    }

    public void ReadSyncData(BinaryReader _br, ushort syncFlags, int senderId)
    {
        if (lootContainer == null) return;
        var num     = (int)_br.ReadByte();
        var array   = new ItemStack[num];
        var total   = lootContainer.GetContainerSize().x * lootContainer.GetContainerSize().y;
        for (int j = 0; j < num; j++)
        {
            var stack = new ItemStack();
            array[j] = stack.Read(_br);
            if (j >= total) { Log.Out("Loot container out of range."); break; }
            lootContainer.UpdateSlot(j, array[j]);
        }
        lootContainer.bPlayerStorage = true;
        _currentWeapon = _br.ReadString();
        SetEntityName(_br.ReadString());
        UpdateWeapon(_currentWeapon);
    }

    /// <summary>
    /// Returns the flags that the server should replicate to other clients.
    /// Currently all flags are replicated; override in subclasses to filter.
    /// </summary>
    public ushort GetSyncFlagsReplicated(ushort syncFlags) => syncFlags;

    public void SendSyncData(ushort syncFlags = 1)
    {
        var primaryPlayerId = GameManager.Instance.World.GetPrimaryPlayerId();
        var package = NetPackageManager.GetPackage<NetPackageEntityAliveSDXV4DataSync>().Setup(this, primaryPlayerId, syncFlags);
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package, false);
        else
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package);
    }
}
