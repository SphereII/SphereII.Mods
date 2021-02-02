using HarmonyLib;

/**
 * SphereII_NoExceptionHijack
 * 
 * This class includes a Harmony patch to disable the console from popping down when a Null ref or other critical error occurs. This is mainly to allow
 * modders to keep control of their game despite erroring out while testing. 
 * 
 */
public class SphereII_NoExceptionHijack
{
    private static readonly string AdvFeatureClass = "ErrorHandling";
    private static readonly string Feature = "NoExceptionHijack";

    [HarmonyPatch(typeof(GUIWindowConsole))]
    [HarmonyPatch("openConsole")]
    public class SphereII_Main_Menu_AutoClick
    {
        static bool Prefix()
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            return false;
        }

    }
}


