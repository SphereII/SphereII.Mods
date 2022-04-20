using GamePath;
using System.Globalization;
using UnityEngine;

namespace UAI
{
    public class UAITaskFarming : UAITaskBase
    {
        private Vector3 _vector;
        private string _buff;
        private int range = 50;
        private bool hadBuff = false;
        private FarmPlotData _farmData;
        private float timeOut = 100f;
        protected override void initializeParameters()
        {
            base.initializeParameters();
            if (Parameters.ContainsKey("buff")) _buff = Parameters["buff"];
        }
        public override void Stop(Context _context)
        {
            // If we have the activity buff, just wait until it wears off
            if (_context.Self.Buffs.HasBuff(_buff)) return;

   
            BlockUtilitiesSDX.removeParticles(new Vector3i(_vector));
            base.Stop(_context);
        }
        public override void Update(Context _context)
        {
            timeOut--;

            // If we have the activity buff, just wait until it wears off
            if (_context.Self.Buffs.HasBuff(_buff)) return;

            // If we had it, and it's gone, then we are done with this location.
            if (hadBuff)
            {
                if (_farmData != null)
                {
                    var seedName = string.Empty;
                    foreach( var stack in _context.Self.lootContainer.items)
                    {
                        if (stack.IsEmpty()) continue;
                        var itemname = stack.itemValue.ItemClass.GetItemName();
                        if ( itemname.StartsWith("planted") && itemname.EndsWith("1"))
                        {
                            seedName = itemname;
                            stack.count--;
                            break;
                        }

                    }

                    var items = _farmData.Manage(seedName);
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            Log.Out($"Collecting: {item.name}");
                            int num = Utils.FastMax(0, item.minCount);
                            ItemStack itemStack = new ItemStack(ItemClass.GetItem(item.name, false), num);
                            ItemStack @is = itemStack.Clone();
                            var lootContainer = _context.Self.lootContainer;
                            if (lootContainer != null)
                            {
                                if (_context.Self.lootContainer.AddItem(itemStack))
                                    _context.Self.PlayOneShot("item_plant_pickup", false);
                            }

                            // Sort and reduce duplicates.
                            ItemStack[] slots = StackSortUtil.CombineAndSortStacks(lootContainer.items, 0);
                            lootContainer.items = slots;
                        }
                    }
                }
                Stop(_context);
                return;
            }

            // Use a timeout in case the NPC gets stuck somewhere trying to get to a position in an awkard corner
            if (timeOut < 0)
            {
                _context.Self.Buffs.RemoveBuff(_buff);
                Stop(_context);
                return;

            }
            var distance = Vector3.Distance(_vector, _context.Self.position);
            if (distance > 1.2)
            {
                _context.Self.moveHelper.SetMoveTo(_vector, false);
                return;
            }

            EntityUtilities.Stop(_context.Self.entityId);
            _context.Self.Buffs.AddBuff(_buff);
            _context.Self.SetLookPosition(_vector);
            _context.Self.RotateTo(_vector.x, _vector.y, _vector.z, 30f, 30f);

            hadBuff = true;

        }


        public override void Start(Context _context)
        {
            hadBuff = false;
            timeOut = 100f;

            var position = new Vector3i(_context.Self.position);
            _farmData = FarmPlotManager.Instance.GetFarmPlotsNearby(position);
            if (_farmData == null)
                _farmData = FarmPlotManager.Instance.GetClosesUnmaintained(position, range);
         
            _vector = _farmData.GetBlockPos();
            SCoreUtils.FindPath(_context, _vector, false);
            _context.ActionData.Started = true;
            _context.ActionData.Executing = true;
            return;


        }
    }
}