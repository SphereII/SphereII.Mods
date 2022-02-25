using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace UAI
{

    public class UAIConsiderationNotInWeaponRange : UAIConsiderationInWeaponRange
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.SameValue(base.GetScore(_context, target), 1f) ? 0f : 1f;
        }
    }
    public class UAIConsiderationInWeaponRange : UAIConsiderationTargetDistance
    {
        private int action_index = 0;
        public override void Init(Dictionary<string, string> _parameters)
        {
            base.Init(_parameters);
            if (_parameters.ContainsKey("action_index"))
                action_index = int.Parse(_parameters["action_index"]);
        }

        public override float GetScore(Context _context, object target)
        {
            var entityAlive = UAIUtils.ConvertToEntityAlive(target);
            var result = 0f;
            if (entityAlive != null)
               result = _context.Self.GetDistance(entityAlive);

            if (target is Vector3 vector3)
                result = (_context.Self.position - vector3).magnitude;

            
            var range = _context.Self.inventory.holdingItem.Actions[action_index].Range;
            if (_context.Self.inventory.holdingItem.Actions[action_index] is ItemActionRanged itemActionRanged)
            {
                if (_context.Self.inventory.holdingItemData.actionData[action_index] is ItemActionRanged.ItemActionDataRanged itemActionData)
                {
                    range = itemActionRanged.GetRange(itemActionData);

                    if (!SCoreUtils.CanShoot(_context.Self, entityAlive, range))
                        return 0f;
                }
            }

            if (result <= range)
                return 1f;

            return 0f;
        }
    }
}