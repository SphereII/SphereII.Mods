using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Quality.Harmony
{
    [HarmonyPatch(typeof(XUiM_Trader))]
    [HarmonyPatch(nameof(XUiM_Trader.GetBuyPrice))]
    public class XUiMTraderGetBuyPrice
    {
        
        // public static bool Prefix()
        // {
        //   //  TraderInfo.QualityMaxMod = QualityUtils.GetMaxQuality();
        //    // TraderInfo.QualityMinMod = QualityUtils.GetMinQuality();
        //     return true;
        // }
        //
        // public static void Postfix()
        // {
        //     
        // }
        
           public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);

        for (int i = 0; i < codes.Count; i++)
        {
            // We're looking for the instruction that loads the float constant 5.0f
            // The operand might be a float or a double, so check both.
            // Also, consider if there are other 5f's you don't want to change.
            // You might need more context (e.g., the instruction before/after)
            // to precisely target the correct 5f if multiple exist.
            if (codes[i].opcode == OpCodes.Ldc_R4 && codes[i].operand is float floatValue && floatValue == 5f)
            {
                // Found the 5f. Now replace it with instructions to call QualityUtils.GetMaxQuality()

                // 1. Remove the original Ldc_R4 5f instruction
                codes.RemoveAt(i);

                // 2. Add instructions to call QualityUtils.GetMaxQuality()
                //    First, load the method itself
                MethodInfo getMaxQualityMethod = AccessTools.Method(typeof(QualityUtils), nameof(QualityUtils.GetMaxQuality));
                if (getMaxQualityMethod == null)
                {
                    // Handle error: method not found
                    // Log an error or throw an exception if necessary
                    Debug.LogError("Harmony Transpiler Error: QualityUtils.GetMaxQuality method not found!");
                    // If method not found, it's safer to re-insert the original instruction
                    // or skip this transformation to avoid breaking the game.
                    // For now, let's re-add the original instruction if the method isn't found
                    // to ensure compilation. In a real scenario, you'd want robust error handling.
                    codes.Insert(i, new CodeInstruction(OpCodes.Ldc_R4, 5f));
                    continue; // Skip to the next instruction
                }

                // Call the static method. The result (an int) will be pushed onto the stack.
                codes.Insert(i, new CodeInstruction(OpCodes.Call, getMaxQualityMethod));
                i++; // Move to the next index for the next insertion

                // The return type of GetMaxQuality is likely 'int'.
                // The original code was dividing by a 'float'.
                // So, we need to convert the 'int' result to 'float' (R4).
                codes.Insert(i, new CodeInstruction(OpCodes.Conv_R4));
                // Do NOT increment i here, as the loop will do it.
                // If you incremented, the next iteration would skip the instruction after Conv_R4.

                // Successfully replaced, move to the next instruction after the inserted ones
                // No need to adjust 'i' further if we only insert. The loop's 'i++' will handle it.
                // However, if you add multiple, you'll need to account for the new indices.
                // In this case, we removed 1 and added 2, so the net change is +1 instruction.
                // The loop's i++ will make it effectively skip the next ORIGINAL instruction.
                // So, if we want to process the next *original* instruction, we need to do nothing special here.
            }
        }

        return codes;
    }
        // private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        // {
        //     var codes = new List<CodeInstruction>(instructions);
        //     foreach (var t in codes)
        //     {
        //         if (t.opcode != OpCodes.Ldc_R4)
        //             continue;
        //         if ( t.operand is not float value) continue;
        //         if (!Mathf.Approximately(value, 5.0f)) continue;
        //         t.operand =(float)QualityUtils.GetMaxQuality();
        //         break;
        //     }
        //
        //     return codes;
        // }
    }
}
    
