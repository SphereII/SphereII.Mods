using HarmonyLib;

/**
 * SphereII__SoftHands
 *
 * This class includes a Harmony patches to ItemAction to deal damage when the player hits something with their bare heands.
 * 
 */
public class SphereII__SoftHands
{
    private static readonly string AdvFeatureClass = "AdvancedPlayerFeatures";
    private static readonly string Feature = "SoftHands";

    // Adds new feature where Durability affects the damage a weapon can do.
    [HarmonyPatch(typeof(ItemActionAttack))]
    [HarmonyPatch("Hit")]
    public class SphereII_ItemAction_Hit_EntityPlayerLocal
    {
        public static void Postfix(ItemActionAttack __instance, ItemActionAttack.AttackHitInfo _attackDetails, ref float _weaponCondition, int _attackerEntityId, ItemValue damagingItemValue)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return;

            EntityAlive entityAlive = GameManager.Instance.World.GetEntity(_attackerEntityId) as EntityAlive;
            if (entityAlive)
            {
                bool isWearingGloves = false;

                // Throw weapon, skipping
                if (damagingItemValue != null && damagingItemValue.ItemClass.HasAnyTags(FastTags.Parse("thrownWeapon")))
                    return;

                // Check if its the player hand
                if (entityAlive.inventory.holdingItem.GetItemName() == "meleeHandPlayer" && _attackDetails.damageGiven > 0 && !isWearingGloves)
                {
                    AdvLogging.DisplayLog(AdvFeatureClass, "Attacking Entity is an EntityAlive: " + entityAlive.inventory.holdingItemItemValue.ItemClass.GetItemName() + " Inflicting Damage");
                    DamageSource dmg = new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Bashing);
                    entityAlive.DamageEntity(dmg, 1, false, 1f);
                }
            }

            return;
        }
    }


}