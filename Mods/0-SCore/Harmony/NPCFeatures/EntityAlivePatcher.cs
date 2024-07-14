using HarmonyLib;
using JetBrains.Annotations;

namespace Harmony.NPCFeatures
{
    /**
     * SCore_EntityAlivePatcher
     * 
     * This class includes a Harmony patches to EntityAlive to provide more features for NPCs, including blocking EntityAliveFarmingAnimalSDX from attacking blocks,
     * to disabling the damage to an ally,  etc
     */
    public class UtilsDrawLine
    {
        private static readonly string AdvFeatureClass = "AdvancedNPCFeatures";
        private static readonly string Feature = "EnhancedFeatures";


        //[HarmonyPatch(typeof(global::EntityAlive))]
        //[HarmonyPatch("Attack")]
        //public class EntityAliveAttack
        //{
        //    private static bool Prefix(global::EntityAlive __instance)
        //    {
        //        // Check if this feature is enabled.
        //        if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
        //            return true;

        //        // Check if there's a door in our way, then open it.
        //        if (__instance.GetAttackTarget() == null)
        //        {
        //            // If it's an animal, don't let them attack blocks
        //            var animal = __instance as EntityAliveFarmingAnimalSDX;
        //            if (animal)
        //                if (__instance.GetAttackTarget() == null)
        //                    return false;
        //        }

        //        if (__instance.GetAttackTarget() != null)
        //            __instance.RotateTo(__instance.GetAttackTarget(), 30f, 30f);

        //        return true;
        //    }
        //}


        // Disables the friendly fire of allies
        [HarmonyPatch(typeof(global::EntityAlive))]
        [HarmonyPatch("DamageEntity")]
        public class EntityAliveDamageEntity
        {
            private static bool Prefix(global::EntityAlive __instance, ref int __result, DamageSource _damageSource)
            {

                // allow an override for the action stuff. If its set here, then use faction targeting.
                if (!EntityUtilities.CheckProperty(__instance.entityId, "AllEntitiesUseFactionTargeting"))
                {
                    // New feature flag, specific to this feature.
                    if (!Configuration.CheckFeatureStatus(AdvFeatureClass, "AllEntitiesUseFactionTargeting"))
                    {
                        // Even if *all* entities don't use faction damage rules, this entity should
                        // if it has one of the UseFactions tags. Only stop now if it doesn't.
                        if (!EntityUtilities.UseFactions(__instance))
                        {
                            return true;
                        }
                    }
                }
                if (_damageSource.damageType == EnumDamageTypes.Suicide)
                    return true;

                // We have to use a different damge test for players, due to multiplayer.
                // There is no NetPackage for setting a revenge target. So, if the player is a
                // revenge target of the damage source, it won't be set locally, and it won't be
                // considered in the targeting tests. Instead, only avoid damage from our allies.
                if (__instance is EntityPlayer)
                {
                    return !EntityTargetingUtilities.IsAlly(
                        __instance.world.GetEntity(_damageSource.getEntityId()),
                        __instance);
                }
                
                return EntityTargetingUtilities.CanTakeDamage(
                    __instance,
                    __instance.world.GetEntity(_damageSource.getEntityId()));
            }
        }

        [HarmonyPatch(typeof(Entity))]
        [HarmonyPatch("CanDamageEntity")]
        public class EntityPlayerCanDamageEntity
        {
            private static bool Prefix(Entity __instance, int _sourceEntityId)
            {
                // This should work on multiplayer, because it is called by ItemActionAttack.Hit,
                // and that is run on the server. If this returns false, then the hit is ignored,
                // and a NetPackage is never even sent to the client.
                if (__instance is EntityPlayer player)
                {
                    return EntityTargetingUtilities.CanTakeDamage(
                        player,
                        __instance.world.GetEntity(_sourceEntityId));
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ItemActionAttack))]
        [HarmonyPatch("Hit")]
        public class ItemActionAttackHit
        {
            private static bool Prefix(
                ItemActionAttack __instance,
                WorldRayHitInfo hitInfo,
                int _attackerEntityId,
                ItemActionAttack.AttackHitInfo _attackDetails)
            {
                if (hitInfo?.tag == null) return false;

                var entity = ItemActionAttack.FindHitEntityNoTagCheck(hitInfo, out var text3);
                if (entity == null) return true;

                if (EntityUtilities.IsAnAlly(entity.entityId, _attackerEntityId))
                {
                    // This prevents the "infinite harvest bug"
                    _attackDetails.bBlockHit = false;
                    _attackDetails.bHarvestTool = false;
                    _attackDetails.itemsToDrop = null;

                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(global::EntityAlive))]
        [HarmonyPatch("SetAttackTarget")]
        public class EntityAliveSetAttackTarget
        {
            private static bool Prefix(global::EntityAlive __instance)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                // If a door is found, try to open it. If it returns false, start attacking it.
                var myEntity = __instance as EntityAliveSDX;
                if (myEntity)
                    myEntity.RestoreSpeed();
                return true;
            }
        }


        // NPCs cause an exception if they have the bleeding buff. Since the entity is dying and not respawning, let's just avoid removing the buffs
        [HarmonyPatch(typeof(EntityBuffs))]
        [HarmonyPatch("RemoveBuff")]
        public class EntityBuffsRemoveBuffs
        {
            private static bool Prefix([CanBeNull] EntityBuffs __instance)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                if (__instance == null || __instance.parent == null)
                    return false;

                if (!(__instance.parent is EntityAliveSDX)) return true;
                return !__instance.parent.IsDead();
            }
        }
    }
}