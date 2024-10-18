### Kill With Item

This class defines a challenge where the player must kill specific entities (typically zombies) using certain items or items with specific tags. It extends the `ChallengeObjectiveKillByTag` class, allowing for more complex conditions like item-based kills, stealth checks, and additional localization options.

In an XML configuration file, the challenge might be set up like this:

```xml
<objective type="KillWithItem, SCore" count="2" item="gunHandgunT1Pistol" />
<objective type="KillWithItem, SCore" count="2" item_tags="handgunSkill" />
<objective type="KillWithItem, SCore" count="2" item="gunHandgunT1Pistol" entity_tags="zombie" target_name_key="xuiZombies" />
```

These configurations define challenges where the player must:

Kill 2 zombies using the gunHandgunT1Pistol.
Kill 2 zombies using any item tagged with handgunSkill.
Kill 2 zombies using the gunHandgunT1Pistol, with additional restrictions on entity tags and localized target name (xuiZombies).

---

### Decapitation

This class defines a challenge where the player must decapitate enemies (typically zombies) to complete the objective. It extends the `ChallengeObjectiveKillWithItem` class to incorporate additional logic specifically for monitoring decapitations as part of the challenge.

In an XML configuration file, the challenge might be set up like this:

```xml
<objective type="Decapitation, SCore" count="2" />
```

This configuration defines a challenge where the player must decapitate 2 entities (e.g., zombies) to complete the challenge.

---

### Stealth Kill Streak

This class defines a challenge where the player must perform consecutive stealth kills in order to complete the objective. The challenge is designed to track stealth kills and reset progress if the player fails to perform a stealth kill during the streak.

In an XML configuration file, the challenge might be set up like this:

```xml
<objective type="StealthKillStreak, SCore" count="2" cvar="longestStreakCVar" />
```
This configuration defines a challenge where the player must perform 2 consecutive stealth kills to complete the objective, and the longest streak is tracked using the CVar "longestStreakCVar".


