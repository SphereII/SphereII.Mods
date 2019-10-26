using Harmony;

public class SphereII_NoExceptionHijack
{
    private static string AdvFeatureClass = "ErrorHandling";
    private static string Feature = "NoExceptionHijack";

    [HarmonyPatch(typeof(GUIWindowConsole))]
    [HarmonyPatch("openConsole")]
    public class SphereII_Main_Menu_AutoClick
    {
        static bool Prefix()
        {
            if(!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            return false;
        }

    }    
}


