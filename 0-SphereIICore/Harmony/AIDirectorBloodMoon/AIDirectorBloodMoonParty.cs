using HarmonyLib;
public class SphereII_AIDirectorBloodMoonParty
{

    [HarmonyPatch(typeof(AIDirectorBloodMoonParty))]
    [HarmonyPatch("SeekTarget")]
    public class SphereII_AIDirectorBloodMoonParty_SeekTarget
    {
        // This is a copy of the ManagedZombie class, which is private in AIDirectorBloodMoonParty. Re-defined here so that the Prefix can access the parameter
        public class ManagedZombie
        {
            public ManagedZombie(EntityEnemy _zombie, EntityPlayer _player)
            {
                zombie = _zombie;
                player = _player;
            }

            public EntityPlayer player;
            public EntityEnemy zombie;
            public float updateDelay;
        }
        public static bool Prefix(ref bool __result, ManagedZombie mz)
        {
            EntityAlive zombie = mz.zombie;
            if (!zombie || zombie.IsDead() || zombie.IsDespawned || !zombie.gameObject)
                return true;

            // Check if the zombie has an attack or revenge target, and if not, allow them to target a player.
            EntityAlive entity = EntityUtilities.GetAttackOrReventTarget(zombie.entityId) as EntityAlive;
            if (entity == null)
                return true;

            __result = true;
            return false;
        }
    }

}