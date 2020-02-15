using Harmony;
using System.Reflection;
using UnityEngine;
using DMT;
using System;

[HarmonyPatch(typeof(EModelBase))]
[HarmonyPatch("InitCommon")]
public class SphereII_EModelBase_InitCommon
{

    // On dedicated servers, when using an external animation class (MecanimSDX), the server incorrectly assigns a Dummy avatar, rather than our own.
    // We can get around this by disabling ragdolling on external entities, but it also generates warnings in the log file. This should jump in right before we do the 
    // switchandmodel.
    static bool Prefix(ref EModelBase __instance, Entity ___entity)
    {
       
        EntityClass entityClass = EntityClass.list[___entity.entityClass];
        if (entityClass.Properties.Values.ContainsKey(EntityClass.PropAvatarController))
        {
            if (entityClass.Properties.Values[EntityClass.PropAvatarController].Contains("MecanimSDX") && __instance.avatarController is AvatarControllerDummy)
            {
                __instance.avatarController = (__instance.transform.gameObject.AddComponent(Type.GetType(entityClass.Properties.Values[EntityClass.PropAvatarController])) as AvatarController);
                __instance.avatarController.SetVisible(true);
            }
        }
        return true;
    }
}

