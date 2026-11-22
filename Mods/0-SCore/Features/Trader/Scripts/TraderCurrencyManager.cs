
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

    public const string CurrencyScaleProperty = "CurrencyScale";

    // Currency item name -> (buy, sell) multipliers, read once per item.
    private static readonly Dictionary<string, Vector2> _currencyScales = new Dictionary<string, Vector2>();

    /// <summary>
    /// Price multipliers for a trader's alt_currency, read from the currency item's CurrencyScale
    /// property. One value scales buying and selling alike; "buy,sell" scales them separately.
    /// Returns false for traders without an alt_currency, or a currency with no scale.
    /// </summary>
    public static bool TryGetCurrencyScale(int traderID, out float buyScale, out float sellScale)
    {
        buyScale = 1f;
        sellScale = 1f;
        if (!_currencyItems.TryGetValue(traderID, out var currency)) return false;

        if (!_currencyScales.TryGetValue(currency, out var scale))
        {
            scale = Vector2.one;
            var itemClass = ItemClass.GetItemClass(currency, true);
            if (itemClass?.Properties != null && itemClass.Properties.Contains(CurrencyScaleProperty))
            {
                var value = itemClass.Properties.GetString(CurrencyScaleProperty);
                if (value.Contains(","))
                {
                    var parts = value.Split(',');
                    scale = new Vector2(StringParsers.ParseFloat(parts[0].Trim()), StringParsers.ParseFloat(parts[1].Trim()));
                }
                else
                {
                    var single = StringParsers.ParseFloat(value.Trim());
                    scale = new Vector2(single, single);
                }
            }

            _currencyScales[currency] = scale;
        }

        buyScale = scale.x;
        sellScale = scale.y;
        return scale != Vector2.one;
    }

}
