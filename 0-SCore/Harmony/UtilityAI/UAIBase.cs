using HarmonyLib;
using System.Collections.Generic;
using UAI;
using UnityEngine;

namespace Harmony.UtilityAI
{
    public class UAIBasePatches
    {
        private static readonly string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private static readonly string Feature = "UtilityAILogging";

        //[HarmonyPatch(typeof(UAIBase))]
        //[HarmonyPatch("addEntityTargetsToConsider")]
        //public class UAIBase_addEntityTargetsToConsider
        //{
        //    public static void Postfix(Context _context)
        //    {
        //        var leader = EntityUtilities.GetLeaderOrOwner(_context.Self.entityId);
        //        if ( leader != null )
        //        {
        //            if ( !_context.ConsiderationData.EntityTargets.Contains(leader))
        //                _context.ConsiderationData.EntityTargets.Add(leader);
        //        }

        //    }
        //}
        [HarmonyPatch(typeof(UAIBase))]
        [HarmonyPatch("addWaypointTargetsToConsider")]
        public class UAIBase_addWaypointTargetsToConsider
        {
            public static void Postfix(ref Context _context)
            {
                if (_context.ConsiderationData.WaypointTargets == null)
                {
                    _context.ConsiderationData.WaypointTargets = new List<Vector3>();
                }
                //for(int x = 0; x < 5; x++)
                //{
                //    _context.World.GetRandomSpawnPositionMinMaxToPosition(_context.Self.position, 5, 10, 2, false, out Vector3 position, false);
                //    _context.ConsiderationData.WaypointTargets.Add(position);
                //}
                //var path = SphereCache.GetPaths(_context.Self.entityId);
                //if (path?.Count > 0)
                //    _context.ConsiderationData.WaypointTargets.Add(path[0]);

                AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Scanned WayPoints {_context.ConsiderationData.WaypointTargets.Count}");

                // Disabled functionality due to using the Pathing feature for the Utility AI.

                AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Scanning WayPoints...");

                //var blockPosition = _context.Self.GetBlockPosition();
                //var chunkX = World.toChunkXZ(blockPosition.x);
                //var chunkZ = World.toChunkXZ(blockPosition.z);

                //for (int i = -1; i < 2; i++)
                //{
                //    for (int j = -1; j < 2; j++)
                //    {
                //        var chunk = (Chunk)_context.Self.world.GetChunkSync(chunkX + j, chunkZ + i);
                //        if (chunk == null) continue;

                //        var tileEntities = chunk.GetTileEntities();
                //        for (var k = 0; k < tileEntities.list.Count; k++)
                //        {
                //            var position = tileEntities.list[k].ToWorldPos().ToVector3();
                //            if (!_context.ConsiderationData.WaypointTargets.Contains(position))
                //                _context.ConsiderationData.WaypointTargets.Add(position);
                //        }
                //    }
                //}

                //AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Scanned WayPoints {_context.ConsiderationData.WaypointTargets.Count}");
                //UAIBase.ActionChoiceDelay = 1f;
                //UAIBase.MaxWaypointsToConsider = _context.ConsiderationData.WaypointTargets.Count;
                //if (_context.ConsiderationData.WaypointTargets.Count > 1)
                //{
                //    _context.ConsiderationData.WaypointTargets.Sort(new UAIUtils.NearestWaypointSorter(_context.Self));
                //}
            }

        }
    }
}