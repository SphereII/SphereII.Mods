using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SCoreQuestEventManager
{
    private static SCoreQuestEventManager instance = null;

    public delegate void QuestEvent_EntityAliveSDXKilled(string name);
    public event QuestEvent_EntityAliveSDXKilled EntityAliveSDXKill;

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
}

