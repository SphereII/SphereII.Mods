
    using Platform;

    public class BlockSCoreSign : BlockSign{
        public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue,  PlatformUserIdentifierAbs _addedByPlayer)
        {
            base.OnBlockAdded(world, _chunk, _blockPos, _blockValue, _addedByPlayer);
        }
        public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
        {
            if (_ebcd == null) return;
            base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _ebcd);
        }
    }
