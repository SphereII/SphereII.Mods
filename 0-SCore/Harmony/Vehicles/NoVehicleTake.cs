using HarmonyLib;
using System.Linq;

namespace Harmony.NoVehicleTake
{
    public class NoVehicleTake
    {
        private static readonly string AdvFeatureClass = "AdvancedPlayerFeatures";
        private static readonly string Feature = "VehicleNoTake";
        private static readonly string VehiclePickUpTags = "takeable";
        private static readonly string cvarPickupAll = "PickUpAllVehicles";


        [HarmonyPatch(typeof(EntityVehicle))]
        [HarmonyPatch("GetActivationCommands")]
        public class EntityVehicleGetActivationCommands
        {
            public static EntityActivationCommand[] Postfix(EntityActivationCommand[] __result, EntityVehicle __instance, global::EntityAlive _entityFocusing)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return __result;


                if (__instance.HasAnyTags(FastTags.Parse(VehiclePickUpTags))) return __result;

                // If the player has a cvar with the following code, allow them to take it. 
                // This will allow modders to fine tune vehicle pick up via perk or cvar.
                var vehicleID = $"{__instance.EntityName}_pickup";
                if (_entityFocusing.Buffs.HasCustomVar(vehicleID) && _entityFocusing.Buffs.GetCustomVar(vehicleID) > 0)
                    return __result;

                // If they can pick up everything...
                if (_entityFocusing.Buffs.HasCustomVar(cvarPickupAll) && _entityFocusing.Buffs.GetCustomVar(cvarPickupAll) > 0)
                    return __result;

                for (var i = 0; i < __result.Length; i++)
                {
                    if (__result[i].text == "take")
                        __result[i].enabled = false;
                }

                return __result;
                //var list = __result.Cast<EntityActivationCommand>().ToList();
                //for (var x = 0; x < list.Count; x++)
                //{
                //    if (list[x].text != "take") continue;
                //    list.RemoveAt(x);
                //    break;
                //}


                //return list.ToArray();
            }
        }
    }
}