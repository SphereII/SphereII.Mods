using System.IO;
using Challenges;
using HarmonyLib;

public enum ChallengeObjectiveTypeSCore : byte {
    ChallengeObjectiveEnterPOI
}

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

            return true;
        }
    }
}