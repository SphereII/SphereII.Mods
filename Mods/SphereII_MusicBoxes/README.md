SphereII and Xyth's Music Carousel and Video Player
===================================================

Created for the Winter Project A17, the Music Carousel and Video Players add in three additional blocks, with the ability to play music and videos.

How To Find:
============
By default, these blocks are only available through a Tier 1 Quest line from a Trader. The CDs and DVDs are found in loot around the world. 

Usage:
======

They use a unified SDX Class:  		  
	<property name="Class" value="MusicBox, Mods"/> 

A Music Player will contain an AudioSource on its prefab, that is set to Loop. An optional SoundDataNode reference is recommended to point to a default play list.  It will play any item with the following property:

	<!-- This points to the sounds.xml's christmasmusic02 SoundDataNode -->
	<property name="SoundDataNode" value="christmasmusic02" />


A Video Player only needs it's Class set to work. It will play any item with the following property:

	<!-- This points to the Videos.unity3d file's zantaclaws.mp4 file -->
	<property name="VideoSource" value="#@modfolder:Resources/Videos.unity3d?zantaclaws" />


What They Are:
==============
Animated Carousel:
	This animated Carousel plays Christmas music by default. Two lootable CDs exists in the lootgroup, which when added to the Carousel's loot container, can change the music being played.
	
	The item's contains a SoundDataNode, which the music box group will search for, and play the corresponding sounds.xml entry. If multiple AudioClip are in the SoundDataNode, the music box will randomly pick the next AudioClip when the current one is finished.
	
	If the Carousel contains more than one CD (identified by the SoundDataNode), then all music will be available to be randomly selected.
	
		<block name="Carousel">
			<property name="Class" value="MusicBox, Mods"/> <!-- Music Box Class -->
			<!-- SoundDataNode reference: Default Sound that plays without a CD -->
			<property name="SoundDataNode" value="christmasmusic01" />

			<!-- The prefab where the AudioSource and Animator exists -->
			<!-- Example: #@modfolder:Resources/Carousel.unity3d?Carousel_X -->
			<property name="Model" value="#@modfolder:Resources/XmasCarousel.unity3d?Carousel_X" />
			
			<!-- An AudioSource attached to the unity object: Must be set to Loop! -->
			<property name="AudioSource" value="CarouselNoMesh_X" />
			<!-- snip -->
		</block>  
	
		<item name="audioCD01">
			<property name="Extends" value="casinoCoin"/>
			<property name="Meshfile" value="#@modfolder:Resources/MediaCases.unity3d?xmascd" /> 
			<property name="HandMeshfile" value="#@modfolder:Resources/MediaCases.unity3d?xmascd"/> 
			<property name="HoldType" value="21"/> 
			<property name="CustomIcon" value="CD" />
			<property name="Material" value="Mplastics"/>
			<property name="Stacknumber" value="1"/>
			<!-- Sound Data node is tied to the Carousol -->
			<property name="SoundDataNode" value="christmasmusic02" />
		</item>

		<SoundDataNode name="christmasmusic02">
			<!-- AudioSource.  -->
			<AudioSource name="#@modfolder:Resources/XmasCarousel.unity3d?CarouselNoMesh_X" />
			<Noise ID="2" range="7" volume="15" time="3" muffled_when_crouched="0.5" />
			<!-- Audio Clips available from https://www.singing-bell.com/our-free-songs/list-of-free-christmas-carols-mp3/ -->
			<AudioClip ClipName="#@modfolder:Resources/singing-bell-com.unity3d?09_Jingle-Bells-Singing-Bell" />
			<AudioClip ClipName="#@modfolder:Resources/singing-bell-com.unity3d?09_Little-drummer-boy-Singin-Bell" />
			<AudioClip ClipName="#@modfolder:Resources/singing-bell-com.unity3d?10_Adeste-fideles-Singing-Bell" />
			<AudioClip ClipName="#@modfolder:Resources/singing-bell-com.unity3d?10_Oh-Christmas-tree-Singing-Bell" />
			<AudioClip ClipName="#@modfolder:Resources/singing-bell-com.unity3d?11_Carol-of-the-Bells-Singing-Bell" />
			<AudioClip ClipName="#@modfolder:Resources/singing-bell-com.unity3d?11_Santa-Claus-is-coming-to-town-Singing-Bell" />
			<AudioClip ClipName="#@modfolder:Resources/singing-bell-com.unity3d?12_First-Noel-Singing-Bell" />
			<AudioClip ClipName="#@modfolder:Resources/singing-bell-com.unity3d?12_Rudolph-the-Red-Nosed-Reindeer-Singing-Bell" />
			<AudioClip ClipName="#@modfolder:Resources/singing-bell-com.unity3d?09_12-Days-of-Christmas-Singing-Bell" />
			<AudioClip ClipName="#@modfolder:Resources/singing-bell-com.unity3d?09_Rockin-around-the-Christms-tree-Singing-Bell" />
			<AudioClip ClipName="#@modfolder:Resources/singing-bell-com.unity3d?10_Hark-the-Herald-Angels-sing-in-G-Singing-Bell" />
			<AudioClip ClipName="#@modfolder:Resources/singing-bell-com.unity3d?12_Winter-Wonderland-Singing-Bell" />
			<LocalCrouchVolumeScale value="0.5" />
			<CrouchNoiseScale value="1" />
			<NoiseScale value="1" />
			<MaxVoices value="10" />
			<MaxRepeatRate value="0.001" />
		</SoundDataNode>
		

Video Players:
	Two video players, of different sizes, are available. They behave like the Animated Carousel, with a 6-slot loot container, with the noted exception that they will play DVD items.
	
	A DVD item has a VideoSource, which contains a reference to a unity bundle and the prefab. Like the Carousel, it will randomly pick a DVD from its slots. However, unlike the Carousol, only one video per DVD is supported.

		<block name="VideoPlayer">
			<property name="Class" value="MusicBox, Mods"/>  <!-- Music Box Class -->
			<!-- Video Player Model by TechnoCraft is licensed under CC Attribution -->
			<property name="Model" value="#@modfolder:Resources/VideoPlayers.unity3d?VideoPlayer_X" />
			<!-- snip -->
		</block>
		
		<!-- DVD examples. The VideoSource references an mp4 file in an asset bundle-->
		<item name="dvd01">
			<property name="Extends" value="casinoCoin"/>
			<property name="Meshfile" value="#@modfolder:Resources/MediaCases.unity3d?DVDCase01_X" /> 
			<property name="HandMeshfile" value="#@modfolder:Resources/MediaCases.unity3d?DVDCase01_X"/> 
			<property name="HoldType" value="21"/> 
			<property name="CustomIcon" value="DVD" />
			<property name="Material" value="Mplastics"/>
			<property name="Stacknumber" value="1"/>
			
			<!-- In the MyVideos.unity3d bundle, it expects  Sample320.mp4 -->
			<property name="VideoSource" value="#@modfolder:Resources/Videos.unity3d?spunky" />
		</item>

