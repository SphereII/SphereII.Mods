using HarmonyLib;
using JetBrains.Annotations;

namespace Harmony.GamePath
{
    
    public class UtilsDrawLine
    {
        // Disabling DrawLine(), which was being called from AstarVoxelGrid and may be causing performance issues.
        [HarmonyPatch(typeof(Utils))]
        [HarmonyPatch("DrawLine")]
        public class UtilsDrawLinePatch
        {
            public static bool Prefix()
            {
                return false;
            }
        }

   
     
    }
}