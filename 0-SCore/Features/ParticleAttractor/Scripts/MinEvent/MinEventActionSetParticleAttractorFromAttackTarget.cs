using System.Xml.Linq;
using UnityEngine;

// 	<triggered_effect trigger="onSelfBuffUpdate" action="SetParticleAttractorFromAttackTarget, SCore" transform="LocationForScript" target_transform="Head" cansee="false" speed="5"  />

public class MinEventActionSetParticleAttractorFromAttackTarget : MinEventActionTargetedBase
{
    protected bool _canSee;
    protected float _speed = 5f;
    protected string _transformName = string.Empty;
    protected string _targetTransform = string.Empty;
    protected EntityAlive _sourceEntity;

    public override void Execute(MinEventParams _params)
    {
        _sourceEntity = _params.Self;

        // If no transform is listed, then just find the first one, and make the link.
        if (string.IsNullOrEmpty(_transformName))
        {
            Log.Out("SetParticleAttractorFromAttackTarget:: No Transform specified, looking for particle attractorLinear...");
            var particleAttractor = _sourceEntity.GetComponentInChildren<particleAttractorLinear>();
            ConfigureAttractor(particleAttractor);
            return;
        }

        // if we are given a transform name, look throughout the entity for it.
        Log.Out($"SetParticleAttractorFromAttackTarget:: Searching for Particle Attractor Linear on all transforms called: {_transformName}");
        foreach (var ps in _params.Self.GetComponentsInChildren<ParticleSystem>())
        {
            if ( ps.transform.name != _transformName) continue;
            var particleAttractor = ps.transform.gameObject.GetOrAddComponent<particleAttractorLinear>();
            ConfigureAttractor(particleAttractor);
        }
        
        
    }

    private void ConfigureAttractor(particleAttractorLinear particleAttractor)
    {
        var scoreParticleAttractor = new SCoreParticleAttractor(_sourceEntity, _sourceEntity.GetAttackTarget(), particleAttractor)
                {
                    Speed = _speed,
                    CanSee = _canSee,
                    TargetTransform = _targetTransform
                };
        scoreParticleAttractor.TriggerParticleAttractor();
    }
    // private void ConfigureAttractor2(particleAttractorLinear particleAttractor)
    // {
    //     if (particleAttractor == null) return;
    //     if (_sourceEntity == null) return;
    //
    //     var attackTarget = _sourceEntity.GetAttackTarget();
    //     if (attackTarget == null)
    //     {
    //         Log.Out("SetParticleAttractorFromAttackTarget:: No Attack Target.");
    //         particleAttractor.target = null;
    //         return;
    //     }
    //
    //     if (_canSee)
    //     {
    //         if (!_sourceEntity.CanSee(attackTarget))
    //         {
    //             Log.Out($"SetParticleAttractorFromAttackTarget:: Cannot see {attackTarget.EntityName}");
    //             particleAttractor.target = null;
    //             return;
    //         }
    //     }
    //
    //     if (attackTarget.IsDead())
    //     {
    //         particleAttractor.target = null;
    //         return;
    //     }
    //     if (!string.IsNullOrEmpty(_targetTransform))
    //     {
    //         Log.Out($"SetParticleAttractorFromAttackTarget:: Target Transform was specified. Searching for {_targetTransform}");
    //
    //         switch( _targetTransform.ToLower())
    //         {
    //             case "head":
    //                 Log.Out($"SetParticleAttractorFromAttackTarget:: Getting Head Transform");
    //                 particleAttractor.target = attackTarget.emodel.GetHeadTransform();
    //                 break;
    //             case"hips":
    //                 Log.Out($"SetParticleAttractorFromAttackTarget:: Getting Hips");
    //                 particleAttractor.target = attackTarget.emodel.GetPelvisTransform();
    //                 break;
    //             default:
    //                 Log.Out($"SetParticleAttractorFromAttackTarget:: Searching for {_targetTransform}");
    //                 foreach (var transform in attackTarget.GetComponentsInChildren<Transform>())
    //                 {
    //                     if (transform.name.ToLower() != _targetTransform.ToLower()) continue;
    //                     particleAttractor.target = transform;
    //                     break;
    //                 }
    //                 if (particleAttractor.target == null)
    //                 {
    //                     Log.Out($"SetParticleAttractorFromAttackTarget:: Target Transform Not Found: {_targetTransform}");
    //                     return;
    //                 }
    //                 break;
    //         }
    //         
    //         
    //     }
    //     else
    //     {
    //         Log.Out("SetParticleAttractorFromAttackTarget:: Target transform was not set. Aiming for the Head Transform.");
    //         particleAttractor.target = attackTarget.emodel.GetHeadTransform();
    //     }
    //
    //     Log.Out($"SetParticleAttractorFromAttackTarget:: Setting Speed to {_speed}");
    //     particleAttractor.speed = _speed;
    // }

    public override bool ParseXmlAttribute(XAttribute attribute)
    {
        var flag = base.ParseXmlAttribute(attribute);
        if (flag) return true;
        var name = attribute.Name.LocalName;
        if (name == "speed")
        {
            _speed = StringParsers.ParseFloat(attribute.Value);
            return true;
        }

        if (name == "cansee")
        {
            _canSee = StringParsers.ParseBool(attribute.Value);
            return true;
        }

        if (name == "transform")
        {
            _transformName = attribute.Value;
            return true;
        }

        if (name == "target_transform")
        {
            _targetTransform = attribute.Value;
            return true;
        }

        return false;
    }
}