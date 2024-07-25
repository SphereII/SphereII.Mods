using HarmonyLib;

namespace SCore.Harmony.TileEntities
{
    public class TileEntityLootContainerPatches
    {
        // in Alpha 21.1, NPCs on dedi will sometimes throw a null reference when grabbing the loot container.
        // This could be due to a timing issue, so we'll try this patch to get around it.
        [HarmonyPatch(typeof(TileEntityLootContainer))]
        [HarmonyPatch("GetOpenTime")]
        public class TileEntityLootContainerGetOpenTime
        {
            public static bool Prefix(TileEntityLootContainer __instance, ref float __result)
            {
                if (string.IsNullOrEmpty(__instance.lootListName))
                {
                    __result = 1.5f;
                    return false;
                }

                if (LootContainer.GetLootContainer(__instance.lootListName, true) == null)
                {
                    __result = 1.5f;
                    return false;
                }
                return true;
            }
        }
    }
}