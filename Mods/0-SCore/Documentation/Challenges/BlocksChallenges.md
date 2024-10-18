### Block Destroyed

Tracks and manages the player's progress in a challenge where they must destroy specific blocks based on various conditions like block type, material, biome, or Points of Interest (POIs).

In an XML configuration file, the challenge might be set up like this:

```xml
<objective type="BlockDestroyed, SCore" count="20" block="myblock" biome="burn_forest" poi="traderJen" />
<objective type="BlockDestroyed, SCore" count="20" material="myMaterial" biome="pine_forest" poi_tags="wilderness" />
```
These configurations define challenges where the player must:

Destroy 20 blocks named myblock located in the burn_forest biome and associated with the POI traderJen.
Destroy 20 blocks made of myMaterial, located in the pine_forest biome, and associated with POIs that have the tag wilderness.

More Examples:

<objective type="BlockDestroyed, SCore" count="20" block="cntRetroFridgeVer1Closed" />
<objective type="BlockDestroyed, SCore" count="20" block="cntRetroFridgeVer1Closed" biome="burn_forest" />
<objective type="BlockDestroyed, SCore" count="20" block="cntRetroFridgeVer1Closed" biome="burn_forest" poi="traderJen" />
<objective type="BlockDestroyed, SCore" count="20" material="Mmetal" biome="pine_forest" poi_tags="wilderness" />


---

### Block Upgrades

Tracks and manages the player's progress in a challenge where they need to upgrade blocks in the game, possibly using specific items, in specific biomes, or while holding certain tools.

In an XML configuration file, the challenge might be set up like this:

```xml
<objective type="BlockUpgradeSCore,SCore" block="frameShapes:VariantHelper" count="10" held="meleeToolRepairT0StoneAxe" needed_resource="resourceWood" needed_resource_count="8" />
<objective type="BlockUpgradeSCore,SCore" block_tags="wood" count="10" held="meleeToolRepairT0StoneAxe" needed_resource="resourceWood" needed_resource_count="8" />
<objective type="BlockUpgradeSCore,SCore" block_tags="wood" count="10" biome="burnt_forest" />
```

These configurations define challenges where the player must:

Upgrade 10 specific blocks named frameShapes:VariantHelper while holding a stone axe and using resourceWood as a required material.
Upgrade 10 blocks tagged as wood with the same tool and resource requirements.
Upgrade 10 blocks tagged as wood while being in the burnt_forest biome.
