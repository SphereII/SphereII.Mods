using System;
using HarmonyLib;
using UAI;
namespace Harmony.EAI
{
    [HarmonyPatch(typeof(EAIManager))]
    [HarmonyPatch("MakeDebugName")]
    public class EAIManager_MakeDebugName
    {
        public static bool Prefix(ref string __result, EAIManager __instance, EntityPlayer player, ref global::EntityAlive ___entity)
        {
            // If we use EAI system, allow us to pass through
            bool useAIPackages = EntityClass.list[___entity.entityClass].UseAIPackages;
            if (!useAIPackages) return true;

            if (string.IsNullOrEmpty(___entity.DebugNameInfo))
                ___entity.DebugNameInfo = ___entity.EntityName;

            if (!GamePrefs.GetBool(EnumGamePrefs.DebugMenuShowTasks))
                return true;
            
            // otherwise, grab the debug information populated by the UAI
            __result = ___entity.DebugNameInfo;
            return false;
        }
    }
}
