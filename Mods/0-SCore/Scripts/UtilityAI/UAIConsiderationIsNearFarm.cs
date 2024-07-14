using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace UAI
{
    public class UAIConsiderationIsNearFarm : UAIConsiderationTargetType
    {
        private int distance = 50;
        public override void Init(Dictionary<string, string> parameters)
        {
            base.Init(parameters);
            if (parameters.ContainsKey("distance"))
                StringParsers.TryParseSInt32(parameters["distance"], out distance, 0, -1, NumberStyles.Integer);
        }
        public override float GetScore(Context _context, object target)
        {
            // If we have the crop manager running, and there is a block that is close by, go tend it.
            var position = new Vector3i(_context.Self.position);

            var farmPlot = FarmPlotManager.Instance.GetFarmPlotsNearby(position);
            if (farmPlot != null)
            {
                return 1f;
            }

            farmPlot = FarmPlotManager.Instance.GetClosesUnmaintained(position, distance);
            if (farmPlot != null)
            {
                return 1f;
            }


            farmPlot = FarmPlotManager.Instance.GetFarmPlotsNearbyWithPlants(position);
            if (farmPlot != null)
            {
                return 1f;
            }


            farmPlot = FarmPlotManager.Instance.GetClosesUnmaintainedWithPlants(position,distance);
            if (farmPlot != null)
            {
                return 1f;
            }


            // If we don't have any at our feet, find another one that is close by.
            var plants = FarmPlotManager.Instance.GetClosePositions(position, distance);
            if (plants.Count > 0)
            {
                return 1f;
            }


            var wilted = FarmPlotManager.Instance.GetCloseFarmPlotsWilted(position, distance);
            if (wilted.Count > 0)
            {
                return 1f;
            }
           
            FarmPlotManager.Instance.ResetPlantsInRange(position, distance);

            return 0f;
        }

      

    }
}