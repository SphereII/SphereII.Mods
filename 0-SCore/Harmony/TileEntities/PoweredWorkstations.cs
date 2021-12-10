using HarmonyLib;

namespace Harmony.TileEntities
{
    /**
     * SCorePoweredWorkstations
     * 
     * This class includes a Harmony patch allows a tile entity to require power, and recieve it when a power source is within a radious.
     * 
     * Usage XML:
     * <property name="RequirePower" value="true" />
     */
    public class PoweredWorkstations
    {
        private static readonly string AdvFeatureClass = "AdvancedWorkstationFeatures";
        private static readonly string Feature = "EnablePoweredWorkstations";

        public class PowerWorkstationHelper
        {
            public static bool CheckWorkstationForPower(TileEntity myTileEntity)
            {
                AdvLogging.DisplayLog(AdvFeatureClass, "Does Workstation have access to Fuel?");
                var blNeedsPower = false;

                // Read the block properties whenever we check, since we don't really persists whether or not we need power.
                var block = GameManager.Instance.World.GetBlock(myTileEntity.ToWorldPos());
                var block2 = Block.list[block.type];
                blNeedsPower = false;
                if (block2.Properties.Values.ContainsKey("RequirePower"))
                    blNeedsPower = StringParsers.ParseBool(block2.Properties.Values["RequirePower"]);

                AdvLogging.DisplayLog(AdvFeatureClass, "Require Power? : " + blNeedsPower);

                // Doesn't need any power, so don't check.
                if (blNeedsPower == false)
                    return true;

                AdvLogging.DisplayLog(AdvFeatureClass, "Workstation requires power. Checking near by tile entities.");

                // Check the nearby chunks for Power source tile entities, and check if they are on. 
                var blockPosition = myTileEntity.ToWorldPos();
                var world = GameManager.Instance.World;
                var num = World.toChunkXZ(blockPosition.x);
                var num2 = World.toChunkXZ(blockPosition.z);
                for (var i = -1; i < 2; i++)
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
                                var distanceSq = (blockPosition.ToVector3() - tileEntity.ToWorldPos().ToVector3()).sqrMagnitude;
                                if (!(distanceSq <= 20 * 20)) continue;
                                AdvLogging.DisplayLog(AdvFeatureClass, myTileEntity + " Found Power Source as Fuel with " + tileEntity);
                                //myTileEntity.HasPowerSDX = true;
                                return true;
                            }

                            AdvLogging.DisplayLog(AdvFeatureClass, "\tTile Entity is off: " + t);
                        }
                    }

                return false;
            }
        }

        [HarmonyPatch(typeof(TileEntityWorkstation))]
        [HarmonyPatch("UpdateTick")]
        public class TileEntityWorkstationHandleFuel
        {
            public static bool Prefix(TileEntityWorkstation __instance, ref float ___currentBurnTimeLeft)
            {
                AdvLogging.DisplayLog(AdvFeatureClass, "UpdateTick()");
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;
                if (___currentBurnTimeLeft < 0.5f)
                    if (PowerWorkstationHelper.CheckWorkstationForPower(__instance))
                        ___currentBurnTimeLeft = 5f;
                AdvLogging.DisplayLog(AdvFeatureClass, "Current Burn Time: " + ___currentBurnTimeLeft);
                return true;
            }
        }
    }
}