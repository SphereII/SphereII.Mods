using System;
using HarmonyLib;

namespace Harmony.NoVehicleTake
{
    public class NoVehicleTake
    {
        private static readonly string AdvFeatureClass = "AdvancedPlayerFeatures";
        private static readonly string Feature = "VehicleNoTake";
        private static readonly string VehiclePickUpTags = "takeable";
        private static readonly string cvarPickupAll = "PickUpAllVehicles";

        // EntityVehicle.GetActivationCommands no longer exists; vehicle command gating
        // is now done via AllowActivationCommand(string commandType, EntityAlive entityFocusing).
        [HarmonyPatch(typeof(EntityVehicle))]
        [HarmonyPatch("AllowActivationCommand")]
        public class EntityVehicleAllowActivationCommand
        {
            public static void Postfix(ref bool __result, EntityVehicle __instance, ReadOnlySpan<char>  _commandName, EntityPlayerLocal _playerFocusing)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;
                if (__instance.CommandIs(_commandName, "take"))
                {
                 
                    if (__instance.HasAnyTags(FastTags<TagGroup.Global>.Parse(VehiclePickUpTags))) return;

                    var vehicleID = $"{__instance.EntityName}_pickup";
                    if (_playerFocusing.Buffs.HasCustomVar(vehicleID) &&
                        _playerFocusing.Buffs.GetCustomVar(vehicleID) > 0) return;
                    if (_playerFocusing.Buffs.HasCustomVar(cvarPickupAll) &&
                        _playerFocusing.Buffs.GetCustomVar(cvarPickupAll) > 0) return;

                    __result = false;
                }
            }
        }
    }
}