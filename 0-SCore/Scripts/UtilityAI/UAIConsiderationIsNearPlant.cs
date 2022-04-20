using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace UAI
{
    public class UAIConsiderationIsNearPlant : UAIConsiderationTargetType
    {
        private int distance = 10;
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
            PlantData plantData = CropManager.Instance.GetPlantDataNearby(position);
            if (plantData != null)
                return 1f;

            plantData = CropManager.Instance.GetClosesUnmaintained(position, distance);
            if (plantData != null)
                return 1f;

            return 0f;
        }
    }
}