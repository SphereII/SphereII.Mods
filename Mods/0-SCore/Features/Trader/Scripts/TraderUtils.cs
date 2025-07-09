public class TraderUtils
{
    // Helper method to try to get the current trader ID.
    public static int GetCurrentTraderID(){
        var player = GameManager.Instance.World.GetPrimaryPlayer();
        if (player == null) return -1;
        var tileEntityTrader = GetCurrentTraderTileEntity();
        if (tileEntityTrader?.TraderData == null) return -1;
        return tileEntityTrader.TraderData.TraderID;
    }
    
    public static TileEntityTrader GetCurrentTraderTileEntity(){
        var player = GameManager.Instance.World.GetPrimaryPlayer();
        return player == null ? null : player.playerUI?.xui?.Trader?.TraderTileEntity;
    }
}
