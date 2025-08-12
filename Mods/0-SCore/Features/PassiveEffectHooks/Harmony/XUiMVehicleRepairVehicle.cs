using HarmonyLib;
using UnityEngine;

namespace SCore.Features.PassiveEffectHooks.Harmony
{
    [HarmonyPatch(typeof(XUiM_Vehicle))]
    [HarmonyPatch(nameof(XUiM_Vehicle.RepairVehicle))]
    public class XUiMVehicleRepairVehiclePatches
    {
        public static void Postfix( bool __result, XUiM_Vehicle __instance, XUi _xui, Vehicle vehicle)
        {
            if (__result == false) return;
             vehicle ??= _xui.vehicle.GetVehicle();
            
             vehicle.itemValue.SetMetadata("DamageAmount", vehicle.GetHealth(), TypedMetadataValue.TypeTag.Integer);
             vehicle.itemValue.SetMetadata("PercentDamaged", vehicle.GetHealthPercent(), TypedMetadataValue.TypeTag.Float);
             
            var entityPlayer = _xui.playerUI.entityPlayer;
            var minEventParams = new MinEventParams {
                ItemValue = vehicle.itemValue,
                Self = entityPlayer
            };

            vehicle.itemValue.ItemClass.FireEvent(MinEventTypes.onSelfItemRepaired, minEventParams);
            minEventParams.Self.MinEventContext = minEventParams;
            minEventParams.Self.FireEvent(MinEventTypes.onSelfItemRepaired);
        }
    }
}