namespace GameEvent.SequenceRequirements
{
    public class RequirementInVehicleSDX : BaseRequirement
    {
        private FastTags<TagGroup.Global> _fastTags = FastTags<TagGroup.Global>.none;
        private const string PropEntityTags = "entity_tags";

        public override void OnInit()
        {
        }

        public override bool CanPerform(Entity target)
        {
            if (!target.AttachedToEntity) return this.Invert;

            if (target.HasAnyTags(_fastTags))
                return !this.Invert;

            return this.Invert;
        }

        public override BaseRequirement CloneChildSettings()
        {
            return new RequirementInVehicleSDX
            {
                Invert = this.Invert,
                _fastTags = _fastTags
            };
        }

        public override void ParseProperties(DynamicProperties properties)
        {
            base.ParseProperties(properties);
            if (properties.Values.ContainsKey(PropEntityTags))
            {
                _fastTags = FastTags<TagGroup.Global>.Parse(properties.Values[PropEntityTags]);
            }
        }
    }
}