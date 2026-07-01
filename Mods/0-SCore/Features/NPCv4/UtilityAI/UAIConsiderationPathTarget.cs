namespace UAI
{
    public class UAIConsiderationPathTargetV4 : UAIConsiderationTargetType
    {
        private static readonly string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private static readonly string Feature = "UtilityAILoggingMin";

        public override float GetScore(Context _context, object target)
        {
            var paths = SphereCache.GetPaths(_context.Self.entityId);
            if (paths.Count == 0)
                return 0f;

            for (int i = 0; i < this.type.Length; i++)
            {
                var vector = new Vector3i(paths[0]);
                var filterType = this.type[i];
                var targetType = EnumUtils.Parse<TileEntityType>(filterType, true);
                if (targetType == TileEntityType.None) // Not a tile entity.
                    continue;

                var tileEntity = _context.World.GetTileEntity(vector);
                if (tileEntity == null) continue;
                if (targetType == TileEntityType.None) continue;

                if (tileEntity.GetTileEntityType() == targetType)
                {
                    AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"{GetType()} : {tileEntity.ToString()}  My Position: {_context.Self.position}  Type: {tileEntity.GetTileEntityType()}");
                    if (tileEntity is TileEntityComposite tec)
                    {
                        var storage = tec.GetFeature<TEFeatureStorage>();
                        if (storage == null || !storage.bTouched) return 1f;
                    }
                    else
                    {
                        return 1f;
                    }
                }
            }

            return 0f;

        }
    }
}