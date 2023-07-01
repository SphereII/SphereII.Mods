using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace UAI
{
    public class UAIConsiderationCanNotHearTarget : UAIConsiderationCanHearTarget
    {
        public override float GetScore(Context _context, object target)
        {
            var targetEntity = UAIUtils.ConvertToEntityAlive(target);
            if (targetEntity != null && targetEntity.IsDead())
                return 0f;

            return (base.GetScore(_context, target) == 1f) ? 0f : 1f;
        }
    }

    public class UAIConsiderationCanHearTarget : UAIConsiderationBase
    {
        public override float GetScore(Context _context, object target)
        {
            var targetEntity = UAIUtils.ConvertToEntityAlive(target) as EntityPlayer;
            if (targetEntity == null)
                return 0f;

            // If the entity has an investigation position, set this to be true.
            // This consideration is only used to know if the NPC can hear the player or not.
            // However, the sound is so short lived, it won't ever be reliable unless the player is making a lot of repeatable sounds.
            if (_context.Self.HasInvestigatePosition)
                return 1f;

            // Taken from PlayerStealth's Tick()
            var distance = targetEntity.GetDistance(_context.Self);

            // figure out the noise volume and distance, using the EAI system settings for zombies.
            var num11 = targetEntity.Stealth.noiseVolume * (1f + EAIManager.CalcSenseScale() * _context.Self.aiManager.feralSense);
            num11 /= distance * 0.6f + 0.4f;
            var flag = true;
            if (_context.Self.noisePlayer)
                flag = (num11 > _context.Self.noisePlayerVolume);

            // if we can still hear them, set the target
            if (flag)
            {
                _context.Self.noisePlayer = targetEntity;
                _context.Self.noisePlayerDistance = distance;
                _context.Self.noisePlayerVolume = num11;
            }

            // If we have a noisy player, and we can hear them.
            if (!_context.Self.noisePlayer) return 0f;
            if (!(_context.Self.noisePlayerVolume >= _context.Self.noiseWake)) return 0f;
            _context.Self.SetInvestigatePosition(_context.Self.noisePlayer.position, 1200, true);
            return 1;

        }
    }
}