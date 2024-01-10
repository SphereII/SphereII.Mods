    using System.Xml.Linq;
    using UnityEngine;

    // 				<triggered_effect trigger="onSelfBuffUpdate" action="SetParticleAttractorFromAttackTarget, SCore" cansee="false"/>

    public class MinEventActionSetParticleAttractorFromAttackTarget : MinEventActionTargetedBase
    {
        private bool _canSee;
        public override void Execute(MinEventParams _params)
        {
            var particleAttractor = _params.Self.GetComponentInChildren<particleAttractorLinear>();
            if (particleAttractor == null)
            {
                return;
            }

            var attackTarget = _params.Self.GetAttackTarget();
            if ( attackTarget == null )
                return;
            if (_canSee)
            {
                if (!_params.Self.CanSee(attackTarget)) return;
            }
            particleAttractor.target = attackTarget.emodel.GetHeadTransform();
        }
        
        public override bool ParseXmlAttribute(XAttribute attribute)
        {
            var flag = base.ParseXmlAttribute(attribute);
            if (flag) return true;
            var name = attribute.Name.LocalName;
            if (name != "cansee") return false;
            _canSee = StringParsers.ParseBool(attribute.Value);
            return true;
        }
    }
