using DMT;
using Harmony;
using System;
using System.Reflection;
using UnityEngine;

public class SphereII_Progression
{
    public class SphereII_Progression_Init : IHarmony
    {
        public void Start()
        {
            Debug.Log(" Loading Patch: " + GetType().ToString());
            var harmony = HarmonyInstance.Create(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    
    [HarmonyPatch(typeof(Progression))]
    [HarmonyPatch("AddLevelExpRecursive")]
    public class SphereII_Progression_AddLevelExpRecursive
    {
        static bool Prefix()
        {
            return false;
        }
    }


    [HarmonyPatch(typeof(Progression))]
    [HarmonyPatch("AddLevelExp")]
    public class SphereII_Progression_AddLevelExp
    {
        static bool Prefix()
        {
            return false;
        }
    }
}