using System.Collections.Generic;

public class NPCQuestUtils {
    private EntityAlive _entityAlive;
    public List<Quest> activeQuests;

        
    public NPCQuestUtils(EntityAlive entityAlive) {
        _entityAlive = entityAlive;
    }
}