using Harmony;
using System;
using UnityEngine;

[HarmonyPatch(typeof(EntityFactory))]
[HarmonyPatch("addEntityToGameObject")]
[HarmonyPatch(new Type[] { typeof(GameObject), typeof(string) })]
public class SphereII_EntityFactoryPatch 
{
    static Entity Postfix(Entity __result, GameObject _gameObject, string _className)
    {
        if (__result == null)
        {
            Type type = Type.GetType(_className + ", Mods");
            if (type != null)
                return (Entity)_gameObject.AddComponent(type);
        }
        return __result;
    }
}



