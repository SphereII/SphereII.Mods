using HarmonyLib;

namespace SCore.Features.PassiveEffectHooks.Harmony
{
    [HarmonyPatch(typeof(XUiM_Vehicle))]
    [HarmonyPatch(nameof(XUiM_Vehicle.RepairVehicle))]
    public class XUiMVehicleRepairVehiclePatches
    {
        public static void Postfix(bool __result, XUi _xui, Vehicle _vehicle)
        {
            if (__result == false) return;
            _vehicle ??= _xui.Vehicle.CurrentVehicle.GetVehicle();

            _vehicle.itemValue.SetMetadata("DamageAmount", _vehicle.GetHealth(), TypedMetadataValue.TypeTag.Integer);
            _vehicle.itemValue.SetMetadata("PercentDamaged", _vehicle.GetHealthPercent(), TypedMetadataValue.TypeTag.Float);

            var entityPlayer = _xui.playerUI.entityPlayer;
            var minEventParams = new MinEventParams {
                ItemValue = _vehicle.itemValue,
                Self = entityPlayer
            };

            _vehicle.itemValue.ItemClass.FireEvent(MinEventTypes.onSelfItemRepaired, minEventParams);
            minEventParams.Self.MinEventContext = minEventParams;
            minEventParams.Self.FireEvent(MinEventTypes.onSelfItemRepaired);
        }
    }
}