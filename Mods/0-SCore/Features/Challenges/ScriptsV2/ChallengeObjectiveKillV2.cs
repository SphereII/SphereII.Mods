namespace Challenges
{
    public class ChallengeObjectiveKillV2 : BaseChallengeObjective
    {
        public override ChallengeObjectiveType ObjectiveType {
            get { return (ChallengeObjectiveType)ChallengeObjectiveTypeSCore.ChallengeObjectiveKillV2; }
        }

        public override void HandleAddHooks()
        {
            QuestEventManager.Current.EntityKill -= Current_EntityKill;
            QuestEventManager.Current.EntityKill += Current_EntityKill;
        }

        public override void HandleRemoveHooks()
        {
            QuestEventManager.Current.EntityKill -= Current_EntityKill;
        }

        private void Current_EntityKill(EntityAlive killedBy, EntityAlive killedEntity)
        {
            // Check all the requirements
            if (!ChallengeRequirementManager.IsValid(Owner.ChallengeClass.Name)) return;

            Current++;
            CheckObjectiveComplete();

        }

        public override BaseChallengeObjective Clone()
        {
            return new ChallengeObjectiveKillV2();
        }

    }
}