// PathingCube is a POI programming marker: an invisible sign block whose text carries the
// pathing code ("task=...;buff=...;pc=..."), read back out via PathingCubeParser.
//
// This used to extend BlockSign, which the game abandoned in 3.0 - no vanilla block uses that
// class any more (0 blocks, vs 805 on CompositeFeatures). BlockSign never sets HasTileEntity and
// creates no tile entity, so every "GetTileEntity(pos) as TileEntityComposite" here returned null:
// the activation commands were unreachable, and TileEntityLegacyUtils.ReadLegacySignIntoComposite
// refused to migrate saved pathing cubes (its guard requires a BlockCompositeTileEntity), which
// threw away their codes and NRE'd Prefab.readTileEntities. BlockSign also stamps a raw duplicate
// of itself UpwardsCount cells up - defaulting to 1, independent of MultiBlockDim - which is why a
// "1,1,1" pathing cube occupied two cells.
//
// As a composite block the edit / lock / unlock / keypad commands come from the TEFeatureSignable
// and TEFeatureLockable features declared in PathingBlocks.xml, which own the owner/ACL checks
// themselves, so the hand-rolled command table this class used to carry is gone with them.
internal class BlockPathFinding : BlockCompositeTileEntity {
    public override void Init() {
        base.Init();

        // BlockSign forced this on and PathingCube inherited it; keep it so reparenting doesn't
        // quietly change decoration placement. Set here rather than in XML so blocks extending
        // PathingCube inherit it too.
        IsTerrainDecoration = true;
    }

    public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos,
        BlockValue _blockValue, BlockEntityData _ebcd) {
        if (_ebcd == null) return;

        // Base builds the composite tile entity and hands the transform to the features, so the
        // model can only be hidden after it has run.
        base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _ebcd);

        if (_ebcd.transform != null)
            _ebcd.transform.gameObject.SetActive(false);
    }
}
