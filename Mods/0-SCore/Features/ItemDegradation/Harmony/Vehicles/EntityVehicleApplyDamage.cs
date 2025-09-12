using HarmonyLib;
using SCore.Features.ItemDegradation.Utils;

namespace SCore.Features.ItemDegradation.Harmony.Vehicles
{
    [HarmonyPatch(typeof(EntityVehicle))]
    [HarmonyPatch(nameof(EntityVehicle.ApplyDamage))]
    public class EntityVehicleApplyDamagePatch
    {
        public static void Postfix(EntityVehicle __instance)
        {
            if (__instance.Health <= 0) return;
            var vehicle = __instance.GetVehicle();
            var itemValue = vehicle.GetUpdatedItemValue();
            var currentPlayer = __instance.AttachedMainEntity as EntityPlayer;
            ItemDegradationHelpers.CheckModificationOnItem(itemValue.Modifications, currentPlayer);
        }
    }
}
