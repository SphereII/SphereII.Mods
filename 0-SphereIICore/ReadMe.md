0-SphereIICore
==============

The 0-SphereIICore is the key component to enable extra functionality for 7 Days To Die. Using a mixture of Harmony, SDX Patch Scripts, and Scripts, new features are enabled for modders and players to use.

The name convention of this mod is to ensure that it's one of the first mods to be loaded, thus satisfying reqreuiments for other modlets. It should be one of the first, if not the first, mod in the listing.

Config
------
Config contains XPath XML files to add critical settings, including the main Configuration Block.

Harmony
-------
Harmony patches are applied through a hook that the DMT Viewer tool installs into the game. The 0-SphereIICore contains a Harmony folder, where various Harmony scripts are stored. These Harmony Scripts are stored in a folder structure to help identify and navigate what area they affect.

Harmony Patches are initialized by the Harmony/HarmonyInit.cs script. This will initialize all harmony scripts in the folder; No other start is required.

Patches
-------
Patches are provided by writing SDX-style Patch Scripts, which inject changes into the DLL. 

Scripts
-------
Scripts contain classes which extent 7 Days To Die base classes, as well as add new utility classes to help with repetitive tasks.

