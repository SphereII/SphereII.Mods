using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Minimal in-memory loot container for NPCs (HarvestManager).
/// Extends TileEntity (which implements ITileEntity + ILockTarget) and adds ITileEntityLootable.
/// Does not participate in the world's tile-entity system or trigger network packets.
/// </summary>
public class SCoreLootContainer : TileEntity, ITileEntityLootable, IInventory
{
    public int EntityId { get; set; }

    public string lootListName { get; set; }
    public float LootStageMod { get; private set; }
    public float LootStageBonus { get; private set; }
    public bool bPlayerStorage { get; set; }
    public PreferenceTracker preferences { get; set; }
    public bool bTouched { get; set; }
    public ulong worldTimeTouched { get; set; }
    public bool bWasTouched { get; set; }
    public ItemStack[] items { get; set; }
    public bool HasSlotLocksSupport => false;
    public PackedBoolArray SlotLocks { get; set; }

    private Vector2i _containerSize = Vector2i.zero;

    public SCoreLootContainer(Chunk _chunk) : base(_chunk) { }

    public override TileEntityType GetTileEntityType() => TileEntityType.None;

    public Vector2i GetContainerSize() => _containerSize;

    public void SetContainerSize(Vector2i _containerSize, bool _clearItems = true)
    {
        this._containerSize = _containerSize;
        var total = _containerSize.x * _containerSize.y;
        if (_clearItems || items == null || items.Length != total)
        {
            items = new ItemStack[total];
            for (int i = 0; i < total; i++) items[i] = ItemStack.Empty;
        }
    }

    public void UpdateSlot(int _idx, ItemStack _item)
    {
        if (items != null && _idx >= 0 && _idx < items.Length)
            items[_idx] = _item;
    }

    public void RemoveItem(ItemValue _itemValue)
    {
        if (items == null) return;
        for (int i = 0; i < items.Length; i++)
            if (items[i].itemValue.type == _itemValue.type) { items[i] = ItemStack.Empty; return; }
    }

    public int RemoveItems(ItemValue _itemValue, int _count) => 0;

    public bool IsEmpty()
    {
        if (items == null) return true;
        foreach (var s in items) if (!s.IsEmpty()) return false;
        return true;
    }

    public void SetEmpty()
    {
        if (items != null) for (int i = 0; i < items.Length; i++) items[i] = ItemStack.Empty;
    }

    public bool ShouldDestroyOnClose() => false;

    public ItemStack[] GetItems() => items ?? System.Array.Empty<ItemStack>();

    public bool AddItem(ItemStack _itemStack)
    {
        if (items == null) return false;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].IsEmpty()) { items[i] = _itemStack; return true; }
        }
        return false;
    }

    public (bool anyMoved, bool allMoved) TryStackItem(int _startIndex, ItemStack _itemStack) => (false, false);

    public bool HasItem(ItemValue _item)
    {
        if (items == null) return false;
        foreach (var s in items)
            if (!s.IsEmpty() && s.itemValue.type == _item.type) return true;
        return false;
    }
}
