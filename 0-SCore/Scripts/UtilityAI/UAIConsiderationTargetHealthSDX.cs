using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace UAI
{

    public class UAIConsiderationTargetHealthSDX : UAIConsiderationBase
    {
        private float _min = 0.01f;
        private float _max = 0.75f;

        public override void Init(Dictionary<string, string> _parameters)
        {
            base.Init(_parameters);
            if (_parameters.ContainsKey("min"))
                _min = StringParsers.ParseFloat(_parameters["min"], 0, -1, NumberStyles.Any);

            if (_parameters.ContainsKey("max"))
                _max = StringParsers.ParseFloat(_parameters["max"], 0, -1, NumberStyles.Any);
        }

        public override float GetScore(Context _context, object target)
        {
            // If we aren't using the percent hooks, just use the base class
            var currentHealth = -1f;

            var entityAlive = UAIUtils.ConvertToEntityAlive(target);
            if (entityAlive != null)
                currentHealth = ((float)entityAlive.Health / (float)entityAlive.GetMaxHealth());

            if (target is Vector3 vector3)
            {
                var block = _context.Self.world.GetBlock(new Vector3i(vector3));
                var block2 = block.Block;
                currentHealth = ((float)(block2.MaxDamage - block.damage) / (float)block2.MaxDamage);
            }

            
            if (currentHealth <= 0)
                return 0f;

            if (currentHealth >= _max || currentHealth <= _min)
                return 0f;

            return 1f;

            //return currentHealth;

        }
    }
}