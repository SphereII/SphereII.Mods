using DMT;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

public class SphereIICore_Init : IHarmony
{
    public void Start()
    {
        Debug.Log(" Loading Patch: " + this.GetType().ToString());

        // Reduce extra logging stuff
        Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);
        Application.SetStackTraceLogType(UnityEngine.LogType.Warning, StackTraceLogType.None);

        var harmony = new Harmony(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}


[HarmonyPatch(typeof(XUiC_MainMenu))]
[HarmonyPatch("OnOpen")]
public class SphereII_Main_Menu_ClearCache
{ 
    static void Postfix(XUiC_MainMenu __instance)
    {
        Debug.Log("Clearing SphereCache...");
        SphereCache.DoorCache.Clear();
        SphereCache.PathingCache.Clear();
        SphereCache.caveChunks.Clear();
        SphereCache.caveEntrances.Clear();
    }
}



