using UnityEngine;

namespace Challenges
{
    public class ChallengeObjectiveHarvestV2 : BaseChallengeObjective
    {
        public override ChallengeObjectiveType ObjectiveType {
            get { return (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveHarvestV2; }
        }

        public override void HandleAddHooks()
        {
            QuestEventManager.Current.HarvestItem -= Current_HarvestItem;
            QuestEventManager.Current.HarvestItem += Current_HarvestItem;
        }

        public override void HandleRemoveHooks()
        {
            QuestEventManager.Current.HarvestItem -= Current_HarvestItem;
        }
        private void Current_HarvestItem(ItemValue held, ItemStack stack, BlockValue bv)
        {
            // Check all the requirements
            if (!ChallengeRequirementManager.IsValid(Owner.ChallengeClass.Name)) return;

            Current++;
            CheckObjectiveComplete();
        }

        public override BaseChallengeObjective Clone()
        {
            return new ChallengeObjectiveHarvestV2();
        }
    }
}