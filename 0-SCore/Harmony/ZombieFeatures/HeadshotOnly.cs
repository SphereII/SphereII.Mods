using HarmonyLib;

namespace Harmony.ZombieFeatures
{
    /**
     * SCoreHeadshotOnly
     * 
     * This class includes a Harmony patch force head-shot only, enabled via the Config/blocks.xml
     */
    public class SCoreHeadshotOnly
    {
        private static readonly string AdvFeatureClass = "AdvancedZombieFeatures";
        private static readonly string Feature = "HeadshotOnly";

        // Give a damage boost to headshots
        [HarmonyPatch(typeof(global::EntityAlive))]
        [HarmonyPatch("DamageEntity")]
        public class EntityAliveDamageEntity
        {
            public static bool Prefix(global::EntityAlive __instance, ref DamageSource _damageSource, ref int _strength, bool _criticalHit, float _impulseScale)
            {
                // Apply a damage boost if there is a head shot.
                if (!(__instance is EntityZombie)) return true;

                // No head shots for snakes.
                if (__instance is EntityAnimalSnake)
                    return true;

                if (_strength > 999)
                {
                    AdvLogging.DisplayLog(AdvFeatureClass, " Massive Damage Detected. Falling back to base");
                    return true;
                }

                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;

                // If its not a player, deal with default
                var entityAlive = GameManager.Instance.World.GetEntity(_damageSource.getEntityId()) as EntityPlayerLocal;
                if (entityAlive == null)
                    return true;

                var bodyPart = _damageSource.GetEntityDamageBodyPart(__instance);
                if (bodyPart == EnumBodyPartHit.Head)
                {
                    AdvLogging.DisplayLog(AdvFeatureClass, " Headshot Mode Active: Headshot! ");
                    // Apply a damage multiplier for the head shot, and bump the dismember bonus for the head shot
                    // This will allow the head to splode of, which accordingly to legendary legend, is the only went to truly kill a zombie.
                    _damageSource.DamageMultiplier = 1f;
                    // _strength = 1;
                    _damageSource.DismemberChance = 0.8f;
                }
                // Reducing the damage to the torso will prevent the entity from being killed by torso shots, while also maintaining de-limbing.
                else
                {
                    AdvLogging.DisplayLog(AdvFeatureClass, " Headshot Mode Active: Non-Headshot");
                    _damageSource.DamageMultiplier = 0.1f;
                    _strength = 1;
                }

                return true;
            }
        }
    }
}