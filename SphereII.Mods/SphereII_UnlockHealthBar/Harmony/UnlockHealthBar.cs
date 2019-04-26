using DMT;
using Harmony;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

[HarmonyPatch(typeof(XUiC_TargetBar))]
[HarmonyPatch("Update")]
public class SphereII_XUiC_TargetBar : IHarmony
{
    public void Start()
    {
        Debug.Log(" Loading Patch: " + GetType().ToString());
        var harmony = HarmonyInstance.Create(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

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
          //  Debug.Log(" OpCode: " + codes[i].opcode.ToString());
            if(codes[i].opcode == OpCodes.Ret)
            {
            //    Debug.Log(" Return Detected: " + i);
                startIndex = i;
                break;
            }
        }

        if ( startIndex > -1)
            codes.RemoveAt(startIndex);

        return codes.AsEnumerable();
    }
}



