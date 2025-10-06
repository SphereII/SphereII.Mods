using HarmonyLib;

namespace SCore.Harmony.Vehicles {
    public class VehicleGetFuelTypePatch {
        [HarmonyPatch(typeof(Vehicle))]
        [HarmonyPatch(nameof(Vehicle.GetFuelItem))]
        public class VehicleGetFuelType {
            public static bool Prefix(ref string __result, Vehicle __instance)
            {
                var fuelType = __instance.Properties.GetString("fuelTank", "fuelType");
                if (string.IsNullOrEmpty(fuelType)) return true;
                
                __result = fuelType;
                return false;

            }
        }
    }
}