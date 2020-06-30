/*
* Class: MusicBox
* Author:  sphereii 
* Category: Entity
* Description:
*   This script allows the attached block to play music, videos and do animations.
*   
*   Music Player:
*       The music player will read the sounds.xml and play SoundDataNode. The music players are all loot containers; this means you can add an item that contains a "SoundDataNode" that 
*       points to an entry in the sounds.xml. Multiple music items can be added to the loot container, and it will randomly play from them. 
*       
*       Rather than items in the slots, you can also add the <property name="SoundDataNode" value="mymusic" /> to the block, which will allow it to play without an extra item.
*   
*   Animation Player:
*       When the music box is attached, it will check the Model to find an Animator at the top level. An animator SetBool is called on "IsOn" to turn it on.
*   
*   Video Player:
*       The video player works like the music player, and will play videos listed in the <property name="VideoSource" value="#@modfolder:Resources/MyVideos.unity3d?Squeeze" />.
*       This assumes that the Squeeze.mp4 video is inside of the MyVideos.unity3d bundle. As in the Music Player, multiple items can be added to the container slots to play from.
*/

using UnityEngine;
using System;
using UnityEngine.Video;

using GUI_2;
using System.Collections.Generic;
using Audio;

public class BlockMusicBox : BlockLoot
{

    // Video Source contains the asset bundle where the video is stored, if configured through the block.
    private String strVideoSource;

    // The AudioSource is a reference to the SoundDataNode's AudioSource.
    private String strAudioSource;

    // This is the SoundDataNode, if configured through the block
    private String strSoundSource;

    private float TakeDelay = 5f;
    Vector2i vLootContainerSize;

    private BlockActivationCommand[] cmds = new BlockActivationCommand[]
    {
        new BlockActivationCommand("light", "electric_switch", true),
        new BlockActivationCommand("Open", "hand", true),
        new BlockActivationCommand("take", "hand", false)
    };

    public BlockMusicBox()
    {
        this.HasTileEntity = true;
    }

    public override void Init()
    {
        base.Init();

        // SoundSource is the referenced SoundDataNode
        if (this.Properties.Values.ContainsKey("SoundDataNode"))
            this.strSoundSource = this.Properties.Values["SoundDataNode"];

        // Audio Source is the prefab name, without the asset bundle reference.
        // This is used to keep track of which audio source is actually playing, so we can turn it on and off.
        if (this.Properties.Values.ContainsKey("AudioSource"))
            this.strAudioSource = this.Properties.Values["AudioSource"];

        // The VideoSource can be an asset bundle or URL. If there is an embedded clip on the Video Player, it'll play that in preference to the URL, but
        // a video in the asset bundle will over-ride
        if (this.Properties.Values.ContainsKey("VideoSource"))
            this.strVideoSource = this.Properties.Values["VideoSource"];

        if (this.Properties.Values.ContainsKey("LootContainerSize"))
            this.vLootContainerSize = StringParsers.ParseVector2i(this.Properties.Values["LootContainerSize"], ',');
        else
            this.vLootContainerSize = new Vector2i(2, 3);
    }

    // We want to set down the file if it doesn't already exist, but we don't want to do the Loot container check
    // We want it to only have a 1,1 slot, but don't want to waste a loot container id for it.
    public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        #region OnBlockAdded
        if (_blockValue.ischild)
        {
            return;
        }
        this.shape.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
        if (this.isMultiBlock)
        {
            this.multiBlockPos.AddChilds(world, _chunk,  _chunk.ClrIdx, _blockPos, _blockValue);
        }

        if (!(world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntitySecureLootContainer))
        {
            TileEntityLootContainer tileEntityLootContainer = new TileEntityLootContainer(_chunk);
            tileEntityLootContainer.localChunkPos = World.toBlock(_blockPos);
            tileEntityLootContainer.lootListIndex = 25;
            tileEntityLootContainer.SetContainerSize(vLootContainerSize, true);
            _chunk.AddTileEntity(tileEntityLootContainer);
        }

        _chunk.AddEntityBlockStub(new BlockEntityData(_blockValue, _blockPos)
        {
            bNeedsTemperature = true

        });

        #endregion
    }


    // Display custom messages for turning on and off the music box, based on the block's name.
    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        #region GetActivationText

        PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
         string keybindString = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain);
        //string keybindString = UIUtils.GetKeybindString(playerInput.Activate, playerInput.PermanentActions.Activate);
        Block block = Block.list[_blockValue.type];
        string blockName = block.GetBlockName();

        string strReturn = string.Format(Localization.Get("pickupPrompt"), Localization.Get(blockName));

        BlockEntityData _ebcd = _world.GetChunkFromWorldPos(_blockPos).GetBlockEntity(_blockPos);
        if (_ebcd != null && _ebcd.transform)
        {
            MusicBoxScript myMusicBoxScript = _ebcd.transform.GetComponent<MusicBoxScript>();
            if (myMusicBoxScript == null)
            {
                myMusicBoxScript = _ebcd.transform.gameObject.AddComponent<MusicBoxScript>();
                myMusicBoxScript.enabled = false;
            }
            if (myMusicBoxScript)
            {
                if (myMusicBoxScript.enabled)
                    strReturn = string.Format(Localization.Get("musicbox_turnOff") + this.GetBlockName(), keybindString);
                else
                    strReturn = string.Format(Localization.Get("musicbox_turnOn") + this.GetBlockName(), keybindString);
            }
        }
        return strReturn;
        #endregion
    }


    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        this.cmds[0].enabled = true;
        this.cmds[1].enabled = true;
        this.cmds[2].enabled = (this.TakeDelay > 0f);
        return this.cmds;
    }

    // Handles what happens to the contents of the box when you pick up the block.
    private void EventData_Event(TimerEventData timerData)
    {
        #region EventData_Event
        World world = GameManager.Instance.World;

        object[] array = (object[])timerData.Data;
        int clrIdx = (int)array[0];
        BlockValue blockValue = (BlockValue)array[1];
        Vector3i vector3i = (Vector3i)array[2];
        BlockValue block = world.GetBlock(vector3i);
        EntityPlayerLocal entityPlayerLocal = array[3] as EntityPlayerLocal;
        TileEntityLootContainer tileEntityLootContainer = world.GetTileEntity(clrIdx, vector3i) as TileEntityLootContainer;
        if (tileEntityLootContainer == null)
            world.GetGameManager().DropContentOfLootContainerServer(blockValue, vector3i, tileEntityLootContainer.entityId);

        // Pick up the item and put it inyor your inventory.
        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
        ItemStack itemStack = new ItemStack(block.ToItemValue(), 1);
        if (!uiforPlayer.xui.PlayerInventory.AddItem(itemStack, true))
        {
            uiforPlayer.xui.PlayerInventory.DropItem(itemStack);
        }
        world.SetBlockRPC(clrIdx, vector3i, BlockValue.Air);

        #endregion
    }

    // We want to give the user the ability to pick up the blocks too, but the loot containers don't support that directly.
    public void TakeItemWithTimer(int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        #region TakeItemWithTimer
        if (_blockValue.damage > 0)
        {
            GameManager.ShowTooltipWithAlert(_player as EntityPlayerLocal, Localization.Get("ttRepairBeforePickup"), "ui_denied");
            return;
        }
        LocalPlayerUI playerUI = (_player as EntityPlayerLocal).PlayerUI;
        playerUI.windowManager.Open("timer", true, false, true);
        XUiC_Timer childByType = playerUI.xui.GetChildByType<XUiC_Timer>();
        TimerEventData timerEventData = new TimerEventData();
        timerEventData.Data = new object[]
        {
        _cIdx,
        _blockValue,
        _blockPos,
        _player
        };
        timerEventData.Event += this.EventData_Event;
        childByType.SetTimer(this.TakeDelay, timerEventData, -1f, "");
        #endregion
    }



    // Play the music when its activated. We stop the sound broadcasting, in case they want to restart it again; otherwise we can get two sounds playing.
    public override bool OnBlockActivated(int _indexInBlockActivationCommands, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        #region OnBlockActivated

        // If there's no transform, no sense on keeping going for this class.
        BlockEntityData _ebcd = _world.GetChunkFromWorldPos(_blockPos).GetBlockEntity(_blockPos);
        if (_ebcd == null || _ebcd.transform == null)
            return false;

        MusicBoxScript myMusicBoxScript = _ebcd.transform.GetComponent<MusicBoxScript>();
        if (myMusicBoxScript == null)
            myMusicBoxScript = _ebcd.transform.gameObject.AddComponent<MusicBoxScript>();

        bool bRuntimeSwitch = myMusicBoxScript.enabled;


        // Turn off the music box before we do anything with it.
        myMusicBoxScript.enabled = false;

        if (_indexInBlockActivationCommands != 0)
        {
            if (_indexInBlockActivationCommands == 1)
                base.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);

            if (_indexInBlockActivationCommands == 2)
                TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
        }
        else
        {
            bRuntimeSwitch = !bRuntimeSwitch;

            // Check if we have an animator and set it
            myMusicBoxScript.anim = _ebcd.transform.GetComponent<Animator>();

            // Check if we have a video player as well.
            myMusicBoxScript.videoPlayer = _ebcd.transform.GetComponent<VideoPlayer>();

            myMusicBoxScript.myBlockPos = _blockPos;
            // If the switch is on, then we want to look in the loot container to find a reference to any potential items, 
            // which will over-ride the default audio clip / video clip.
            if (bRuntimeSwitch)
            {
                // We'll try to support getting sounds from multiple sound data nodes, based on all the items in the loot container.
                List<String> mySounds = new List<String>();
                List<String> myVideos = new List<String>();

                TileEntityLootContainer tileLootContainer = (TileEntityLootContainer)_world.GetTileEntity(_cIdx, _blockPos);

                if (tileLootContainer.items != null)
                {
                    ItemStack[] array = tileLootContainer.items;
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array[i].IsEmpty())
                            continue;

                        if (array[i].itemValue.ItemClass.Properties.Values.ContainsKey("OnPlayBuff"))
                        {
                            String Buff = array[i].itemValue.ItemClass.Properties.Values["OnPlayBuff"];
                            _player.Buffs.AddBuff(Buff);
                        }

                        if (array[i].itemValue.ItemClass.Properties.Values.ContainsKey("OnPlayQuest"))
                        {
                            String Quest = array[i].itemValue.ItemClass.Properties.Values["OnPlayQuest"];
                            if (_player is EntityPlayerLocal)
                            {
                                Quest myQuest = QuestClass.CreateQuest(Quest);
                                if (myQuest != null)
                                    (_player as EntityPlayerLocal).QuestJournal.AddQuest(myQuest);
                            }

                        }
                        // Check for a SoundDataNode for a potential sound clip.
                        if (array[i].itemValue.ItemClass.Properties.Values.ContainsKey("SoundDataNode"))
                        {
                            String strSound = array[i].itemValue.ItemClass.Properties.Values["SoundDataNode"];
                            if (!mySounds.Contains(strSound))
                                mySounds.Add(strSound);
                        }
                        // Check for a video Source for a video clip. If we find it, load the asset and add it to the music box script.
                        if (array[i].itemValue.ItemClass.Properties.Values.ContainsKey("VideoSource"))
                        {
                            // Check if the video source is an asset bundle, and if so, load it directly into the video clip on
                            String strVideo = array[i].itemValue.ItemClass.Properties.Values["VideoSource"];
                            if (strVideo.IndexOf('#') == 0 && strVideo.IndexOf('?') > 0)
                            {
                                if (!myVideos.Contains(strVideo))
                                {
                                    myVideos.Add(strVideo);
                                }
                            }
                        }
                    }
                }

                // Initialize the data with our defaults.
                myMusicBoxScript.strAudioSource = this.strAudioSource;
                myMusicBoxScript.strSoundSource = this.strSoundSource;
                myMusicBoxScript.strVideoSource = this.strVideoSource;
                myMusicBoxScript.myEntity = _player;

                // List of Videos and Sound clips.
                myMusicBoxScript.VideoGroups = myVideos;
                myMusicBoxScript.SoundGroups = mySounds;

                myMusicBoxScript.myVideoClip = null;
                myMusicBoxScript.enabled = bRuntimeSwitch;

            }
        }


        return false;
        #endregion
    }
}