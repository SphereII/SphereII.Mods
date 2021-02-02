using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
class SphereII_Block_Workarounds
{
    // Removes the WRN about block position WRN No chunk for position -2368, 23, -696, can not add childs to pos -2369, 22, -696! Block cntCollapsedChemistryStation
    [HarmonyPatch(typeof(Block.MultiBlockArray))]
    [HarmonyPatch("AddChilds")]
    public class SphereII_Block_AddChilds
    {
        // Loops around the instructions and removes the return condition.
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Grab all the instructions
            var codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_0)
                {
                    codes[i].opcode = OpCodes.Ldc_I4_1;
                    break;

                }
            }

            return codes.AsEnumerable();
        }
    }

    // Removes the Block error: ERR Block on position 1636, 42, -2404 with name 'bodyBagPile' should be a parent but is not! (1)
    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("IsMovementBlocked")]
    [HarmonyPatch(new Type[] { typeof(IBlockAccess), typeof(Vector3i), typeof(BlockValue), typeof(BlockFace) })]
    public class SphereII_Block_IsMovementBlock
    {
        public static bool Prefix(ref bool __result, Block __instance, IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue)
        {
            if (__instance.isMultiBlock && _blockValue.ischild)
            {
                Vector3i parentPos = __instance.multiBlockPos.GetParentPos(_blockPos, _blockValue);
                BlockValue block = _world.GetBlock(parentPos);
                if (block.ischild)
                {
                    __result = true;
                    return false;
                }
            }
            return true;
        }
    }


    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("IsMovementBlocked")]
    [HarmonyPatch(new Type[] { typeof(IBlockAccess), typeof(Vector3i), typeof(BlockValue), typeof(BlockFaceFlag) })]

    public class SphereII_Block_IsMovementBlock_Flag
    {
        public static bool Prefix(ref bool __result, Block __instance, IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue)
        {
            if (__instance.isMultiBlock && _blockValue.ischild)
            {
                Vector3i parentPos = __instance.multiBlockPos.GetParentPos(_blockPos, _blockValue);
                BlockValue block = _world.GetBlock(parentPos);
                if (block.ischild)
                {
                    __result = true;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("IsMovementBlocked")]
    [HarmonyPatch(new Type[] { typeof(IBlockAccess), typeof(Vector3i), typeof(BlockValue), typeof(Vector3) })]

    public class SphereII_Block_IsMovementBlock_2
    {
        public static bool Prefix(ref bool __result, Block __instance, IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue)
        {
            if (__instance.isMultiBlock && _blockValue.ischild)
            {
                Vector3i parentPos = __instance.multiBlockPos.GetParentPos(_blockPos, _blockValue);
                BlockValue block = _world.GetBlock(parentPos);
                if (block.ischild)
                {
                    __result = true;
                    return false;
                }
            }
            return true;
        }
    }


}

