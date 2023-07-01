using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace UAI
{

    public class UAIConsiderationNotInWeaponRange : UAIConsiderationInWeaponRange
    {
        public override float GetScore(Context context, object target)
        {
            return EntityUtilities.SameValue(base.GetScore(context, target), 1f) ? 0f : 1f;
        }
    }
    public class UAIConsiderationInWeaponRange : UAIConsiderationTargetDistance
    {
        private int _actionIndex = 0;
        public override void Init(Dictionary<string, string> parameters)
        {
            base.Init(parameters);
            if (parameters.ContainsKey("action_index"))
                _actionIndex = int.Parse(parameters["action_index"]);
        }

        public override float GetScore(Context context, object target)
        {
            var entityAlive = UAIUtils.ConvertToEntityAlive(target);
            var result = 0f;
            if (entityAlive != null)
               result = context.Self.GetDistance(entityAlive);

            if (target is Vector3 vector3)
                result = (context.Self.position - vector3).magnitude;

            
            var range = context.Self.inventory.holdingItem.Actions[_actionIndex].Range;
            if (range == 0f)
                range = 1.50f;
            
            if (context.Self.inventory.holdingItem.Actions[_actionIndex] is not ItemActionRanged itemActionRanged)
                return result <= range ? 1f : 0f;
            if (context.Self.inventory.holdingItemData.actionData[_actionIndex] is not
                ItemActionRanged.ItemActionDataRanged itemActionData)
            {
                return result <= range ? 1f : 0f;
            }
            range = itemActionRanged.GetRange(itemActionData);

            if (!SCoreUtils.CanShoot(context.Self, entityAlive, range))
            {
                return 0f;
            }

            return result <= range ? 1f : 0f;
        }
    }
}