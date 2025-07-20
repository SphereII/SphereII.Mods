# WARNING: Proof of Concept. Not complete, or even fully tested.

# High-Level Overview: The "Learn by Doing" (LBD) Progression System

## 1. The Vision: Immersive Character Growth

This document outlines a dynamic "Learn by Doing" (LBD) progression system designed to create a more immersive and
rewarding character development experience. The core goal is to allow players to level up their attributes and perks by
actively performing related tasks, providing a natural sense of growth that complements the standard skill point system.

Instead of just spending points, players will feel their character getting stronger and more skilled simply by playing
the game in their preferred style.

## 2. The Core Architecture: The "Manager Buff" System

After exploring several designs, we perfected a centralized, modular, and highly compatible architecture built around *
*Attribute Manager Buffs**.

**The Problem:** Modifying every single item in the game to grant XP is messy, creates a massive number of XML files,
and is highly prone to conflicts with other mods.

**The Solution:** We've decoupled the LBD logic from the items entirely. The system is run by a set of permanent, hidden
buffs that are always active on the player.

The architecture consists of three main components:

* **A Single Global Initializer (`ProgressionLearnByDoing_Init`):** This one-time buff is applied to the player on first
  spawn. Its only jobs are to create all the necessary CVars for tracking XP and to apply the permanent Manager Buffs.
* **Attribute Manager Buffs (e.g., `buffLBD_PerceptionManager`):** This is the heart of the system. There is one
  permanent Manager Buff for each attribute (Perception, Strength, etc.). This buff is always "listening" for player
  actions (like attacking, harvesting, or crafting). When an action occurs, it checks the tags of the item the player is
  holding or using to determine which LBD skill to grant XP to.
* **Supporting Buffs (Cooldowns & Level-Up Handlers):** These are small, temporary buffs that handle specific tasks like
  preventing XP spam or processing a level-up when an XP threshold is met.

## 3. The Gameplay Loop in Action

From the player's perspective, the system is seamless. Here's what happens under the hood:

1. **Player Performs an Action:** A player hits an animal with a spear.
2. **The Manager Buff Reacts:** The `buffLBD_PerceptionManager` detects the `onSelfDamagedOther` event.
3. **Requirements are Checked:** It checks the held item's tags (`perkJavelinMaster`) and the target's tags (`animal`).
4. **XP is Granted:** If the requirements are met, it adds a small amount of XP to a CVar (e.g.,
   `$perkjavelinmaster_lbd_xp`).
5. **Level Up Condition is Met:** The Manager Buff sees that the XP CVar has now reached its goal.
6. **Processing the Level Up:** It applies a temporary `LevelUpCheck` buff. This buff contains the final logic: it uses
   a custom requirement to validate that the perk is not locked by attribute requirements, then calls the vanilla
   `AddProgressionLevel` action to level up the player's *actual* `perkJavelinMaster` perk.
7. **The Goalposts Move:** The `LevelUpCheck` buff then subtracts the XP cost and calculates the new, higher XP
   requirement for the next level.

## 4. Key Design Decisions & Rationale (The "Why")

We made several critical design decisions to ensure the system is robust, balanced, and professional.

* **Why a Centralized Manager Buff?**
  To eliminate conflicts and keep the system clean. By not editing `items.xml`, our mod is dramatically more compatible
  with other mods and easier to manage.

* **Why Custom SCore Requirements?**
  Because vanilla tools were insufficient. To create a truly smart system, we needed custom tools like
  `RequirementIsProgressionLocked` and `RequirementBlockHasHarvestTags`. This allows our XML to directly ask the game
  engine complex questions, resulting in cleaner and more powerful logic than any workaround could provide.

* **Why an Exploit-Proof System for Traps?**
  The `perkInfiltrator` presented a unique challenge: how to reward placing a mine without letting players pick it up
  and place it again for infinite XP. Our final "Placement Credit" system, which uses a CVar to track a player's "bank"
  of placeable mines, is a lightweight, elegant, and completely exploit-proof solution that fairly rewards both crafting
  and looting.

* **Why Layered Logic for Complex Perks?**
  For perks like `perkAnimalTracker`, we designed a multi-faceted system that rewards different aspects of the core
  activity (e.g., a standard reward for a kill, a high reward for a skillful kill, and a bonus reward for harvesting).
  This creates a deeper and more engaging progression.

* **Why Meticulous Debugging?**
  A complex system requires robust testing. By adding `LogMessage` triggers gated by a `HasBuff` check for `god` mode,
  we've built a powerful, developer-only debugging tool directly into the system. This allows for easy verification and
  balancing without affecting the experience for the end-user.

This LBD system represents a professional-grade framework for creating immersive, data-driven character progression that
feels like a natural extension of the core gameplay loop.
