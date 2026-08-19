using HarmonyLib;
using UnityEngine;


public static class OnSelfItemScrap
{
    public static void CheckForScrapping(ItemStack stack)
    {
        if (stack == null || stack.IsEmpty()) return;

        var minEventParams = new MinEventParams {
            TileEntity = TraderUtils.GetCurrentTraderTileEntity(),
            ItemValue = stack.itemValue,
            Self = GameManager.Instance.World.GetPrimaryPlayer(),
            Biome = GameManager.Instance.World.GetPrimaryPlayer()?.biomeStandingOn
        };

        
        // Fires into the scrapped item's OWN effect groups, ie. anything declared under
        // <effect_group> in items.xml for that item. ItemClass.FireEvent forwards to
        // Effects.FireEvent, which is the item's MinEffectController and nothing else.
        stack.itemValue.ItemClass.FireEvent((MinEventTypes)SCoreMinEventTypes.onSelfScrapItem, minEventParams);

        // ...and this is what reaches BUFFS on the player, which is where nearly every
        // consumer of this trigger actually lives. The line above cannot do it: an
        // item's effect controller has no route to the entity's buff list.
        //
        // It was commented out, so onSelfScrapItem was effectively unlistenable from a
        // buff. Learn By Doing alone had 135 triggered_effects across 21 files waiting on
        // it, none of which had ever fired.
        //
        // MinEventContext must be assigned before firing. EntityAlive.FireEvent(type)
        // takes the single-argument overload and reads the entity's current context, so
        // without this the requirements would evaluate against whatever params were left
        // over from the previous event - ItemValue included, which is exactly what gates
        // like ItemHasTags and ItemHasProperty test.
        if (minEventParams.Self == null) return;
        minEventParams.Self.MinEventContext = minEventParams;
        minEventParams.Self.FireEvent((MinEventTypes)SCoreMinEventTypes.onSelfScrapItem);
    }
}