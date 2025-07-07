
using System.Collections.Generic;
using UnityEngine;

public class TraderCurrencyManager
{
    public static string DefaultCurrency = "casinoCoin";
    
    private static Dictionary<int, string> _currencyItems = new Dictionary<int, string>();
    public static void SetTraderCurrency(int traderID, string currency)
    {
        if (!_currencyItems.TryAdd(traderID, currency))
        {
           // Log.Out($"Duplicate Trader ID detected. Using {Localization.Get(currency)} for Trader ID {traderID}");
        }
    }

    public static bool HasCustomCurrency(int traderID)
    {
        return _currencyItems.ContainsKey(traderID);
    }
    public static string GetTraderCurrency(int traderID)
    {
        
        var result= _currencyItems.GetValueOrDefault(traderID, DefaultCurrency);
        return result;
    }

}
