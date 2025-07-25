using System.Xml.Linq;
using HarmonyLib;

public class TestingHooksForPassive
{
    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch(nameof(EntityAlive.FireEvent))]
    public class EntityAliveFireEvent
    {
        private static EntityPlayerLocal _entityPlayerLocal;
        public static bool Prefix(EntityAlive __instance, MinEventTypes _eventType)
        {
            if (_entityPlayerLocal == null)
            {
                _entityPlayerLocal = GameManager.Instance.World.GetPrimaryPlayer();
                if (_entityPlayerLocal == null) return true;
            }

            if (_eventType.ToString().EndsWith("Update")) return true;
            
            var entityId = _entityPlayerLocal.Buffs.GetCustomVar("$fireeventtracker");
            if (entityId == 0) return true;
            if ((int)entityId == __instance.entityId)
            {
                if (EnumUtils.TryParse<SCoreMinEventTypes>(_eventType.ToString(), out var scoreMinEventTypes))
                {
                    Log.Out($"{__instance.entityName} ( {__instance.entityId} ): {scoreMinEventTypes}");
                    return true;
                };
                Log.Out($"{__instance.entityName} ( {__instance.entityId} ): {_eventType}");

            }

            return true;

        }
        
    }
    
}
