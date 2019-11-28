//using Harmony;
//using System;
//using System.Collections.Generic;
//using UnityEngine;

//public class SphereII_TileEntityAlwaysActive
//{
  
//    [HarmonyPatch(typeof(TileEntity))]
//    [HarmonyPatch("IsActive")]
//    public class SphereII_TileEntity_IsActive
//    {
//        public static bool Prefix(ref bool __result, TileEntity __instance, World world)
//        {
//            Debug.Log("IsActive?");
//            BlockValue block = GameManager.Instance.World.GetBlock(__instance.ToWorldPos());
//            Block block2 = Block.list[block.type];
//            bool isAlwaysActive = false;
//            if (block2.Properties.Values.ContainsKey("AlwaysActive"))
//            {
//                Debug.Log("Block has AlwaysActive ");
//                isAlwaysActive = StringParsers.ParseBool(block2.Properties.Values["AlwaysActive"], 0, -1, true);
//                if (isAlwaysActive)
//                {
//                    Debug.Log("Always Active");
//                    if (block2.RadiusEffects != null)
//                    {
//                        Debug.Log("Raidus effects detected");
//                        List<global::Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(null, new Bounds(__instance.ToWorldPos().ToVector3(), Vector3.one * 5f));
//                        Debug.Log("Entities in Bound: " + entitiesInBounds.Count);
//                        if (entitiesInBounds.Count > 0)
//                        {
//                            for (int i = 0; i < entitiesInBounds.Count; i++)
//                            {
//                                EntityPlayer player = entitiesInBounds[i] as EntityPlayer;
//                                if (player)
//                                {
//                                    Debug.Log("Player in bounds");
//                                    float distanceSq = entitiesInBounds[i].GetDistanceSq(__instance.ToWorldPos().ToVector3());
//                                    for (int l = 0; l < block2.RadiusEffects.Length; l++)
//                                    {
//                                        BlockRadiusEffect blockRadiusEffect = block2.RadiusEffects[l];
//                                        Debug.Log("Checking radius: " + blockRadiusEffect.variable);
//                                        if (distanceSq <= blockRadiusEffect.radius * blockRadiusEffect.radius && player.Buffs.HasBuff(blockRadiusEffect.variable))
//                                        {
//                                            Debug.Log("Activating Block");
//                                            Block.list[block.type].ActivateBlock(world, __instance.GetClrIdx(), __instance.ToWorldPos(), block, true, true);
//                                        }
//                                    }
//                                }

//                            }
//                        }
//                    }
//                }

//            }

//            return true;
//        }
//    }
//}