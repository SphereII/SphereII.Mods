// using this namespace is necessary for Utilities AI Tasks
//       <task class="HealSelf, SCore" />
// The game adds UAI.UAITask to the class name for discover.

using System.Collections;
using UnityEngine;

namespace UAI
{
    public class UAITaskHealSelf : UAITaskBase
    {
        public static bool isRunning = false;
        private int _maxDistance = 0;
        protected override void initializeParameters()
        {
            if (Parameters.ContainsKey("max_distance")) _maxDistance = (int)StringParsers.ParseFloat(Parameters["max_distance"]);
        }


        public override void Update(Context _context)
        {
            base.Update(_context);

            if (isRunning) return;

            if (_context.Self.inventory.IsHoldingItemActionRunning())
                return;

            // Current holding index
            var originalIndex = _context.Self.inventory.GetFocusedItemIdx();
            var stack = EntityUtilities.GetItemStackByTag(_context.Self.entityId, "medical");
            if (Equals(stack, ItemStack.Empty))
                return;

            isRunning = true;

            // If the NPC doesn't have this cvar, give it an initial value, so it can heal somewhat from a bandage that it found in its inventory.
            if (!_context.Self.Buffs.HasCustomVar("medRegHealthIncSpeed"))
                _context.Self.Buffs.SetCustomVar("medRegHealthIncSpeed", 1f, true);

            GameManager.Instance.StartCoroutine(_context.Self.inventory.SimulateActionExecution(0, stack, delegate
            {
                _context.Self.inventory.DecItem(stack.itemValue, 1, false);
                _context.Self.inventory.SetHoldingItemIdx(originalIndex);
                _context.Self.inventory.SetItem(_context.Self.inventory.DUMMY_SLOT_IDX, ItemStack.Empty.Clone());
                _context.Self.inventory.OnUpdate();
                GameManager.Instance.StartCoroutine(SwitchBack(_context, originalIndex));
            }));

        }

        private IEnumerator SwitchBack(Context _context, int oldSlot)
        {
            while (_context.Self.inventory.IsHolsterDelayActive())
            {
                yield return null;
            }
            _context.Self.inventory.SetHoldingItemIdx(oldSlot);
            isRunning = false;
            this.Stop(_context);
        }
    }
}