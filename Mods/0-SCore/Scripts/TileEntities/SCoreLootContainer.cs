using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// Minimal in-memory loot container for NPCs (HarvestManager).
///
/// v3.3: loot containers are no longer stand-alone tile entities. Storage is a
/// <see cref="TEFeatureStorage"/> feature hanging off a <see cref="TileEntityComposite"/>, and the
/// loot window (XUiC_LootWindowGroup.OpenLooting / XUiC_LootWindow.SetTileEntityChest) binds to
/// that concrete type - an interface no longer exists to pose as. So this derives from
/// TEFeatureStorage and supplies a detached composite parent that exists purely to satisfy the
/// TEFeatureAbs -> Parent delegation (chunk, position, listeners, user-accessing flag).
///
/// The parent is never registered with the world's tile-entity system and has
/// SetDisableModifiedCheck(true), so TileEntity.setModified() short-circuits before it can send a
/// NetPackageTileEntity. That is what keeps SetModified() harmless here - the inherited AddItem /
/// UpdateSlot / SetEmpty all call it, unlike the hand-rolled versions this replaced.
/// </summary>
public class SCoreLootContainer : TEFeatureStorage
{
    public int EntityId { get; set; }

    private Vector2i _containerSize = Vector2i.zero;

    /// <summary>
    /// Required by the engine, not used by SCore. TileEntityCompositeData.Init() reflects over every
    /// ITileEntityFeature at startup and logs
    /// "CompositeTileEntity feature SCoreLootContainer has no parameterless constructor!" for any
    /// that lacks a PUBLIC one. Deriving from TEFeatureStorage makes this type visible to that scan,
    /// so provide the constructor rather than ship the warning. It deliberately builds nothing: a
    /// feature created this way is set up by TEFeatureAbs.Init/TEFeatureStorage.Init instead, which
    /// assigns Parent and creates itemGrid from the block's XML.
    /// </summary>
    public SCoreLootContainer()
    {
    }

    public SCoreLootContainer(Chunk _chunk)
    {
        var parent = new TileEntityComposite(_chunk);

        // Nothing about this container belongs to the world: suppress the modified/network path
        // rather than relying on every caller to avoid SetModified().
        parent.SetDisableModifiedCheck(true);
        GiveEmptyFeatureTable(parent);
        Parent = parent;

        SetContainerSize(Vector2i.zero);
    }

    /// <summary>
    /// A composite built with TileEntityComposite(Chunk) never gets its feature table (teData), so any
    /// feature lookup through it - e.g. XUiC_LootWindow.SetTileEntityChest's
    /// GetSelfOrFeature&lt;ITileEntitySignable&gt;(), which a feature answers by asking its Parent - threw
    /// a NullReferenceException in TileEntityComposite.GetFeature. The only TileEntityCompositeData
    /// constructor needs a composite block and throws without one, so build an empty table by hand:
    /// every lookup then answers "no such feature".
    /// </summary>
    private static void GiveEmptyFeatureTable(TileEntityComposite parent)
    {
        var data = (TileEntityCompositeData)FormatterServices.GetUninitializedObject(typeof(TileEntityCompositeData));
        SetReadOnlyField(data, nameof(TileEntityCompositeData.Features), new List<TileEntityFeatureData>());
        SetReadOnlyField(data, nameof(TileEntityCompositeData.featureIndexByType), new Dictionary<Type, int>());
        SetReadOnlyField(data, nameof(TileEntityCompositeData.featureIndexByName),
            new Dictionary<ReadOnlyMemory<char>, int>(TileEntityCompositeData.MemStringEqualityComparer.Instance));
        SetReadOnlyField(data, nameof(TileEntityCompositeData.featureIndexByHash), new Dictionary<int, int>());

        parent.teData = data;
        parent.modulesCustomOrder = Array.Empty<ITileEntityFeature>();
        parent.modulesInternalOrder = Array.Empty<ITileEntityFeature>();
    }

    private static void SetReadOnlyField(object target, string name, object value) =>
        typeof(TileEntityCompositeData).GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(target, value);

    // ItemStackGrid is the v3.3 backing store; keep the old array-shaped surface SCore callers use.
    public ItemStack[] items => ItemGrid?.items;

    public bool bPlayerStorage
    {
        get => ItemGrid != null && ItemGrid.PlayerOwned;
        set { if (ItemGrid != null) ItemGrid.PlayerOwned = value; }
    }

    // v3.3 collapsed bTouched/bWasTouched into one world-time stamp on the grid.
    public bool bTouched
    {
        get => ItemGrid != null && ItemGrid.Touched;
        set
        {
            if (ItemGrid == null) return;
            if (value) ItemGrid.Touch();
            else ItemGrid.WorldTimeTouched = 0uL;
        }
    }

    public bool bWasTouched
    {
        get => bTouched;
        set => bTouched = value;
    }

    /// <summary>The chunk the detached parent is anchored to. See HarvestManager.PositionAtEntity.</summary>
    public Chunk chunk
    {
        get => Parent.chunk;
        set => Parent.chunk = value;
    }

    public Vector3i localChunkPos
    {
        get => Parent.localChunkPos;
        set => Parent.localChunkPos = value;
    }

    public Vector2i GetContainerSize() => _containerSize;

    public void SetContainerSize(Vector2i _containerSize, bool _clearItems = true)
    {
        this._containerSize = _containerSize;

        if (ItemGrid == null)
        {
            itemGrid = ItemStackGrid.Create(_containerSize, XUiC_ItemStack.StackLocationTypes.LootContainer,
                _hasLocks: true, _hasPreferences: true, this);
            itemGrid.SlotChanged += OnSlotChanged;
            return;
        }

        if (_clearItems || ItemGrid.ContainerSize != _containerSize)
        {
            ItemGrid.Resize(_containerSize);
            if (_clearItems) ItemGrid.Clear();
        }
    }

    public ItemStack[] GetItems() => ItemGrid?.items ?? System.Array.Empty<ItemStack>();
}
