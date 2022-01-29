using HarmonyLib;
using JetBrains.Annotations;

namespace Harmony.NPCFeatures
{

    public class EntityAlivePatcherAwardKill
    {

        [HarmonyPatch(typeof(global::EntityAlive))]
        [HarmonyPatch("AwardKill")]
        public class EntityAliveAwardKill
        {
            private static void Postfix(global::EntityAlive __instance, global::EntityAlive killer)
            {
                if (killer == null) return;

                var playerkill = 0;
                var zombiekill = 0;

                var entityAliveSDX = killer as EntityAliveSDX;
                if (entityAliveSDX == null) return;

                if (__instance is EntityPlayer)
                    playerkill++;
                else
                    zombiekill++;

                Log.Out($"AwardKill to {entityAliveSDX.EntityName} {zombiekill} {playerkill} ");
                entityAliveSDX.AddKillXP(__instance, 1f);
                GameManager.Instance.AwardKill(killer, __instance);
                GameManager.Instance.AddScoreServer(killer.entityId, zombiekill, playerkill, __instance.TeamNumber, 0);

            }
        }
    }
}