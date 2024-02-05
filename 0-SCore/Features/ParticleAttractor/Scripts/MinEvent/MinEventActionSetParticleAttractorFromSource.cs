    using System.Xml.Linq;
    using UnityEngine;

    // 	<triggered_effect trigger="onSelfBuffStart" action="SetParticleAttractorFromSource, SCore"/>

    public class MinEventActionSetParticleAttractorFromSource : MinEventActionSetParticleAttractorFromAttackTarget
    {
        public override void Execute(MinEventParams _params)
        {
            _sourceEntity = _params.Self;

            // If no transform is listed, then just find the first one, and make the link.
            if (string.IsNullOrEmpty(_transformName))
            {
                Log.Out("SetParticleAttractorFromSource:: No Transform specified, looking for particle attractorLinear...");
                var particleAttractor = _sourceEntity.GetComponentInChildren<particleAttractorLinear>();
                ConfigureAttractor(particleAttractor);
                return;
            }

            // if we are given a transform name, look throughout the entity for it.
            Log.Out($"SetParticleAttractorFromSource:: Searching for Particle Attractor Linear on all transforms called: {_transformName}");
            foreach (var ps in _params.Self.GetComponentsInChildren<ParticleSystem>())
            {
                if ( ps.transform.name != _transformName) continue;
                var particleAttractor = ps.transform.gameObject.GetOrAddComponent<particleAttractorLinear>();
                ConfigureAttractor(particleAttractor);
            }

        }
        
        private void ConfigureAttractor(particleAttractorLinear particleAttractor)
        {
            foreach (var t in targets)
            {
                var scoreParticleAttractor = new SCoreParticleAttractor(_sourceEntity, t, particleAttractor)
                {
                    Speed = _speed,
                    CanSee = _canSee,
                    TargetTransform = _targetTransform
                };
                scoreParticleAttractor.TriggerParticleAttractor();
            }
        }

     
        
    }
