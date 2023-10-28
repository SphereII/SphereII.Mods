﻿using GameEvent.SequenceRequirements;

namespace SCore.Scripts.GameEvents
{
    public class RequirementInVechileSDX : BaseRequirement
    {
        private FastTags _fastTags = FastTags.none;
        private const string PropEntityTags = "entity_tags";

        protected override void OnInit()
        {
        }

        public override bool CanPerform(Entity target)
        {
            if (!target.AttachedToEntity) return this.Invert;
            
            if ( target.HasAnyTags(_fastTags))
                return !this.Invert;
            
            return this.Invert;
        }

        protected override BaseRequirement CloneChildSettings()
        {
            return new RequirementInVechileSDX
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
                _fastTags = FastTags.Parse(properties.Values[PropEntityTags]);
            }
        }
        
    }
}