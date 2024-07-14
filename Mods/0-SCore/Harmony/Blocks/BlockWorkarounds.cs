using HarmonyLib;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace Features.LockPicking
{
    public class BlockWorkarounds
    {
        public class BlockAddChilds
        {
            // Removes the WRN about block position WRN No chunk for position -2368, 23, -696, can not add childs to pos -2369, 22, -696! Block cntCollapsedChemistryStation
            [HarmonyPatch(typeof(Block.MultiBlockArray))]
            [HarmonyPatch("AddChilds")]
            [UsedImplicitly]
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                // Grab all the instructions
                var codes = new List<CodeInstruction>(instructions);

                foreach (var t in codes)
                {
                    if (t.opcode != OpCodes.Ldc_I4_0)
                        continue;

                    t.opcode = OpCodes.Ldc_I4_1;
                    break;
                }

                return codes.AsEnumerable();
            }
        }

        // Removes the Block error: ERR Block on position 1636, 42, -2404 with name 'bodyBagPile' should be a parent but is not! (1)
        [HarmonyPatch(typeof(Block))]
        [HarmonyPatch("IsMovementBlocked")]
        [HarmonyPatch(new[] { typeof(IBlockAccess), typeof(Vector3i), typeof(BlockValue), typeof(BlockFace) })]
        public class BlockIsMovementBlock
        {
            public static bool Prefix(ref bool __result, Block __instance, IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue)
            {
                if (!__instance.isMultiBlock || !_blockValue.ischild)
                    return true;

                var parentPos = __instance.multiBlockPos.GetParentPos(_blockPos, _blockValue);
                var block = _world.GetBlock(parentPos);
                if (!block.ischild)
                    return true;

                __result = true;
                return false;
            }
        }


        [HarmonyPatch(typeof(Block))]
        [HarmonyPatch("IsMovementBlocked")]
        [HarmonyPatch(new[] { typeof(IBlockAccess), typeof(Vector3i), typeof(BlockValue), typeof(BlockFaceFlag) })]
        public class SCoreBlockIsMovementBlockFlag
        {
            public static bool Prefix(ref bool __result, Block __instance, IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue)
            {
                if (!__instance.isMultiBlock || _blockValue.ischild)
                    return true;


                var parentPos = __instance.multiBlockPos.GetParentPos(_blockPos, _blockValue);
                var block = _world.GetBlock(parentPos);
                if (!block.ischild)
                    return true;

                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(Block))]
        [HarmonyPatch("IsMovementBlocked")]
        [HarmonyPatch(new[] { typeof(IBlockAccess), typeof(Vector3i), typeof(BlockValue), typeof(Vector3) })]
        public class SCoreBlockIsMovementBlock
        {
            public static bool Prefix(ref bool __result, Block __instance, IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue)
            {
                if (!__instance.isMultiBlock || _blockValue.ischild)
                    return true;


                var parentPos = __instance.multiBlockPos.GetParentPos(_blockPos, _blockValue);
                var block = _world.GetBlock(parentPos);
                if (!block.ischild)
                    return true;

                __result = true;
                return false;
            }
        }
    }
}