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
                {
                    itemCrafted.Quality = (ushort)realQuality;

                    // The ItemValue constructor sizes Modifications based on the random quality it
                    // generated during construction.  Now that we've set the real quality we need
                    // to resize the array so the item has the correct number of mod slots.
                    int correctSlots = UnityEngine.Mathf.Clamp(
                        (int)EffectManager.GetValue(
                            PassiveEffects.ModSlots, itemCrafted,
                            (float)Utils.FastMax(0, realQuality - 1),
                            null, null,
                            default(FastTags<TagGroup.Global>),
                            true, true, true, true, true, 1, true, false),
                        0, 255);

                    // v3.3: modifications is the raw backing field and is null when the item has
                    // no mod slots yet, so measure with ModificationCount rather than .Length.
                    var existingSlots = itemCrafted.ModificationCount;
                    if (existingSlots != correctSlots)
                    {
                        var resized = new ItemValue[correctSlots];
                        // Preserve any mods that already fit (e.g. default mod items from recipe).
                        for (int i = 0; i < UnityEngine.Mathf.Min(existingSlots, correctSlots); i++)
                            resized[i] = itemCrafted.GetModification(i);
                        // Fill any new empty slots.
                        for (int i = existingSlots; i < correctSlots; i++)
                            resized[i] = ItemValue.None.Clone();
                        itemCrafted.modifications = resized;
                    }
                }

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
