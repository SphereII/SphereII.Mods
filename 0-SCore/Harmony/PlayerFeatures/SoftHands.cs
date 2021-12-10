using HarmonyLib;

namespace Harmony.PlayerFeatures
{
    /**
     * SCore_SoftHands
     * 
     * This class includes a Harmony patches to ItemAction to deal damage when the player hits something with their bare heands.
     */
    public class SoftHands
    {
        private static readonly string AdvFeatureClass = "AdvancedPlayerFeatures";
        private static readonly string Feature = "SoftHands";

        // Adds new feature where Durability affects the damage a weapon can do.
        [HarmonyPatch(typeof(ItemActionAttack))]
        [HarmonyPatch("Hit")]
        public class ItemActionHitEntityPlayerLocal
        {
            public static void Postfix(ItemActionAttack __instance, ItemActionAttack.AttackHitInfo _attackDetails, ref float _weaponCondition, int _attackerEntityId, ItemValue damagingItemValue)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;

                var entityAlive = GameManager.Instance.World.GetEntity(_attackerEntityId) as global::EntityAlive;
                if (!entityAlive) return;

                var isWearingGloves = false;

                // Throw weapon, skipping
                if (damagingItemValue != null && damagingItemValue.ItemClass.HasAnyTags(FastTags.Parse("thrownWeapon")))
                    return;

                // Check if its the player hand
                if (entityAlive.inventory.holdingItem.GetItemName() != "meleeHandPlayer" || _attackDetails.damageGiven <= 0 || isWearingGloves) return;

                if (_attackDetails.bBlockHit)
                {
                    if (_attackDetails.blockBeingDamaged.Block.blockMaterial.Properties.Values.ContainsKey("DamageOnHit"))
                    {
                        var damageFromBlock = StringParsers.ParseSInt32(_attackDetails.blockBeingDamaged.Block.blockMaterial.Properties.Values["DamageOnHit"]);
                        var dmgFromBlock = new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Bashing);
                        entityAlive.DamageEntity(dmgFromBlock, damageFromBlock, false);
                        return;
                    }
                }
                AdvLogging.DisplayLog(AdvFeatureClass, "Attacking Entity is an EntityAlive: " + entityAlive.inventory.holdingItemItemValue.ItemClass.GetItemName() + " Inflicting Damage");
                var dmg = new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Bashing);
                entityAlive.DamageEntity(dmg, 1, false);
            }
        }
    }
}