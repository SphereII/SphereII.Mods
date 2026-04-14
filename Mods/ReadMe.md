# SphereII Mods

The mods contained in this repo have been made available for anyone to use with 7 Days To Die. They are pre-built, but also come with all sln and project files to build yourself.

* All Mods here are freely available to re-use, re-implemented, or disassembled and re-used in your own mods.*

* You are welcome to use individual snippets as needed *

## Overview

The mods contained within are meant to be used by modders and players. Some mods are more complex than others, such as 0-SCore.

The primary hosting https://github.com/SphereII/SphereII.Mods  

An automatic mirror is hosted on gitlab, to allow for ease-of-use for direct links.

## Definitions

Platform 		| Supported			| Definition
:------------ 	| :-------------	| :-------------	
Server 			| &check;			| Determines if this mod must be installed on the server.
Client 			| &check;			| Determines if this mod must be installed on the client connecting to a server.
Single-Player 	| &check;			| Determines if this mod must be installed on the client playing single player.
Server-Only 	| &cross;			| Determines if this mod must exist only on the server.
EAC-Safe	 	| &cross;			| Determines if the server, or client, must have EAC off.

Note with regards to EAC and Dedicated Servers: EAC never runs on the server itself, regardless if EAC is enabled or disabled on the server. This setting just controls if clients are required to have EAC on.


## Direct Downloads

Everything: https://github.com/SphereII/SphereII.Mods/archive/refs/heads/master.zip

### Individual Downloads:

#### 0-SCore 

This is the main mod. This mod is primarily aimed at modders who want to extend their own mod without coding in the features themselves. However, players can also use it freely. 

It contains Harmony patches, C# classes, and XML examples. It is not EAC-Safe.

The main configuration of the features is documented under Config/Blocks.xml.

Platform 		| Supported
:------------ 	| :-------------
Server 			| &check;
Client 			| &check;
Single-Player 	| &check;
Server-Only 	| &cross;
EAC-Safe	 	| &cross;
	

***

#### Bloom's Family Farming

This mod turns on features from the 0-SCore mod to enable more farming features, and provides examples. This enables crops requiring water, and allows for enhanced water pipes to allow you to run water from a water source to pipes.

Requires: 
	0-SCore

Platform 		| Supported
:------------ 	| :-------------
Server 			| &check;
Client 			| &check;
Single-Player 	| &check;
Server-Only 	| &cross;
EAC-Safe	 	| &cross;

	
***

#### SphereII A Better Life

Adds in fish into the natural water ways of the world. 

Requires: 
	0-SCore

Platform 		| Supported
:------------ 	| :-------------
Server 			| &check;
Client 			| &check;
Single-Player 	| &check;
Server-Only 	| &cross;
EAC-Safe	 	| &cross;
	

***

#### SphereII Dedicated Tweaks

Theoretical mod to help reduce memory consumption on dedicated servers. It has no impact on single-player.

Platform 		| Supported
:------------ 	| :-------------
Server 			| &cross;
Client 			| &cross;
Single-Player 	| &cross;
Server-Only 	| &check;
EAC-Safe	 	| &check;



***

#### SphereII Legacy Distant Terrain

Re-implements pre-Alpha 18 distant terrain, which may improve performance for lower end PCs. It also disables the Splat Map, which allows players to play down terrain blocks and not have them auto-switch to the current biome's terrain.

Platform 		| Supported
:------------ 	| :-------------
Server 			| &cross;
Client 			| &check;
Single-Player 	| &check;
Server-Only 	| &cross;
EAC-Safe	 	| &cross;
	

***

SphereII PG13

This mod replaces the Party girl's model with the Nurse's model. This is just a visual change.

Platform 		| Supported
:------------ 	| :-------------
Server 			| &check;
Client 			| &cross;
Single-Player 	| &check;
Server-Only 	| &check;
EAC-Safe	 	| &check;


***

SphereII Peace Of Mind

This mod replaces a few items in the game that may be a trigger event, such as ropes and nooses. Stay healthy.

Platform 		| Supported
:------------ 	| :-------------
Server 			| &check;
Client 			| &cross;
Single-Player 	| &check;
Server-Only 	| &check;
EAC-Safe	 	| &check;


***

SphereII Skip Tutorial Quests

Skips the starting quests chain, giving you the starting skill points, and the quest to find a trader.

Platform 		| Supported
:------------ 	| :-------------
Server 			| &check;
Client 			| &cross;
Single-Player 	| &check;
Server-Only 	| &check;
EAC-Safe	 	| &check;


***

SphereII Take And Replace

Allows certain blocks to be picked up, such as removing boards from a window.

Platform 		| Supported
:------------ 	| :-------------
Server 			| &check;
Client 			| &check;
Single-Player 	| &check;
Server-Only 	| &cross;
EAC-Safe	 	| &cross;


***

SphereII Music Boxes

Adds in support for Audio / Video players, using provided items as CDs and DVDs.

Platform 		| Supported
:------------ 	| :-------------
Server 			| &check;
Client 			| &check;
Single-Player 	| &check;
Server-Only 	| &cross;
EAC-Safe	 	| &cross;



***

## Changelog

### 2.6.31.1401

#### 0-SCore

**Farming: NPC Farmer Water Range Fix**

NPC farmers were only checking immediate neighbors (±1 block) when determining if a farm plot had water access, while vanilla crops use a 4-block horizontal radius. Outer plots that were valid for vanilla crop growth were ignored by the farmer AI until the player manually interacted with them. `FarmPlotData.HasWater()` now includes the same 4-block horizontal fallback scan used by vanilla, plus a one-block-down check for each position.

**Farming: Double Water Consumption Fix for Multi-Block Plants**

Two-block-tall plants (e.g. coffee) register each block independently in `CropManager`. Because `UpdateTick` and `CheckPlantAlive` ran on every block, these plants consumed water twice per growth tick. A new `IsRootBlock()` helper identifies the lowest block of a plant stack (the block beneath it is not also a `BlockPlantGrowingSDX`). Water checks and consumption now only run on the root block.

**Farming: FarmHere / RecallFarmer NPC Commands**

Two new `ExecuteCommandSDX` actions for hired NPC farmers:

- **`FarmHere`** — Removes the NPC from the player's active party (no teleport, no `hired_X` stamp) but stores the player's entity ID in a `FarmOwnerEntityId` cvar so the NPC retains land-claim access for the farming task. The NPC stays at its current position in `Stay` order.
- **`RecallFarmer`** — Restores the NPC to the player's active party. Only the player whose entity ID matches `FarmOwnerEntityId` can execute this; other players are silently blocked at both the dialog-requirement and code layers.

The `FollowMe` command also gains an ownership guard: if an NPC is in farm mode, only its registered owner can follow-request it.

`UAITaskFarmingV4` resolves land-claim player data from `FarmOwnerEntityId` when `Leader`/`Owner` are both zero, so the NPC continues tending the farm correctly after being released from the party.

Example dialog XML:

```xml
<!-- Assign NPC to farm the area. Shown to the current leader only, when not already in farm mode. -->
<response id="farm_here" text="farm_here_key" ref_text="Stay here and tend the farm." nextstatementid="farm_mode_active">
    <requirement type="Leader, SCore" requirementtype="Hide" />
    <requirement type="IsFarmOwner, SCore" requirementtype="Hide" value="not" />
    <action type="ExecuteCommandSDX, SCore" id="FarmHere" />
</response>

<!-- Recall NPC back to active follow. Shown only to the registered farm owner. -->
<response id="recall_farmer" text="recall_farmer_key" ref_text="Come follow me again." nextstatementid="recalled">
    <requirement type="IsFarmOwner, SCore" requirementtype="Hide" />
    <action type="ExecuteCommandSDX, SCore" id="RecallFarmer" />
</response>
```

**New Dialog Requirement: `IsFarmOwner`**

Checks if the talking player is the registered farm owner of the current NPC. Passes when `FarmOwnerEntityId` on the NPC matches the player's entity ID. With `value="not"`, passes when the NPC is *not* in farm mode.

```xml
<!-- Only the farm owner sees this option -->
<requirement type="IsFarmOwner, SCore" requirementtype="Hide" />

<!-- Only shown when NPC is NOT in farm mode -->
<requirement type="IsFarmOwner, SCore" requirementtype="Hide" value="not" />
```

**Updated Dialog Requirement: `HiredSDX`**

`HiredSDX` now also returns true when an NPC is in FarmHere mode (`FarmOwnerEntityId > 0`), treating that state as hired. This prevents the Hire button from re-appearing on farm-mode NPCs.

**New Console Command: `scorefarming` / `scf`**

Debug and validation command for the farming system. All output is mirrored to both the console and the log file.

Subcommands:
- `scf count` — Number of registered farm plots and crop plants.
- `scf validate` — Prunes stale entries whose blocks no longer exist in the world.
- `scf listplots [range]` — Lists registered farm plots (optionally within range of the player) and reports the crop block above each.
- `scf listcrops [range]` — Lists registered crop plants (optionally within range of the player) and reports the farm plot below each.

#### Bloom's Family Farming

Added `FarmHere` and `RecallFarmer` dialog options to the Frankie Farmer NPC:

- **Stay here and tend the farm** — visible to Frankie's current leader when not already in farm mode; sends Frankie into farm mode.
- **Come follow me again** — visible only to the player who originally assigned Frankie to farm mode; recalls him to active follow.

New localization keys: `Frankie_farm_here`, `Frankie_farm_mode_active`, `Frankie_recall_farmer`, `Frankie_farm_recalled`.
