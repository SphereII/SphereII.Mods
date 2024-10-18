### Complete Quest With Stealth

This class defines a challenge where the player must complete a quest with a specified number of consecutive stealth kills. The challenge can be configured to require full quest completion in stealth, and it tracks the player's stealth kills during the quest.

In an XML configuration file, the challenge might be set up like this:

```xml
<objective type="CompleteQuestStealth, SCore" count="2"/>
<objective type="CompleteQuestStealth, SCore" count="1000"/>
```

These configurations define challenges where the player must:
- Perform **2 consecutive stealth kills** during a quest.
- Complete the **entire quest** using only stealth kills (assuming a count of 1000).

---

### Wear Equipment with Tag

This class defines a challenge where the player must wear items with specific tags, such as armor or equipment with certain properties (e.g., `armorHead`). It extends the `ChallengeObjectiveWear` class, adding support for item tags, allowing for more flexible conditions related to worn items.

In an XML configuration file, the challenge might be set up like this:

```xml
<objective type="WearTags,SCore" item_tags="armorHead"/>
```

This configuration defines a challenge where the player must **wear an item** with the `armorHead` tag to complete the challenge.

---

### Gather By Tags

This class defines a challenge where the player must gather items with specific tags. It tracks the number of items collected that match the required tags and progresses the challenge as the player gathers the correct items.

In an XML configuration file, the challenge might be set up like this:

```xml
<objective type="GatherTags, SCore" item_tags="junk" count="10"/>
```

This configuration defines a challenge where the player must **gather 10 items** that are tagged as `junk` to complete the challenge.

---

### Craft With Ingredients

This class defines a challenge where the player must craft items that include a specific ingredient. The challenge tracks the player's crafting actions and checks if the required ingredient is part of the crafting recipe.

In an XML configuration file, the challenge might be set up like this:

```xml
<objective type="CraftWithIngredient, SCore" count="2" ingredient="resourceLegendaryParts"/>
```

This configuration defines a challenge where the player must **craft 2 items** using the ingredient `resourceLegendaryParts` to complete the challenge.

---

### Enter POI

This class defines a challenge where the player must enter a specific Point of Interest (POI) or a POI with specific tags to complete the objective. The challenge tracks whether the player enters the required POI and progresses accordingly.

In an XML configuration file, the challenge might be set up like this:

```xml
<objective type="EnterPOI, SCore" prefab="traderJen" tags="trader"/>
```

This configuration defines a challenge where the player must **enter a POI** named `traderJen` and tagged as `trader` to complete the objective.

---

### Harvest

This class defines a challenge where the player must harvest specific items or blocks. It tracks items harvested in the game and checks if the player's actions meet the challenge's conditions, such as using specific tools, harvesting in certain biomes, or gathering items with specific tags.

In an XML configuration file, the challenge might be set up like this:

```xml
<objective type="Harvest, SCore" count="20" item="resourceWood" />
<objective type="Harvest, SCore" count="20" item="resourceWood" biome="burnt_forest" />
<objective type="Harvest, SCore" count="20" item_tags="woodtag" />
<objective type="Harvest, SCore" count="20" block_tags="blocktag" />
```

These configurations define challenges where the player must:
- Harvest **20 units** of `resourceWood`.
- Harvest **20 units** of `resourceWood` in the `burnt_forest` biome.
- Harvest **20 items** with the tag `woodtag`.
- Harvest **20 blocks** with the tag `blocktag`.
