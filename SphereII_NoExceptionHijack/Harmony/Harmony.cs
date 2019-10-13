using DMT;
using Harmony;
using System;
using System.Reflection;
using UnityEngine;

public class SphereII_NoExceptionHijack : IHarmony
{
    public void Start()
    {
        Debug.Log(" Loading Patch: " + GetType().ToString());
        var harmony = HarmonyInstance.Create(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
    [HarmonyPatch(typeof(GUIWindowConsole))]
    [HarmonyPatch("openConsole")]
    public class SphereII_Main_Menu_AutoClick
    {
        static bool Prefix()
        {
            return false;
        }

    }    
}


