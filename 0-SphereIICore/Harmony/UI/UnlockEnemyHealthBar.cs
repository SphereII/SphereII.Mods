using DMT;
using Harmony;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

[HarmonyPatch(typeof(XUiC_TargetBar))]
[HarmonyPatch("Update")]
public class SphereII_XUiC_TargetBar 
{
    private static string AdvFeatureClass = "AdvancedUI";

    // Loops around the instructions and removes the return condition.
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        int startIndex = -1;
        // Grab all the instructions
        var codes = new List<CodeInstruction>(instructions);

        // Only look at the first 15 IL codes for this change.
        int MaxIteration = 15;
        if (codes.Count < MaxIteration)
            MaxIteration = codes.Count;

        for(int i = 0; i < MaxIteration; i++)
        {
            if(codes[i].opcode == OpCodes.Ret)
            {
                AdvLogging.DisplayLog(AdvFeatureClass, "Enabling UnlockEnemyHealthBar");
                startIndex = i;
                break;
            }
        }

        if ( startIndex > -1)
            codes.RemoveAt(startIndex);

        return codes.AsEnumerable();
    }
}



