using System.Collections.Generic;

namespace UAI
{
    public class UAIConsiderationNotHasOrder : UAIConsiderationHasOrder
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.SameValue(base.GetScore(_context, target), 1f) ? 0f : 1f;
        }
    }

    public class UAIConsiderationHasOrder : UAIConsiderationBase
    {
        private List<EntityUtilities.Orders> _orders = new List<EntityUtilities.Orders>();

        public override void Init(Dictionary<string, string> parameters)
        {
            base.Init(parameters);
            if (parameters.ContainsKey("order"))
            {
                // One order, or a comma-separated list such as "Follow,Loot".
                foreach (var order in parameters["order"].Split(','))
                {
                    if (string.IsNullOrWhiteSpace(order)) continue;
                    _orders.Add(EnumUtils.Parse<EntityUtilities.Orders>(order.Trim(), true));
                }
            }
            else
                _orders.Add(EntityUtilities.Orders.Wander);

        }

        public override float GetScore(Context _context, object target)
        {
            var currentOrder = EntityUtilities.GetCurrentOrder(_context.Self.entityId);
            return _orders.Contains(currentOrder) ? 1f : 0f;
        }
    }
}