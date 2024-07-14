using HarmonyLib;

namespace Harmony.AIDirectorBloodMoon
{
    /// <summary>
    /// Location: Harmony.AIDirectorBloodMoon
    /// Allows zombies to keep attacking their target if its already set. This allows the zombies to focus on NPCs, as well, rather than blind focus on the player.
    /// </summary>
    public class AiDirectorBloodMoonParty
    {
        [HarmonyPatch(typeof(AIDirectorBloodMoonParty))]
        [HarmonyPatch("SeekTarget")]
        public static bool Prefix(ref bool __result, ManagedZombie mz)
        {
            global::EntityAlive zombie = mz.zombie;
            if (!zombie || zombie.IsDead() || zombie.IsDespawned || !zombie.gameObject)
                return true;

            // Check if the zombie has an attack or revenge target, and if not, allow them to target a player.
            if (!(EntityUtilities.GetAttackOrRevengeTarget(zombie.entityId) is global::EntityAlive))
                return true;

            __result = true;
            return false;
        }

        // This is a copy of the ManagedZombie class, which is private in AIDirectorBloodMoonParty. Re-defined here so that the Prefix can access the parameter
        public class ManagedZombie
        {
            private readonly EntityPlayer _player;
            public readonly EntityEnemy zombie;
            public float updateDelay;

            public ManagedZombie(EntityEnemy zombie, EntityPlayer player)
            {
                this.zombie = zombie;
                _player = player;
            }
        }
    }
}