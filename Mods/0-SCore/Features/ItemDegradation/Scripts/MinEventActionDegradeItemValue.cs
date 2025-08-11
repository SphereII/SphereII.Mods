using SCore.Features.ItemDegradation.Harmony;
using UnityEngine;

public class MinEventActionDegradeItemValueMod: MinEventActionTargetedBase
{
    public override void Execute(MinEventParams _params)
    {
        var player = _params.Self as EntityPlayerLocal;
        if (player == null) return;

        var itemValue = _params.ItemValue;
        if (itemValue == null) return;
        
        // Only Work on a mod.
        if (!itemValue.IsMod) return;
        ItemDegradationHelpers.CheckModification(itemValue, player);
    }
 
}