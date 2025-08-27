using System;
using System.Collections.Generic;
using System.IO;
using Challenges;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Challenges.Harmony
{
    [HarmonyPatch(typeof(BaseChallengeObjective))]
    [HarmonyPatch("ReadObjective")]
    public class ChallengeReadObjective
    {
        // Use a static dictionary to map enum types to their corresponding class factories.
        private static readonly Dictionary<ChallengeObjectiveTypeSCore, Func<BaseChallengeObjective>> _objectiveFactories;

        // Static constructor to initialize the dictionary once.
        static ChallengeReadObjective()
        {
            _objectiveFactories = new Dictionary<ChallengeObjectiveTypeSCore, Func<BaseChallengeObjective>>
            {
                { ChallengeObjectiveTypeSCore.ChallengeObjectiveEnterPOI, () => new ChallengeObjectiveEnterPOI() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectiveCompleteQuestStealth, () => new ChallengeObjectiveCompleteQuestStealth() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectiveStealthKillStreak, () => new ChallengeObjectiveStealthKillStreak() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectiveKillWithItem, () => new ChallengeObjectiveKillWithItem() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectiveDecapitation, () => new ChallengeObjectiveDecapitation() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectiveCraftWithIngredient, () => new ChallengeObjectiveCraftWithIngredient() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectiveBlockDestroyedByFire, () => new ChallengeObjectiveBlockDestroyedByFire() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectiveBlockDestroyed, () => new ChallengeObjectiveBlockDestroyed() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectiveStartFire, () => new ChallengeObjectiveStartFire() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectiveBigFire, () => new ChallengeObjectiveBigFire() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectiveExtinguishFire, () => new ChallengeObjectiveExtinguishFire() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectiveHireNPC, () => new ChallengeObjectiveHireNPC() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectiveHarvest, () => new ChallengeObjectiveHarvest() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectiveBlockUpgradeSCore, () => new ChallengeObjectiveBlockUpgradeSCore() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectiveGatherTags, () => new ChallengeObjectiveGatherTags() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectiveWearTags, () => new ChallengeObjectiveWearTags() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectiveCVar, () => new ChallengeObjectiveCVar() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectiveCraftWithTags, () => new ChallengeObjectiveCraftWithTags() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectivePlaceBlockByTag, () => new ChallengeObjectivePlaceBlockByTag() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectiveClearSleepers, () => new ChallengeObjectiveClearSleepers() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectiveKillV2, () => new ChallengeObjectiveKillV2() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectiveHarvestV2, () => new ChallengeObjectiveHarvestV2() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectivePlaceBlockByTagV2, () => new ChallengeObjectivePlaceBlockByTagV2() },
                { ChallengeObjectiveTypeSCore.ChallengeObjectiveCVarV2, () => new ChallengeObjectiveCVarV2() }



            };
        }

        public static BaseChallengeObjective Postfix(BaseChallengeObjective __result, byte _currentVersion, ChallengeObjectiveType _type, BinaryReader _br)
        {
            // If the base method already returned an objective, we don't need to do anything.
            if (__result != null) return __result;
            
            // Look up the objective type in our factory dictionary.
            if (_objectiveFactories.TryGetValue((ChallengeObjectiveTypeSCore)_type, out var factory))
            {
                __result = factory.Invoke();
                __result.Read(_currentVersion, _br);
            }
            // If the key is not in the dictionary, __result remains null, and the base game will handle it or an error will occur.
            
            return __result;
        }
    }
}