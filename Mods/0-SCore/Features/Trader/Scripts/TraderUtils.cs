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
        if (trader == null) return -1;
        return trader.Trader?.TraderData?.TraderID ?? -1;
    }

    /// <summary>
    /// Gets the vending machine tile entity for the current trader interaction.
    /// </summary>
    /// <returns>The TileEntityVendingMachine, or null if no trader is being accessed.</returns>
    public static TileEntityVendingMachine GetCurrentTraderTileEntity()
    {
        return TraderController?.Trader as TileEntityVendingMachine;
    }

    /// <summary>
    /// Gets the NPC entity for the current trader interaction (entity traders only).
    /// </summary>
    public static EntityNPC GetCurrentTraderEntity()
    {
        return TraderController?.Trader as EntityNPC;
    }
}
