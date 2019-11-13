//using Harmony;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection.Emit;
//using UnityEngine;


//public class ItemRadiusBuffEffect
//{

//    // hook into the ItemStack, which should cover all types of containers. This will run in the update task.
//    [HarmonyPatch(typeof(XUiC_ItemStack))]
//    [HarmonyPatch("Update")]
//    public class SphereII_XUiC_ItemStack_Update
//    {
//        public static bool Prefix(XUiC_ItemStack __instance, bool ___bLocked, bool ___isDragAndDrop)
//        {
//            // Make sure we are dealing with legitimate stacks.
//            if(__instance.ItemStack.IsEmpty())
//                return true;

//            if(__instance.ItemStack.itemValue == null)
//                return true;

//            if(___bLocked && ___isDragAndDrop)
//                return true;

//            // If the item class has a SpoilageTime, that means it can spoil over time.        
//            if (__instance.ItemStack.itemValue.ItemClass != null && __instance.ItemStack.itemValue.ItemClass.Properties.Contains("ActivatedBuff"))
//            {
//                List<BlockRadiusEffect> blockRadius = ItemsUtilities.GetRadiusEffect(__instance.ItemStack.itemValue.ItemClass.GetItemName());

//                String strDisplay = "XUiC_ItemStack: " + __instance.ItemStack.itemValue.ItemClass.GetItemName();
//                // Additional Spoiler flags to increase or decrease the spoil rate
//                switch (__instance.StackLocation)
//                {
//                    case XUiC_ItemStack.StackLocationTypes.ToolBelt:  // Tool belt Storage check
//                    case XUiC_ItemStack.StackLocationTypes.Backpack:        // Back pack storage check

                        
//                        break;
//                    case XUiC_ItemStack.StackLocationTypes.LootContainer:    // Loot Container Storage check
//                        TileEntityLootContainer container = __instance.xui.lootContainer;
//                        if (container != null)
//                        {
//                            BlockValue Container = GameManager.Instance.World.GetBlock(container.ToWorldPos());
//                            Block myBlock = Container.Block;
//                            BlockUtilitiesSDX.AddRadiusEffect(__instance.ItemStack.itemValue.ItemClass.GetItemName(), ref myBlock);
//                        }
//                        break;
//                    case XUiC_ItemStack.StackLocationTypes.Creative:  // Ignore Creative Containers
//                        return true;
//                    default:
//                        break;
//                }

//            }
     
//            return true;

//        }
//    }

//}


