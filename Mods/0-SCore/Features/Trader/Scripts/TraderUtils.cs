using UnityEngine;

public static class TraderUtils
{
    // A private property to handle the common player and trader null checks.
    private static XUiM_Trader TraderController
    {
        get
        {
            var player = GameManager.Instance.World.GetPrimaryPlayer();
            return player?.playerUI?.xui?.Trader;
        }
    }

    /// <summary>
    /// Gets the current trader's ID if a trader is being accessed.
    /// </summary>
    /// <returns>The ID of the current trader, or -1 if no trader is being accessed.</returns>
    public static int GetCurrentTraderID()
    {
        var trader = TraderController;
        if (trader == null)
        {
            return -1;
        }

        // Use a single, concise check for the trader ID.
        if (trader.TraderEntity != null)
        {
            return trader.TraderEntity.NPCInfo.TraderID;
        }

        if (trader.Trader != null)
        {
            return trader.Trader.TraderID;
        }
        
        if (trader.TraderTileEntity != null)
        {
            return trader.TraderTileEntity.TraderData.TraderID;
        }

        return -1;
    }

    /// <summary>
    /// Gets the TileEntityTrader of the current trader.
    /// </summary>
    /// <returns>The TileEntityTrader, or null if no trader is being accessed.</returns>
    public static TileEntityTrader GetCurrentTraderTileEntity()
    {
        return TraderController?.TraderTileEntity;
    }
}