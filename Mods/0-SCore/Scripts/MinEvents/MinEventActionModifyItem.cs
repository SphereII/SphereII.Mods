using System.Collections.Generic;
using System.Xml.Linq;
using HarmonyLib;
using UniLinq;
using UnityEngine;

public class MinEventActionModifyItem : MinEventActionTargetedBase
{
    private bool breakOnLowQuality = false;

    public override void Execute(MinEventParams _params)
    {
        var player = _params.Self as EntityPlayerLocal;
        if (player == null) return;
        
        var itemValue = _params.ItemValue;

        if (!itemValue.HasQuality) return;
        if (itemValue.Quality == 1) return;

        var newQuality = itemValue.Quality - 1;
        var newItem = new ItemValue(itemValue.type, newQuality, newQuality);
        foreach (var itemMod in itemValue.Modifications)
        {
            if (itemMod == null || itemMod.IsEmpty()) continue;
            var itemStack = new ItemStack(itemMod, 1);
            if (player.playerUI.xui.PlayerInventory.AddItem(itemStack)) continue;
            player.playerUI.xui.PlayerInventory.DropItem(itemStack);
        }

        itemValue.Modifications = newItem.Modifications;
        itemValue.Quality = (ushort)newQuality;
    }
 
}