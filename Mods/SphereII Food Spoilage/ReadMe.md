# SphereII Food Spoilage

Turns on and tunes **0-SCore's** food spoilage. Food does not simply vanish: a cooked meal goes
off into **Spoiled Leftovers**, still edible but far more likely to give you dysentery, and only
then rots away entirely. Coolers, fridges and freezers slow the whole chain down.

Fresh food is also **out of the loot tables**. In a world where food spoils, finding a hot meal in
a derelict fridge undercuts the whole system, so perishable food now has three sources: cook it,
buy it from a trader or vending machine, or earn it from a quest.

**Requires 0-SCore 3.2.17.915 or newer.** The spoilage engine is SCore's; this modlet is
configuration and item data only, with no code of its own. Without SCore installed every xpath in
it finds nothing and is skipped silently, so the modlet does nothing rather than erroring.

That version matters rather than being merely recommended. Before it, a spoilable item held in the
hand holstered and re-drew itself every time it aged, which on screen looked like the item
reloading. Spoilage lives entirely in item metadata, and `Inventory.SetItem` re-shows the held item
whenever the incoming value is not `EqualsExceptUseTimesAndAmmo` to the current one - a comparison
that includes the metadata dictionary. SCore's `InventorySetItemSpoilage` patch excludes the three
spoilage keys from that test.

---

## The chain

| stage | becomes | notes |
| --- | --- | --- |
| Cooked meal | Spoiled Leftovers | roughly a day in a backpack |
| Spoiled Leftovers | Spoiled Remains | roughly eight more hours |
| Raw meat | Rotting Flesh | no leftovers stage, and the one thing that really is rotting flesh |
| Eggs | Spoiled Remains | as quick as raw meat |
| Harvested crops | Spoiled Remains | slower, about a day and a half |
| Teas, coffee, juice | Murky Water | roughly a day |

**Spoiled Leftovers** carry a `.DiseaseRoll` of **40** against the vanilla **12** that charred meat
and murky water use. Iron Gut still helps: the roll is reduced by `$MetabolismResist` exactly as
vanilla's risky foods are. They are worth about a third of the meal they came from and cost you a
little health.

**Spoiled Remains** is where the chain stops, at a `.DiseaseRoll` of **50**. It exists so that
spoilage never pays. `foodRottingFlesh` is a real crafting ingredient - ten of it makes a farm
plot, and it also feeds chicken feed, canned sham, hobo stew and Fort Bites - so routing crops and
leftovers into it would have rewarded players for neglecting their food. Remains are worth nothing,
sell for nothing, and are an ingredient in no recipe. Only raw meat still yields rotting flesh,
where it is a straight conversion of something you already had rather than a new resource.

## Where food comes from

In a world where food spoils, finding a hot meal in a derelict fridge undercuts the whole system.
Perishable food is out of the ordinary loot tables: **cook it, buy it, or earn it.**

| source | status |
| --- | --- |
| Cooking | unchanged - the main way you eat |
| Traders and vending machines | unchanged - traders sell 29 perishable foods by name |
| Quest rewards | unchanged - `groupQuestFood` keeps all 17 of its perishable entries |
| Airdrops | unchanged - a supply drop was packed and sent, not scavenged |
| Bird nests | unchanged - robbing a nest is foraging, and eggs are an ingredient |
| Twitch rewards | unchanged - viewer gifts, not loot |
| **Every other container** | **stripped** - fridges, coolers, dumpsters, ovens, grills, food piles |

Canned goods, honey, cornmeal, jerky, water, beer and Grandpa's brews all stay in loot, so
scavenging still feeds you. It just feeds you out of a tin.

Traders and vending machines are safe by construction, not by luck: traders sell perishable food
by name in `traders.xml`, and none of the loot groups edited here is reachable from it. Vending
machines are trader inventories too - `foodVending` pulls the `cannedfood` group, `drinkVending`
pulls `groupDrinks`.

`groupFoodRare` held four cooked meals and nothing else, so stripping it would have left an empty
group that nine others draw on, including `groupRefrigerator01`, `groupCooler01` and
`groupShamwayFreezer01`. It is restocked with canned sham, salmon, tuna and honey - shelf-stable,
but still a find.

## Time to spoil

In in-game hours, from the formula `TickPerLoss * SpoilageMax / (SpoilagePerTick + location) / 1000`,
where world time runs at 1000 units per in-game hour.

| | toolbelt | backpack | container | cooler | retro fridge | steel fridge | freezer |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Cooked meal | 22.2h | 1.0d | 1.2d | 1.4d | 1.5d | 1.6d | 1.7d |
| Raw meat, egg | 6.2h | 7.1h | 8.3h | 10.0h | 11.1h | 11.8h | 12.5h |
| Crops | 1.0d | 1.2d | 1.4d | 1.7d | 1.9d | 2.0d | 2.1d |
| Drinks | 22.2h | 1.0d | 1.2d | 1.4d | 1.5d | 1.6d | 1.7d |
| Spoiled Leftovers | 7.5h | 8.3h | 9.4h | 10.7h | 11.5h | 12.0h | 12.5h |

Carrying food on your belt is the worst place for it and a powered-looking container the best.
Full chain for a cooked meal in a backpack: about a day and a half from fresh to rotten.

## What never spoils

Canned goods, honey, cornmeal, jerky, plain and boiled water, beer, and all three of Grandpa's
brews - which is also, deliberately, most of what loot now yields. Rotting Flesh and Spoiled
Remains are both terminal: neither carries a `Spoilable` property, so nothing rots twice.

## Container preservation

`PreserveBonus` is **subtracted** from the location modifier, so a **positive** number preserves
and a larger one preserves better.

| container | PreserveBonus |
| --- | --- |
| Coolers | 1 |
| Retro fridges | 1.5 |
| Stainless steel fridges | 1.75 |
| Commercial freezers | 2 |

None of them need power. If you want a container that preserves food completely, SCore treats a
`PreserveBonus` of **exactly -99** as a "never spoils" flag. Do not improvise other negative
values: anything else negative is subtracted as normal and makes food spoil dramatically *faster*.

## Notes for modders

The broad selectors match on an exact tag token rather than a name prefix:

```xml
//item[property[@name='Tags' and contains(concat(',',@value,','),',food,')]]
```

The `concat` wrapping matters. A bare `contains(@value,'food')` also matches the `foodSkill` tag,
which every drink carries, so drinks would be swept into the food rules.

An xpath predicate only sees properties an item declares itself, never ones it inherits through
`Extends`. That works in our favour: fourteen of the fifteen `foodCan*` items inherit their Tags
and fall out of the selector for free. Three items sit on the wrong side of that line and are
handled by name - `foodShamChowder` and `drinkJarBlackStrapCoffee` are added back in, and
`foodCanBeef`, the one can that declares its own Tags, is taken back out.

`foodSpoiledLeftovers` and `foodSpoiledRemains` are declared **last** in `items.xml` on purpose.
The broad appends run in document order, so declaring them afterwards means neither can be caught
by its own food selector. Being caught would have made Leftovers spoil into themselves, and would
have given Remains the `Spoilable` property it must not have if the chain is to end anywhere.

Both are also written out in full rather than extending a vanilla food. Extending would inherit
that food's `effect_group`, so the inherited `set .DiseaseRoll 12` and our own would both fire on
`onSelfPrimaryActionEnd` with the winner decided by document order - a fragile way to set the one
number each item exists for.

Items taken back out keep a few orphan `SpoiledItem` and `TickPerLoss` properties from the broad
append. They are inert - SCore gates entirely on `Spoilable`, in both `ShouldSkip` and its
`ItemValue.Clone` patch - and removing only `Spoilable` keeps the exclusion list readable.

`loot.xml` puts the **exemptions in the predicate rather than the item list**:

```xml
<remove xpath="//lootgroup[not(starts-with(@name,'twitch_')) and @name!='groupQuestFood'
        and @name!='airdropfood' and @name!='airdropdrink' and @name!='groupBirdNest02']/item[@name='foodMeatStew']"/>
```

One line per item strips it from every ordinary group at once, and keeps it stripped if a future
game version adds it somewhere new. The alternative - enumerating 106 group-and-item pairs - goes
stale the moment vanilla moves anything.

Two traps worth knowing if you extend this. **Trader stock can be coupled to loot**: trader item
groups reference lootgroups by name, so `foodVending` pulling the `cannedfood` group means editing
that group would silently edit vending machines. Check the transitive closure of everything
`traders.xml` references before removing from a group. And **watch for groups you empty**:
`groupFoodRare` was four cooked meals and nothing else, with nine groups drawing on it, so it is
restocked rather than left bare.

## Installing

Install on **both client and server**. SCore uses C# and so is **not EAC compatible**.

**Start a new save.** Existing items in an existing world have no spoilage metadata, and food
already in containers will begin its clock from the moment you first open them.

## In-game help

If `0-Help Screens` is installed, this modlet adds a **Food Spoilage** section to it (ESC →
Help) with four pages: Overview, Storage, Spoiled Food, and What Keeps. It is written for
players rather than modders - no XML, no property names, just what spoils, where to keep it and
what is safe to eat.

`Config/XUi_InGame/windows.xml` holds the section and is wrapped in
`<if cond="mod_loaded('sphereii_help_screens')">`, so it costs nothing when that modlet is
absent. Four pages fill a four-slot strip exactly; adding a fifth means raising `cols` on the
grid in the same file, or the surplus page gets a null `TabButton` that
`XUiC_TabSelectorTab.TabSelected` dereferences unguarded.

## Tuning

Five files: `Config/blocks.xml` for the global rates and container bonuses, `Config/items.xml`
for per-item timings and the two rot items, `Config/loot.xml` for what scavenging yields,
`Config/XUi_InGame/windows.xml` for the help section, and `Config/Localization.csv` for the item
names, their descriptions and every word of the help pages.

**Speed.** Raise `TickPerLoss` on the items to slow everything down. To change how much storage
matters, adjust the `Toolbelt` / `Backpack` / `Container` modifiers on SCore's
`ConfigFeatureBlock`.

**Risk.** Change the `.DiseaseRoll` values on `foodSpoiledLeftovers` (40) and `foodSpoiledRemains`
(50). Vanilla's own risky foods sit at 12 and rotting flesh at 15. Keep Remains at or above
Leftovers so the chain still gets worse as it goes.

**Early game.** This bites hardest on day one, before you have a campfire or any dukes. If it
proves too tight, the softest lever is putting a couple of cheap cooked meals back into
`groupFoodCommon` rather than reversing anything in `loot.xml` - scavenging keeps its new shape
and you only blunt the first day.
