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
        public override void initializeParameters()
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

            // v3.3: SimulateActionExecution needs the grid slot the item lives in, not a copy of it.
            if (!EntityUtilities.TryFindItemGridSlotByTag(_context.Self.entityId, "medical", out var grid, out var slotIdx))
                return;

            var itemValue = grid.items[slotIdx].itemValue.Clone();

            isRunning = true;

            // If the NPC doesn't have this cvar, give it an initial value, so it can heal somewhat from a bandage that it found in its inventory.
            if (!_context.Self.Buffs.HasCustomVar("medRegHealthIncSpeed"))
                _context.Self.Buffs.SetCustomVar("medRegHealthIncSpeed", 1f, true);

            GameManager.Instance.StartCoroutine(HealRoutine(_context, grid, slotIdx, itemValue, originalIndex));

        }

        // v3.3: Hand owns the transient hold slot and restores the previously held item itself, so
        // all that is left here is waiting the simulation out and doing the consume/restore bookkeeping.
        private IEnumerator HealRoutine(Context _context, ItemStackGrid _grid, int _slotIdx, ItemValue _itemValue, int _originalIndex)
        {
            yield return _context.Self.inventory.SimulateActionExecution(0, _grid, _slotIdx);
            EntityUtilities.DecItemFromAnyStore(_context.Self, _itemValue, 1);
            yield return SwitchBack(_context, _originalIndex);
        }

        private IEnumerator SwitchBack(Context _context, int oldSlot)
        {
            while (_context.Self.inventory.IsHandSwitching())
            {
                yield return null;
            }
            _context.Self.inventory.SetHoldingItemIdx(oldSlot);
            isRunning = false;
            this.Stop(_context);
        }
    }
}