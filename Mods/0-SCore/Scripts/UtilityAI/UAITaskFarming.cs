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

        // --- Shared claim registry ---
        // Tracks which farm plot positions are currently claimed by a farmer NPC.
        // Static so all task instances share the same set; safe because UAI runs on Unity's main thread.
        private static readonly HashSet<Vector3i> _claimedPlots = new HashSet<Vector3i>();

        // --- Task State Variables ---
        private Vector3 _targetFarmPlotPosition; // The position of the farm plot block being targeted.
        private Vector3i _claimedPosition;        // The position currently held in _claimedPlots by this instance.
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
            if (Parameters.TryGetValue("cooldownBuff", out var cooldownBuff) && !string.IsNullOrEmpty(cooldownBuff))
                _cooldownBuff = cooldownBuff;
         
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

            // Claim the plot immediately so other farmers skip it while this NPC is en route.
            _claimedPosition = _targetFarmPlotData.GetBlockPos();
            _claimedPlots.Add(_claimedPosition);

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
                BlockUtilitiesSDX.removeParticlesCenteredServer(new Vector3i(_targetFarmPlotPosition)); // Clean up particles if any
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
            if (GameManager.IsDedicatedServer)
                BlockUtilitiesSDX.addParticlesCenteredServer(string.Empty,
                    new Vector3i(_targetFarmPlotPosition)); // Add visual effect (DS: server→clients)
            else
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
            // Release the plot claim so other farmers can target it on their next cycle.
            if (_claimedPosition != Vector3i.zero)
            {
                _claimedPlots.Remove(_claimedPosition);
                _claimedPosition = Vector3i.zero;
            }

            // Always try to remove particles associated with this task's target location.
            BlockUtilitiesSDX.removeParticlesCenteredServer(new Vector3i(_targetFarmPlotPosition));
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
            ItemStack[] items = null;
            if (_context.Self is EntityTrader && HarvestManager.Has(_context.Self.entityId))
                items = HarvestManager.GetOrCreate(_context.Self.entityId).items;
            else if (_context.Self.lootContainer != null)
                items = _context.Self.lootContainer.items;

            if (items == null) return false;

            foreach (var itemStack in items)
            {
                if (itemStack.IsEmpty()) continue;
                if (MatchesSeedPattern(itemStack.itemValue.ItemClass.GetItemName()) && itemStack.count > 0)
                    return true;
            }
            return false;
        }

        private bool MatchesSeedPattern(string itemName)
        {
            if (_seedPatternEndsWith == null)
                return itemName.Equals(_seedPatternStartsWith, System.StringComparison.OrdinalIgnoreCase);
            return itemName.StartsWith(_seedPatternStartsWith, System.StringComparison.OrdinalIgnoreCase) &&
                   itemName.EndsWith(_seedPatternEndsWith, System.StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Finds the most suitable farm plot based on entity position and whether it has seeds.
        /// Implements a prioritized search strategy.
        /// </summary>
        /// <returns>FarmPlotData of the chosen plot, or null if none found.</returns>
        /// <summary>
        /// Returns true if the candidate plot is already claimed by another farmer NPC.
        /// </summary>
        private static bool IsClaimed(FarmPlotData plot)
        {
            return plot != null && _claimedPlots.Contains(plot.GetBlockPos());
        }

        private FarmPlotData FindTargetFarmPlot(Vector3i position, bool hasSeed)
        {
            FarmPlotData foundPlot = null;

            // Priority 1: If holding seeds, look for nearby empty plots first.
            if (hasSeed)
            {
                foundPlot = FarmPlotManager.Instance.GetFarmPlotsNearby(position, true);
                if (IsClaimed(foundPlot)) foundPlot = null;

                if (foundPlot == null)
                {
                    var farmDatas = FarmPlotManager.Instance.GetCloseFarmPlots(position);
                    foreach (var candidate in farmDatas)
                    {
                        if (!IsClaimed(candidate)) { foundPlot = candidate; break; }
                    }
                }
            }

            // Priority 2: Look for plots needing maintenance (e.g., harvestable) nearby.
            if (foundPlot == null)
            {
                foundPlot = FarmPlotManager.Instance.GetClosesUnmaintainedWithPlants(position);
                if (IsClaimed(foundPlot)) foundPlot = null;
            }

            if (foundPlot == null)
            {
                foundPlot = FarmPlotManager.Instance.GetClosesUnmaintained(position, _searchRange);
                if (IsClaimed(foundPlot)) foundPlot = null;
            }

            if (foundPlot == null)
            {
                foundPlot = FarmPlotManager.Instance.GetFarmPlotsNearbyWithPlants(position);
                if (IsClaimed(foundPlot)) foundPlot = null;
            }

            // Priority 3: Look for wilted plots (if applicable).
            if (foundPlot == null)
            {
                foundPlot = FarmPlotManager.Instance.GetClosesFarmPlotsWilted(position);
                if (IsClaimed(foundPlot)) foundPlot = null;
            }

            return foundPlot;
        }


        /// <summary>
        /// Handles the logic for managing the farm plot after the work buff expires.
        /// This typically involves harvesting items and potentially replanting.
        /// </summary>
        private void HandleHarvestingAndCleanup(Context _context)
        {
            BlockUtilitiesSDX.removeParticlesCenteredServer(new Vector3i(_targetFarmPlotPosition));

            if (_targetFarmPlotData == null)
            {
                Debug.LogWarning($"UAITaskFarming: _targetFarmPlotData was null during HandleHarvesting for entity {_context.Self.entityId}.");
                _hasWorkBuffApplied = false;
                return;
            }

            var harvestedItemStacks = _targetFarmPlotData.Manage(_context.Self);

            AdvLogging.DisplayLog("AdvancedTroubleshootingFeatures", "UAITaskFarming",
                $"Entity {_context.Self.entityId} harvested {harvestedItemStacks?.Count ?? 0} item type(s) from {_targetFarmPlotPosition}.");

            // EntityAliveSDXV4 extends EntityTrader. TileEntityLootContainer.AddItem() always
            // calls SetModified() internally, which for TileEntityTrader triggers a network
            // packet that serialises both items[] and TraderData in one stream. Those two stores
            // can diverge, causing EndOfStreamException on the client. Use HarvestManager — a
            // plain TileEntityLootContainer per entity, never attached to the world tile-entity
            // system — to store crops safely. The player opens it via the OpenInventory dialog cmd.
            bool isTraderEntity = _context.Self is EntityTrader;

            var lootContainer = _context.Self.lootContainer;
            if (!isTraderEntity && lootContainer == null)
                Debug.LogWarning($"UAITaskFarming: lootContainer is null on entity {_context.Self.entityId}. Harvest items will be dropped.");

            if (harvestedItemStacks != null && harvestedItemStacks.Count > 0)
            {
                var random = GameManager.Instance.World.GetGameRandom();
                bool addedItems = false;

                foreach (var item in harvestedItemStacks)
                {
                    // Skip items that fail the probability roll.
                    if (item.prob < 1f && random.RandomFloat > item.prob)
                    {
                        AdvLogging.DisplayLog("AdvancedTroubleshootingFeatures", "UAITaskFarming",
                            $"  Skipping '{item.name}' — prob roll failed (prob={item.prob:F2}).");
                        continue;
                    }

                    // Use a random count in [min, max]. Clamp min to 1 so a zero minCount
                    // never produces an empty ItemStack that AddItem silently rejects.
                    var count = random.RandomRange(
                        Utils.FastMax(1, item.minCount),
                        Utils.FastMax(1, item.maxCount) + 1);

                    var itemValue = ItemClass.GetItem(item.name, false);
                    if (itemValue == null || itemValue.Equals(ItemValue.None))
                    {
                        Debug.LogWarning($"UAITaskFarming: Unknown item '{item.name}' in harvest drops for entity {_context.Self.entityId}. Skipping.");
                        continue;
                    }

                    var itemStack = new ItemStack(itemValue, count);

                    AdvLogging.DisplayLog("AdvancedTroubleshootingFeatures", "UAITaskFarming",
                        $"  Attempting to add {count}x '{item.name}' to entity {_context.Self.entityId} inventory.");

                    if (isTraderEntity)
                    {
                        // Store in HarvestManager — never touches the trader's lootContainer.
                        if (!HarvestManager.AddItem(_context.Self.entityId, itemStack))
                        {
                            AdvLogging.DisplayLog("AdvancedTroubleshootingFeatures", "UAITaskFarming",
                                $"  Harvest container full — dropping {count}x '{item.name}' on ground.");
                            _context.Self.world.GetGameManager().ItemDropServer(
                                itemStack, _context.Self.position, Vector3.zero,
                                _context.Self.entityId, 60f, false);
                        }
                        else
                        {
                            AdvLogging.DisplayLog("AdvancedTroubleshootingFeatures", "UAITaskFarming",
                                $"  Added {count}x '{item.name}' to harvest container.");
                            addedItems = true;
                        }
                    }
                    else if (lootContainer != null && lootContainer.AddItem(itemStack))
                    {
                        AdvLogging.DisplayLog("AdvancedTroubleshootingFeatures", "UAITaskFarming",
                            $"  Added {count}x '{item.name}' to inventory.");
                        addedItems = true;
                    }
                    else
                    {
                        AdvLogging.DisplayLog("AdvancedTroubleshootingFeatures", "UAITaskFarming",
                            $"  Inventory full or null — dropping {count}x '{item.name}' on ground.");
                        _context.Self.world.GetGameManager().ItemDropServer(
                            itemStack, _context.Self.position, Vector3.zero,
                            _context.Self.entityId, 60f, false);
                    }
                }

                if (addedItems)
                {
                    _context.Self.PlayOneShot("item_plant_pickup", false);
                    if (isTraderEntity)
                    {
                        HarvestManager.Save();
                    }
                    else if (lootContainer != null)
                    {
                        lootContainer.items = StackSortUtil.CombineAndSortStacks(lootContainer.items, 0);
                        lootContainer.SetModified();
                    }
                }
            }

            _targetFarmPlotData = null;
            _hasWorkBuffApplied = false;
        }
    }
}