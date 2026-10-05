using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace Harmony.Standard
{
    [HarmonyPatch(typeof(Mod))]
    [HarmonyPatch(nameof(Mod.InitModCode))]
    public class SCoreMod_InitModCode
    {
        // NOTE: this prefix is a no-op - the body below is entirely commented out. It is kept
        // only for the reference code. v3.3 retyped Mod.allAssemblies from
        // Dictionary<string, Assembly> to List<Assembly>, so the old ___allAssemblies injection
        // failed to bind and aborted Harmony.PatchAll for the whole SCore assembly.
        private static bool Prefix(Mod __instance)
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