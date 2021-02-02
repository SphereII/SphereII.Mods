using DMT;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

public class SphereIIQuickContinue_Init : IHarmony
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



