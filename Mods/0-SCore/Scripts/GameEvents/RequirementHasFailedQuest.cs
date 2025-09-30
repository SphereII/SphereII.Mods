namespace GameEvent.SequenceRequirements
{
    /*
     * <action_sequence name="quest_XYZ_restart">
            <requirement type="HasFailedQuestSDX, SCore" value="quest_XYZ" />
            <action type="AddQuest" quest="quest_XYZ" /
        </action_sequence>

     */
    public class RequirementHasFailedQuest : BaseRequirement
    {
        private const string PropQuest = "quest";
        private string Value;
        
        public override void OnInit()
        {
        }

        public override bool CanPerform(Entity target)
        {
            if (string.IsNullOrEmpty(Value))
                return false;

            var invert = false;
            if (Value.StartsWith("!"))
            {
                invert = true;
                Value = Value.Replace("!", "");
            }

            if (target is not EntityPlayer player) return false;
            var myQuest = player.QuestJournal.FindQuest(Value.ToLower());
            if (myQuest == null) return false;

            if (invert)
                return myQuest.CurrentState != Quest.QuestState.Failed;

            return myQuest.CurrentState == Quest.QuestState.Failed;
        }

        public override BaseRequirement CloneChildSettings()
        {
            return new RequirementHasFailedQuest
            {
                Invert = this.Invert,
                Value = Value
            };
        }

        public override void ParseProperties(DynamicProperties properties)
        {
            base.ParseProperties(properties);
            if (properties.Values.ContainsKey(PropQuest))
            {
                Value = properties.Values[PropQuest];
            }
        }
    }
}