using DMT;
using Harmony;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;


public class SphereII_XuiC_TargetBar
{
    private static string AdvFeatureClass = "AdvancedUI";
    private static string Feature = "UnlockEnemyHealthBar";

    [HarmonyPatch(typeof(XUiC_TargetBar))]
    [HarmonyPatch("Update")]
    public class SphereII_XUiC_TargetBar_Transpiler
    {
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

            for (int i = 0; i < MaxIteration; i++)
            {
                if (codes[i].opcode == OpCodes.Ret)
                {
                    startIndex = i;
                    break;
                }
            }

            if (startIndex > -1)
                codes.RemoveAt(startIndex);

            return codes.AsEnumerable();
        }


    }

    [HarmonyPatch(typeof(XUiC_TargetBar))]
    [HarmonyPatch("Update")]
    public class SphereII_XUiC_TargetBar_Prefix
    {
        public static bool Prefix(XUiC_TargetBar __instance,ref  XUiView ___viewComponent)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) // if disables, don't execute the base Update()
                return false;

            return true;
        }


    }

}