The Trader feature extends the game's economy by allowing you to define and use custom currencies for interactions with
traders, offering a way to create unique trading systems.

## Custom Currency Functionality

The core of this feature relies on the `TraderCurrencyManager`, which is designed to associate specific item types (
acting as currency) with individual traders. This means you can dictate that a particular trader only accepts "Dukes" as
payment, while another might exclusively trade in "resourceScrapLead" or a completely custom item.

* **Setting Custom Currency**: The system allows for a currency (identified by its item name or ID) to be assigned to a
  specific trader ID.
* **Retrieving Custom Currency**: When trading, the system checks the assigned currency for the trader. If no custom
  currency is specified, the system defaults to "casinoCoin".

## XML Examples for Custom Currencies

```xml
 <trader_info id="8" reset_interval="3" open_time="4:05" close_time="21:50" alt_currency="oldCash">

</trader_info>
```

The game's code would then read these properties and use them to populate the `TraderCurrencyManager` at runtime,
enabling the custom currency for each defined trader.