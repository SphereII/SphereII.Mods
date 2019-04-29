using DMT;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[HarmonyPatch(typeof(EntityFactory))]
[HarmonyPatch("addEntityToGameObject")]
public class SphereII_EntityFactoryPatch : IHarmony
{
    public void Start()
    {
        Debug.Log(" Loading Patch: " + this.GetType().ToString());
        var harmony = HarmonyInstance.Create(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    static Entity Postfix(Entity __result, GameObject _gameObject, string _className)
    {
        if(__result == null)
        {
            Type type = Type.GetType(_className + ", Mods");
            if(type != null)
                return (Entity)_gameObject.AddComponent(type);
        }
        return __result;
    }
}



