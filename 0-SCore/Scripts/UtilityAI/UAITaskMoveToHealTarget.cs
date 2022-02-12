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

        private string items;

        public bool isRunning = true;

        protected override void initializeParameters()
        {
            base.initializeParameters();
            if (distance == 0)
                distance = 3;

            if (Parameters.ContainsKey("items"))
                items = Parameters["items"];
        }

        public override void Update(Context _context)
        {
            base.Update(_context);

            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive != null)
            {
              //  SCoreUtils.SetLookPosition(_context, entityAlive);
                var dist = Vector3.Distance(entityAlive.position, _context.Self.position);
                if (dist < distance)
                {
                    EntityUtilities.Stop(_context.Self.entityId);

                    if (_context.Self.inventory.IsHoldingItemActionRunning())
                        return;

                    if (isRunning)
                        return;

                    isRunning = true;

                    var stack = ItemStack.Empty;
                    ItemStack[] array = _context.Self.bag.GetSlots();
                    if (_context.Self.lootContainer != null)
                    {
                        array = _context.Self.lootContainer.GetItems();
                    }
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array[i] != null && array[i] != ItemStack.Empty )
                        {
                            ItemValue itemValue = array[i].itemValue.Clone();
                            if (itemValue.ItemClass != null && itemValue.ItemClass.GetItemName().Contains(items))
                            {
                                //array[i].count--;
                                //if (array[i].count == 0)
                                //{
                                //    array[i] = ItemStack.Empty.Clone();
                                //}
                                //_context.Self.bag.SetSlots(array);
                                //_context.Self.bag.OnUpdate();
                                //_context.Self.lootContainer.UpdateSlot(i, array[i]);
                                stack = new ItemStack(itemValue, 1);
                                break;
                            }
                        }
                    }

                    // If the NPC doesn't have this cvar, give it an initial value, so it can heal somewhat from a bandage that it found in its inventory.
                    if (!_context.Self.Buffs.HasCustomVar("medRegHealthIncSpeed"))
                        _context.Self.Buffs.SetCustomVar("medRegHealthIncSpeed", 1f, true);

                    //var stack = EntityUtilities.GetItemStackByTag(_context.Self.entityId, "medical");
                   //stack = EntityUtilities.GetItemByName(_context.Self.entityId, items);
                    
                    var originalIndex = 0;

                    // Heal the target
                    //GameManager.Instance.StartCoroutine(SimulateActionExecution(_context, entityAlive, 1, stack, null));
                    if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
                        SimulateActionExecution(_context, entityAlive, 1, stack);

                }
            }


        }

        public void SimulateActionExecution(Context _context, EntityAlive target, int _actionIdx, ItemStack _itemStack)
        {


            var originalItem = _context.Self.inventory.GetItem(0);
            _context.Self.MinEventContext.Other = target;
            _context.Self.inventory.SetItem(0, _itemStack);
            _context.Self.inventory.SetHoldingItemIdx(0);
            Debug.Log("Healing"); 
            if (_context.Self.inventory.holdingItemData.actionData[1] is ItemActionUseOther.FeedInventoryData feedInventoryData)
            {
                feedInventoryData.TargetEntity = target;
                _itemStack.itemValue.ItemClass.Actions[_actionIdx].ExecuteAction(feedInventoryData, true);

                //  _context.Self.inventory.Execute(_actionIdx, false);
 //               _context.Self.inventory.Execute(_actionIdx, true);

   //             feedInventoryData.invData.holdingEntity.MinEventContext.Other = target;
     //           feedInventoryData.invData.holdingEntity.MinEventContext.ItemValue = _itemStack.itemValue;
       //         feedInventoryData.invData.holdingEntity.FireEvent(MinEventTypes.onSelfHealedOther, true);
         //       feedInventoryData.invData.holdingEntity.FireEvent((_actionIdx == 0) ? MinEventTypes.onSelfPrimaryActionEnd : MinEventTypes.onSelfSecondaryActionEnd, true);
                //_itemStack.itemValue.ItemClass.Actions[_actionIdx].ExecuteAction(feedInventoryData, false);
                //_itemStack.itemValue.ItemClass.Actions[_actionIdx].ExecuteAction(feedInventoryData, true);
                //_itemStack.count--;
                //if (_itemStack.count < 1)
                //_itemStack = ItemStack.Empty;

                //     _context.Self.inventory.SetItem(_context.Self.inventory.DUMMY_SLOT_IDX, ItemStack.Empty.Clone());

                _context.Self.inventory.SetItem(0, _itemStack);
                _context.Self.inventory.SetHoldingItemIdx(0); _context.Self.inventory.OnUpdate();
                this.Stop(_context);
            }
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
                Debug.Log("Execute Action false");
                _itemStack.itemValue.ItemClass.Actions[1].ExecuteAction(feedInventoryData, false);
                yield return new WaitForSeconds(0.1f);
                Debug.Log("Execute Action true");
                _itemStack.itemValue.ItemClass.Actions[1].ExecuteAction(feedInventoryData, true);
                yield return new WaitForSeconds(0.1f);
                Debug.Log("Reduce");
                _context.Self.inventory.DecHoldingItem(1);
                yield return new WaitForSeconds(0.1f);
                Debug.Log("Clear Dummy");
                _context.Self.inventory.SetItem(_context.Self.inventory.DUMMY_SLOT_IDX, ItemStack.Empty.Clone());
                yield return new WaitForSeconds(0.1f);
                Debug.Log("reset weapon");
                _context.Self.inventory.SetHoldingItemIdx(0);
                _context.Self.inventory.OnUpdate();
                yield return new WaitForSeconds(0.1f);
                Debug.Log("Stopping");
                this.Stop(_context);
            }
        }

        public override void Start(Context _context)
        {
            Debug.Log($"Moving to Heal! {_context.Self.ToString()} with my {items}");
            isRunning = false;
            base.Start(_context);
            SCoreUtils.SetCrouching(_context);

            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive != null)
                SCoreUtils.FindPath(_context, entityAlive.position, true);


        }

    }
}