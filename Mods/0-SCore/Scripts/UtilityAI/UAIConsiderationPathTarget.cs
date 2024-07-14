namespace UAI
{
    public class UAIConsiderationPathTarget : UAIConsiderationTargetType
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

                var tileEntity = _context.World.GetTileEntity(0, vector);
                if (tileEntity == null) continue;
                if (targetType == TileEntityType.None) continue;

                if (tileEntity.GetTileEntityType() == targetType)
                {
                    AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"{GetType()} : {tileEntity.ToString()}  My Position: {_context.Self.position}  Type: {tileEntity.GetTileEntityType()}");
                    switch (tileEntity.GetTileEntityType())
                    {
                        // If the loot containers were already touched, don't path to them.
                        case TileEntityType.Loot:
                            if (!((TileEntityLootContainer)tileEntity).bTouched)
                                return 1f;
                            break;
                        case TileEntityType.SecureLoot:
                            if (!((TileEntitySecureLootContainer)tileEntity).bTouched)
                                return 1f;
                            break;
                        default:
                            return 1f;
                    }
                }
            }

            return 0f;

        }
    }
}