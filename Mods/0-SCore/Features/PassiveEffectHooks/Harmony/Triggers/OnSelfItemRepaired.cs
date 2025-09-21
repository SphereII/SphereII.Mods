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

        stack.ItemClass.FireEvent(MinEventTypes.onSelfItemRepaired, minEventParams);
        minEventParams.Self.MinEventContext = minEventParams;
     //   minEventParams.Self.FireEvent(MinEventTypes.onSelfItemRepaired);
        
        minEventParams.ItemValue.SetMetadata("DamageAmount", 0f, TypedMetadataValue.TypeTag.Float);
        minEventParams.ItemValue.SetMetadata("PercentDamaged", 0f, TypedMetadataValue.TypeTag.Float);
        

    }
    
    
}