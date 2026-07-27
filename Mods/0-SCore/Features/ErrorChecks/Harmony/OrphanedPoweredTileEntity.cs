namespace SCore.Features.ErrorChecks.Harmony {
    // Shared orphan test for the two FixOrphanedPoweredTileEntities guards: the Chunk.save sweep
    // and the TileEntityPowered.OnReadComplete prefix.
    //
    // Both used to test "block is BlockPowered", which is wrong. BlockPowerSource derives straight
    // from Block, not from BlockPowered, so BlockGenerator, BlockSolarPanel and BlockBatteryBank
    // all failed the test: placing a generator and saving the chunk deleted its live
    // TileEntityPowerSource out of the chunk, and opening the block then NRE'd in
    // TileEntityPowerSource.CurrentFuel. SCore's own BlockPoweredWorkstationSDX (a BlockWorkstation
    // hosting a TileEntityPoweredWorkstationSdx) has the same shape and was equally exposed.
    //
    // The corruption case these guards exist for is a powered tile entity whose block was
    // overwritten raw by a later decoration pass, so no OnBlockRemoved ever fired. The block is
    // then air or plain terrain and owns no tile entity at all. Block.HasTileEntity is the
    // engine's own flag for exactly that, and BlockPowered, BlockPowerSource and BlockWorkstation
    // all set it in their constructors, so it covers modded powered blocks without a class
    // whitelist to keep in sync.
    //
    // Anything that does claim a tile entity is left alone. Leaving a questionable pair in place
    // is no worse than vanilla; dropping it destroys player equipment.
    internal static class OrphanedPoweredTileEntity {
        public static bool IsOrphaned(Block block) {
            // A missing block definition is as orphaned as it gets.
            if (block == null) return true;

            return !block.HasTileEntity;
        }
    }
}
