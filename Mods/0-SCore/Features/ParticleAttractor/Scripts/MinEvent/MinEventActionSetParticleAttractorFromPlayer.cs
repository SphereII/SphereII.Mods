    using UnityEngine;

    //				<triggered_effect trigger="onSelfBuffStart" action="SetParticleAttractorFromPlayer, SCore"/>

    public class MinEventActionSetParticleAttractorFromPlayer : MinEventActionTargetedBase
    {
        public override void Execute(MinEventParams _params)
        {
            Log.Out($"FromPlayer: {_params.Self.EntityName} is checking Targets::");
            for (int i = 0; i < targets.Count; i++)
            {
                var entityAlive = targets[i];
                Log.Out($"FromPlayer: Checking Target {entityAlive.EntityName} to see if they have a particleAttractorLinear.");
                var particleAttractor =entityAlive.GetComponentInChildren<particleAttractorLinear>();
                if (particleAttractor == null)
                {
                    Log.Out($"FromPlayer: Particle Attractor Linear Not Found On {entityAlive.EntityName}");
                    continue;
                }
                Log.Out($"FromPlayer: Setting Target to : {_params.Self.EntityName}");

                particleAttractor.target = _params.Self.emodel.GetHeadTransform();
                break;
            }
        }
        
    }
