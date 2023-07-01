using HarmonyLib;
namespace Features.Fire.Harmony
{
    // Allows the spread of the particles to catch things on fire.
    [HarmonyPatch(typeof(Explosion))]
    [HarmonyPatch("AttackBlocks")]
    public class SCoreExplosionattackBlocks
    {
        public static void Postfix(Explosion __instance, int _entityThatCausedExplosion, ExplosionData ___explosionData)
        {
            if (FireManager.Instance == null) return;
            if (FireManager.Instance.Enabled == false) return;

            // BlockDamage set to 0 does nothing.
            if (___explosionData.BlockDamage == 0) return;

            var entityAlive = GameManager.Instance.World.GetEntity(_entityThatCausedExplosion) as EntityAlive;
            if (entityAlive != null)
            {
                if (entityAlive.EntityClass.Properties.Contains("SpreadFire"))
                {
                    if (entityAlive.EntityClass.Properties.GetBool("SpreadFire") == false)
                        return;
                }

                if (entityAlive.Buffs.HasCustomVar("SpreadFire"))
                {
                    var spreadFire = entityAlive.Buffs.GetCustomVar("SpreadFire");
                    if (spreadFire == -1f) return;
                }
            }

            foreach (var position in __instance.ChangedBlockPositions)
            {
                // Negative block damages extinguishes
                if (___explosionData.BlockDamage < 0f)
                    FireManager.Instance.Extinguish(position.Key);
                else
                    FireManager.Instance.Add(position.Key);
            }
        }
    }
}