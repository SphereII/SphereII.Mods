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

        for(int i = 0; i < codes.Count; i++)
        {
            if(codes[i].opcode == OpCodes.Ret)
            {
                startIndex = i + 1;
                break;
            }
        }
        codes.RemoveRange(startIndex, 8);

        return codes.AsEnumerable();
    }
}



