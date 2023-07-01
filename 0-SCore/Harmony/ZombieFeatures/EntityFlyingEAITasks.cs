using GamePath;
using HarmonyLib;
using UAI;

namespace Harmony.ZombieFeatures
{
    public class EntityFlyingEAITasks
    {
        //[HarmonyPatch(typeof(EntityVulture))]
        //[HarmonyPatch("Init")]
        //public class EntityFlyingEAITasksInit
        //{
        //    public static void Postfix(ref global::EntityAlive __instance)
        //    {
        //        if (!__instance.HasAnyTags(FastTags.Parse("allowEAI")))
        //            return;
        //        var entityClass = EntityClass.list[__instance.entityClass];
        //        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) __instance.aiManager.CopyPropertiesFromEntityClass(entityClass);
        //    }
        //}

        [HarmonyPatch(typeof(EntityVulture))]
        [HarmonyPatch("updateTasks")]
        public class EntityFlyingEAITasksUpdateTask
        {
            public static bool Prefix(EntityVulture __instance, Context ___utilityAIContext, EntitySeeCache ___seeCache, EntityLookHelper ___lookHelper)
            {
                if (!__instance.HasAnyTags(FastTags.Parse("allowEAI")))
                    return true;

                if (GamePrefs.GetBool(EnumGamePrefs.DebugStopEnemiesMoving))
                {
                    __instance.SetMoveForwardWithModifiers(0f, 0f, false);
                    __instance.aiManager?.UpdateDebugName();
                    return true;
                }

                var useAIPackages = EntityClass.list[__instance.entityClass].UseAIPackages;
                if (!useAIPackages)
                {
                    __instance.aiManager.Update();
                }
                else
                {
                    UAIBase.Update(___utilityAIContext);
                }
                PathInfo path = PathFinderThread.Instance.GetPath(__instance.entityId);
                if (path.path != null)
                {
                    bool flag = true;
                    if (!useAIPackages)
                    {
                        flag = __instance.aiManager.CheckPath(path);
                    }
                    if (flag)
                    {
                        __instance.navigator.SetPath(path, path.speed);
                    }
                }
                __instance.navigator.UpdateNavigation();

                if (__instance.GetAttackTarget() != null)
                    __instance.SetRevengeTarget(__instance.GetAttackTarget());

                __instance.CheckDespawn();
                ___seeCache.ClearIfExpired();


                __instance.moveHelper.UpdateMoveHelper();
                ___lookHelper.onUpdateLook();
                if (__instance.distraction != null && (__instance.distraction.IsDead() || __instance.distraction.IsMarkedForUnload())) __instance.distraction = null;
                if (__instance.pendingDistraction != null && (__instance.pendingDistraction.IsDead() || __instance.pendingDistraction.IsMarkedForUnload())) __instance.pendingDistraction = null;

                return true;
            }
        }

        [HarmonyPatch(typeof(EntityVulture))]
        [HarmonyPatch("Init")]
        [HarmonyPatch(new[] { typeof(int) })]

        public class EntityFlyingEAITasksInitEntityVulture
        {
            public static void Postfix(ref global::EntityAlive __instance)
            {
                if (!__instance.HasAnyTags(FastTags.Parse("allowEAI")))
                    return;
                var entityClass = EntityClass.list[__instance.entityClass];
                if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) __instance.aiManager.CopyPropertiesFromEntityClass(entityClass);
            }
        }


    }
}