To enable Visual Studio 2019 Debugging:


1) Install the latest version of Visual Studio 2019 Community
	-> Install Game development with Unity ( for the Visual Studio Tools for Unity )

2) Load up your Solution, and confirm you can compile.
3) Right click on the Project, and then blick on Build.
	-> Define DEBUG constant checked
	-> Defined TRACE constant checked
	-> Click on Advanced (at the bottom of that window)
		-> Debugging information set to Portable
4) Rebuild Solution, and confirm pdb files along with the DLL files.
5) Copy the contents of this folder over into your game folder, over-writing as necessary.
6) Double click on 7daystodie.exe.
7) In the pop up, "You can attach a native debugger now if you want.". Click on OK.

Two Options here:
	8) In Visual Studio, go to Debug -> Attach Unity Debugger
		-> Select the game instance,and click on OK.
			NOTE: The project column should be populated, even if it's <no name>. If it's blank, then check step 7).
	
	8) In dnSpy, go to Debug -> Attach To Process ( Unity )
		-> Select the game instance,and click on OK.
			NOTE: The project column should be populated, even if it's <no name>. If it's blank, then check step 7).
			
Debugging should now be connected. Break points should function.


The population of this folder was done as follows. This will need to be done each time the game version updates.

1) Download the correct version of Unity for the game version.
2) Go to the Install target, then look for the files under here:  Editor\Data\PlaybackEngines\windowsstandalonesupport\Variations\win64_development_mono\
3) Copy all those files, replacing the ones here. You may delete the Mono folder itself.
4) The Data folder must be renamed to 7DaysToDie_Data
5) Rename the UnityPlayer.exe to 7DaysToDie.exe

	