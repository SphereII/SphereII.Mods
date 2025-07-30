using HarmonyLib;
using UnityEngine;

public static class OnRepair
{
  

    public static void CheckForDegradation(ItemValue stack)
    {
        if (stack == null || stack.IsEmpty()) return;

        var minEventParams = new MinEventParams {
            TileEntity = TraderUtils.GetCurrentTraderTileEntity(),
            ItemValue = stack,
            Self = GameManager.Instance.World.GetPrimaryPlayer()
        };

        stack.ItemClass.FireEvent(MinEventTypes.onSelfItemRepaired, minEventParams);
        minEventParams.Self.MinEventContext = minEventParams;
        minEventParams.Self.FireEvent(MinEventTypes.onSelfItemRepaired);
        
        minEventParams.ItemValue.SetMetadata("DamageAmount", 0f, TypedMetadataValue.TypeTag.Float);
        minEventParams.ItemValue.SetMetadata("PercentDamaged", 0f, TypedMetadataValue.TypeTag.Float);

    }
    
    
}