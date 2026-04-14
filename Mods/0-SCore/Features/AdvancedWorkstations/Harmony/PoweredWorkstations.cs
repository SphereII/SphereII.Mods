using System.Collections.Generic;
using HarmonyLib;
using Platform;

namespace Harmony.TileEntities {
    /**
     * SCorePoweredWorkstations
     *
     * This class includes a Harmony patch allows a tile entity to require power, and recieve it when a power source is within a radious.
     *
     * Usage XML:
     * <property name="RequirePower" value="true" />
     */
    public class PoweredWorkstations {
        private static readonly string AdvFeatureClass = "AdvancedWorkstationFeatures";
        private static readonly string Feature = "EnablePoweredWorkstations";

        // Tracks the last game tick at which power was successfully applied to each workstation.
        // Keyed by world position. Used to compute elapsed offline ticks so that workstations
        // receive enough burn time to cover the period the chunk was unloaded (catch-up on reload).
        // This is in-memory only; on the first tick after a game restart the elapsed value will be
        // 0 and the workstation receives the normal 15f buffer, which is acceptable.
        private static readonly Dictionary<Vector3i, ulong> _lastPowerTick = new Dictionary<Vector3i, ulong>();

        public class PowerWorkstationHelper {
            public static bool CheckWorkstationForPower(TileEntity myTileEntity) {
                AdvLogging.DisplayLog(AdvFeatureClass, "Workstation requires power. Checking near by tile entities.");

                // Check the nearby chunks for Power source tile entities, and check if they are on. 
                var blockPosition = myTileEntity.ToWorldPos();
                var world = GameManager.Instance.World;
                var num = World.toChunkXZ(blockPosition.x);
                var num2 = World.toChunkXZ(blockPosition.z);
                for (var i = -1; i < 2; i++)
                {
                    for (var j = -1; j < 2; j++)
                    {
                        var chunk = (Chunk)world.GetChunkSync(num + j, num2 + i);
                        if (chunk == null) continue;
                        var tileEntities = chunk.GetTileEntities();
                        foreach (var t in tileEntities.list)
                        {
                            AdvLogging.DisplayLog(AdvFeatureClass, "\tDetected Tile Entity: " + t);
                            var tileEntity = t as TileEntityPowerSource;
                            if (tileEntity == null) continue;
                            if (tileEntity.IsActive(world))
                                AdvLogging.DisplayLog(AdvFeatureClass, "\tEntity is Active " + t);
                            if (tileEntity.IsOn)
                            {
                                AdvLogging.DisplayLog(AdvFeatureClass, "\tTile Entity is On: " + t);
                                var distanceSq = (blockPosition.ToVector3() - tileEntity.ToWorldPos().ToVector3())
                                    .sqrMagnitude;
                                if (!(distanceSq <= 20 * 20)) continue;
                                AdvLogging.DisplayLog(AdvFeatureClass,
                                    myTileEntity + " Found Power Source as Fuel with " + tileEntity);
                                //myTileEntity.HasPowerSDX = true;

                                return true;
                            }

                            AdvLogging.DisplayLog(AdvFeatureClass, "\tTile Entity is off: " + t);
                        }
                    }
                }

                return false;
            }
        }

     

        [HarmonyPatch(typeof(TileEntityWorkstation))]
        [HarmonyPatch("UpdateTick")]
        public class TileEntityWorkstationHandleFuel {
            
            private static bool IsEmpty(ItemStack[] items) {
                if (items == null) return true;
                foreach (var t in items)
                {
                    if (!t.IsEmpty())
                    {
                        return false;
                    }
                }

                return true;
            }

            private static bool RequirePower(ITileEntity _tileEntity) {
                var blNeedsPower = false;
                var block2 = _tileEntity.blockValue.Block;
                if (block2.Properties.Values.ContainsKey("RequirePower"))
                    blNeedsPower = StringParsers.ParseBool(block2.Properties.Values["RequirePower"]);
                return blNeedsPower;
            }
            public static bool Prefix(TileEntityWorkstation __instance, ref float ___currentBurnTimeLeft, ItemStack[] ___fuel) {
                AdvLogging.DisplayLog(AdvFeatureClass, "UpdateTick()");
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                if (!RequirePower(__instance)) return true;

                // If there's no fuel, then check for wireless power.
                if (IsEmpty(___fuel))
                {
                    var pos = __instance.ToWorldPos();
                    if (PowerWorkstationHelper.CheckWorkstationForPower(__instance))
                    {
                        var now = GameTimer.Instance.ticks;

                        // Compute how many ticks elapsed since we last applied power to this workstation.
                        // When the chunk was unloaded, UpdateTick did not fire, so the gap between now
                        // and lastTick equals the offline period. Setting currentBurnTimeLeft to at least
                        // (elapsed + 15) gives the vanilla catch-up loop enough burn time to process all
                        // missed ticks without stalling, mirroring how TileEntityForge handles offline time.
                        var elapsed = _lastPowerTick.TryGetValue(pos, out var lastTick)
                            ? (float)(now - lastTick)
                            : 0f;
                        _lastPowerTick[pos] = now;

                        var needed = elapsed + 15f;
                        if (___currentBurnTimeLeft < needed)
                        {
                            AdvLogging.DisplayLog(AdvFeatureClass, $"\tApplying catch-up burn time: elapsed={elapsed} needed={needed} current={___currentBurnTimeLeft}");
                            ___currentBurnTimeLeft = needed;
                        }
                    }
                    else
                    {
                        // No power available — cut burn time and stop tracking this workstation.
                        ___currentBurnTimeLeft = 0f;
                        _lastPowerTick.Remove(pos);
                    }
                }

                AdvLogging.DisplayLog(AdvFeatureClass, "Current Burn Time: " + ___currentBurnTimeLeft);
                return true;
            }
        }
    }
}