using DMT;
using Harmony;
using System;
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

    /*****************
     * __result = return value of the vanilla method
     * _gameObject / _className:  These are the original vanilla method parameter that we want to reference.
     *     Note: if you want to change the values, add   ref string _className
     *  Return Type is Entity, as we want to return the result. 
     *      Return Type could also be void, with us setting __result to the value we want, ather than returning it
     */
    static Entity Postfix(Entity __result, GameObject _gameObject, string _className)
    {
        if(__result == null)
        {
            Type type = Type.GetType(_className + ", Mods");
            if(type != null)
            {
                Debug.Log(" Type is: " + type.FullName);
                return (Entity)_gameObject.AddComponent(type);
            }
        }
        return __result;
    }
}



