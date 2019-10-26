aXUi_Dialog 
===========

The SphereII NPC Dialog Windows provides a sample XUI from Sirillion to allow the NPCs to have a different dialog display format, including a sample NPC dialog with a quest chain.

npc.xml
--------------

This XML defines the dialog options for an NPC. 

~~~~~~~~~~~~~~~{.xml}
<configs>
  <append xpath="/npc">
    <npc_info
    id="spheretest"
    name="NPC"
    name_key="npc_traitorJoel"
    faction="whiteriver"
    portrait="npc_joel"
    greeting_type="nearby"
    stance="Like" voice_set="trader"
    trader_id="1" dialog_id="GenericNPCWithQuest"
    quest_list="npcqest" />
  </append>
</configs>
~~~~~~~~~~~~~~~

The id="spheretest" would be used as the NPCID on the entity class, and links the entity class to its NPC hooks.

~~~~~~~~~~~~~~~{.xml}
<property name="NPCID" value="spheretest"/>
~~~~~~~~~~~~~~~

the dialog-id, "GenericNPC" makes a reference to the dialog.xml file, and brings up the chat and interaction window.

~~~~~~~~~~~~~~~{.xml}
   <!-- Generic Dialog with Quest options (if quests are available -->
    <dialog id="GenericNPCWithQuest" startstatementid="start">
      <statement id="start" text="dialog_trader_statement_start">
        <response_entry id="FollowMe" />   
        <response_entry id="jobshave" />
        <response_entry id="ShowMe" />
        <response_entry id="StayHere" />
        <response_entry id="GuardHere" />
        <response_entry id="Wander" />
        <response_entry id="SetPatrol" />
        <response_entry id="Patrol" />
        <!-- response_entry id="Loot" / -->
        <response_entry id="Hire" />
        <response_entry id="done" />
      </statement>
~~~~~~~~~~~~~~~

If the NPC can give a quest or multiple quests, a quest_list needs to be added to the quests.xml, along with whatever quests you want to provide it. For example, you may want to create a quest line that a single NPC
gives you. Once you complete all the requirements, the NPC may join you as a follower.


~~~~~~~~~~~~~~~{.xml}
    <quest_list id="npcqest">
      <quest id="test_fetch" />
    </quest_list>
~~~~~~~~~~~~~~~
