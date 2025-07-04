using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/* Disabled until someone has a need for it */


namespace Harmony.TileEntities
{
    /**
     * SCoreTileEntityAlwaysActive
     *
     * This class includes a Harmony patch allow an Always Active block, thus allowing a buff to be placed on it.
     *
     * Usage XML:
     * <!-- Allows the  Trigger to work -->
     * <property name="AlwaysActive" value="true" />
     * <!-- How far out the tile entity will re-scan to detect the player -->
     * <property name="ActivationDistance" value="5" />
     * <property name="ActivateOnLook" value="true" />
     * <!-- Triggers the block if the buff buffCursed is active on the player, or if the player has a cvar called "cvar" with a value of 4, or if myOtherCvar is available, regardless of value -->
     * <property name="ActivationBuffs" value="buffCursed,cvar(4),myOtherCvar" />
     */
    public class TileEntityIsActivePatches
    {
        private static readonly string AdvFeatureClass = "AdvancedTileEntities";

        [HarmonyPatch(typeof(TileEntity))]
        [HarmonyPatch("IsActive")]
        public class TileEntityAlwaysActive
        {
            public static void Postfix(TileEntity __instance, ref bool __result, World world)
            {
                if (__result) return;
                CheckTileEntity(__instance, out __result, world);
            }
        }

        [HarmonyPatch(typeof(TileEntityWorkstation))]
        [HarmonyPatch("IsActive")]
        public class TileEntityWorkstationAlwaysActive
        {
            public static void Postfix(TileEntity __instance, ref bool __result, World world)
            {
                if (__result) return;
                CheckTileEntity(__instance, out __result, world);
            }
        }

        [HarmonyPatch(typeof(TileEntityForge))]
        [HarmonyPatch("IsActive")]
        public class TileEntityForgeAlwaysActive
        {
            public static void Postfix(TileEntity __instance, ref bool __result, World world)
            {
                if (__result) return;
                CheckTileEntity(__instance, out __result, world);
            }
        }

        [HarmonyPatch(typeof(TileEntityComposite))]
        [HarmonyPatch("IsActive")]
        public class TileEntityCompositeAlwaysActive
        {
            public static void Postfix(TileEntity __instance, ref bool __result, World world)
            {
                if (__result) return;
                CheckTileEntity(__instance, out __result, world);
            }
        }


        public static void CheckTileEntity(TileEntity __instance, out bool __result, World world)
        {
            var block = GameManager.Instance.World.GetBlock(__instance.ToWorldPos());
            var isAlwaysActive = false;
            if (!block.Block.Properties.Values.ContainsKey("AlwaysActive"))
            {
                __result = false;
                return;
            }

            isAlwaysActive = StringParsers.ParseBool(block.Block.Properties.Values["AlwaysActive"]);
            if (!isAlwaysActive)
            {
                __result = false;
                return;
            }


            __result = true;
            AdvLogging.DisplayLog(AdvFeatureClass, block.Block.GetBlockName() + ": TileEntity is Active.");
            var blCanTrigger = false;

            var bounds = 6;
            if (block.Block.Properties.Values.ContainsKey("ActivationDistance"))
                bounds = StringParsers.ParseSInt32(block.Block.Properties.Values["ActivationDistance"]);

            // Scan for the player in the radius as defined by the Activation distance of the block
            var entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(null,
                new Bounds(__instance.ToWorldPos().ToVector3(), Vector3.one * 20));
            if (entitiesInBounds.Count > 0)
            {
                AdvLogging.DisplayLog(AdvFeatureClass,
                    block.Block.GetBlockName() + ": TileEntity has Entities in Bound of " + bounds);
                foreach (var t in entitiesInBounds)
                {
                    var player = t as EntityPlayer;
                    if (!player) continue;
                    var distance = (player.position - __instance.ToWorldPos().ToVector3()).sqrMagnitude;
                    if (distance > block.Block.GetActivationDistanceSq())
                        continue;

                    AdvLogging.DisplayLog(AdvFeatureClass,
                        block.Block.GetBlockName() + ": Player: " + player.EntityName + " is in bounds");

                    if (block.Block.Properties.Values.ContainsKey("ActivationBuffs"))
                    {
                        foreach (var strbuff in block.Block.Properties.Values["ActivationBuffs"].Split(','))
                        {
                            AdvLogging.DisplayLog(AdvFeatureClass,
                                block.Block.GetBlockName() + ": Checking ActivationBuffs: " + strbuff);
                            var strBuffName = strbuff;
                            var checkValue = -1f;

                            // Check if there's a ( ) at the end of the buff; this will be used as a cvar hook.
                            var start = strbuff.IndexOf('(');
                            var end = strbuff.IndexOf(')');
                            if (start != -1 && end != -1 && end > start + 1)
                            {
                                checkValue =
                                    StringParsers.ParseFloat(strbuff.Substring(start + 1, end - start - 1));
                                strBuffName = strbuff.Substring(0, start);
                                AdvLogging.DisplayLog(AdvFeatureClass,
                                    block.Block.GetBlockName() + ": Actviation Buff is a cvar: " + strBuffName +
                                    ". Requires value: " + checkValue);
                            }

                            // If the player has a buff by this name, trigger it.
                            if (player.Buffs.HasBuff(strBuffName))
                            {
                                AdvLogging.DisplayLog(AdvFeatureClass,
                                    block.Block.GetBlockName() + ": Buff has been found: " + strBuffName);
                                blCanTrigger = true;
                            }

                            // If the player has a cvar, then do some extra checks
                            if (!player.Buffs.HasCustomVar(strBuffName)) continue;


                            AdvLogging.DisplayLog(AdvFeatureClass,
                                block.Block.GetBlockName() + ": Cvar has been found: " + strBuffName);
                            // If there's no cvar value specified, just allow it.
                            if (checkValue == -1)
                            {
                                AdvLogging.DisplayLog(AdvFeatureClass,
                                    block.Block.GetBlockName() +
                                    ": Cvar found, and does not require a specific value.");
                                blCanTrigger = true;
                            }

                            // If a cvar is set, then check to see if it matches
                            if (checkValue > -1)
                            {
                                AdvLogging.DisplayLog(AdvFeatureClass,
                                    block.Block.GetBlockName() + ": Cvar found, and requires a value of " +
                                    checkValue +
                                    " Player has: " + player.Buffs.GetCustomVar(strBuffName));
                                if (player.Buffs.GetCustomVar(strBuffName) == checkValue)
                                    blCanTrigger = true;
                            }

                            // If any of these conditions match, trigger the Activate block
                            if (blCanTrigger)
                            {
                                AdvLogging.DisplayLog(AdvFeatureClass,
                                    block.Block.GetBlockName() + ":  Checks successfully passed for player: " +
                                    player.EntityName);
                                break;
                            }

                            AdvLogging.DisplayLog(AdvFeatureClass,
                                block.Block.GetBlockName() + ": Checks were NOT success for player: " +
                                player.EntityName);
                        }
                    }
                    else
                        blCanTrigger = true;

                    if (blCanTrigger)
                        break;
                }
            }

            if (blCanTrigger)
            {
                AdvLogging.DisplayLog(AdvFeatureClass,
                    block.Block.GetBlockName() + ": TileEntity can call ActivateBlock. Calling it...");
                Block.list[block.type].ActivateBlock(world, __instance.GetClrIdx(), __instance.ToWorldPos(), block,
                    true, true);
            }
            else
            {
                AdvLogging.DisplayLog(AdvFeatureClass,
                    block.Block.GetBlockName() + ": TileEntity is Active but is not Activating.");
                Block.list[block.type].ActivateBlock(world, __instance.GetClrIdx(), __instance.ToWorldPos(), block,
                    false, true);
            }

            __result = true;
            return;
        }
    }
}