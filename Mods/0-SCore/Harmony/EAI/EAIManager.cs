using HarmonyLib;

namespace Harmony.EAI
{
    [HarmonyPatch(typeof(EAIManager))]
    [HarmonyPatch("MakeDebugName")]
    public class EAIManager_MakeDebugName
    {
        public static bool Prefix(ref string __result, EAIManager __instance, EntityPlayer player, ref global::EntityAlive ___entity)
        {
            // If we use EAI system, allow us to pass through
            var useAIPackages = EntityClass.list[___entity.entityClass].UseAIPackages;
            if (!useAIPackages) return true;

            __result = ___entity.DebugNameInfo;
            return false;
        }
    }

    [HarmonyPatch(typeof(AIDirector))]
    [HarmonyPatch("DebugReceiveNameInfo")]
    public class EaiManagerDebugReceiveNameInfo
    {
        public static bool Prefix(int entityId, byte[] _data)
        {
            var world = GameManager.Instance.World;
            if (world == null) return false;

            // Nothing to display, then don't pass it along.
            if (_data == null) return false;
            if (_data.Length == 0) return false;

            var entity = GameManager.Instance.World.GetEntity(entityId) as global::EntityAlive;
            if ( entity == null ) return false;

            // If we use EAI system, allow us to pass through
            var useAIPackages = EntityClass.list[entity.entityClass].UseAIPackages;
            if (!useAIPackages) return true;

                // If there's a primary player, check to see if they are our leader to decide to toggle on and off.
            if (GameManager.Instance.World.GetPrimaryPlayerId() < 0) return false;
            var leader = EntityUtilities.GetLeaderOrOwner(entityId);
            if (leader == null) return false;
            return leader.entityId == GameManager.Instance.World.GetPrimaryPlayerId();
        }
    }
}
