using System;
using System.Reflection;
using UnityEngine;

namespace Harmony
{
    public class SphereIICoreInit : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            Log.Out(" Loading Patch: " + GetType());

            // Reduce extra logging stuff
            Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(UnityEngine.LogType.Warning, StackTraceLogType.None);

            var harmony = new HarmonyLib.Harmony(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            // Check Harmony/ModEvents.cs for registration of other events which can be called here.
            SCoreModEvents.Init();
        }
    }
}