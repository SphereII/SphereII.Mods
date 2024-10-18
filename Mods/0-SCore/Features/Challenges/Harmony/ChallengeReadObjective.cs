using System;
using System.IO;
using Challenges;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Challenges.Harmony {
    [HarmonyPatch(typeof(BaseChallengeObjective))]
    [HarmonyPatch("ReadObjective")]
    public class ChallengeReadObjective {
        public static BaseChallengeObjective Postfix(BaseChallengeObjective __result, byte _currentVersion,
            ChallengeObjectiveType _type, BinaryReader _br) {
           // Debug.Log($"Type: {(ChallengeObjectiveType)_type}");
            if (__result != null) return __result;
            
          //  Debug.Log($" SCore Type: {(ChallengeObjectiveTypeSCore)_type}");
            switch ((ChallengeObjectiveTypeSCore)_type)
            {
                case ChallengeObjectiveTypeSCore.ChallengeObjectiveEnterPOI:
                    __result = new ChallengeObjectiveEnterPOI();
                    __result.Read(_currentVersion, _br);
                    break;
                case ChallengeObjectiveTypeSCore.ChallengeObjectiveCompleteQuestStealth:
                    __result = new ChallengeObjectiveCompleteQuestStealth();
                    __result.Read(_currentVersion, _br);
                    break;
                case ChallengeObjectiveTypeSCore.ChallengeObjectiveStealthKillStreak:
                    __result = new ChallengeObjectiveStealthKillStreak();
                    __result.Read(_currentVersion, _br);
                    break;
                case ChallengeObjectiveTypeSCore.ChallengeObjectiveKillWithItem:
                    __result = new ChallengeObjectiveKillWithItem();
                    __result.Read(_currentVersion, _br);
                    break;
                case ChallengeObjectiveTypeSCore.ChallengeObjectiveDecapitation:
                    __result = new ChallengeObjectiveDecapitation();
                    __result.Read(_currentVersion, _br);
                    break;
                case ChallengeObjectiveTypeSCore.ChallengeObjectiveCraftWithIngredient:
                    __result = new ChallengeObjectiveCraftWithIngredient();
                    __result.Read(_currentVersion, _br);
                    break;
                case ChallengeObjectiveTypeSCore.ChallengeObjectiveBlockDestroyedByFire:
                    __result = new ChallengeObjectiveBlockDestroyedByFire();
                    __result.Read(_currentVersion, _br);
                    break;
                case ChallengeObjectiveTypeSCore.ChallengeObjectiveBlockDestroyed:
                    __result = new ChallengeObjectiveBlockDestroyed();
                    __result.Read(_currentVersion, _br);
                    break;
                case ChallengeObjectiveTypeSCore.ChallengeObjectiveStartFire:
                    __result = new ChallengeObjectiveStartFire();
                    __result.Read(_currentVersion, _br);
                    break;
                case ChallengeObjectiveTypeSCore.ChallengeObjectiveBigFire:
                    __result = new ChallengeObjectiveBigFire();
                    __result.Read(_currentVersion, _br);
                    break;
                case ChallengeObjectiveTypeSCore.ChallengeObjectiveExtinguishFire:
                    __result = new ChallengeObjectiveExtinguishFire();
                    __result.Read(_currentVersion, _br);
                    break;
                case ChallengeObjectiveTypeSCore.ChallengeObjectiveHireNPC:
                    __result = new ChallengeObjectiveHireNPC();
                    __result.Read(_currentVersion, _br);
                    break;
                case ChallengeObjectiveTypeSCore.ChallengeObjectiveHarvest:
                    __result = new ChallengeObjectiveHarvest();
                    __result.Read(_currentVersion, _br);
                    break;
                case ChallengeObjectiveTypeSCore.ChallengeObjectiveBlockUpgradeSCore:
                    __result = new ChallengeObjectiveBlockUpgradeSCore();
                    __result.Read(_currentVersion, _br);
                    break;
                
                case ChallengeObjectiveTypeSCore.ChallengeObjectiveGatherTags:
                    __result = new ChallengeObjectiveGatherTags();
                    __result.Read(_currentVersion, _br);
                    break;
                
                case ChallengeObjectiveTypeSCore.ChallengeObjectiveWearTags:
                    __result = new ChallengeObjectiveWearTags();
                    __result.Read(_currentVersion, _br);
                    break;
                case ChallengeObjectiveTypeSCore.ChallengeObjectiveCVar:
                    __result = new ChallengeObjectiveCVar();
                    __result.Read(_currentVersion, _br);
                    break;
                case ChallengeObjectiveTypeSCore.ChallengeObjectiveCraftWithTags:
                    __result = new ChallengeObjectiveCraftWithTags();
                    __result.Read(_currentVersion, _br);
                    break;
                    
                
                    
            }

            
            
            
            return __result;
        }
    }
    
    // [HarmonyPatch(typeof(ChallengeClass))]
    // [HarmonyPatch("ResetObjectives")]
    // public class ChallengeClassResetObjective {
    //     public static bool Prefix(ChallengeClass __instance, Challenge challenge) {
    //         
    //         Debug.Log($"Challenge Objective List: {challenge.ObjectiveList.Count}");
    //         Debug.Log($"Challenge Name: {challenge.ChallengeClass.Name}");
    //         Debug.Log($"Instance Objective List: {__instance.ObjectiveList.Count}");
    //         Debug.Log($"Instance Name: {__instance.Name}");
    //         return true;
    //         if (challenge.ObjectiveList.Count != __instance.ObjectiveList.Count)
    //         {
    //             Debug.Log("No Objectives");
    //             return false;
    //         }
    //         for (int i = 0; i < __instance.ObjectiveList.Count; i++)
    //         {
    //             BaseChallengeObjective baseChallengeObjective = __instance.ObjectiveList[i];
    //             if (baseChallengeObjective.GetType() != challenge.ObjectiveList[i].GetType())
    //             {
    //               Debug.Log($"Challenge Objective: {challenge.ObjectiveList[i].GetType()}");
    //               Debug.Log($"Instance Objective: {baseChallengeObjective.GetType()}");
    //             }
    //             BaseChallengeObjective baseChallengeObjective3 = baseChallengeObjective.Clone();
    //             baseChallengeObjective3.Owner = challenge;
    //             baseChallengeObjective3.IsRequirement = baseChallengeObjective.IsRequirement;
    //             baseChallengeObjective3.MaxCount = baseChallengeObjective.MaxCount;
    //             baseChallengeObjective3.ShowRequirements = baseChallengeObjective.ShowRequirements;
    //             baseChallengeObjective3.HandleOnCreated();
    //             //baseChallengeObjective3.CopyValues(baseChallengeObjective2, baseChallengeObjective);
    //             challenge.ObjectiveList[i] = baseChallengeObjective3;
    //         }
    //         return true;  
    //         return false;
    //
    //     }
    // }
}