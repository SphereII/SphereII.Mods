using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace Harmony.Standard
{
    [HarmonyPatch(typeof(Mod))]
    [HarmonyPatch("InitModCode")]
    public class SCoreMod_InitModCode
    {
        private static bool Prefix(Mod __instance, Dictionary<string, Assembly> ___allAssemblies)
        {
            //string[] files = Directory.GetFiles(__instance.Path);
            //if (files.Length != 0)
            //{
            //	foreach (string text in files)
            //	{
            //		Debug.Log("\t" + text);
            //		if (text.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            //		{
            //			try
            //			{
            //				String Key = text.GetHashCode().ToString();
            //				if ( !___allAssemblies.ContainsKey(Key))
            //					___allAssemblies.Add(Key, Assembly.LoadFrom(text));
            //			}
            //			catch (Exception e)
            //			{
            //				Debug.Log("[MODS] Failed loading DLL " + text);
            //				Debug.Log(e);

            //				return true;
            //			}
            //		}
            //	}

            //}
            return true;
        }
    }
}