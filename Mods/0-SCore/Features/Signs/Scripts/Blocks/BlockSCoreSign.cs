
    using Platform;

    public class BlockSCoreSign : BlockPlayerSign{
        public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue,  PlatformUserIdentifierAbs _addedByPlayer)
        {
            if (_blockValue.ischild)
            {
                return;
            }
            if ((TileEntitySCoreSign)world.GetTileEntity(_chunk.ClrIdx, _blockPos) == null)
            {
                var tileEntitySign = new TileEntitySCoreSign(_chunk) {
                    localChunkPos = World.toBlock(_blockPos)
                };
                _chunk.AddTileEntity(tileEntitySign);
            }
            base.OnBlockAdded(world, _chunk, _blockPos, _blockValue, _addedByPlayer);

        }
        public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
        {
            if (_ebcd == null)
            {
                return;
            }
            var chunk = (Chunk)((World)_world).GetChunkFromWorldPos(_blockPos);
            var tileEntitySign = (TileEntitySCoreSign)_world.GetTileEntity(_cIdx, _blockPos);
            if (tileEntitySign == null)
            {
                tileEntitySign = new TileEntitySCoreSign(chunk);
                tileEntitySign.localChunkPos = World.toBlock(_blockPos);
                chunk.AddTileEntity(tileEntitySign);
            }

            tileEntitySign.SetBlockEntityData(_ebcd);
            base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
        }

   
    }
