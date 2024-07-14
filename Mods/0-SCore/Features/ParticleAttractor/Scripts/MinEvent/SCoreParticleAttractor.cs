using UnityEngine;

public class SCoreParticleAttractor
{
    public EntityAlive SourceEntity;
    public EntityAlive TargetEntity;
    public bool CanSee;
    public particleAttractorLinear ParticleAttractorLinear;
    public float Speed = 5f;
    public string TargetTransform = string.Empty;

    public SCoreParticleAttractor(EntityAlive sourceEntity, EntityAlive targetEntity,
        particleAttractorLinear particleAttractorLinear)
    {
        SourceEntity = sourceEntity;
        TargetEntity = targetEntity;
        ParticleAttractorLinear = particleAttractorLinear;
    }

    public void TriggerParticleAttractor()
    {
        if (ParticleAttractorLinear == null) return;
        if (SourceEntity == null) return;
        if (TargetEntity == null)
        {
            ParticleAttractorLinear.target = null;
            return;
        }

        if (CanSee)
        {
            if (!SourceEntity.CanSee(TargetEntity))
            {
                Log.Out($"TriggerParticleAttractor:: Cannot see {TargetEntity.EntityName}");
                ParticleAttractorLinear.target = null;
                return;
            }
        }

        if (TargetEntity.IsDead())
        {
            ParticleAttractorLinear.target = null;
            return;
        }

        if (!string.IsNullOrEmpty(TargetTransform))
        {
            Log.Out(
                $"TriggerParticleAttractor:: Target Transform was specified. Searching for {TargetTransform}");

            switch (TargetTransform.ToLower())
            {
                case "head":
                    Log.Out($"TriggerParticleAttractor:: Getting Head Transform");
                    ParticleAttractorLinear.target = TargetEntity.emodel.GetHeadTransform();
                    break;
                case "hips":
                    Log.Out($"TriggerParticleAttractor:: Getting Hips");
                    ParticleAttractorLinear.target = TargetEntity.emodel.GetPelvisTransform();
                    break;
                default:
                    Log.Out($"TriggerParticleAttractor:: Searching for {TargetTransform}");
                    foreach (var transform in TargetEntity.GetComponentsInChildren<Transform>())
                    {
                        if (transform.name.ToLower() != TargetTransform.ToLower()) continue;
                        ParticleAttractorLinear.target = transform;
                        break;
                    }

                    if (ParticleAttractorLinear.target == null)
                    {
                        Log.Out(
                            $"TriggerParticleAttractor:: Target Transform Not Found: {TargetTransform}");
                        return;
                    }

                    break;
            }


        }
        else
        {
            Log.Out(
                "TriggerParticleAttractor:: Target transform was not set. Aiming for the Head Transform.");
            ParticleAttractorLinear.target = TargetEntity.emodel.GetHeadTransform();
        }

        Log.Out($"TriggerParticleAttractor:: Setting Speed to {Speed}");
        ParticleAttractorLinear.speed = Speed;
    }
}