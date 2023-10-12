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
        private PlantData plantData;
        private float timeOut = 100f;
        private string seed = "planted*1";
        private bool _hasSeed = false;
        protected override void initializeParameters()
        {
            base.initializeParameters();
            if (Parameters.ContainsKey("buff")) _buff = Parameters["buff"];
            if (Parameters.ContainsKey("seed")) seed = Parameters["seed"];
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
            _context.Self.SetLookPosition(_vector);
            _context.Self.RotateTo(_vector.x, _vector.y, _vector.z, 8f,8f);
            var headPosition = _context.Self.getHeadPosition();
            var dir = _vector - headPosition;
            var forwardVector = _context.Self.GetForwardVector();
            var angleBetween = Utils.GetAngleBetween(dir, forwardVector);
            var num2 = 70 * 0.5f;
            var isInFront = (angleBetween >= -num2 && angleBetween <= num2);
            if (!isInFront)
            {
                return;
            }
            
            // If we have the activity buff, just wait until it wears off
            if (_context.Self.Buffs.HasBuff(_buff)) return;
        //    timeOut--;


            // If we had it, and it's gone, then we are done with this location.
            if (hadBuff)
            {
                // If we are dealing with a farm plot, then we want to manage that a bit. Plant a seed, harvest, etc.
                if (_farmData != null)
                {
                    var items = _farmData.Manage( _context.Self);
                    var lootContainer = _context.Self.lootContainer;
                    if (items != null && lootContainer != null)
                    {
                        foreach (var item in items)
                        {
                            var num = Utils.FastMax(0, item.minCount);
                            var itemStack = new ItemStack(ItemClass.GetItem(item.name, false), num);
                            if (_context.Self.lootContainer.AddItem(itemStack))
                                _context.Self.PlayOneShot("item_plant_pickup", false);

                            // Sort and reduce duplicates.
                            var slots = StackSortUtil.CombineAndSortStacks(lootContainer.items, 0);
                            lootContainer.items = slots;
                        }
                        lootContainer.SetModified();
                    }
                }
                Stop(_context);
                return;
            }
      
            // Use a timeout in case the NPC gets stuck somewhere trying to get to a position in an awkward corner
            if (timeOut < 0)
            {
                _context.Self.Buffs.RemoveBuff(_buff);
                Stop(_context);
                return;
            }

    
            var distance = Vector3.Distance(_vector, _context.Self.position);
            if (distance > 1)
            {
                _context.Self.moveHelper.SetMoveTo(_vector, false);
                return;
            }

            BlockUtilitiesSDX.addParticlesCentered(string.Empty, new Vector3i(_vector));
           // EntityUtilities.Stop(_context.Self.entityId);
            _context.Self.Buffs.AddBuff(_buff);
            hadBuff = true;

        }

    

        // Yes. This is gross. I've done worse.
        public override void Start(Context _context)
        {
            hadBuff = false;
            timeOut = 100f;

            _farmData = null;
            plantData = null;
            var position = new Vector3i(_context.Self.position);
            _hasSeed = HasSeed(_context);

            // If the NPC has any seeds in its inventory, then look for empty farm plots that need tending
            if (_hasSeed)
            {
                _farmData = FarmPlotManager.Instance.GetFarmPlotsNearby(position, true);
                if (_farmData == null)
                {
                    var farmDatas = FarmPlotManager.Instance.GetCloseFarmPlots(position);
                    if (farmDatas.Count > 0)
                        _farmData = farmDatas[0];
                }
            }

            // No farm plots or maintenance that are that close? Cast the net a bit wider
            if (_farmData == null)
            {
                //    _farmData = FarmPlotManager.Instance.GetFarmPlotsNearbyWithPlants(position);
                //if (_farmData == null)
                _farmData = FarmPlotManager.Instance.GetClosesUnmaintainedWithPlants(position);
                if (_farmData == null)
                    _farmData = FarmPlotManager.Instance.GetClosesUnmaintained(position, range);
            }

            // Check for wilted...?
            if (_farmData == null)
            {
                _farmData = FarmPlotManager.Instance.GetClosesFarmPlotsWilted(position);
            }

            if (_farmData == null)
            {
                FarmPlotManager.Instance.ResetPlantsInRange(position);
                hadBuff = true;
                Stop(_context);
                return;
            }

            _vector = _farmData.GetBlockPos();

            SCoreUtils.FindPath(_context, _vector, false);
            _context.ActionData.Started = true;
            _context.ActionData.Executing = true;
        }

        private bool HasSeed(Context _context)
        {
            var startsWith = "";
            var endsWith = "";
            if (seed.Contains("*"))
            {
                startsWith = seed.Split('*')[0];
                endsWith = seed.Split('*')[1];
            }

            if (_context.Self.lootContainer != null)
            {
                foreach (var items in _context.Self.lootContainer.items)
                {
                    if (items.IsEmpty()) continue;
                    var itemName = items.itemValue.ItemClass.GetItemName();
                    if (itemName.StartsWith(startsWith) && itemName.EndsWith(endsWith))
                    {
                        if (items.count > 0)
                            return true;
                    }
                }
            }
            return false;
        }

    }
}