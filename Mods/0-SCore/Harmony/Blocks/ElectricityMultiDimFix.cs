using HarmonyLib;

// It allows to use the wire tool anywhere on a MultiDim Block - from ocbMaurice
// <property name="MultiDimPowerBlock" value="true"/>

public class ElectricityMultiDimFix
{

    // ####################################################################
    // ####################################################################

    private static void FixMultiDimPowerBlock(ref ItemActionData _actionData)
    {
        if (_actionData.invData.hitInfo.bHitValid == false) return;
        BlockValue block = _actionData.invData.hitInfo.hit.blockValue;
        if (block.ischild == false) return; // Correct only on child hit
        // ToDo: remove check and enable this fix without custom property?
        if (!block.Block.Properties.GetBool("MultiDimPowerBlock")) return;
        // Fix hit position of child block to point to master block
        _actionData.invData.hitInfo.hit.blockPos += block.parent;
        _actionData.invData.hitInfo.hit.voxelData.BlockValue =
            _actionData.invData.world.GetBlock(
                _actionData.invData.hitInfo.hit.blockPos);
    }

    // ####################################################################
    // ####################################################################

    [HarmonyPatch(typeof(ItemActionConnectPower), "OnHoldingUpdate")]
    public class ItemActionConnectPowerOnHoldingUpdatePatch
    {
        static void Prefix(ref ItemActionData _actionData)
            => FixMultiDimPowerBlock(ref _actionData);
    }

    [HarmonyPatch(typeof(ItemActionDisconnectPower), "OnHoldingUpdate")]
    public class ItemActionDisconnectPowerOnHoldingUpdatePatch
    {
        static void Prefix(ref ItemActionData _actionData)
            => FixMultiDimPowerBlock(ref _actionData);
    }

    // ####################################################################
    // ####################################################################

}