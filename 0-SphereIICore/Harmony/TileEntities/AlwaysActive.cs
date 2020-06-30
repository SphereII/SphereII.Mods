using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/**
 * SphereII_TileEntityAlwaysActive
 *
 * This class includes a Harmony patch allow an Always Active block, thus allowing a buff to be placed on it.
 *
 * Usage XML:
 * 
 *   <!-- Allows the  Trigger to work -->
 *     <property name="AlwaysActive" value="true" />
 *
 *     <!-- How far out the tile entity will re-scan to detect the player -->
 *     <property name="ActivationDistance" value="5" />
 *
 *      <property name="ActivateOnLook" value="true" />
 *
 *     <!-- Triggers the block if the buff buffCursed is active on the player, or if the player has a cvar called "cvar" with a value of 4, or if myOtherCvar is available, regardless of value -->
 *     <property name="ActivationBuffs" value="buffCursed,cvar(4),myOtherCvar" />
*/
public class SphereII_TileEntityAlwaysActive
{
    private static string AdvFeatureClass = "AdvancedTileEntities";
    

    [HarmonyPatch(typeof(TileEntity))]
    [HarmonyPatch("IsActive")]
    public class SphereII_TileEntity_IsActive
    {
        public static bool Postfix(bool ___result, TileEntity __instance, World world)
        {
            BlockValue block = GameManager.Instance.World.GetBlock(__instance.ToWorldPos());
            Block block2 = Block.list[block.type];
            bool isAlwaysActive = false;
            if (block2.Properties.Values.ContainsKey("AlwaysActive"))
            {
                isAlwaysActive = StringParsers.ParseBool(block2.Properties.Values["AlwaysActive"], 0, -1, true);
                if (isAlwaysActive)
                {
                    ___result = true;
                    AdvLogging.DisplayLog(AdvFeatureClass, block2.GetBlockName() + ": TileEntity is Active.");
                    bool blCanTrigger = false;

                    int Bounds = 6;
                    if (block2.Properties.Values.ContainsKey("ActivationDistance"))
                        Bounds = StringParsers.ParseSInt32(block2.Properties.Values["ActivationDistance"].ToString());

                    // Scan for the player in the radius as defined by the Activation distance of the block
                    List <Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(null, new Bounds(__instance.ToWorldPos().ToVector3(),  (Vector3.one * 20 )));
                    if (entitiesInBounds.Count > 0)
                    {
                        AdvLogging.DisplayLog(AdvFeatureClass, block2.GetBlockName() + ": TileEntity has Entities in Bound of " + Bounds);
                        for (int i = 0; i < entitiesInBounds.Count; i++)
                        {
                            EntityPlayer player = entitiesInBounds[i] as EntityPlayer;
                            if (player)
                            {
                                float distance = (player.position - __instance.ToWorldPos().ToVector3()).sqrMagnitude;
                                if (distance > block2.GetActivationDistanceSq())
                                    continue;

                                AdvLogging.DisplayLog(AdvFeatureClass, block2.GetBlockName() + ": Player: " + player.EntityName + " is in bounds");

                                if (block2.Properties.Values.ContainsKey("ActivationBuffs"))
                                {
                                    foreach (String strbuff in block2.Properties.Values["ActivationBuffs"].Split(','))
                                    {
                                        AdvLogging.DisplayLog(AdvFeatureClass, block2.GetBlockName() + ": Checking ActivationBuffs: " + strbuff );
                                        String strBuffName = strbuff;
                                        float CheckValue = -1f;

                                        // Check if there's a ( ) at the end of the buff; this will be used as a cvar hook.
                                        int start = strbuff.IndexOf('(');
                                        int end = strbuff.IndexOf(')');
                                        if (start != -1 && end != -1 && end > start + 1)
                                        {
                                            
                                            CheckValue = StringParsers.ParseFloat(strbuff.Substring(start + 1, end - start - 1), 0, -1, NumberStyles.Any);
                                            strBuffName = strbuff.Substring(0, start);
                                            AdvLogging.DisplayLog(AdvFeatureClass, block2.GetBlockName() + ": Actviation Buff is a cvar: " + strBuffName + ". Requires value: " + CheckValue );
                                        }

                                        // If the player has a buff by this name, trigger it.
                                        if (player.Buffs.HasBuff(strBuffName))
                                        {
                                            AdvLogging.DisplayLog(AdvFeatureClass, block2.GetBlockName() + ": Buff has been found: " + strBuffName );
                                            blCanTrigger = true;
                                        }

                                        // If the player has a cvar, then do some extra checks
                                        if (player.Buffs.HasCustomVar(strBuffName))
                                        {
                                            AdvLogging.DisplayLog(AdvFeatureClass, block2.GetBlockName() + ": Cvar has been found: " + strBuffName);
                                            // If there's no cvar value specified, just allow it.
                                            if (CheckValue == -1)
                                            {
                                                AdvLogging.DisplayLog(AdvFeatureClass, block2.GetBlockName() + ": Cvar found, and does not require a specific value." );
                                                blCanTrigger = true;
                                            }
                                            // If a cvar is set, then check to see if it matches
                                            if (CheckValue > -1)
                                            {
                                                AdvLogging.DisplayLog(AdvFeatureClass, block2.GetBlockName() + ": Cvar found, and requires a value of " + CheckValue + " Player has: " + player.Buffs.GetCustomVar( strBuffName ));
                                                if (player.Buffs.GetCustomVar(strBuffName) == CheckValue)
                                                    blCanTrigger = true;
                                            }

                                            // If any of these conditions match, trigger the Activate block
                                            if (blCanTrigger)
                                            {
                                                AdvLogging.DisplayLog(AdvFeatureClass, block2.GetBlockName() + ":  Checks successfully passed for player: " + player.EntityName);
                                                break;
                                            }
                                            else
                                            {
                                                AdvLogging.DisplayLog(AdvFeatureClass, block2.GetBlockName() + ": Checks were NOT success for player: " + player.EntityName);
                                            }
                                        }

                                    }

                                }
                                else
                                {
                                    blCanTrigger = true;
                                }

                                if (blCanTrigger)
                                    break;
                            }

                        }
                    }

                    if (blCanTrigger)
                    {
                     
                        AdvLogging.DisplayLog(AdvFeatureClass, block2.GetBlockName() + ": TileEntity can call ActivateBlock. Calling it...");
                        Block.list[block.type].ActivateBlock(world, __instance.GetClrIdx(), __instance.ToWorldPos(), block, true, true);
                    }
                    else
                    {
                        AdvLogging.DisplayLog(AdvFeatureClass, block2.GetBlockName() + ": TileEntity is Active but is not Activating.");
                        Block.list[block.type].ActivateBlock(world, __instance.GetClrIdx(), __instance.ToWorldPos(), block, false, true);
                    }
                    return true;


                }

            }
            return false;
        }
    }
}
