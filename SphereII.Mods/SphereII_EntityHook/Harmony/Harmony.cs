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

    static Entity Postfix(Entity __result, GameObject _gameObject, string _className)
    {
        if(__result == null)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var result = assemblies.FirstOrDefault(a => a.GetType(_className, false) != null);
            Debug.Log(" Result: " + result);
            Debug.Log("  Result 2: " + result.FullName);
            
            Type myType = Type.GetType(_className + ", " + result);
            if(myType != null)
            {
                Debug.Log(" Found Type: " + _className + ", " + result);
                return (Entity)_gameObject.AddComponent(myType);
            }    
            //Type objectType = (from asm in AppDomain.CurrentDomain.GetAssemblies()
            //                   from mytype in asm.GetTypes()
            //                   where mytype.IsClass && mytype.Name == _className
            //                   select mytype).Single();
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



