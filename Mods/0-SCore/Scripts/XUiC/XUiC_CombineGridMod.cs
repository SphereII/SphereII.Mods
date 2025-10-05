using System;
using UnityEngine;

// provided by dwallorde

public class XUiC_CombineGridMod : XUiC_ItemStackGrid
{
    private XUiC_RequiredItemStack stack1;
    private XUiC_RequiredItemStack stack2;
    private XUiC_RequiredItemStack result;

    public override void Init()
    {
        base.Init();

        var children = GetChildrenByType<XUiC_ItemStack>(null);
        if (children.Length != 3) return;

        stack1 = (XUiC_RequiredItemStack)children[0];
        stack2 = (XUiC_RequiredItemStack)children[1];
        result = (XUiC_RequiredItemStack)children[2];

        stack1.SlotChangedEvent += OnMergeChanged;
        stack2.SlotChangedEvent += OnMergeChanged;
        result.SlotChangedEvent += OnResultTaken;

        result.HiddenLock = true;
        result.TakeOnly = true;
    }

    private void OnResultTaken(int slot, ItemStack stack)
    {

        stack1.ItemStack = ItemStack.Empty.Clone();
        stack2.ItemStack = ItemStack.Empty.Clone();

        // Force UI refresh workaround
        var tmp = result.ItemStack;
        result.ItemStack = ItemStack.Empty.Clone();
        result.ItemStack = tmp;
    }

    private void OnMergeChanged(int slot, ItemStack stack)
    {
        if (stack1.ItemStack.IsEmpty() || stack2.ItemStack.IsEmpty())
        {
            ClearResult();
            return;
        }

        if (stack1.ItemStack.itemValue.type != stack2.ItemStack.itemValue.type)
        {
            ClearResult();
            return;
        }

        var iv1 = stack1.ItemStack.itemValue;
        var iv2 = stack2.ItemStack.itemValue;

        int inputMaxQuality = Math.Max(iv1.Quality, iv2.Quality);
        int maxAllowedQuality = QualityUtils.GetMaxQuality();

        // Allow combining up to input max quality if >=6, else max 5
        int maxQuality = (inputMaxQuality >=  QualityUtils.GetMaxQuality()) ? inputMaxQuality : maxAllowedQuality;

        int baseQuality = inputMaxQuality;

        // Get max use times for each quality level
        int maxUses1 = (int)new ItemValue(iv1.type, true) { Quality = iv1.Quality }.MaxUseTimes;
        int maxUses2 = (int)new ItemValue(iv2.type, true) { Quality = iv2.Quality }.MaxUseTimes;

        if (maxUses1 <= 0 || maxUses2 <= 0)
        {
            Debug.Log("[CombineGridMod] Invalid max use times detected.");
            ClearResult();
            return;
        }

        // Convert to raw remaining durability (NOT inverted UseTimes)
        int remaining1 = (int)(maxUses1 - iv1.UseTimes);
        int remaining2 = (int)(maxUses2 - iv2.UseTimes);

        // Total combined durability
        int totalRemaining = remaining1 + remaining2;

        int resultQuality = baseQuality;

        // Try to upgrade quality if enough durability overflows, but don't exceed maxQuality
        while (resultQuality < maxQuality)
        {
            int maxUsesAtNext = (int)new ItemValue(iv1.type, true) { Quality = (ushort)(resultQuality + 1) }.MaxUseTimes;
            if (totalRemaining > maxUsesAtNext)
            {
                totalRemaining -= maxUsesAtNext;
                resultQuality++;
            }
            else
            {
                break;
            }
        }

        int resultMaxUses = (int)new ItemValue(iv1.type, true) { Quality = (ushort)resultQuality }.MaxUseTimes;
        int resultUseTimes = resultMaxUses - Math.Min(totalRemaining, resultMaxUses);

        var newItemValue = new ItemValue(iv1.type, true)
        {
            Quality = (ushort)resultQuality,
            UseTimes = resultUseTimes
        };

        var combinedStack = new ItemStack(newItemValue, 1);

        result.SlotChangedEvent -= OnResultTaken;
        result.ItemStack = combinedStack;
        result.HiddenLock = false;
        result.SlotChangedEvent += OnResultTaken;

        Debug.Log($"[CombineGridMod] Combined Q{iv1.Quality} + Q{iv2.Quality} => Q{resultQuality}, Durability: {resultMaxUses - resultUseTimes}/{resultMaxUses}");
    }

    private void ClearResult()
    {
        result.SlotChangedEvent -= OnResultTaken;
        result.ItemStack = ItemStack.Empty.Clone();
        result.HiddenLock = true;
        result.SlotChangedEvent += OnResultTaken;
    }
}
