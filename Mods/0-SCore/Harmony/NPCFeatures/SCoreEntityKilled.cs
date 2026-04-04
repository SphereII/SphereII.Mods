using Platform;
using UnityEngine;

public class SCoreEntityKilled
{
    // Call this from your Mod's Init() or Awake()
    public static void Init()
    {
        ModEvents.EntityKilled.RegisterHandler(OnEntityKilled);
    }

    private static void OnEntityKilled(ref ModEvents.SEntityKilledData data)
    {
        // 1. Safety & Authority Checks
        // We only want this running on the Server to prevent "Double XP" (Client + Server both adding it).
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;

        EntityAlive victim = data.KilledEntitiy as EntityAlive;
        EntityAlive killer = data.KillingEntity as EntityAlive;

        if (victim == null || killer == null) return;

        // 2. Filter for your Custom NPC Class
        // We cast to EntityAliveSDX to ensure this is one of your hired NPCs.
        var entityId = 1;
        var npcKiller = killer as EntityAliveSDX;
        if (npcKiller != null)
        {
            entityId = npcKiller.entityId;
            // 3. Give XP to the NPC (The Killer)
            // Since the NPC did the work, we manually ensure they get the raw XP for the kill.
            if (npcKiller.Progression != null)
            {
                int baseXP = EntityClass.list[victim.entityClass].ExperienceValue;
                npcKiller.Progression.AddLevelExp(baseXP, "_xpFromKill", Progression.XPTypes.Kill, true);
            }
        }
        else
        {
            var npcKillerV4 = killer as EntityAliveSDXV4;
            if (npcKillerV4 != null)
            {
                entityId = npcKillerV4.entityId;

                // 3. Give XP to the NPC (The Killer)
                // Since the NPC did the work, we manually ensure they get the raw XP for the kill.
                if (npcKillerV4.Progression != null)
                {
                    int baseXP = EntityClass.list[victim.entityClass].ExperienceValue;
                    npcKillerV4.Progression.AddLevelExp(baseXP, "_xpFromKill", Progression.XPTypes.Kill, true);
                }
            }
        }


        // 4. Delegate to Leader
        // We find the leader and tell the game "The Leader killed this".
        EntityPlayer leader = EntityUtilities.GetLeaderOrOwner(entityId) as EntityPlayer;
        if (leader != null)
        {
            // By calling AddKillXP on the leader, we leverage the logic we fixed earlier:
            // - It calculates the Leader's XP (Taxed by Party).
            // - It distributes shares to Party members.
            // - It updates Quests (SharedKillServer) for the whole party.
            leader.AddKillXP(victim);
        }
    }
}