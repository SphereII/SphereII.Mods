

### Start A Fire

This class defines a challenge where the player must **start a specific number of fires**. It tracks when the player starts fires and updates the challenge’s progress accordingly.

In an XML configuration file, the challenge might be set up like this:

```xml
<objective type="StartFire, SCore" count="20" />
```

This configuration defines a challenge where the player must **start 20 fires** to complete the objective.

---

### Block Destroyed By Fire

Tracks and manages the player's progress in a challenge where they need to destroy a certain number of blocks using fire.

In an XML configuration file, the challenge might be set up like this:

```xml
<objective type="BlockDestroyedByFire, SCore" count="20" />
```

This configuration defines a challenge where the player must destroy 20 blocks using fire.

---

### Extinguish Fire

Tracks and manages the player's progress in a challenge where they need to extinguish a certain number of fires.


In an XML configuration file, the challenge might be set up like this:

```xml
<objective type="ExtinguishFire, SCore" count="20" />
```
This configuration defines a challenge where the player must extinguish 20 fires.


### Start Big Fire 

Tracks and manages the player's progress in a challenge where they need to sustain or reach a large fire size or intensity, encouraging the player to create significant fires.

In an XML configuration file, the challenge might be set up like this:

```xml
<objective type="BigFire, SCore" count="20" />
```

This configuration defines a challenge where the player must sustain or create a "big fire" with a size or intensity of 20.