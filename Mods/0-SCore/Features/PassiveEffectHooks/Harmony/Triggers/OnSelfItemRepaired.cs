using HarmonyLib;
using SCore.Features.ItemDegradation.Harmony;
using SCore.Features.ItemDegradation.Utils;
using UnityEngine;

public static class OnRepair
{
  

    public static void CheckForDegradation(ItemValue stack)
    {
        if (stack == null || stack.IsEmpty()) return;

        var minEventParams = new MinEventParams {
            TileEntity = TraderUtils.GetCurrentTraderTileEntity(),
            ItemValue = stack,
            Self = GameManager.Instance.World.GetPrimaryPlayer(),
            Biome = GameManager.Instance.World.GetPrimaryPlayer()?.biomeStandingOn
        };

        if (Configuration.CheckFeatureStatus(ItemDegradationHelpers.AdvFeatureClass, "RepairModsWithItem"))
        {
            foreach (var mod in stack.Modifications)
            {
                if (ItemDegradationHelpers.CanDegrade(mod))
                {
                    mod.UseTimes = 1f;
                }
            }
        }

        // Reaches the repaired item's own <effect_group> entries in items.xml, and only those.
        stack.ItemClass.FireEvent(MinEventTypes.onSelfItemRepaired, minEventParams);

        // ...and this is what reaches BUFFS on the player. It was commented out, which made
        // onSelfItemRepaired unlistenable from a buff: vanilla never fires this event at all,
        // so SCore is its only source, and the line above cannot cross from an item's effect
        // controller to the entity's buff list.
        //
        // The vehicle equivalent in XUiMVehicleRepairVehicle.cs always did fire on Self,
        // which is why repairing a VEHICLE worked while repairing an ITEM did not.
        //
        // ORDERING IS LOAD-BEARING. It must sit above the two SetMetadata calls below.
        // XUiCRecipeStackOutputStackOnrepairPatch writes PercentDamaged as
        // UseTimes / MaxUseTimes in a Prefix, ie. the wear the item had BEFORE the repair,
        // and the calls below reset it to zero afterwards. Gates such as
        // "ItemPercentDamaged GTE 0.5" are asking "was this at least half worn when I fixed
        // it", so they have to be evaluated in that window. Firing after the reset would
        // hand every one of them a flat 0.
        //
        // Guarded rather than early-returned so the reset still happens with no player
        // present; MinEventContext was previously assigned unguarded on the line above.
        if (minEventParams.Self != null)
        {
            minEventParams.Self.MinEventContext = minEventParams;
            minEventParams.Self.FireEvent(MinEventTypes.onSelfItemRepaired);
        }

        minEventParams.ItemValue.SetMetadata("DamageAmount", 0f, TypedMetadataValue.TypeTag.Float);
        minEventParams.ItemValue.SetMetadata("PercentDamaged", 0f, TypedMetadataValue.TypeTag.Float);
        

    }
    
    
}