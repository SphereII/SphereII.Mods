
using System.Collections.Generic;
public class TraderCurrencyManager
{
    private static readonly Dictionary<int, string> CurrencyItems = new Dictionary<int, string>();

    public static void SetTraderCurrency(int traderID, string currency)
    {
        CurrencyItems.Add(traderID, currency);
    }

    public static string GetTraderCurrency(int traderID)
    {
        var result= CurrencyItems.GetValueOrDefault(traderID, "casinoCoin");
        return result;
    }
}
