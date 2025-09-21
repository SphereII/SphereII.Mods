public static class OnSelfPlaceBlock
{
 public static void PlaceBlock(string blockName, Vector3i blockPos)
    {
        var block = GameManager.Instance.World.GetBlock(blockPos);
        var minEventParams = new MinEventParams {
            TileEntity = TraderUtils.GetCurrentTraderTileEntity(),
            Self = GameManager.Instance.World.GetPrimaryPlayer(),
            ItemValue = ItemValue.None,
            BlockValue = block,
            Biome = GameManager.Instance.World.GetPrimaryPlayer()?.biomeStandingOn
        };
        minEventParams.Self.MinEventContext = minEventParams;
        minEventParams.Self.FireEvent(MinEventTypes.onSelfPlaceBlock);

    }
}
