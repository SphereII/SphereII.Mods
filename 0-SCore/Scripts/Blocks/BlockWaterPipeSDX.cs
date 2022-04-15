using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class BlockWaterPipeSDX : Block
{
    // This allows for the water logic to pass through it.
}

public class BlockWaterSourceSDX : Block
{
    // This is not a direct source of water in itself. However, if it's connected to a series of BlockWaterPipeSDXs which connect up to 
    // a BlockLiquidV2, then it can act like the water block itself, providing the same range of water power as if its the block itself.
    // If there is no BlockWaterPipeSDXs connected to water, it does nothing.
}

