using HarmonyLib;
using System;

namespace Harmony.Animation
{
    
    /// <summary>
    ///  On dedicated servers, when using an external animation class (Mecanim SDX), the server incorrectly assigns a Dummy avatar, rather than our own.
    /// We can get around this by disabling rag dolling on external entities, but it also generates warnings in the log file.
    /// </summary>
    [HarmonyPatch(typeof(EModelBase))]
    [HarmonyPatch("InitCommon")]
    public class ModelBaseInitCommon
    {
        public static bool Prefix(ref EModelBase __instance, Entity ___entity)
        {
            var entityClass = EntityClass.list[___entity.entityClass];
            if (!entityClass.Properties.Values.ContainsKey(EntityClass.PropAvatarController))
                return true;

            // Don't run if this is using the AvatarController, which is for the dedicated servers
            if (__instance.avatarController is AvatarControllerDummy == false)
                return true;

            // If the avatar controller is not mecanim, bail.
            if (!entityClass.Properties.Values[EntityClass.PropAvatarController].Contains("MecanimSDX"))
                return true;

            __instance.avatarController = __instance.transform.gameObject.AddComponent(Type.GetType(entityClass.Properties.Values[EntityClass.PropAvatarController])) as global::AvatarController;
            if (__instance.avatarController == null)
                return true;

            __instance.avatarController.SetVisible(true);
            return true;
        }
    }
}