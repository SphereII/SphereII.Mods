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
    public class QuestEventManager_EntityKilled
    {
        private static void Postfix(Entity entity)
        {
            if (entity is EntityEnemySDX )
            {
                SCoreQuestEventManager.Instance.EntityEnemyKilled(EntityClass.list[entity.entityClass].entityClassName);
                return;
            }
            if (entity is EntityAliveSDX)
            {
                SCoreQuestEventManager.Instance.EntityAliveKilled(EntityClass.list[entity.entityClass].entityClassName);
                return;
            }
        }
    }
}
