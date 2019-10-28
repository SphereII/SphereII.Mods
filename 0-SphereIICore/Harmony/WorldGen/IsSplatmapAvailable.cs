using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Harmony;
using UnityEngine;
using System.Diagnostics;
class IsSplatmapAvailable
{
    private static string AdvFeatureClass = "AdvancedWorldGen";
    private static string Feature = "DisableSplatMap";

    [HarmonyPatch(typeof(GameManager))]
    [HarmonyPatch("IsSplatMapAvailable")]
    public class SphereII_GameManager_SplatMap
    {
        public static bool Prefix(bool __result)
        {

           StackTrace stackTrace = new StackTrace();
            UnityEngine.Debug.Log("--------------------------");
            UnityEngine.Debug.Log(stackTrace.GetFrames().ToString());
            UnityEngine.Debug.Log("--------------------------");
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            AdvLogging.DisplayLog(AdvFeatureClass, "Disabling Splat Map");
            __result = false;
            return false;
        }

        //static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        //{

        //    // Grab all the instructions
        //    List<CodeInstruction> codes = new List<CodeInstruction>();
        //    CodeInstruction temp = new CodeInstruction(OpCodes.Ldc_I4_0);
        //    codes.Add(temp);
        //    temp = new CodeInstruction(OpCodes.Ret);
        //    codes.Add(temp);
        //    return codes.AsEnumerable();
        //}
    }
}

