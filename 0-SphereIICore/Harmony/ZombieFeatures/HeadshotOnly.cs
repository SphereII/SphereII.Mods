using HarmonyLib;


/**
 * SphereII_HeadshotOnly
 *
 * This class includes a Harmony patch force head-shot only, enabled via the Config/blocks.xml
 */
public class SphereII_HeadshotOnly
{
    private static readonly string AdvFeatureClass = "AdvancedZombieFeatures";
    private static readonly string Feature = "HeadshotOnly";

    // Give a damage boost to headshots
    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("DamageEntity")]
    public class SphereII_EntityAlive_DamageEntity
    {
        public static bool Prefix(EntityAlive __instance, ref DamageSource _damageSource, ref int _strength, bool _criticalHit, float _impulseScale)
        {
            // Apply a damage boost if there is a head shot.
            if (__instance is EntityZombie)
            {
                // No head shots for snakes.
                if (__instance is EntityAnimalSnake)
                    return true;

                if (_strength > 999)
                {
                    AdvLogging.DisplayLog(AdvFeatureClass, " Massive Damage Detected. Falling back to base");
                    return true;
                }
                if (Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                {
                    // If its not a player, deal with default
                    EntityPlayerLocal entityAlive = GameManager.Instance.World.GetEntity(_damageSource.getEntityId()) as EntityPlayerLocal;
                    if (entityAlive == null)
                        return true;

                    EnumBodyPartHit bodyPart = _damageSource.GetEntityDamageBodyPart(__instance);
                    if (bodyPart == EnumBodyPartHit.Head)
                    {
                        AdvLogging.DisplayLog(AdvFeatureClass, " Headshot Mode Active: Headshot! ");
                        // Apply a damage multiplier for the head shot, and bump the dismember bonus for the head shot
                        // This will allow the heads to go explode off, which according to legend, if the only want to truly kill a zombie.
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
                }

            }
            return true;
        }
    }

}

