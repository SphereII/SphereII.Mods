using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SCoreQuestEventManager
{
    private static SCoreQuestEventManager instance = null;

    public delegate void QuestEvent_EntityAliveSDXKilled(string name);
    public delegate void QuestEvent_EntityEnemySDXKilled(string name);
    public event QuestEvent_EntityAliveSDXKilled EntityAliveSDXKill;
    public event QuestEvent_EntityEnemySDXKilled EntityEnemySDXKill;

    public static SCoreQuestEventManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new SCoreQuestEventManager();
            }
            return instance;
        }
    }


    public void EntityAliveKilled(string className)
    {
        if (EntityAliveSDXKill != null)
            EntityAliveSDXKill(className);
    }

    public void EntityEnemyKilled(string className)
    {
        if (EntityEnemySDXKill != null)
            EntityEnemySDXKill(className);
    }
}

