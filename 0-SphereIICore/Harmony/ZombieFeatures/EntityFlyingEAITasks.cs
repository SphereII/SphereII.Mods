using GamePath;
using HarmonyLib;
using UAI;


class SphereII_EntityFlyingEAITasks
{

    [HarmonyPatch(typeof(EntityHornet))]
    [HarmonyPatch("Init")]
    public class SphereII_EntityFlyingEAITasks_Init
    {
        public static void Postfix(ref EntityAlive __instance)
        {
            if (!__instance.HasAnyTags(FastTags.Parse("allowEAI")))
                return;
            var entityClass = EntityClass.list[__instance.entityClass];
            if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            {
                __instance.aiManager.CopyPropertiesFromEntityClass(entityClass);
            }

        }
    }
    [HarmonyPatch(typeof(EntityHornet))]
    [HarmonyPatch("updateTasks")]
    public class SphereII_EntityFlyingEAITasks_UpdateTask
    {
        public static bool Prefix(EntityHornet __instance, Context ___utilityAIContext, EntitySeeCache ___seeCache, EntityLookHelper ___lookHelper, ref int ___aggroCooldown)
        {

            if (!__instance.HasAnyTags(FastTags.Parse("allowEAI")))
                return true;

            if (GamePrefs.GetBool(EnumGamePrefs.DebugStopEnemiesMoving))
            {
                __instance.SetMoveForwardWithModifiers(0f, 0f, false);
                if (__instance.aiManager != null)
                {
                    __instance.aiManager.UpdateDebugName();
                }
                return true;
            }

            if (__instance.GetAttackTarget() != null)
                __instance.SetRevengeTarget(__instance.GetAttackTarget());

            __instance.CheckDespawn();
            ___seeCache.ClearIfExpired();
            using (ProfilerUsingFactory.Profile("entities.live.ai.manager"))
            {
                __instance.aiManager.Update();
            }

            using (ProfilerUsingFactory.Profile("entities.live.ai.getpath"))
            {
                PathInfo path = PathFinderThread.Instance.GetPath(__instance.entityId);
                if (path.path != null)
                {
                    bool flag = true;
                    flag = __instance.aiManager.CheckPath(path);

                    if (flag)
                    {
                        __instance.navigator.SetPath(path, path.speed);
                    }
                }
                __instance.navigator.UpdateNavigation();
            }
            __instance.moveHelper.UpdateMoveHelper();
            ___lookHelper.onUpdateLook();
            if (__instance.distraction != null && (__instance.distraction.IsDead() || __instance.distraction.IsMarkedForUnload()))
            {
                __instance.distraction = null;
            }
            if (__instance.pendingDistraction != null && (__instance.pendingDistraction.IsDead() || __instance.pendingDistraction.IsMarkedForUnload()))
            {
                __instance.pendingDistraction = null;
            }

            return true;
        }

    }
}

