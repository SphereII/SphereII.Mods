using DMT;
using Harmony;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

// Remove the cross hair
[HarmonyPatch(typeof(Mod))]
[HarmonyPatch("InitModCode")]
public class SphereII_ModLoader: IHarmony
{
    public void Start()
    {
        Debug.Log(" Loading Patch: " + GetType().ToString());
        Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);
        Application.SetStackTraceLogType(UnityEngine.LogType.Warning, StackTraceLogType.None);
        LoadMods();
        //var harmony = HarmonyInstance.Create(GetType().ToString());
        //harmony.PatchAll(Assembly.GetExecutingAssembly());
    }


    static bool LoadMods()
    {
        string MOD_PATH = (Application.platform != RuntimePlatform.OSXPlayer) ? (Application.dataPath + "/../Mods") : (Application.dataPath + "/../../Mods");
        Debug.Log("Mod Path: " + MOD_PATH);
        string[] directories = Directory.GetDirectories(MOD_PATH);
        Array.Sort<string>(directories);
        foreach(string path in directories)
        {
            Debug.Log(" Checking Path..." + path);
            string[] files = Directory.GetFiles(path);
            foreach(string text in files)
            {
                if(text.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        Debug.Log(" Loading DLL: " + text);
                        var asm = AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(text));
                       //Assembly asm = Assembly.LoadFrom(text);
                        Debug.Log(" Asm: " + asm.ToString());
                        foreach(var r in asm.GetExportedTypes())
                        {
                            if(String.IsNullOrEmpty(r.FullName))
                            {
                                continue;
                            }
                            Debug.Log(" Assembly: " + r.ToString());
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.Log("[MODS] Failed loading DLL " + text);
                        Debug.Log(e);
                        return false;
                    }
                }
            }
        }
        return true;
    }

}



