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
	
https://gitlab.com/sphereii/SphereII-Mods/-/archive/master/SphereII-Mods-master.zip?path=0-SCore

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

https://gitlab.com/sphereii/SphereII-Mods/-/archive/master/SphereII-Mods-master.zip?path=Blooms%20Family%20Farming
	
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
	
https://gitlab.com/sphereii/SphereII-Mods/-/archive/master/SphereII-Mods-master.zip?path=SphereII%20A%20Better%20Life

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


https://gitlab.com/sphereii/SphereII-Mods/-/archive/master/SphereII-Mods-master.zip?path=SphereII%20Dedicated%20Tweaks

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
	
https://gitlab.com/sphereii/SphereII-Mods/-/archive/master/SphereII-Mods-master.zip?path=SphereII%20Legacy%20Distant%20Terrain

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

https://gitlab.com/sphereii/SphereII-Mods/-/archive/master/SphereII-Mods-master.zip?path=SphereII%20PG13

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

https://gitlab.com/sphereii/SphereII-Mods/-/archive/master/SphereII-Mods-master.zip?path=SphereII%20Peace%20Of%20Mind

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

https://gitlab.com/sphereii/SphereII-Mods/-/archive/master/SphereII-Mods-master.zip?path=SphereII%20Skip%20Tutorial%20Quests

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

https://gitlab.com/sphereii/SphereII-Mods/-/archive/master/SphereII-Mods-master.zip?path=SphereII%20Take%20And%20Replace

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

https://gitlab.com/sphereii/SphereII-Mods/-/archive/master/SphereII-Mods-master.zip?path=SphereII%20Music%20Boxes


***
