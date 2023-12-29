# Food Spoilage and Preserved Goods

Introduces food spoilage to the game, and new preserved foods that do not spoil.

## Features

* Most foods that are not canned or otherwise preserved can now spoil.
* Prepared drinks (such as teas) can now spoil.
* Battery-powered refrigerators and beverage coolers can be crafted.
  Recipes are unlocked in the same perk and level as battery banks.
* New canned/preserved foods:
  * IPA (India Pale Ale). Beer that takes more hops to craft and doesn't spoil.
    Recipe unlocked in the same perk and level as the vanilla beer recipe.
  * Canned corn, mushrooms, potato, and pumpkin.
    Recipes unlocked in the same perk and level as blueberry pie.
  * Preserved blueberries. Recipe unlocked in the same perk and level as blueberry pie.
  * "Shamway Fruit Pie" (in box, similar to Hostess or Drake's). Loot only, cannot be crafted.
  * Smoked/cured meat. Recipe unlocked in the same perk and level as grilled meat.
* Advanced recipes (stews, pies, etc.) have additional recipes that use the new canned vegetables
    and preserved blueberries.
* Spoilable foods are removed from (non-Twitch) loot groups,
    and replaced with canned/preserved equivalents.

## Dependent and Compatible Modlets

**This modlet requires the Alpha 21 version of the `0-SCore` modlet.**
Earlier versions will not work!

## Technical Details

This modlet uses XPath to modify XML files, and does not require C#.

However, the `0-SCore` modlet **does** require C# code.
It is *not* compatible with EAC.

Additionally, this modlet contains new assets (icons).
It must be installed on both clients and servers.

After installation, **you absolutely must start a new game!**

If you load up an existing game after installing this modlet,
**it will become corrupted and you will lose the game world.**

### How food spoilage works

This modlet depends upon the food spoilage code provided by the `0-SCore` modlet.
That modlet has custom C# code that is configurable via XML.
It is loosely based on item degradation from the vanilla game.

Food spoilage is calculated when the player opens a container with one or more stacks of
consumable items in it.
The term SpereII uses for this calculation is _loss calculation._

A loss calculation can only happen after a certain number of game ticks since the last time
the loss was calculated.
This number of ticks is called _ticks per loss._
The ticks per loss is configurable in XML, and can be set individually per consumable item.

The loss caluclation can happen multiple times at once, if the ticks per loss would have been
reached multiple times since the container was last opened.
For example, if ticks per loss is 100, and it has been 1000 ticks since the user last opened the
container, then the loss calculation will happen 10 times when the user opens that container.

Each time the loss calculation occurs, it uses a number of different values to determine the
amount of food spoilage:

* The spoilage amount starts as an amount specified for that consumable item in the item's XML.
  This amount is called _spoilage per tick_
  (though it should probably be called "spoilage per loss" or "spoilage per calculation").
* The stack of consumable items is always located in some container,
  and that container _adds_ a number to the spoilage amount.
  The container can be the player's backpack, the player's toolbar, or a loot container.
  All three of these locations have their values set _globally_ in XML.
* If the item stack is located in a loot container, that container can offset the global amount
  of spoilage that is added for all loot containers.
  This is called the _preserve bonus_ of the container.
  The preserve bonus is _subtracted_ from the spoilage amount.

Once that amount is calculated, it is compared against the total amount of spoilage that can occur
before _one_ consumable item spoils.
That total amount of spoilage is called the _maximum spoilage._

This process happens until the calculated spoilage amount is less than the maximum spoilage:
* One consumable item is removed from the item stack.
* A _spoiled item,_ representing a spoiled version of the original item, is created.
  The spoiled item can be specified in the XML for each consumable item,
  or defaults to rotten meat.
  If this is set to the consumable item itself, then the consumable item spoils into nothing.
* The spoiled item is placed in _the player's backpack,_ creating a new stack if necessary.
  (It goes into the player's backpack regardless of which container held the original item.)
  If there is no room in the player's backpack, the spoiled item is dropped on the ground.
* The maximum spoilage is subtracted from the calculated spoilage amount.

Here are the details about configuring all this in the XML files.

#### Feature configuration block XML
SphereII created a block in `blocks.xml` that is designed to hold feature configuration data
for the entire Core modlet.
Each property node in that block is devoted to one feature, and that node has child property nodes
that hold the configuration data.

The property node that deals with food spoilage has the "FoodSpoilage" class.
These are the names of child property nodes and the features they configure:

* "Logging": Enables or disables verbose logging.
* "FoodSpoilage": Enables or disables food spoilage in the game.
* "Toolbelt": The amount of spoilage to add, per loss calculation,
  when consumable item stacks are located in the player's toolbelt.
* "Backpack": The amount of spoilage to add, per loss calculation,
  when consumable item stacks are located in the player's backpack.
* "Container": The amount of spoilage to add, per loss calculation,
  when consumable item stacks are located in loot containers.
* "MinimumSpoilage": The absolute minimum spoilage per loss calculation.
* "TickPerLoss": The default ticks per loss.
* "SpoiledItem": The default item that consumable items turn into when they spoil.

#### Consumable item XML
These can be added as new properties to each item in `items.xml` that is consumable.

* "Spoilable": Enables or disables food spoilage on this item.
* "ShowQuality": Whether to _only_ show the quality bar underneath the item.
* "QualityTierColor": The tier number for the color of the quality bar underneath the item.
  This can be an integer from 0 through 7.
* "SpoiledItem": The item that this consumable item turns into when it spoils.
* "TickPerLoss": The ticks per loss for this consumable item.
* "SpoilageMax": The maximum spoilage before one consumable item spoils.
* "SpoilagePerTick": The spoilage "per tick" (actually, per loss).

#### Container block XML
This can be added as a new property to each container in `blocks.xml`, as desired.

* "PreserveBonus": The preservation bonus, per loss calculation, for this container.

### Hints for customizing

I recommend that you keep `SpoilageMax` and `SpoilagePerTick` at their default values of
1000 and 1, respectively.
This makes the math easier.

If `SpoilageMax` is 1000 and `SpoilagePerTick` is 1, then `TickPerLoss` should be the number of
in-game _hours_ that it takes for _one_ item to spoil in ideal conditions.
(The math works out because there are 1000 ticks per in-game hour.)

If we want `n` items to spoil in those in-game hours, set `SpoilagePerTick` to `n`
(but don't adjust `TickPerLoss`).

There will be an extra number of items spoiled according to the stack location,
and the container block's `PreserveBonus` value (if any).
For instance, if the stack is in a "Container" (set to 4 in the config block),
and the container block has a `PreserveBonus` of 1,
an extra 3 items will spoil in that number of in-game hours.
(The spoilage will not occur until the next loss tick, as determined by `TickPerLoss`.)

This actually makes some fairly complicated things possible.
For example, let's say that you don't want corn bread to be affected very much by the container
it's in.
But you still want as many items to spoil as any other food - say, 3 times per week.

In this case, you can use a longer number of in-game hours to represent the time it takes for
one piece of cornbread to spoil - say, 4 weeks.
To make up for it, you set the `SpoilagePerTick` to 12, so it still averages out to 3 per week.

Even if an extra 4 pieces of cornbread spoil because it's in a container with no `PreserveBonus`
value, those 4 pieces are spread out over 4 weeks, so they're much less noticable to players.

On the other hand, if you want meat stew to be _very_ affected by the container it's in,
use a short number of in-game hours - say, 2 days - and set `SpoilagePerTick` to 1.
If it's in a container with no `PreserveBonus` value, an extra 4 bowls of stew will spoil during
those 2 days.
But if it's in a container with a `PreserveBonus` value of 2, only 2 extra bowls of stew will
spoil during those same two in-game days.
