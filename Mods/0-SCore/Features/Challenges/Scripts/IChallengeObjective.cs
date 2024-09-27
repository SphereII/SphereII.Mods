namespace Challenges {
    public interface IChallengeObjective {
        ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.Invalid;
        void HandleAddHooks();
        void HandleRemoveHooks();
        BaseChallengeObjective Clone();

    }
}