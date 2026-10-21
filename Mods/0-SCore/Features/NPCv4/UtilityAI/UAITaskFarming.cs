using GamePath;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UAI
{
    /// <summary>
    /// Represents a Utility AI task for an entity to find and manage farm plots.
    /// The entity will search for suitable farm plots based on whether it has seeds and the state of nearby plots (empty, needs maintenance, wilted).
    /// It moves to the plot, applies a buff (simulating work), and then manages the plot (harvesting/planting) after the buff expires.
    /// </summary>
    public class UAITaskFarmingV4 : UAITaskBase
    {
        // --- Constants ---
        private const float DefaultTimeout = 100f; // Time in seconds before the task gives up if stuck.
        private const float ApproachDistance = 1.1f; // How close the entity needs to be to the plot to start working.

        private const float
            SquaredApproachDistance = ApproachDistance * ApproachDistance; // Use squared distance for performance.

        private const float RotationSpeed = 8f; // How fast the entity turns towards the plot.
        private const int DefaultSearchRange = 50; // Default search radius for farm plots if not found nearby.
        private const string DefaultSeedPattern = "planted*1"; // Default pattern to identify seeds in inventory.

        // Logging switch: ConfigFeatureBlock / AdvancedTroubleshootingFeatures / UAITaskFarming.
        // The [FARM] lifecycle lines also turn on with
        // setcvar AdvancedTroubleshootingFeatures_UAITaskFarming 1 in the console.
        private const string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private const string FarmingFeature = "UAITaskFarming";

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
        private bool _run = false; // "run" parameter: panic speed between plots instead of a walk.

        // Resolved once in Start(): non-null when this NPC is hired by a player leader.
        // When set, FindTargetFarmPlot restricts candidates to plots inside that player's land claim.
        // Left null when that player has no land claim, so the NPC may work any plot.
        private PersistentPlayerData _leaderPlayerData;


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

            // run="true" moves between plots at panic speed; the default is a walk.
            if (Parameters.TryGetValue("run", out var run))
                _run = StringParsers.ParseBool(run);

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

            // Resolve the land-claim owner once per task invocation.
            //
            // Priority 1 — active hire: the NPC has a live Leader/Owner cvar pointing at a
            //   player who is currently in the world.  Plots are restricted to that player's
            //   land claim.
            //
            // Priority 2 — FarmHere mode: the player issued "FarmHere", which zeroed the
            //   Leader/Owner cvars (removing the NPC from the party / stopping TP) but stored
            //   the player's entity ID in "FarmOwnerEntityId".  The farming task honours that
            //   stored ID for land-claim checks so behaviour is identical to being hired.
            //
            // Priority 3 — un-hired NPC: no restriction, any plot is fair game.
            _leaderPlayerData = null;
            var leader = EntityUtilities.GetLeaderOrOwner(_context.Self.entityId);
            if (leader != null)
            {
                _leaderPlayerData = GameManager.Instance?.GetPersistentPlayerList()
                                               ?.GetPlayerDataFromEntityID(leader.entityId);
            }
            else if (_context.Self.Buffs.HasCustomVar("FarmOwnerEntityId"))
            {
                var ownerId = (int)_context.Self.Buffs.GetCustomVar("FarmOwnerEntityId");
                _leaderPlayerData = GameManager.Instance?.GetPersistentPlayerList()
                                               ?.GetPlayerDataFromEntityID(ownerId);
            }

            // Only enforce the land claim when the owner has one. Without a claim there is
            // nothing to restrict to, and every plot would be refused.
            if (_leaderPlayerData != null && !FarmLandClaimUtils.HasLandClaim(_leaderPlayerData))
                _leaderPlayerData = null;

            Vector3i currentPosition = new Vector3i(_context.Self.position);
            _hasSeedInInventory = CheckHasSeed(_context);

            // Attempt to find the best farm plot to work on.
            _targetFarmPlotData = FindTargetFarmPlot(currentPosition, _hasSeedInInventory);

            if (_targetFarmPlotData == null)
            {
                FarmLog(_context, $"Start: no plot found (hasSeed={_hasSeedInInventory}, landClaimFilter={_leaderPlayerData != null}, range={_searchRange}); giving up, adding {_cooldownBuff}");
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
            FarmLog(_context, $"Start: target plot {_targetFarmPlotPosition} dist={Vector3.Distance(_context.Self.position, _targetFarmPlotPosition):F1} empty={_targetFarmPlotData.IsEmpty()} water={_targetFarmPlotData.HasWater()} hasSeed={_hasSeedInInventory} landClaimFilter={_leaderPlayerData != null}");
            PathingUtils.FindPath(_context, _targetFarmPlotPosition, _run); // walk unless run="true"

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
               Log.Out(
                    $"UAITaskFarmingV4: Entity {_context.Self.entityId} is blocked while moving to farm plot.");
                _currentTimeout = -1;
            }

            // --- Timeout Check ---
            _currentTimeout -= Time.deltaTime;
            if (_currentTimeout <= 0f)
            {
                FarmLog(_context, $"Update: timed out or blocked going to {_targetFarmPlotPosition} (dist={Vector3.Distance(_context.Self.position, _targetFarmPlotPosition):F1}, blockedTime={_context.Self.moveHelper.BlockedTime:F2}); marking plot visited");
                // Task timed out (e.g., stuck). Clean up and stop.
               Log.Out($"UAITaskFarmingV4 timed out for entity {_context.Self.entityId}");
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
                FarmLog(_context, $"Update: work finished at {_targetFarmPlotPosition}, managing plot");
                HandleHarvestingAndCleanup(_context);
                Stop(_context); // Task complete for this plot.
                return;
            }

            // State 3: Moving To Plot or Starting Work (Buff Not Active, Work Not Finished)
            float sqrDist = (_targetFarmPlotPosition - _context.Self.position).sqrMagnitude;
            if (sqrDist >= SquaredApproachDistance)
            {
                // Still too far, continue moving.
                // Pass the speed: the two-argument overload uses the aggro speed, which is a run.
                var speed = _run ? _context.Self.GetMoveSpeedPanic() : _context.Self.GetMoveSpeed();
                _context.Self.moveHelper.SetMoveTo(_targetFarmPlotPosition, false, speed);
                return;
            }

            // Arrived at the plot. Stop moving and apply the work buff.
            FarmLog(_context, $"Update: arrived at {_targetFarmPlotPosition}, starting work buff '{_workBuff}'");
            _context.Self.moveHelper.Stop(); // Ensure entity stops moving

            // StopMove skips resetting the walk input while the entity is jumping, and a step up
            // onto the plot counts as a jump. Left alone, the last walk direction keeps driving the
            // body for the whole work buff (an SDCS NPC ran 16 m off the plot this way), so clear
            // it here. The NPC is already within ApproachDistance, so losing the rest of the step
            // costs nothing.
            _context.Self.SetMoveForward(0f);
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
               Log.Out("UAITaskFarmingV4: No work buff specified, cannot simulate work time.");
                HandleHarvestingAndCleanup(_context); // Or maybe just Stop()?
                Stop(_context);
            }
        }


        /// <summary>
        /// Called when the task stops or is interrupted. Cleans up resources like particles.
        /// </summary>
        public override void Stop(Context _context)
        {
            // A non-null target here means the task was cut off before the plot was managed
            // (another action won, or a timeout); null means it finished or never started.
            FarmLog(_context, $"Stop: {(_targetFarmPlotData != null ? "interrupted" : "done")} at pos={_context.Self.position} plot={_targetFarmPlotPosition}; home area set to plot, radius 10");

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

        // Task lifecycle line. Checks the console cvar as well as the XML flag, so it can be
        // switched on mid-game; the older harvest detail lines follow the XML flag only.
        private static void FarmLog(Context _context, string message)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, FarmingFeature, true)) return;
            Log.Out($"[FARM] {_context.Self.EntityName}#{_context.Self.entityId} {message}");
        }

        /// <summary>
        /// Checks if the entity has items matching the seed identifier in its inventory.
        /// </summary>
        private bool CheckHasSeed(Context _context)
        {
            ItemStack[] items = null;
            if (HarvestManager.Has(_context.Self.entityId))
                items = HarvestManager.GetOrCreate(_context.Self.entityId).items;
            else
                items = _context.Self.bag?.ItemGrid?.items;

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
        /// Returns true if the candidate plot is already claimed by another farmer NPC.
        /// </summary>
        private static bool IsClaimed(FarmPlotData plot)
        {
            return plot != null && _claimedPlots.Contains(plot.GetBlockPos());
        }

        /// <summary>
        /// Finds the most suitable farm plot based on entity position and whether it has seeds.
        /// When the NPC is hired (has a player leader), only plots inside that player's land
        /// claim are considered.  Un-hired NPCs use any available plot as before.
        /// </summary>
        /// <returns>FarmPlotData of the chosen plot, or null if none found.</returns>
        private FarmPlotData FindTargetFarmPlot(Vector3i position, bool hasSeed)
        {
            FarmPlotData foundPlot = null;

            // Priority 1: If holding seeds, look for nearby empty plots first.
            if (hasSeed)
            {
                foundPlot = FarmPlotManager.Instance.GetFarmPlotsNearby(position, true);
                if (IsClaimed(foundPlot) || !IsAllowed(foundPlot)) foundPlot = null;

                if (foundPlot == null)
                {
                    foreach (var candidate in FarmPlotManager.Instance.GetCloseFarmPlots(position))
                    {
                        if (!IsClaimed(candidate) && IsAllowed(candidate)) { foundPlot = candidate; break; }
                    }
                }
            }

            // Priority 2: Look for plots needing maintenance (e.g., harvestable) nearby.
            if (foundPlot == null)
            {
                foundPlot = FarmPlotManager.Instance.GetClosesUnmaintainedWithPlants(position);
                if (IsClaimed(foundPlot) || !IsAllowed(foundPlot)) foundPlot = null;
            }

            if (foundPlot == null)
            {
                foundPlot = FarmPlotManager.Instance.GetClosesUnmaintained(position, _searchRange);
                if (IsClaimed(foundPlot) || !IsAllowed(foundPlot)) foundPlot = null;
            }

            if (foundPlot == null)
            {
                foundPlot = FarmPlotManager.Instance.GetFarmPlotsNearbyWithPlants(position);
                if (IsClaimed(foundPlot) || !IsAllowed(foundPlot)) foundPlot = null;
            }

            // Priority 3: Look for wilted plots (if applicable).
            if (foundPlot == null)
            {
                foundPlot = FarmPlotManager.Instance.GetClosesFarmPlotsWilted(position);
                if (IsClaimed(foundPlot) || !IsAllowed(foundPlot)) foundPlot = null;
            }

            return foundPlot;
        }

        /// <summary>
        /// Returns true if this NPC is allowed to work the given plot.
        /// Un-hired NPCs (<see cref="_leaderPlayerData"/> is null) may work any plot.
        /// Hired NPCs are restricted to plots inside their leader's land claim, when the leader
        /// has one (Start clears <see cref="_leaderPlayerData"/> otherwise).
        /// </summary>
        private bool IsAllowed(FarmPlotData plot)
        {
            if (plot == null) return false;
            if (_leaderPlayerData == null) return true;  // not hired — no restriction
            return FarmLandClaimUtils.IsPlotInLandClaim(plot.GetBlockPos(), _leaderPlayerData);
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
               Log.Out($"UAITaskFarmingV4: _targetFarmPlotData was null during HandleHarvesting for entity {_context.Self.entityId}.");
                _hasWorkBuffApplied = false;
                return;
            }

            var harvestedItemStacks = _targetFarmPlotData.Manage(_context.Self);

            AdvLogging.DisplayLog(AdvFeatureClass, FarmingFeature,
                $"Entity {_context.Self.entityId} harvested {harvestedItemStacks?.Count ?? 0} item type(s) from {_targetFarmPlotPosition}.");

            // EntityAliveSDXV4 extends EntityTrader. TileEntityComposite.AddItem() always
            // calls SetModified() internally, which for TileEntityTrader triggers a network
            // packet that serialises both items[] and TraderData in one stream. Those two stores
            // can diverge, causing EndOfStreamException on the client. Use HarvestManager — a
            // plain TileEntityComposite per entity, never attached to the world tile-entity
            // system — to store crops safely. The player opens it via the OpenInventory dialog cmd.
            bool isTraderEntity = _context.Self is EntityTrader;

            var lootContainer = isTraderEntity
                ? (SCoreLootContainer)null
                : (_context.Self as EntityAliveSDX)?.lootContainer ?? (_context.Self as EntityAliveSDXV4)?.lootContainer;

            if (harvestedItemStacks != null && harvestedItemStacks.Count > 0)
            {
                var random = GameManager.Instance.World.GetGameRandom();
                bool addedItems = false;

                foreach (var item in harvestedItemStacks)
                {
                    // Skip items that fail the probability roll.
                    if (item.prob < 1f && random.RandomFloat > item.prob)
                    {
                        AdvLogging.DisplayLog(AdvFeatureClass, FarmingFeature,
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
                        Log.Out($"UAITaskFarmingV4: Unknown item '{item.name}' in harvest drops for entity {_context.Self.entityId}. Skipping.");
                        continue;
                    }

                    var itemStack = new ItemStack(itemValue, count);

                    AdvLogging.DisplayLog(AdvFeatureClass, FarmingFeature,
                        $"  Attempting to add {count}x '{item.name}' to entity {_context.Self.entityId} inventory.");

                    if (isTraderEntity)
                    {
                        // Store in HarvestManager — never touches the trader's lootContainer.
                        if (!HarvestManager.AddItem(_context.Self.entityId, itemStack))
                        {
                            AdvLogging.DisplayLog(AdvFeatureClass, FarmingFeature,
                                $"  Harvest container full — dropping {count}x '{item.name}' on ground.");
                            _context.Self.world.GetGameManager().ItemDropServer(
                                itemStack, _context.Self.position, Vector3.zero);
                        }
                        else
                        {
                            AdvLogging.DisplayLog(AdvFeatureClass, FarmingFeature,
                                $"  Added {count}x '{item.name}' to harvest container.");
                            addedItems = true;
                        }
                    }
                    else if (lootContainer != null && lootContainer.AddItem(itemStack))
                    {
                        AdvLogging.DisplayLog(AdvFeatureClass, FarmingFeature,
                            $"  Added {count}x '{item.name}' to inventory.");
                        addedItems = true;
                    }
                    else
                    {
                        AdvLogging.DisplayLog(AdvFeatureClass, FarmingFeature,
                            $"  Inventory full or null — dropping {count}x '{item.name}' on ground.");
                        _context.Self.world.GetGameManager().ItemDropServer(
                            itemStack, _context.Self.position, Vector3.zero);
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
                        lootContainer.ItemGrid.SetItems(StackSortUtil.CombineAndSortStacks(lootContainer.items, 0), false);
                        lootContainer.SetModified();
                    }
                }
            }

            _targetFarmPlotData = null;
            _hasWorkBuffApplied = false;
        }
    }
}