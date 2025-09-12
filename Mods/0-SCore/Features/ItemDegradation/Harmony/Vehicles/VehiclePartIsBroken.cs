using HarmonyLib;
using SCore.Features.ItemDegradation.Utils;

namespace SCore.Features.ItemDegradation.Harmony.Vehicles
{
    [HarmonyPatch(typeof(VehiclePart), nameof(VehiclePart.IsBroken))]
    public class VehiclePartIsBroken
    {
        public static void Postfix(ref bool __result, VehiclePart __instance)
        {
            // If the part is already broken, no need to perform additional checks.
            if (__result) return;

            // Use pattern matching with a switch statement to handle different vehicle part types.
            __result = __instance switch
            {
                VPHeadlight => CheckModificationsForTag(__instance, "light"),
                VPFuelTank => CheckModificationsForTag(__instance, "fuelsaver,fueltank"),
                // Default case, return false if the part is not a headlight or fuel tank.
                _ => false
            };
        }

        /// <summary>
        /// Checks the vehicle's modifications for degradation based on a given tag.
        /// </summary>
        /// <param name="vehiclePart">The vehicle part instance.</param>
        /// <param name="tagString">The comma-separated string of tags to check for.</param>
        /// <returns>True if a degradable modification with the specified tag is degraded, otherwise false.</returns>
        private static bool CheckModificationsForTag(VehiclePart vehiclePart, string tagString)
        {
            var tags = FastTags<TagGroup.Global>.Parse(tagString);

            foreach (var mod in vehiclePart.vehicle.itemValue.Modifications)
            {
                // Use 'is' and 'as' for cleaner null and type checks.
                if (mod?.ItemClass is not ItemClassModifier itemClassModifier) continue;

                // Check if the modifier has the specified tags and is degradable.
                if (itemClassModifier.HasAnyTags(tags) && ItemDegradationHelpers.CanDegrade(mod))
                {
                    // Return true as soon as a degraded mod is found, no need to check the rest.
                    if (ItemDegradationHelpers.IsDegraded(mod))
                    {
                        return true;
                    }
                }
            }
            // No degraded mods with the specified tags were found.
            return false;
        }
    }
}