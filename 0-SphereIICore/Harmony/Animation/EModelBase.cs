//using Harmony;
//using System.Reflection;
//using UnityEngine;
//using DMT;
//using System;

//[HarmonyPatch(typeof(EModelBase))]
//[HarmonyPatch("InitCommon")]
//public class SphereII_EModelBase_InitCommon
//{

//    // On dedicated servers, when using an external animation class (MecanimSDX), the server incorrectly assigns a Dummy avatar, rather than our own.
//    // We can get around this by disabling ragdolling on external entities, but it also generates warnings in the log file. This should jump in right before we do the 
//    // switchandmodel.
//    static bool Prefix(ref EModelBase __instance, Entity ___entity)
//    {
//        EntityClass entityClass = EntityClass.list[___entity.entityClass];
//        if(entityClass.Properties.Values.ContainsKey(EntityClass.PropAvatarController) && GameManager.IsDedicatedServer )
//        {
//            if(entityClass.Properties.Values[EntityClass.PropAvatarController].Contains("MecanimSDX"))
//            {
//                Debug.Log("MecanimSDX detected on the server.");
//                Debug.Log(" Current Animator: " + __instance.avatarController.ToString());
//                __instance.avatarController = (__instance.GetModelTransform().gameObject.AddComponent(Type.GetType(entityClass.Properties.Values[EntityClass.PropAvatarController])) as AvatarController);
//                __instance.avatarController.SetVisible(true);
//                Debug.Log(" New Animator: " + __instance.avatarController.ToString());
//            }
//        }
//        return true;
//    }
//}

