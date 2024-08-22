public class SCoreKillTracker {

    
    public void Init() {
        
    }

    public void RegisterHandler() {
        QuestEventManager.Current.EntityKill += OnEntityKilled;
        EventOnBloodMoonStart.BloodMoonStart += OnBloodMoonStart;

    }

    private void OnBloodMoonStart() {
        
    }

    private void OnEntityKilled(EntityAlive killedby, EntityAlive killedentity) {
        
    }

    public void UnRegisterHandler(){}
    public void Update() {
        
    }
    
    
}