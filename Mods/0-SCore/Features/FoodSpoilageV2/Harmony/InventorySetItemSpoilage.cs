using HarmonyLib;

namespace SphereII.FoodSpoilage.HarmonyPatches
{
    /// <summary>
    /// Stops a held item holstering and re-drawing itself every time it ages.
    /// <para>
    /// <c>Inventory.SetItem</c> opens with a re-show whenever the slot being written is the one
    /// the player is holding and the incoming value is not
    /// <c>EqualsExceptUseTimesAndAmmo</c> to the current one:
    /// </para>
    /// <code>
    /// if (_idx == holdingItemIdx &amp;&amp; !_itemValue.EqualsExceptUseTimesAndAmmo(slots[_idx].itemStack.itemValue))
    ///     ShowHeldItem(0.2f, hideFirst: true);
    /// </code>
    /// <para>
    /// That comparison includes the metadata dictionary, and spoilage lives entirely in metadata -
    /// NextSpoilageTick, SpoilageValue and Freshness. <c>ShowHeldItem</c> with
    /// <c>hideFirst</c> holsters the item, plays the holster sound, waits, re-draws it and fires
    /// the item-has-changed animation trigger, which reads on screen as a reload.
    /// </para>
    /// <para>
    /// The toolbelt makes it worse than one slot: a slot-changed event pushes every slot back
    /// through <c>Inventory.SetSlots</c>, which calls <c>SetItem</c> for all of them, so any stack
    /// spoiling anywhere on the belt drags the held slot through the comparison too.
    /// </para>
    /// <para>
    /// Copying just the spoilage keys onto the outgoing value before the comparison runs makes
    /// them stop counting as a change. Anything else about the item - quality, mods, a genuine
    /// swap - still re-shows exactly as vanilla intends. The value being written to is replaced by
    /// <c>_itemValue.Clone()</c> a few lines later regardless, so it is only alive for the test.
    /// </para>
    /// </summary>
    [HarmonyPatch(typeof(Inventory))]
    [HarmonyPatch(nameof(Inventory.SetItem))]
    [HarmonyPatch(new[] { typeof(int), typeof(ItemValue), typeof(int), typeof(bool) })]
    public class InventorySetItemSpoilage
    {
        private static readonly string[] SpoilageKeys =
        {
            SpoilageConstants.MetaNextSpoilageTick,
            SpoilageConstants.MetaSpoilageAmount,
            SpoilageConstants.MetaFreshness
        };

        public static void Prefix(Inventory __instance, int _idx, ItemValue _itemValue)
        {
            if (!SpoilageConfig.IsFoodSpoilageEnabled) return;
            if (_itemValue == null) return;

            // Only the held slot triggers the re-show, so nothing else needs touching.
            if (_idx != __instance.holdingItemIdx) return;

            var slots = __instance.slots;
            if (slots == null || _idx < 0 || _idx >= slots.Length) return;

            var held = slots[_idx]?.itemStack?.itemValue;
            if (held == null || held.type == 0) return;

            // A real swap to a different item should still re-show.
            if (held.type != _itemValue.type) return;

            // Confine this to food that actually spoils.
            var itemClass = held.ItemClass;
            if (itemClass == null || !itemClass.Properties.GetBool(SpoilageConstants.PropSpoilable))
                return;

            foreach (var key in SpoilageKeys)
            {
                if (_itemValue.Metadata != null && _itemValue.Metadata.TryGetValue(key, out var value))
                    held.SetMetadata(key, value);
                else
                    held.Metadata?.Remove(key);
            }
        }
    }
}
