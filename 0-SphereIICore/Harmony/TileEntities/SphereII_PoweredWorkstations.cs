using HarmonyLib;
using System;
using System.Collections.Generic;

/**
 * SphereII_PoweredWorkstations
 *
 * This class includes a Harmony patch allows a tile entity to require power, and recieve it when a power source is within a radious.
 * 
 * Usage XML:
 * 
 * <property name="RequirePower" value="true"/>
 */
public class SphereII_PoweredWorkstations
{
    private static string AdvFeatureClass = "AdvancedWorkstationFeatures";
    private static string Feature = "EnablePoweredWorkstations";

    public class SphereII_PowerWorkstationHelper
    {
        public static bool CheckWorkstationForPower(TileEntity myTileEntity)
        {
            AdvLogging.DisplayLog(AdvFeatureClass, "Does Workstation have access to Fuel?");
            bool blNeedsPower = false;

            // Read the block properties whenever we check, since we don't really persists whether or not we need power.
            BlockValue block = GameManager.Instance.World.GetBlock(myTileEntity.ToWorldPos());
            Block block2 = Block.list[block.type];
            blNeedsPower = false;
            if(block2.Properties.Values.ContainsKey("RequirePower"))
                blNeedsPower = StringParsers.ParseBool(block2.Properties.Values["RequirePower"], 0, -1, true);

            AdvLogging.DisplayLog(AdvFeatureClass, "Require Power? : " + blNeedsPower);

            // Doesn't need any power, so don't check.
            if(blNeedsPower == false)
                return true;

            AdvLogging.DisplayLog(AdvFeatureClass, "Workstation requires power. Checking near by tile entities.");

            // Check the nearby chunks for Power source tile entities, and check if they are on. 
            Vector3i blockPosition = myTileEntity.ToWorldPos();
            World world = GameManager.Instance.World;
            int num = World.toChunkXZ(blockPosition.x);
            int num2 = World.toChunkXZ(blockPosition.z);
            for(int i = -1; i < 2; i++)
            {
                for(int j = -1; j < 2; j++)
                {
                    Chunk chunk = (Chunk)world.GetChunkSync(num + j, num2 + i);
                    if(chunk != null)
                    {
                        DictionaryList<Vector3i, TileEntity> tileEntities = chunk.GetTileEntities();
                        for(int k = 0; k < tileEntities.list.Count; k++)
                        {
                            AdvLogging.DisplayLog(AdvFeatureClass, "\tDetected Tile Entity: " + tileEntities.list[k].ToString());
                            TileEntityPowerSource tileEntity = tileEntities.list[k] as TileEntityPowerSource;
                            if(tileEntity != null)
                            {
                                if(tileEntity.IsActive(world))
                                    AdvLogging.DisplayLog(AdvFeatureClass, "\tEntity is Avtive " + tileEntities.list[k].ToString());
                                if(tileEntity.IsOn)
                                {
                                    AdvLogging.DisplayLog(AdvFeatureClass, "\tTile Entity is On: " + tileEntities.list[k].ToString());
                                    float distanceSq = (blockPosition.ToVector3() - tileEntity.ToWorldPos().ToVector3()).sqrMagnitude;
                                    if(distanceSq <= 20 * 20)
                                    {
                                        AdvLogging.DisplayLog(AdvFeatureClass, myTileEntity.ToString() + " Found Power Source as Fuel with " + tileEntity.ToString());
                                        //myTileEntity.HasPowerSDX = true;
                                        return true;
                                    }
                                }
                                else
                                    AdvLogging.DisplayLog(AdvFeatureClass, "\tTile Entity is off: " + tileEntities.list[k].ToString());
                            }
                        }
                    }
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(TileEntityWorkstation))]
    [HarmonyPatch("UpdateTick")]
    public class SphereII_TileEntityWorkstation_HandleFuel
    {
        public static bool Prefix(TileEntityWorkstation __instance, ref float ___currentBurnTimeLeft)
        {
            AdvLogging.DisplayLog(AdvFeatureClass, "UpdateTick()");
            if(!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;
            if(___currentBurnTimeLeft < 0.5f)
            {
                if ( SphereII_PowerWorkstationHelper.CheckWorkstationForPower(__instance) )
                    ___currentBurnTimeLeft = 5f;
            }
            AdvLogging.DisplayLog(AdvFeatureClass, "Current Burn Time: " + ___currentBurnTimeLeft);
            return true;
        }
    }


}