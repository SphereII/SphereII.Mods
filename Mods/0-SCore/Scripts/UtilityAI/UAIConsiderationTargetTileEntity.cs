using UnityEngine;

namespace UAI
{
    public class UAIConsiderationTargetTileEntity : UAIConsiderationTargetTileEntityBase
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.SameValue(base.GetScore(_context, target), 1f) ? 0f : 1f;

        }
    }
    public class UAIConsiderationTargetTileEntityBase : UAIConsiderationTargetType
    {
        private float _min = 0f;
        private float _max = 100f;

        private static readonly string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private static readonly string Feature = "UtilityAILoggingMin";

        public override float GetScore(Context _context, object target)
        {

            // If the target isn't a vector, don't process.
            if (target.GetType() != typeof(Vector3))
                return 1f;

            for (int i = 0; i < this.type.Length; i++)
            {
                var vector = new Vector3i((Vector3)target);
                var filterType = this.type[i];
                var targetType = EnumUtils.Parse<TileEntityType>(filterType, true);
                if (targetType == TileEntityType.None) // Not a tile entity.
                    continue;

                var tileEntity = _context.World.GetTileEntity(0, vector);
                if (tileEntity.GetTileEntityType() == targetType)
                {
                    float num = UAIUtils.DistanceSqr(_context.Self.position, vector);
                    float scoreClamp = Mathf.Clamp01(Mathf.Max(0f, num - this._min) / (this._max - this._min));
                    float score = (Mathf.Max(i, num - this._min) / (this._max - this._min));
                    AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"{GetType()} : {tileEntity.ToString()} Score {score} Clamped {scoreClamp}  My Position: {_context.Self.position} Distance {num} Type: {tileEntity.GetTileEntityType()}");
                    switch (tileEntity.GetTileEntityType())
                    {
                        // If the loot containers were already touched, don't path to them.
                        case TileEntityType.Loot:
                            if (!((TileEntityLootContainer)tileEntity).bTouched)
                                return scoreClamp;
                            break;
                        case TileEntityType.SecureLoot:
                            if (!((TileEntitySecureLootContainer)tileEntity).bTouched)
                                return scoreClamp;
                            break;
                        default:
                            //  var temp = _context.Self.rand.RandomFloat;
                            //Debug.Log($"\tRandomScore: {Mathf.Clamp01(temp)}");
                            return scoreClamp;
                    }
                }
            }

            return 1f;
        }
    }
}