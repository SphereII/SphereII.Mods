using System;
using System.Collections;
using UnityEngine;

// using this namespace is necessary for Utilities AI Tasks
//       <task class="MoveToHealTarget, SCore" />
// The game adds UAI.UAITask to the class name for discover.
namespace UAI
{
    public class UAITaskMoveToHealTarget : UAITaskMoveToTarget
    {

        public bool isRunning = true;

        protected override void initializeParameters()
        {
            base.initializeParameters();
            if (distance == 0)
                distance = 2;
        }

        public override void Update(Context _context)
        {
            base.Update(_context);

            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive != null)
            {
                _context.Self.RotateTo(entityAlive, 15f, 15f);
                _context.Self.SetLookPosition(entityAlive.position);

                var dist = Vector3.Distance(entityAlive.position, _context.Self.position);
                if (dist < distance)
                {

                    if (_context.Self.inventory.IsHoldingItemActionRunning())
                        return;

                    if (isRunning)
                        return;
                    isRunning = true;
                    EntityUtilities.Stop(_context.Self.entityId);

                    // If the NPC doesn't have this cvar, give it an initial value, so it can heal somewhat from a bandage that it found in its inventory.
                    if (!_context.Self.Buffs.HasCustomVar("medRegHealthIncSpeed"))
                        _context.Self.Buffs.SetCustomVar("medRegHealthIncSpeed", 1f, true);

                    var stack = EntityUtilities.GetItemStackByTag(_context.Self.entityId, "medical");

                    var originalIndex = 0;

                    // Heal the target
                    GameManager.Instance.StartCoroutine(SimulateActionExecution(_context, entityAlive, 1, stack, delegate
                    {
                        _context.Self.inventory.SetItem(_context.Self.inventory.DUMMY_SLOT_IDX, ItemStack.Empty.Clone());
                        _context.Self.inventory.OnUpdate();
                        GameManager.Instance.StartCoroutine(SwitchBack(_context, originalIndex));
                    }));

                }
            }


        }

        private IEnumerator SwitchBack(Context _context, int oldSlot)
        {
            _context.Self.inventory.SetHoldingItemIdx(oldSlot);
            _context.Self.inventory.OnUpdate();
            yield return new WaitForSeconds(0.1f);
            this.Stop(_context);
        }

        public IEnumerator SimulateActionExecution(Context _context, EntityAlive target, int _actionIdx, ItemStack _itemStack, Action onComplete)
        {
            _context.Self.inventory.SetItem(_context.Self.inventory.DUMMY_SLOT_IDX, _itemStack);
            yield return new WaitForSeconds(0.1f);
            _context.Self.inventory.SetHoldingItemIdx(_context.Self.inventory.DUMMY_SLOT_IDX);
            yield return new WaitForSeconds(0.1f);

            if (_context.Self.inventory.holdingItemData.actionData[1] is ItemActionUseOther.FeedInventoryData feedInventoryData)
            {
                feedInventoryData.TargetEntity = target;
                _itemStack.itemValue.ItemClass.Actions[1].ExecuteAction(feedInventoryData, false);
                yield return new WaitForSeconds(0.1f);
                _itemStack.itemValue.ItemClass.Actions[1].ExecuteAction(feedInventoryData, true);
                yield return new WaitForSeconds(0.1f);
                _context.Self.inventory.DecHoldingItem(1);
            }
            onComplete();
        }

        public override void Start(Context _context)
        {
            Debug.Log($"Moving to Heal! {_context.Self.ToString()}");
            isRunning = false;            
            base.Start(_context);
            SCoreUtils.SetCrouching(_context);

            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive != null)
                SCoreUtils.FindPath(_context, entityAlive.position, true);


        }

    }
}