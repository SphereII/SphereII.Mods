using HarmonyLib;

namespace SphereII.FoodSpoilage.HarmonyPatches
{
    /// <summary>
    /// Stops a held item holstering and re-drawing itself every time it ages.
    /// <para>
    /// Spoilage lives entirely in item metadata - NextSpoilageTick, SpoilageValue and Freshness -
    /// and the game's "has the held item changed enough to rebuild the model?" test compares
    /// metadata. So every spoilage tick reads on screen as a reload: the item holsters, plays the
    /// holster sound, re-draws and fires the item-has-changed animation trigger.
    /// </para>
    /// <para>
    /// v3.3 moved that test. It used to sit at the top of
    /// <c>Inventory.SetItem(int, ItemValue, int, bool)</c>, which no longer exists; held-item state
    /// now belongs to <c>Hand</c>, and the decision is
    /// <c>ItemInventoryData.NeedsRebuild</c>:
    /// </para>
    /// <code>
    /// return !itemValue.EqualsExceptUseTimesAndMeta(cachedItemValue);
    /// </code>
    /// <para>
    /// The name is misleading - <c>EqualsExceptUseTimesAndMeta</c> still compares
    /// <c>Metadata</c> - so the original problem survives the refactor.
    /// </para>
    /// <para>
    /// The fix keeps the same shape as before: copy the live value's spoilage keys onto the stale
    /// side of the comparison so they stop counting as a change. <c>cachedItemValue</c> is a
    /// throwaway clone taken during Build and discarded on Teardown, used only for this test and
    /// for <c>BuiltItemValue</c>, so writing to it cannot corrupt the real stack's spoilage data.
    /// Anything else about the item - quality, mods, a genuine swap - still rebuilds exactly as
    /// vanilla intends.
    /// </para>
    /// </summary>
    [HarmonyPatch(typeof(ItemInventoryData))]
    [HarmonyPatch(nameof(ItemInventoryData.NeedsRebuild), MethodType.Getter)]
    public class ItemInventoryDataNeedsRebuildSpoilage
    {
        private static readonly string[] SpoilageKeys =
        {
            SpoilageConstants.MetaNextSpoilageTick,
            SpoilageConstants.MetaSpoilageAmount,
            SpoilageConstants.MetaFreshness
        };

        public static void Prefix(ItemInventoryData __instance)
        {
            if (!SpoilageConfig.IsFoodSpoilageEnabled) return;

            var cached = __instance.itemValue;
            if (cached == null) return;

            var live = __instance.stack?.itemValue;
            if (live == null || live.type == 0) return;

            // A real swap to a different item should still rebuild.
            if (live.type != cached.type) return;

            // Confine this to food that actually spoils.
            var itemClass = live.ItemClass;
            if (itemClass == null || !itemClass.Properties.GetBool(SpoilageConstants.PropSpoilable))
                return;

            foreach (var key in SpoilageKeys)
            {
                if (live.Metadata != null && live.Metadata.TryGetValue(key, out var value))
                    cached.SetMetadata(key, value);
                else
                    cached.Metadata?.Remove(key);
            }
        }
    }
}
