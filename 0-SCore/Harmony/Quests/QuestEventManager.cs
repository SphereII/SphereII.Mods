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
        private static void Postfix(Entity entity)
        {
            switch (entity)
            {
                case EntityEnemySDX:
                    SCoreQuestEventManager.Instance.EntityEnemyKilled(EntityClass.list[entity.entityClass].entityClassName);
                    return;
                case EntityAliveSDX:
                    SCoreQuestEventManager.Instance.EntityAliveKilled(EntityClass.list[entity.entityClass].entityClassName);
                    return;
            }
        }
    }
}
