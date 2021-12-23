Sample Project

This project and its sub folders are meant to be copied to your Mods folder of your game.

	7DaysToDie_Data\
	Data\
	Mods\SampleProject\
	Mods\SampleProject\SampleProject.dll
	Mods\SampleProject.sln


Creating A New Solution
-----------------------

	1) Install the latest version of Visual Studio 2019 Community
		-> Install Game development with Unity ( for the Visual Studio Tools for Unity )
	2) Open Visual Studio and Create New project
	3) Select ""Class Library (.NET Framework )"" as the project Template.
	4) Configure the Project:
		Project Name: Your Modlet name
		Location: C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods
		Add Checkmark in Place Solution and project in the same directory
			( Otherwise it'll create Mods\SampleProject\SampleProject\)
		Framework:  .NET Framework 4.5



Project Setup
-------------

References:
	You may add new References by right clicking on References, and pathing to your 7 Days To Die folder. By default, Visual studio sets up references using relative path.

	After you add a new reference, click on the newly added Reference and change Copy Local to False.  This will prevent Visual Studio from copying the reference files to your mod folder.

Folders:

	The root of your Visual Studio Project is the contents of your Modlet. You may right click on the Project and select Add New Item and add in new folders, such as Config, and add in an XML file called ModInfo.xml

	For my projects, I've chosen to keep a similar folder structure as DMT, using a Harmony folder for Harmony Patches and Scripts for C# classes. This is more to do with organization and clean up, keeping the modlet's folder relatively clean.





Right click on the Project name in the Solution Explorer, and go to Properties.

[ Application ]
	In the Application tab, you can change the Assembly Name. This is the name you will use in your XML references  ( MyBlock, <AssmeblyName>). By default, this is the name of the project, and should be fine for most cases.

	Target Framework:  The Frameworkd should be .NET Framework 4.5

	Output Type: Class

[ Build ]

	In the Output section, replace the default  bin\Debug to .\   This will make Visual Studio compile and place the DLL in the root of the Project, inside your Modlet.

		Build started...
		1>------ Build started: Project: SampleProject, Configuration: Debug Any CPU ------
		1>  SampleProject -> C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods\SampleProject\SampleProject.dll

	To get Debug information, click on the Build Tab, and click on Advanced. In the Debugging Information, select Portable. This will create the pbd file and link it properly, showing you line numbers.

 [ Debug ]

	This is an optional step, but I find it helps me.  I select Start An External Program, and enter in the full path to the 7daystodie.exe, and set the Working Directory to its folder. 

	You can then Build Solution or Project, and click on the Start button to start the game.

	If you are using the 0-SCore, it adds in support for a command line parameter -autostart, which will launch the game and load into your last loaded game.

	By default, this is for the Debug configuration. What I normally do is set up my Debug configuration as -autostart, and do the same thing for Release configuration, but remove the -autostart.


