using GamePath;
using System.Collections.Generic; // Added for List<>
using UnityEngine;

namespace UAI
{
    /// <summary>
    /// Represents a Utility AI task for an entity to find and manage farm plots.
    /// The entity will search for suitable farm plots based on whether it has seeds and the state of nearby plots (empty, needs maintenance, wilted).
    /// It moves to the plot, applies a buff (simulating work), and then manages the plot (harvesting/planting) after the buff expires.
    /// </summary>
    public class UAITaskFarming : UAITaskBase
    {
        // --- Constants ---
        private const float DefaultTimeout = 100f; // Time in seconds before the task gives up if stuck.
        private const float ApproachDistance = 1.1f; // How close the entity needs to be to the plot to start working.

        private const float
            SquaredApproachDistance = ApproachDistance * ApproachDistance; // Use squared distance for performance.

        private const float RotationSpeed = 8f; // How fast the entity turns towards the plot.
        private const int DefaultSearchRange = 50; // Default search radius for farm plots if not found nearby.
        private const string DefaultSeedPattern = "planted*1"; // Default pattern to identify seeds in inventory.

        // --- Configurable Parameters ---
        private string _workBuff = string.Empty; // The buff applied while the entity is "working" on the plot.

        private string
            _seedIdentifier =
                DefaultSeedPattern; // Pattern used to identify seeds in inventory (e.g., "planted*1", "seed*").

        private int _searchRange = DefaultSearchRange; // Search radius for farm plots.

        // --- Task State Variables ---
        private Vector3 _targetFarmPlotPosition; // The position of the farm plot block being targeted.
        private FarmPlotData _targetFarmPlotData; // Data associated with the target farm plot.
        private bool _hasWorkBuffApplied = false; // Flag indicating if the work buff was applied in this cycle.
        private float _currentTimeout; // Countdown timer for the task timeout.
        private bool _hasSeedInInventory = false; // Cached check if the entity has seeds.
        private string _seedPatternStartsWith = string.Empty; // Cache for seed pattern matching.
        private string _seedPatternEndsWith = string.Empty; // Cache for seed pattern matching.
        private string _cooldownBuff = "buffFarmerCoolDown";


        /// <summary>
        /// Initializes parameters from the AI configuration.
        /// Expected parameters: "buff" (required), "seed" (optional), "range" (optional).
        /// </summary>
        public override void initializeParameters()
        {
            base.initializeParameters();
            if (Parameters.TryGetValue("cooldownBuff", out _cooldownBuff)){ }
         
            if (Parameters.TryGetValue("buff", out _workBuff)) { }

            if (Parameters.TryGetValue("seed", out _seedIdentifier))
            {
                if (_seedIdentifier.Contains("*"))
                {
                    var parts = _seedIdentifier.Split('*');
                    _seedPatternStartsWith = parts.Length > 0 ? parts[0] : string.Empty;
                    _seedPatternEndsWith = parts.Length > 1 ? parts[1] : string.Empty;
                }
                else
                {
                    _seedPatternStartsWith = _seedIdentifier;
                    _seedPatternEndsWith = null; // Indicate exact match needed
                }
            }

            if (Parameters.ContainsKey("range") && int.TryParse(Parameters["range"], out _searchRange))
            {
             
            }
        
        }

        /// <summary>
        /// Called when the task starts. Finds a suitable farm plot to target.
        /// </summary>
        public override void Start(Context _context)
        {
            _hasWorkBuffApplied = false;
            _currentTimeout = DefaultTimeout;
            _targetFarmPlotData = null;
            //plantData = null; // Note: plantData was declared but never used in the original code. Removed.

            Vector3i currentPosition = new Vector3i(_context.Self.position);
            _hasSeedInInventory = CheckHasSeed(_context);

            // Attempt to find the best farm plot to work on.
            _targetFarmPlotData = FindTargetFarmPlot(currentPosition, _hasSeedInInventory);

            if (_targetFarmPlotData == null)
            {
                // Could not find any suitable plot. Maybe reset nearby plots if needed?
                FarmPlotManager.Instance.ResetPlantsInRange(currentPosition); // Consider if this reset is always desired on failure.
                _context.Self.PlayGiveUpSound();
                _context.Self.Buffs.AddBuff(_cooldownBuff);
                Stop(_context); // Stop the task as there's nothing to do.
                return;
            }

            // Found a plot, set target position and pathfind.
            _targetFarmPlotPosition = _targetFarmPlotData.GetBlockPos();
            SCoreUtils.FindPath(_context, _targetFarmPlotPosition,
                false); // Assuming SCoreUtils handles path finding initiation.

            _context.ActionData.Started = true;
            _context.ActionData.Executing = true;
        }


        /// <summary>
        /// Called periodically while the task is active. Handles movement, applying the work buff,
        /// and managing the farm plot after the work is done.
        /// </summary>
        public override void Update(Context _context)
        {
            // Always look towards the target plot
            _context.Self.SetLookPosition(_targetFarmPlotPosition);
            _context.Self.RotateTo(_targetFarmPlotPosition.x + 0.5f, _targetFarmPlotPosition.y + 1f,
                _targetFarmPlotPosition.z + 0.5f,
                RotationSpeed, RotationSpeed);

            // Check if the entity is facing the target direction (optional, but can prevent actions before turning)
            if (!_context.Self.IsInFrontOfMe(_targetFarmPlotPosition))
            {
                return; // Wait until turned
            }

            if (_context.Self.moveHelper.BlockedTime > SCoreConstants.BlockedTime)
            {
                Debug.LogWarning(
                    $"UAITaskFarming: Entity {_context.Self.entityId} is blocked while moving to farm plot.");
                _currentTimeout = -1;
            }

            // --- Timeout Check ---
            _currentTimeout -= Time.deltaTime;
            if (_currentTimeout <= 0f)
            {
                // Task timed out (e.g., stuck). Clean up and stop.
                Debug.LogWarning($"UAITaskFarming timed out for entity {_context.Self.entityId}");
                _targetFarmPlotData.Visited = true;
                BlockUtilitiesSDX.removeParticles(new Vector3i(_targetFarmPlotPosition)); // Clean up particles if any
                if (!string.IsNullOrEmpty(_workBuff))
                    _context.Self.Buffs.RemoveBuff(_workBuff); // Ensure buff is removed on timeout
                Stop(_context);
                return;
            }


            bool hasBuff = !string.IsNullOrEmpty(_workBuff) && _context.Self.Buffs.HasBuff(_workBuff);

            // --- State Machine Logic ---

            // State 1: Working (Buff Active)
            if (hasBuff)
            {
                // Entity is currently "working". Wait for the buff to expire.
                _hasWorkBuffApplied = true; // Ensure we know the buff was active
                return;
            }

            // State 2: Finished Working (Buff Just Expired)
            if (_hasWorkBuffApplied) // Buff was active previously, but isn't now.
            {
                // Work is complete. Handle harvesting/planting.
                HandleHarvestingAndCleanup(_context);
                Stop(_context); // Task complete for this plot.
                return;
            }

            // State 3: Moving To Plot or Starting Work (Buff Not Active, Work Not Finished)
            float sqrDist = (_targetFarmPlotPosition - _context.Self.position).sqrMagnitude;
            if (sqrDist >= SquaredApproachDistance)
            {
                // Still too far, continue moving.
                _context.Self.moveHelper.SetMoveTo(_targetFarmPlotPosition, false);
                return;
            }

            // Arrived at the plot. Stop moving and apply the work buff.
            _context.Self.moveHelper.Stop(); // Ensure entity stops moving
            BlockUtilitiesSDX.addParticlesCentered(string.Empty,
                new Vector3i(_targetFarmPlotPosition)); // Add visual effect

            // Apply the work buff to start the "working" phase.
            if (!string.IsNullOrEmpty(_workBuff))
            {
                _context.Self.Buffs.AddBuff(_workBuff);
                _hasWorkBuffApplied = true; // Mark that we are now starting the work cycle
            }
            else
            {
                // If no buff is defined, we might skip straight to harvesting or just stop.
                // Depending on desired behavior. For now, assume buff is required.
                Debug.LogWarning("UAITaskFarming: No work buff specified, cannot simulate work time.");
                HandleHarvestingAndCleanup(_context); // Or maybe just Stop()?
                Stop(_context);
            }
        }


        /// <summary>
        /// Called when the task stops or is interrupted. Cleans up resources like particles.
        /// </summary>
        public override void Stop(Context _context)
        {
            // If the work buff is still active when stopping (e.g., interrupted), wait for it?
            // Original code had this, but it might prevent task switching. Decide based on game design.
            // if (!string.IsNullOrEmpty(_workBuff) && _context.Self.Buffs.HasBuff(_workBuff)) return;

            // Always try to remove particles associated with this task's target location.
            BlockUtilitiesSDX.removeParticles(new Vector3i(_targetFarmPlotPosition));
            _context.Self.setHomeArea(new Vector3i(_targetFarmPlotPosition), 10);

            // Call the base Stop method for standard cleanup.
            base.Stop(_context);
        }


        // --- Helper Methods ---

        /// <summary>
        /// Checks if the entity has items matching the seed identifier in its inventory.
        /// </summary>
        private bool CheckHasSeed(Context _context)
        {
            if (_context.Self.lootContainer == null) return false;

            foreach (var itemStack in _context.Self.lootContainer.items)
            {
                if (itemStack.IsEmpty()) continue;

                var itemName = itemStack.itemValue.ItemClass.GetItemName();

                bool match = false;
                if (_seedPatternEndsWith == null) // Exact match case
                {
                    match = itemName.Equals(_seedPatternStartsWith, System.StringComparison.OrdinalIgnoreCase);
                }
                else // Pattern match case
                {
                    match = itemName.StartsWith(_seedPatternStartsWith, System.StringComparison.OrdinalIgnoreCase) &&
                            itemName.EndsWith(_seedPatternEndsWith, System.StringComparison.OrdinalIgnoreCase);
                }


                if (match && itemStack.count > 0)
                {
                    return true; // Found at least one matching seed item.
                }
            }

            return false; // No matching seeds found.
        }


        /// <summary>
        /// Finds the most suitable farm plot based on entity position and whether it has seeds.
        /// Implements a prioritized search strategy.
        /// </summary>
        /// <returns>FarmPlotData of the chosen plot, or null if none found.</returns>
        private FarmPlotData FindTargetFarmPlot(Vector3i position, bool hasSeed)
        {
            FarmPlotData foundPlot = null;

            // Priority 1: If holding seeds, look for nearby empty plots first.
            if (hasSeed)
            {
                // Assuming GetFarmPlotsNearby checks a small radius for plots needing seeds.
                foundPlot = FarmPlotManager.Instance.GetFarmPlotsNearby(position, true);
                if (foundPlot == null)
                {
                    // If nothing immediately nearby, check slightly further for empty plots.
                    var farmDatas =
                        FarmPlotManager.Instance
                            .GetCloseFarmPlots(position); // Assumes this gets empty/plantable plots nearby
                    if (farmDatas.Count > 0)
                        foundPlot = farmDatas[0]; // Pick the first one found
                }
            }

            // Priority 2: Look for plots needing maintenance (e.g., harvestable) nearby.
            if (foundPlot == null)
            {
                // Try finding the closest plot needing any kind of maintenance (harvest/replant) within the search range.
                foundPlot = FarmPlotManager.Instance
                    .GetClosesUnmaintainedWithPlants(position); // Check plots with plants first?
                if (foundPlot == null)
                    foundPlot = FarmPlotManager.Instance.GetClosesUnmaintained(position,
                        _searchRange); // Then any unmaintained plot
                if ( foundPlot == null)
                    foundPlot = FarmPlotManager.Instance.GetFarmPlotsNearbyWithPlants(position);
            }

            // Priority 3: Look for wilted plots (if applicable).
            if (foundPlot == null)
            {
                // Assuming this specifically finds plots that need tending due to wilting.
                foundPlot = FarmPlotManager.Instance.GetClosesFarmPlotsWilted(position);
            }

            return foundPlot;
        }


        /// <summary>
        /// Handles the logic for managing the farm plot after the work buff expires.
        /// This typically involves harvesting items and potentially replanting.
        /// </summary>
        private void HandleHarvestingAndCleanup(Context _context)
        {
            BlockUtilitiesSDX.removeParticles(new Vector3i(_targetFarmPlotPosition)); // Clean up work particles

            if (_targetFarmPlotData != null)
            {
                // Let the FarmPlotData handle its own state management (harvest, plant, etc.)
                var harvestedItemStacks =
                    _targetFarmPlotData
                        .Manage(_context.Self); // Assuming Manage now returns List<ItemStack> directly for simplicity

                var lootContainer = _context.Self.lootContainer;
                if (harvestedItemStacks != null && lootContainer != null)
                {
                    bool addedItems = false;
                    foreach (var item in harvestedItemStacks)
                    {
                        var num = Utils.FastMax(0, item.minCount);
                        var itemStack = new ItemStack(ItemClass.GetItem(item.name, false), num);
                        if (lootContainer.AddItem(itemStack)) // Try adding directly
                        {
                            addedItems = true;
                        }
                    }

                    if (addedItems)
                    {
                        _context.Self.PlayOneShot("item_plant_pickup",
                            false); // Play sound effect once if items were added.

                        // Consolidate and sort the inventory after adding items.
                        lootContainer.items = StackSortUtil.CombineAndSortStacks(lootContainer.items, 0);
                        lootContainer.SetModified();
                    }
                }
            }
            else
            {
                Debug.LogWarning(
                    $"UAITaskFarming: _targetFarmPlotData was null during HandleHarvesting for entity {_context.Self.entityId}.");
            }

            // Reset state variables related to the current plot interaction
            _targetFarmPlotData = null;
            _hasWorkBuffApplied = false; // Ready for the next potential run
        }
    }
}