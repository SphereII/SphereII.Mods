using HarmonyLib;
using System.Linq;
namespace Harmony.NoVehicleTake
{
    public class NoVehicleTake
    {
        private static readonly string AdvFeatureClass = "AdvancedPlayerFeatures";
        private static readonly string Feature = "VehicleNoTake";

        [HarmonyPatch(typeof(EntityVehicle))]
        [HarmonyPatch("GetActivationCommands")]
        public class EntityVehicleGetActivationCommands
        {
            public static EntityActivationCommand[] Postfix(EntityActivationCommand[] __result)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return __result;

                var list = __result.Cast<EntityActivationCommand>().ToList();
                for (var x = 0; x < list.Count; x++)
                {
                    if (list[x].text != "take") continue;
                    list.RemoveAt(x);
                    break;
                }


                return list.ToArray();
            }
        }
    }
}