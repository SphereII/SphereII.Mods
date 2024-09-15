using System.IO;
using Challenges;
using HarmonyLib;

namespace SCore.Features.Challenges.Harmony {
    [HarmonyPatch(typeof(BaseChallengeObjective))]
    [HarmonyPatch("ReadObjective")]
    public class ChallengeReadObjective {
        public static bool Prefix(ref BaseChallengeObjective __result, byte _currentVersion,
            ChallengeObjectiveType _type, BinaryReader _br) {
            if ((ChallengeObjectiveTypeSCore)_type == ChallengeObjectiveTypeSCore.ChallengeObjectiveEnterPOI)
            {
                __result = new ChallengeObjectiveEnterPOI();
                __result.Read(_currentVersion, _br);
                return false;
            }
            
            if ((ChallengeObjectiveTypeSCore)_type == ChallengeObjectiveTypeSCore.ChallengeObjectiveCompleteQuestStealth)
            {
                __result = new ChallengeObjectiveCompleteQuestStealth();
                __result.Read(_currentVersion, _br);
                return false;
            }
            
            if ((ChallengeObjectiveTypeSCore)_type == ChallengeObjectiveTypeSCore.ChallengeObjectiveStealthKillStreak)
            {
                __result = new ChallengeObjectiveStealthKillStreak();
                __result.Read(_currentVersion, _br);
                return false;
            }
            
            if ((ChallengeObjectiveTypeSCore)_type == ChallengeObjectiveTypeSCore.ChallengeObjectiveKillWithItem)
            {
                __result = new ChallengeObjectiveKillWithItem();
                __result.Read(_currentVersion, _br);
                return false;
            }
            
            if ((ChallengeObjectiveTypeSCore)_type == ChallengeObjectiveTypeSCore.ChallengeObjectiveDecapitation)
            {
                __result = new ChallengeObjectiveDecapitation();
                __result.Read(_currentVersion, _br);
                return false;
            }
            
            if ((ChallengeObjectiveTypeSCore)_type == ChallengeObjectiveTypeSCore.ChallengeObjectiveCraftWithIngredient)
            {
                __result = new ChallengeObjectiveCraftWithIngredient();
                __result.Read(_currentVersion, _br);
                return false;
            }
            if ((ChallengeObjectiveTypeSCore)_type == ChallengeObjectiveTypeSCore.ChallengeObjectiveBlockDestroyedByFire)
            {
                __result = new ChallengeObjectiveBlockDestroyedByFire();
                __result.Read(_currentVersion, _br);
                return false;
            }
            
            if ((ChallengeObjectiveTypeSCore)_type == ChallengeObjectiveTypeSCore.ChallengeObjectiveBlockDestroyed)
            {
                __result = new ChallengeObjectiveBlockDestroyed();
                __result.Read(_currentVersion, _br);
                return false;
            }
            
            if ((ChallengeObjectiveTypeSCore)_type == ChallengeObjectiveTypeSCore.ChallengeObjectiveStartFire)
            {
                __result = new ChallengeObjectiveStartFire();
                __result.Read(_currentVersion, _br);
                return false;
            }
            
            if ((ChallengeObjectiveTypeSCore)_type == ChallengeObjectiveTypeSCore.ChallengeObjectiveBigFire)
            {
                __result = new ChallengeObjectiveBigFire();
                __result.Read(_currentVersion, _br);
                return false;
            }

            if ((ChallengeObjectiveTypeSCore)_type == ChallengeObjectiveTypeSCore.ChallengeObjectiveExtinguishFire)
            {
                __result = new ChallengeObjectiveExtinguishFire();
                __result.Read(_currentVersion, _br);
                return false;
            }

            
            
            return true;
        }
    }
}