    using UnityEngine;

    // 	<triggered_effect trigger="onSelfBuffStart" action="SetParticleAttractorFromSource, SCore"/>

    public class MinEventActionSetParticleAttractorFromSource : MinEventActionTargetedBase
    {
        public override void Execute(MinEventParams _params)
        {
            var particleAttractor = _params.Self.GetComponentInChildren<particleAttractorLinear>();
            if (particleAttractor == null)
            {
                return;
            }
            
            for (int i = 0; i < targets.Count; i++)
            {
                var entityAlive = targets[i];
                particleAttractor.target = entityAlive.emodel.GetHeadTransform();
                break;
            }
        }
        
    }
