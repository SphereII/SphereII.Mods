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
    
    [HarmonyPatch(typeof(TileEntity))]
    [HarmonyPatch("IsActive")]
    public class TileEntityAlwaysActive
    {
        private static readonly string AdvFeatureClass = "AdvancedTileEntities";

        //[HarmonyPatch(typeof(TileEntity))]
        //[HarmonyPatch("IsActive")]
        //public class TileEntityIsActive
        //{
        //    public static bool Prefix(TileEntity __instance, ref bool __result, World world)
        //    {
        //        if (__instance == null) return true;
        //        Vector3i blockPos = __instance.ToWorldPos();
        //        if (blockPos == Vector3i.zero) return true;
        //        var block = world.GetBlock(blockPos);
        //        //if (!block.Block.Properties.Values.ContainsKey("AlwaysActive"))
        //        //    return true;

        //        //// If the block has an AlwaysActive, then see if its set to true, or if its false, then go back to default.
        //        //if (StringParsers.ParseBool(block.Block.Properties.Values["AlwaysActive"]))
        //        //{
        //        //    __result = true;
        //        //    return false;
        //        //}
        //        return true;
        //    }
        //}

        // [HarmonyTargetMethod]
        // static IEnumerable<MethodBase> TargetMethods()
        // {
        //     yield return typeof(TileEntityForge).GetMethod("IsActive");
        //     yield return typeof(TileEntityLootContainer).GetMethod("IsActive");
        //     yield return typeof(TileEntity).GetMethod("IsActive");
        //
        // }

        // public class TileEntityIsActive
        // {
        //     public static bool Postfix(TileEntity __instance, ref bool __result)
        //     {
        //         if (!__instance.blockValue.Block.Properties.Values.ContainsKey("AlwaysActive"))
        //             return __result;
        //
        //         var result = StringParsers.ParseBool(__instance.blockValue.Block.Properties.Values["AlwaysActive"]);
        //         Debug.Log($"IsAlways Active: {result}");
        //         return result;
        //     }
        // }

        public static void Postfix(TileEntity __instance, ref bool __result, World world)
        {
            var block = GameManager.Instance.World.GetBlock(__instance.ToWorldPos());
            var block2 = Block.list[block.type];
            var isAlwaysActive = false;

            if (!block2.Properties.Values.ContainsKey("AlwaysActive")) return;

            isAlwaysActive = StringParsers.ParseBool(block2.Properties.Values["AlwaysActive"]);

            if (!isAlwaysActive) return;

            __result = true;
            AdvLogging.DisplayLog(AdvFeatureClass, block2.GetBlockName() + ": TileEntity is Active.");
            var blCanTrigger = false;

            var bounds = 6;
            if (block2.Properties.Values.ContainsKey("ActivationDistance"))
                bounds = StringParsers.ParseSInt32(block2.Properties.Values["ActivationDistance"]);

            // Scan for the player in the radius as defined by the Activation distance of the block
            var entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(null,
                new Bounds(__instance.ToWorldPos().ToVector3(), Vector3.one * 20));
            if (entitiesInBounds.Count > 0)
            {
                AdvLogging.DisplayLog(AdvFeatureClass,
                    block2.GetBlockName() + ": TileEntity has Entities in Bound of " + bounds);
                foreach (var t in entitiesInBounds)
                {
                    var player = t as EntityPlayer;
                    if (!player) continue;
                    var distance = (player.position - __instance.ToWorldPos().ToVector3()).sqrMagnitude;
                    if (distance > block2.GetActivationDistanceSq())
                        continue;

                    AdvLogging.DisplayLog(AdvFeatureClass,
                        block2.GetBlockName() + ": Player: " + player.EntityName + " is in bounds");

                    if (block2.Properties.Values.ContainsKey("ActivationBuffs"))
                    {
                        foreach (var strbuff in block2.Properties.Values["ActivationBuffs"].Split(','))
                        {
                            AdvLogging.DisplayLog(AdvFeatureClass,
                                block2.GetBlockName() + ": Checking ActivationBuffs: " + strbuff);
                            var strBuffName = strbuff;
                            var checkValue = -1f;

                            // Check if there's a ( ) at the end of the buff; this will be used as a cvar hook.
                            var start = strbuff.IndexOf('(');
                            var end = strbuff.IndexOf(')');
                            if (start != -1 && end != -1 && end > start + 1)
                            {
                                checkValue = StringParsers.ParseFloat(strbuff.Substring(start + 1, end - start - 1));
                                strBuffName = strbuff.Substring(0, start);
                                AdvLogging.DisplayLog(AdvFeatureClass,
                                    block2.GetBlockName() + ": Actviation Buff is a cvar: " + strBuffName +
                                    ". Requires value: " + checkValue);
                            }

                            // If the player has a buff by this name, trigger it.
                            if (player.Buffs.HasBuff(strBuffName))
                            {
                                AdvLogging.DisplayLog(AdvFeatureClass,
                                    block2.GetBlockName() + ": Buff has been found: " + strBuffName);
                                blCanTrigger = true;
                            }

                            // If the player has a cvar, then do some extra checks
                            if (!player.Buffs.HasCustomVar(strBuffName)) continue;


                            AdvLogging.DisplayLog(AdvFeatureClass,
                                block2.GetBlockName() + ": Cvar has been found: " + strBuffName);
                            // If there's no cvar value specified, just allow it.
                            if (checkValue == -1)
                            {
                                AdvLogging.DisplayLog(AdvFeatureClass,
                                    block2.GetBlockName() + ": Cvar found, and does not require a specific value.");
                                blCanTrigger = true;
                            }

                            // If a cvar is set, then check to see if it matches
                            if (checkValue > -1)
                            {
                                AdvLogging.DisplayLog(AdvFeatureClass,
                                    block2.GetBlockName() + ": Cvar found, and requires a value of " + checkValue +
                                    " Player has: " + player.Buffs.GetCustomVar(strBuffName));
                                if (player.Buffs.GetCustomVar(strBuffName) == checkValue)
                                    blCanTrigger = true;
                            }

                            // If any of these conditions match, trigger the Activate block
                            if (blCanTrigger)
                            {
                                AdvLogging.DisplayLog(AdvFeatureClass,
                                    block2.GetBlockName() + ":  Checks successfully passed for player: " +
                                    player.EntityName);
                                break;
                            }

                            AdvLogging.DisplayLog(AdvFeatureClass,
                                block2.GetBlockName() + ": Checks were NOT success for player: " + player.EntityName);
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
                    block2.GetBlockName() + ": TileEntity can call ActivateBlock. Calling it...");
                Block.list[block.type].ActivateBlock(world, __instance.GetClrIdx(), __instance.ToWorldPos(), block,
                    true, true);
            }
            else
            {
                AdvLogging.DisplayLog(AdvFeatureClass,
                    block2.GetBlockName() + ": TileEntity is Active but is not Activating.");
                Block.list[block.type].ActivateBlock(world, __instance.GetClrIdx(), __instance.ToWorldPos(), block,
                    false, true);
            }

            __result = true;
            return;
        }
    }
}