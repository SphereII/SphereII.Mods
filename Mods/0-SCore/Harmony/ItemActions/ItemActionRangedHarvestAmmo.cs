using HarmonyLib;

namespace Harmony.ItemActions
{
    // NPC RELOAD FROM THE HARVEST WINDOW.
    //
    // A leader hands an NPC ammunition by dropping it into the NPC's inventory window, which is
    // the HarvestManager container. Stock's reload path only ever looks at the entity's bag and
    // toolbelt: ItemActionRanged.CanReload gates on those two stores, and CompleteReload consumes
    // from them. Ammunition handed over through the window could therefore never start, let alone
    // finish, a reload. These postfixes add the harvest container as an additional source,
    // consulted only after the stock stores, writing with the same semantics DecItemFromAnyStore
    // uses (items[] + UpdateSlot; no SetModified, no network packet).
    //
    // Two surfaces, both postfixes - the originals are never altered, and for anyone outside the
    // gate the added cost is one cast and one dictionary lookup:
    //
    //  1. CanReload - stock returns false when bag and toolbelt hold no ammo, so a dry gun whose
    //     only ammo sits in the window would never begin reloading. The postfix flips false to
    //     true only when the gate passes, stock's own outer conditions could have allowed a
    //     reload (not already reloading, and the clip is short or the gun is jammed), and the
    //     container actually holds the selected ammo. A stock true is never touched, and a full,
    //     un-jammed gun is never flipped.
    //
    //  2. CompleteReload - runs after the stock bag/toolbelt consumption. If the magazine is
    //     still short, the deficit comes from the container and the magazine is topped up the way
    //     stock tops it up (same magazine size, same sound-id reset). Stock's own branches are
    //     untouched.
    //
    // THE GATE (why this cannot reach a player, a zombie, or a framework NPC):
    //   - holdingEntity must be an EntityAliveSDXV4. Players are EntityPlayer/EntityPlayerLocal
    //     and base zombies are EntityZombie, so the cast can never match them; the stock path
    //     runs untouched for them, for other mods' entities and for base traders.
    //   - V4 only, deliberately. EntityAliveSDXV4 is a SIBLING of the legacy EntityAliveSDX (both
    //     extend EntityTrader directly), not a subclass, so an EntityAliveSDX gate would be dead
    //     for a V4 entity and reload-from-window would never run. Gating on V4 alone also keeps
    //     NPC-framework NPCs, which are legacy, untouched by construction. Do not "helpfully"
    //     widen this to both generations.
    //   - HarvestManager.Has(entityId) must be true. The container exists only because someone
    //     opened this NPC's inventory window or a harvest flow created it; a reload must never
    //     spawn one. A V4 entity with no container is a no-op.
    //
    // MULTIPLAYER: an NPC's reload is driven by its server-side AI, which calls requestReload ->
    // GameManager.ItemReloadServer. The server runs the reload animator state locally and
    // broadcasts NetPackageItemReload, so CompleteReload runs on the server, where the
    // HarvestManager dictionary is authoritative. A dedicated-server client plays the animation
    // on its entity copy, but its HarvestManager dictionary stays empty - an open window's client
    // container lives in ClientPendingContainer, a different field - so Has() is false and both
    // postfixes are no-ops. SP and listen server are single-process: one animator run, one
    // decrement.
    //
    // GetReloadFlags, the player-UI ammo indicator, still checks only bag and toolbelt. It is
    // player-UI only and an NPC has no ammo crosshair, so it is left stock on purpose.
    public class ItemActionRangedHarvestAmmo
    {
        private static bool GatePasses(ItemActionRanged.ItemActionDataRanged adr)
        {
            var entity = adr?.invData?.holdingEntity;
            if (entity == null)
                return false;

            // Stock CompleteReload dereferences holdingEntity.bag with no null check, so a
            // bagless NPC must never be flipped to "can reload" - the stock method would throw
            // before the CompleteReload postfix ever ran. The window reload is simply unavailable
            // to bagless classes, exactly as in stock; a class with BagItems or a LootList has a
            // bag. This single choke point covers both postfixes.
            if (entity.bag == null)
                return false;

            if (!(entity is EntityAliveSDXV4))
                return false;

            // Has(), never GetOrCreate(): a reload must not create containers.
            return HarvestManager.Has(entity.entityId);
        }

        [HarmonyPatch(typeof(ItemActionRanged))]
        [HarmonyPatch("CanReload")]
        public class CanReloadHarvest
        {
            public static void Postfix(ItemActionRanged __instance, ItemActionData _actionData, ref bool __result)
            {
                if (__result)
                    return; // stock already allows the reload - leave it alone

                var adr = _actionData as ItemActionRanged.ItemActionDataRanged;
                if (!GatePasses(adr))
                    return;
                if (__instance.HasInfiniteAmmo(_actionData))
                    return;
                if (__instance.MagazineItemNames == null)
                    return;

                // Mirror stock's own outer conditions so a full, un-jammed gun is never flipped
                // and a reload is never re-requested mid-reload.
                if (!ItemActionRanged.NotReloading(adr))
                    return;

                var holdingItemValue = adr.invData.holdingEntity.inventory.holdingItemItemValue;
                var magazineSize = (int)EffectManager.GetValue(PassiveEffects.MagazineSize, holdingItemValue,
                    __instance.BulletsPerMagazine, adr.invData.holdingEntity);
                if (adr.invData.itemValue.Meta >= magazineSize &&
                    !holdingItemValue.TryGetMetadata(ItemActionRanged.scGunIsJammed, out int _))
                    return;

                // Does the window hold the selected ammo? Stock already checked bag and toolbelt -
                // that is why __result is false.
                var ammo = ItemClass.GetItem(__instance.MagazineItemNames[adr.invData.itemValue.SelectedAmmoTypeIndex], false);
                if (ammo == null || ammo.IsEmpty())
                    return;

                var container = HarvestManager.GetOrCreate(adr.invData.holdingEntity.entityId);
                if (container?.items == null)
                    return;

                foreach (var stack in container.items)
                {
                    if (!EntityUtilities.MatchesItem(stack, ammo)) continue;
                    __result = true;
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(ItemActionRanged))]
        [HarmonyPatch("CompleteReload")]
        public class CompleteReloadHarvest
        {
            public static void Postfix(ItemActionRanged __instance, ItemActionRanged.ItemActionDataRanged _adr)
            {
                if (!GatePasses(_adr))
                    return;
                if (__instance.HasInfiniteAmmo(_adr))
                    return; // stock consumed nothing and tops up for free
                if (__instance.MagazineItemNames == null)
                    return;

                var entity = _adr.invData.holdingEntity;
                var ammo = ItemClass.GetItem(__instance.MagazineItemNames[_adr.invData.itemValue.SelectedAmmoTypeIndex], false);
                if (ammo == null || ammo.IsEmpty())
                    return;

                var container = HarvestManager.GetOrCreate(entity.entityId);
                if (container?.items == null)
                    return;

                // The magazine size, computed the way the stock method computes it. Nothing runs
                // between the stock call and this postfix that could change passive effects.
                var magazineSize = (int)EffectManager.GetValue(PassiveEffects.MagazineSize,
                    _adr.invData.itemValue, __instance.BulletsPerMagazine, entity);

                int topUp;
                if (__instance.AmmoIsPerMagazine)
                {
                    // Stock consumes one magazine item per reload when it finds one. A clip still
                    // short after stock ran means it found none - take one from the window.
                    if (_adr.invData.itemValue.Meta >= magazineSize)
                        return;
                    topUp = magazineSize * EntityUtilities.DecItemFromLootContainer(container, ammo, 1);
                }
                else
                {
                    // Loose ammo: top up whatever bag and toolbelt could not.
                    var deficit = magazineSize - _adr.invData.itemValue.Meta;
                    if (deficit <= 0)
                        return;
                    topUp = EntityUtilities.DecItemFromLootContainer(container, ammo, deficit);
                }

                if (topUp <= 0)
                    return;

                // Top up the magazine exactly the way the stock method does. Stock's final jam
                // clear already ran before this postfix, so it is not repeated.
                _adr.reloadAmount += topUp;
                _adr.invData.itemValue.Meta = Utils.FastMin(_adr.invData.itemValue.Meta + topUp, magazineSize);
                if (_adr.invData.item?.Properties.GetValue(ItemClass.PropSoundIdle) != null)
                    _adr.invData.holdingEntitySoundID = -1;
            }
        }
    }
}
