using HarmonyLib;

namespace Harmony.ItemActions
{
    /**
     * SCore_AdvancedItems_DurabilityAffectsDamagey
     * 
     * This class includes a Harmony patches to durability to have an effect against the damage. The lower the durability, the lower the damage.
     */
    public class DurabilityAffectsDamage
    {
        private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
        private static readonly string Feature = "DurabilityAffectsDamage";

        // Adds new feature where Durability affects the damage a weapon can do.
        [HarmonyPatch(typeof(ItemActionAttack))]
        [HarmonyPatch("Hit")]
        public class ItemActionHit
        {
            public static bool Prefix(ItemActionAttack __instance, ItemActionAttack.AttackHitInfo _attackDetails, ref float _weaponCondition, int _attackerEntityId)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                var entityAlive = GameManager.Instance.World.GetEntity(_attackerEntityId) as global::EntityAlive;
                if (!entityAlive) return true;

                var itemValue = entityAlive.inventory.holdingItemItemValue;
                if (!itemValue.HasQuality || !(entityAlive is EntityPlayerLocal)) return true;

                var strDisplay = "";
                if (_attackDetails.WeaponTypeTag.Equals(ItemActionAttack.MeleeTag))
                {
                    strDisplay += " Melee ";
                    var percent = itemValue.PercentUsesLeft;
                    if (percent > 0.8f)
                        _weaponCondition = 1f;
                    else if (percent > 0) // Perfecent left will be 0 on non-degradation things
                        _weaponCondition = percent;
                }
                else if (_attackDetails.WeaponTypeTag.Equals(ItemActionAttack.RangedTag))
                {
                    strDisplay += " Ranged ";
                }

                strDisplay += itemValue.ItemClass.GetItemName() + " Percent Left: " + itemValue.PercentUsesLeft + " Weapon Condition: " + _weaponCondition;
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay);

                return true;
            }
        }

        [HarmonyPatch(typeof(ItemActionAttack))]
        [HarmonyPatch("DegradationModifier")]
        public class ItemActionDegradationModifier
        {
            public static bool Prefix(ref float __result, ItemActionAttack __instance, float _strength, float _condition)
            {
                // If it's full, then just work with the base class.
                if (_condition == 1f)
                    return true;

                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                // Condition will be the Percent left of the item
                // Reduce damage based on durability left.
                __result = _strength * _condition;
                AdvLogging.DisplayLog(AdvFeatureClass, "New Calculated Damage: " + __result);

                return false;
            }
        }
    }
}