# Blooms Family Farming

Farming content built on 0-SCore: farm plots, irrigation, and Farmer Frankie, an NPC who plants and
harvests your plots. Requires 0-SCore. The Frankie NPCs are only added when 0-XNPCCore is loaded.

### Farmer Frankie

| Entity class | Format | Dialog |
| ----------- | ----------- | ----------- |
| `MyFarmer` | Legacy NPC (`npcAdvancedEmptyHandTemplate`, Frankie's own mesh) | `FrankieFarmerDialog` |
| `MyFarmerV4` | NPCSdcsCharacters (`npcHumanoidPlayerCtrl`, Baker mesh from 0-XNPCCore) | `FrankieFarmerDialogV4` |

To have Frankie farm: hire him, then pick **Command Menu > Stay here and tend the farm** next to your
plots. He stays on that spot and works plots in range. **Come follow me again** on his greeting
screen takes him back. Give him seeds for empty plots; planted plots need water before he tends them.
If you own a land claim he only works plots inside it.

### Change Log

Unreleased
	Needs 0-SCore 3.3.28.1024 or later.

	[ MyFarmerV4 - NPCSdcsCharacters format ]
		- Now extends npcHumanoidPlayerCtrl instead of
		  npcAdvancedEmptyHandTemplate. That base supplies the V4 entity
		  class, the player controller avatar, the physics body, hiring and
		  the order modes, so the old Class and dialogWindow properties are
		  gone.
		- Tags are the npcSdcsBase male set (sdcsnpc plus the sdcsSwap_ weapon
		  tags the weapon menu and voice swap commands check) plus notrample,
		  so Frankie does not trample crops.
		- AI packages are the npcSdcsBase stack plus the new NPCFarmingSdcs:
		  NPCModCore, SdcsNpcRangedAdvanced, NPCModNPCHired, SdcsNpcLoot,
		  NPCFarmingSdcs. SdcsNpcLeave is left out on purpose so a farmer does
		  not walk off his farm.
		- Spawns with a wooden club (BagItems and ItemsOnEnterGame) and two
		  bandages.
		- Uses its own npc_info, NPCFrankieFarmerV4, so the legacy MyFarmer
		  keeps its dialog unchanged.

	[ MyFarmerV4 - dialog ]
		- New FrankieFarmerDialogV4, extending SdcsNPC and trader. Hire, the
		  Command Menu (patrol, follow, guard, stay, loot, dismiss), Weapon
		  Menu, Admin Menu, Pickup and Show Me come from 0-XNPCCore, and shop
		  and jobs from the trader, so they stay in step with those mods.
		- "Stay here and tend the farm" is the first row of the Command Menu,
		  alongside the other order modes. "Come follow me again" stays on
		  the greeting screen, because farm mode clears Frankie's leader and
		  that hides the Command Menu. Both rows also clear the 0-XNPCCore loot
		  mode, like every other order row, and now show Frankie's reply.
		- His small talk is back as a "Who are you?" row, shown only before he
		  is hired.
		- Fits the dialog's 10 row limit: a hired Frankie talking to an admin
		  player shows at most 10 rows.

	[ Farming AI ]
		- New NPCFarmingSdcs package for MyFarmerV4: FarmingV4 at weight 3, so a
		  farm in reach outranks the Stay order's Guard (weight 2) in farm
		  mode, and Guard brings him back to his spot between plots. It does
		  not run while he is following, so a hired Frankie keeps up with you.
		- NPCFarmingGeneralV4 and NPCFarmingV4 are no longer used by any class.
		  They are left in place.

	[ Fixes ]
		- Added the missing npc_FrankieFarmer localization key ("Farmer
		  Frankie").
		- Replaced two em-dashes in comments in the legacy Frankie dialog;
		  0-XNPCCore notes they crash the game's XML loader.

	[ Related 0-SCore fixes ]
		- Farming now works without a land claim, farmers walk between plots,
		  and they no longer run off a plot after stepping up onto it. Hired
		  NPCs that fall below the world are now rescued. See the 0-SCore
		  ReadMe, version 3.3.28.1024.
