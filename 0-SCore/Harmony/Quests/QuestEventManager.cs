using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
namespace SCore.Harmony.Quests
{
    [HarmonyPatch(typeof(QuestEventManager))]
    [HarmonyPatch("EntityKilled")]
    public class QuestEventManagerEntityKilled
    {
        private static void Postfix(EntityAlive killedEntity)
        {
            switch (killedEntity)
            {
                case EntityEnemySDX:
                    SCoreQuestEventManager.Instance.EntityEnemyKilled(EntityClass.list[killedEntity.entityClass].entityClassName);
                    return;
                case EntityAliveSDX:
                    SCoreQuestEventManager.Instance.EntityAliveKilled(EntityClass.list[killedEntity.entityClass].entityClassName);
                    return;
            }
        }
    }
}
