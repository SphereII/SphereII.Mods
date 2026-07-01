using System.IO;
using HarmonyLib;

namespace SCore.Features.ErrorChecks.Harmony {
    public class TraderDataReadInventoryData {
        [HarmonyPatch(typeof(TraderData))]
        [HarmonyPatch("ReadInventoryData")]

        public class TraderDataReadInventoryDataPatch
        {
            private static readonly string AdvFeatureClass = "ErrorHandling";
            private static readonly string Feature = "TraderDataReadInventory";

            public static bool Prefix (ref TraderData __instance, BinaryReader _br )
            {
                // Disabled: TraderData API changed in new game version (priceMarkupList removed,
                // PrimaryInventory changed type). Let vanilla handle deserialization.
                return true;
            }
        }
    }
}