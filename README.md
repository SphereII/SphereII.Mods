
===============================================================================
Download:  
	Git Clone:  https://github.com/SphereII/SphereII.Mods/
	Zip Download: https://github.com/SphereII/SphereII.Mods/archive/master.zip
	
Required Tool:
	DMT: https://7daystodie.com/forums/showthread.php?117235-DMT-Modding-Tool
===============================================================================

This mod is not EAC-safe, and EAC must be turned off.

	
0-SphereIICore
==============

A new mod pack called 0-SphereIICore is now available for use, either in standalone or inclusion to any other mod package, without any limitation.

This pack contains a large variety of Scripts, PatchScripts, and Harmony scripts that adds to and enhances vanilla features. 

Key Features:
	- AnimationSDX: Provides animation hooks for custom entities
	- Food Spoilage System: Allow food to expire at a global rate, or fine tune individual foods to spoil at different rates. 
	- Enhanced Item Repair: Allow more complicated repair recipes
	- Item Damage / Durability:  The amount of damage an item can do is reduced as durability goes down. Repair often!
	- Anti-Nerd Pole: Disables the ability to nerd pole, or jump-and-place a blocks/block
	- Soft Hands: Your hands are soft! Player takes damage when punching things with bare hands.
	- One Block Crouch: Enhance your Agility character by allowing you to crawl through one block openings.
	- Transmogrifier: Allow random walk types and sizes for zombies to break up the monotony
	- UMA Tweaks: Small tweaks to UMA system, drastically reducing the high resolution UMAs into something more manageable.
	- Head Shot Only: Take off their heads to truly kill them.
	- Zero XP: Don't like getting experience? Turns off the ability to get a new kind of experience.
	- Disable XP Pop up:  Disables the XP icon pop up
	- No Exception Hijack: Prevents the console from popping down with Red error. Useful for when you are testing...
	- Disable Trader Protection: Turn off the invulnerability of the trader compound.
	- Quick Continue: Keep your Scroll Lock button on to automatically load up the last game you played. 
	- Custom Buffs, AI Tasks, Items, Blocks, Entity classes, and XUiC components are already active. Make the right calls in your XML to enable the functionality.
	
This is still a work in progress; Each feature is available, however may not be completely refined yet. Balancing and suggestions are welcomed.

Features can be turned on and off by adding xpath set from another modlet and changing the included "ConfigFeatureBlock" in Config/blocks.xml

Example:

[CODE]
<configs>
  <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='FoodSpoilage']/property[@name='FoodSpoilage']/@value">true</set>
  <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedPlayerFeatures']/property[@name='OneBlockCrouch']/@value">true</set>
  <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedNPCFeatures']/property[@name='MakeTraderVulnerable']/@value">true</set>
  <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedPrefabFeatures']/property[@name='DisableTraderProtection']/@value">true</set>
  <set xpath="/blocks/block[@name='ConfigFeatureBlock']/property[@class='AdvancedWorldGen']/property[@name='DisableSplatMap']/@value">true</set>
</configs>
[/CODE]

Bloom's Animal Husbandry
========================

Introducing basic animal husbandry, Bloom's Animal Husband comes with a few animals that spawn in random herds. Animals have likes and dislikes, and can be encouraged to fall you home with the right incentive.


SphereII A Better Life
======================

Fish, birds, and small animal gains are introduced in this work-in-progress mod.

SphereII Clear UI
=================

Inspired from The Walking Dad, re-worked through Harmony and UI adjustments by Sirillion, Clear UI removes all on screen elements for a truly immersive experience. [ Only a small health and stamina bar are visible in place of the compass ]

SphereII Food Spoilage
======================

This modlet enables the actual food spoilage system found in SphereII Core, with some preset global values for all non-can food items. Use this to get started with adding your own unique twist to spoilage.

SphereII Music Box
==================

This modlet enables the Music and Video player originally implemented in the Winter Project, and can play CDs and DVDs found in the world.

SphereII NPC Dialog Boxes
=========================

This modlet enables a special UI interface, designed by Sirillion, to talk with NPCs


SphereII Take And Replace
=========================

This modlet enables the Take And Replace, which allows you to pull off boards from windows and doors without brute force. Hold a hammer or a crowbar (not included) to make this go faster!

SphereII Winter Project
=======================

The Winter Project - Work In Progress

SyX Security Bots
=================

In the fight against the degradation of society, SyX Security has produced a series of counter-measures against the so-called "zombie apocalypse". Deployed in infected hot spots around the world, these counter-measures have been designed to reduce and ultimately send the zombie apocalypse into remission. 

Warning: These hot spots are extremely dangerous for the non-infected, and any survivors are strongly recommended to be evacuated before they are deployed.