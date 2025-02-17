//
// using HarmonyLib;
// using UnityEngine;
//
// namespace SCore.Features.DynamicBones.Harmony
// {
//     public class SDCSMatchRigsPatch
//     {
//         [HarmonyPatch(typeof(SDCSUtils))]
//         [HarmonyPatch("MatchRigs")]
//
//         public class SDCSUtilsMatchRigs
//         {
//             public static void Postfix(SDCSUtils.SlotData wornItem, Transform source, Transform target, SDCSUtils.TransformCatalog transformCatalog)
//             {
//                 // Test code.
//                 foreach (var children in target.GetComponentsInChildren<Transform>())
//                 {
//                     if (children.name == "bone_test_score")
//                     {
//                         var parent = children.parent;
//                         var bone =children.parent.gameObject.GetOrAddComponent<DynamicBone>();
//                         var head = parent.parent.FindInChildren("Head.002");
//                         bone.m_Root = head;
//                         bone.m_UpdateRate = 60f;
//                         bone.m_UpdateMode = DynamicBone.UpdateMode.Default;
//                         bone.m_Damping = 0.472f;
//                         bone.m_Elasticity = 0.142f;
//                         bone.m_Stiffness = 0.243f;
//                         bone.m_DistanceToObject = 20;
//                     }
//                 }
//             }
//
//         }
//     }
// }
