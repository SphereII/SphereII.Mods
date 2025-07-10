The `AutoRedeemChallenges` feature streamlines gameplay by automatically redeeming completed challenges without
requiring manual player intervention. This ensures players receive their rewards immediately upon finishing a challenge
objective.

## Functionality

This feature is implemented through a Harmony patch on the game's challenge system. When enabled, instead of challenges
remaining in a "completed" state for manual collection, they are automatically processed, and rewards are distributed to
the player as soon as the challenge objectives are met.

## Configuration

The `AutoRedeemChallenges` feature is controlled by a property within the `AdvancedPlayerFeatures` class in your
`blocks.xml` file:

```xml

<property class="AdvancedPlayerFeatures">
    <property name="AutoRedeemChallenges" value="false"/>
</property>
```

**Explanation**:

* **`AutoRedeemChallenges`**: Setting this property's `value` to `true` will enable the feature, causing challenges to
  be automatically redeemed upon completion. If set to `false`, players will need to manually redeem challenges as in
  the vanilla game.

---

The Encumbrance feature introduces a weight-based system that affects a player's movement and performance based on the
total weight of items in their inventory. This adds a layer of realism and strategic inventory management to the game.

## Functionality

When enabled, the game calculates the total weight of items carried by the player. If this total weight exceeds a
defined `MaxEncumbrance` threshold, penalties are incurred, such as reduced movement speed or other impairments. The
system also supports including items in the toolbelt and equipped items in the weight calculation.

* **Patch Target**: `PlayerMoveController.Update` (implied by `Encumbrance.cs` location and function)
* **Purpose**: This patch modifies how player movement is controlled, applying encumbrance penalties if the player's
  carried weight exceeds configured limits.

## Configuration

The Encumbrance feature is controlled by several properties within the `AdvancedPlayerFeatures` class in your
`blocks.xml` file:

```xml

<property class="AdvancedPlayerFeatures">
    <property name="Encumbrance" value="false"/>

    <property name="MaxEncumbrance" value="10000"/>

    <property name="EncumbranceCVar" value="encumbranceCVar"/>

    <property name="Encumbrance_ToolBelt" value="false"/>

    <property name="Encumbrance_Equipment" value="false"/>

    <property name="MinimumWeight" value="0.1"/>
</property>
```

**Explanation**:

* **`Encumbrance`**: `false` - Globally enables or disables the item weight encumbrance system for the player's
  inventory bag.
* **`MaxEncumbrance`**: `10000` - Defines the maximum weight the player can carry before encumbrance penalties begin to
  apply. This value can be overridden by a player CVar with the same name.
* **`EncumbranceCVar`**: `encumbranceCVar` - Specifies the name of a CVar that will store the player's current
  encumbrance as a percentage (e.g., `1.0` for max encumbrance, `1.5` for 50% over).
* **`Encumbrance_ToolBelt`**: `false` - If `true`, the weight of items in the player's toolbelt will be included in the
  total encumbrance calculation.
* **`Encumbrance_Equipment`**: `false` - If `true`, the weight of items currently equipped by the player will be
  included in the total encumbrance calculation.
* **`MinimumWeight`**: `0.1` - Assigns a default weight to any item that does not have a specific `ItemWeight` property
  defined in its XML.

---

The `NerdPoll.cs` patch implements the "Anti-Nerd Pole" feature, which aims to prevent players from using an exploit (
commonly known as "nerd-poling") to rapidly ascend by jumping and placing blocks directly beneath themselves.

## Functionality

This feature is implemented through a Harmony patch on the game's block placement logic. When the "Anti-Nerd Pole"
setting is enabled, the patch modifies the conditions under which a block can be placed, specifically preventing it when
the player is attempting to place a block while jumping directly underneath themselves. This ensures a more balanced and
intended vertical traversal experience.

* **Patch Target**: (Implicit, related to `Block.canPlaceBlockAt` in a jumping context, similar to
  `BlockCanPlaceBlockAt.cs`)
* **Purpose**: To prevent players from "nerd-poling" by blocking block placement when jumping directly upwards and
  underneath the player.

## Configuration

The "Anti-Nerd Pole" feature, which `NerdPoll.cs` helps implement, is controlled by a property within the
`AdvancedPlayerFeatures` class in your `blocks.xml` file:

```xml

<property class="AdvancedPlayerFeatures">
    <property name="AntiNerdPole" value="false"/>
</property>
```

**Explanation**:

* **`AntiNerdPole`**: Setting this property's `value` to `true` will enable the "Anti-Nerd Pole" feature, which this
  patch contributes to. If `false`, the "nerd-poling" technique may be possible.

---

The `OneBlockCrouch.cs` patch enables players to fit through spaces that are only one block high when crouching, adding
a layer of realism and movement flexibility to the game.

## Functionality

This feature is implemented through a Harmony patch that modifies the player's collision or movement logic when
crouching. When enabled, the game adjusts the player's crouching physics, allowing them to pass through narrow,
one-block-high gaps that would normally be impassable, providing new tactical and exploration opportunities.

* **Patch Target**: (Implicitly, methods related to player collision or height while crouching)
* **Purpose**: To allow players to fit through one-block-high spaces when crouching.

## Configuration

The "One-Block Crouch" feature is controlled by properties within the `AdvancedPlayerFeatures` class in your
`blocks.xml` file:

```xml

<property class="AdvancedPlayerFeatures">
    <property name="OneBlockCrouch" value="false"/>
    <property name="PhysicsCrouchHeightModifier" value="0.49"/>
</property>
```

**Explanation**:

* **`OneBlockCrouch`**: `false` - Setting this property's `value` to `true` enables the one-block crouch functionality.
* **`PhysicsCrouchHeightModifier`**: `0.49` - This float value allows for fine-tuning the player's crouch height in
  relation to physics, influencing how narrowly they can fit through spaces.

  ---

The `SoftHands.cs` patch implements the "Soft Hands" feature, which introduces a mechanic where players take damage if
they hit certain objects with their bare hands. This encourages the use of appropriate tools or weapons for tasks like
breaking blocks or attacking, adding a layer of realism and strategic choice.

## Functionality

This feature is implemented through a Harmony patch that modifies the game's interaction or damage-dealing logic when a
player uses their bare hands. When enabled, if a player's bare-handed action (e.g., punching a block) does normally not
cause self-damage, this patch will apply a configurable amount of damage to the player, simulating the impact on their
unprotected hands.

* **Patch Target**: (Implicit, related to player bare-handed attacks or block interactions)
* **Purpose**: To damage the player if they hit something with their bare hands.

## Configuration

The "Soft Hands" feature is controlled by a property within the `AdvancedPlayerFeatures` class in your `blocks.xml`
file:

```xml

<property class="AdvancedPlayerFeatures">
    <property name="SoftHands" value="false"/>
</property>
```

**Explanation**:

* **`SoftHands`**: `false` - Setting this property's `value` to `true` will enable the "Soft Hands" feature, causing
  players to take damage when hitting objects with bare hands.
