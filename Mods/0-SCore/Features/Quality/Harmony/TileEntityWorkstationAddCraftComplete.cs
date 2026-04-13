using HarmonyLib;

namespace SCore.Features.Quality.Harmony
{
    [HarmonyPatch(typeof(TileEntityWorkstation))]
    [HarmonyPatch(nameof(TileEntityWorkstation.AddCraftComplete))]
    public class TileEntityWorkstationAddCraftComplete
    {
        private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
        private static readonly string Feature = "CustomQualityLevels";

        public static bool Prefix(TileEntityWorkstation __instance, ref int crafterEntityID, ItemValue itemCrafted)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;

            // Check if the ID is packed (top 16 bits are non-zero).
            // Plain player entity IDs never exceed 65535, so any value in the top 16 bits
            // means we encoded quality there via XUiCWorkstationWindowGroupSyncTEFromUI.
            if (((crafterEntityID >> 16) & 0xFFFF) != 0)
            {
                int realQuality  = (crafterEntityID >> 16) & 0xFFFF;
                int realPlayerID =  crafterEntityID        & 0xFFFF;

                if (itemCrafted != null)
                    itemCrafted.Quality = (ushort)realQuality;

                crafterEntityID = realPlayerID;
            }

            // Drop the cache entry for this tile entity — the craft is done.
            if (__instance != null)
            {
                var tePos = __instance.ToWorldPos();
                XUiCWorkstationWindowGroupSyncTEFromUI.QualityCache.Remove(tePos);
            }

            return true;
        }
    }
}
