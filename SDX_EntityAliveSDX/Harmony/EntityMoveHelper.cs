using DMT;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

public class SphereII__EntityMoveHelper
{
    public class SphereII_EntityMNoveHelperTweaks : IHarmony
    {
        public void Start()
        {
            Debug.Log(" Loading Patch: " + GetType().ToString());
            var harmony = HarmonyInstance.Create(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    // Loops around the instructions and removes the return condition.
    [HarmonyPatch(typeof(EntityMoveHelper))]
    [HarmonyPatch("CheckBlocked")]
    public class SphereII_EntityMoveHelperCheckBlock
    {

        static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
                        
            Debug.Log(" I am an NPC");
            int startIndex = -1;
            // Grab all the instructions
            //var codes = new List<CodeInstruction>(instructions);
            var codes = original.GetMethodBody().GetILAsByteArray();
            int intTargetSecondIteration = 0;


            for (int i = 0; i < codes.Length; i++)
            {
                Debug.Log(" OpCode: " + codes[i].opcode.ToString());
                if ((codes[i].opcode == OpCodes.Ret))
                {
                    intTargetSecondIteration++;
                    if (intTargetSecondIteration == 2)
                    {
                        startIndex = i;
                        break;
                    }
                }
            }

            if (startIndex > -1)
                codes.RemoveAt(startIndex);

            return codes.AsEnumerable();

            // return instructions;
        }
        public static void PrintAttributes(Type attribType, int iAttribValue)
        {
            if (!attribType.IsEnum)
            {
                // Console.WriteLine("This type is not an enum.");
                return;
            }

            FieldInfo[] fields = attribType.GetFields(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < fields.Length; i++)
            {
                int fieldvalue = (Int32)fields[i].GetValue(null);
                if ((fieldvalue & iAttribValue) == fieldvalue)
                {
                    Debug.Log(fields[i].Name);
                }
            }
        }
    }
    //[HarmonyPatch(typeof(EntityMoveHelper))]
    //[HarmonyPatch("CheckBlocked")]
    //public class SphereII_EntityMoveHelperCheckBlock
    //{
    //    static bool Prefix(EntityMoveHelper __instance, MethodBase original,  Vector3 pos, Vector3 endPos, int baseY)
    //    {
    //        // Get the MethodAttribute enumerated value.
    //        MethodAttributes Myattributes = original.Attributes;
    //        PrintAttributes(typeof(MethodAttributes), (int)Myattributes);


    //        __instance.IsBlocked = false;
    //        BlockValue myBlock = GameManager.Instance.World.GetBlock(new Vector3i(endPos));
    //        Debug.Log("CheckBlocked: " + myBlock.Block.GetBlockName());
    //        if(myBlock.Block.FilterTags.Contains("ftraps"))
    //        {
    //            Debug.Log("HarmonyPatch: CheckedBlock - Trap detected");
    //            __instance.IsBlocked = true;
    //            return false;
    //        }
    //        return true;

    //    }

    //}
    //}
}