    using UnityEngine;

    //				<triggered_effect trigger="onSelfBuffStart" action="SetParticleAttractorFromPlayer, SCore"/>

    public class MinEventActionSetParticleAttractorFromPlayer : MinEventActionTargetedBase
    {
        public override void Execute(MinEventParams _params)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                var entityAlive = targets[i];
                var particleAttractor =entityAlive.GetComponentInChildren<particleAttractorLinear>();
                if (particleAttractor == null)
                {
                    continue;
                }
                particleAttractor.target = _params.Self.emodel.GetHeadTransform();
                break;
            }
        }
        
    }
