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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class BlockMusicBox : BlockLoot
{
    private readonly BlockActivationCommand[] cmds =
    {
        new BlockActivationCommand("light", "electric_switch", true),
        new BlockActivationCommand("search", "search", true),
        new BlockActivationCommand("take", "hand", false)
    };

    private readonly float TakeDelay = 5f;

    // The AudioSource is a reference to the SoundDataNode's AudioSource.
    private string strAudioSource;

    // This is the SoundDataNode, if configured through the block
    private string strSoundSource;

    // Video Source contains the asset bundle where the video is stored, if configured through the block.
    private string strVideoSource;
    private Vector2i vLootContainerSize;

    public BlockMusicBox()
    {
        HasTileEntity = true;
    }

    public override void Init()
    {
        base.Init();

        // SoundSource is the referenced SoundDataNode
        if (Properties.Values.ContainsKey("SoundDataNode"))
            strSoundSource = Properties.Values["SoundDataNode"];

        // Audio Source is the prefab name, without the asset bundle reference.
        // This is used to keep track of which audio source is actually playing, so we can turn it on and off.
        if (Properties.Values.ContainsKey("AudioSource"))
            strAudioSource = Properties.Values["AudioSource"];

        // The VideoSource can be an asset bundle or URL. If there is an embedded clip on the Video Player, it'll play that in preference to the URL, but
        // a video in the asset bundle will over-ride
        if (Properties.Values.ContainsKey("VideoSource"))
            strVideoSource = Properties.Values["VideoSource"];

        if (Properties.Values.ContainsKey("LootContainerSize"))
            vLootContainerSize = StringParsers.ParseVector2i(Properties.Values["LootContainerSize"]);
        else
            vLootContainerSize = new Vector2i(2, 3);
    }

    // We want to set down the file if it doesn't already exist, but we don't want to do the Loot container check
    // We want it to only have a 1,1 slot, but don't want to waste a loot container id for it.
    public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        #region OnBlockAdded

        if (_blockValue.ischild) return;
        shape.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
        if (isMultiBlock) multiBlockPos.AddChilds(world, _chunk, _chunk.ClrIdx, _blockPos, _blockValue);

        if (!(world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntitySecureLootContainer))
        {
            var tileEntityLootContainer = new TileEntityLootContainer(_chunk);
            tileEntityLootContainer.localChunkPos = World.toBlock(_blockPos);
            tileEntityLootContainer.lootListName = "cntDropBag";
            tileEntityLootContainer.SetContainerSize(vLootContainerSize);
            _chunk.AddTileEntity(tileEntityLootContainer);
        }

        _chunk.AddEntityBlockStub(new BlockEntityData(_blockValue, _blockPos)
        {
            bNeedsTemperature = true
        });

        #endregion
    }


    // Display custom messages for turning on and off the music box, based on the block's name.
    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos,
        EntityAlive _entityFocusing)
    {
        #region GetActivationText

        var playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
        var keybindString = playerInput.Activate.GetBindingXuiMarkupString() +
                            playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
        //string keybindString = UIUtils.GetKeybindString(playerInput.Activate, playerInput.PermanentActions.Activate);
        var block = list[_blockValue.type];
        var blockName = block.GetBlockName();

        var strReturn = string.Format(Localization.Get("pickupPrompt"), Localization.Get(blockName));

        var _ebcd = _world.GetChunkFromWorldPos(_blockPos).GetBlockEntity(_blockPos);
        if (_ebcd == null || !_ebcd.transform) return strReturn;

        var myMusicBoxScript = _ebcd.transform.GetComponent<MusicBoxScript>();
        if (myMusicBoxScript == null)
        {
            myMusicBoxScript = _ebcd.transform.gameObject.AddComponent<MusicBoxScript>();
            myMusicBoxScript.enabled = false;
        }

        if (!myMusicBoxScript) return strReturn;
        if (myMusicBoxScript.enabled)
            return $"{Localization.Get("musicbox_turnOff")} {GetBlockName()}";
        else
            return $"{Localization.Get("musicbox_turnOn")} {GetBlockName()}";

        #endregion
    }


    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue,
        int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        cmds[0].enabled = true;
        cmds[1].enabled = true;
        cmds[2].enabled = TakeDelay > 0f;
        return cmds;
    }

    // Handles what happens to the contents of the box when you pick up the block.
    private void EventData_Event(TimerEventData timerData)
    {
        #region EventData_Event

        var world = GameManager.Instance.World;

        var array = (object[])timerData.Data;
        var clrIdx = (int)array[0];
        var blockValue = (BlockValue)array[1];
        var vector3i = (Vector3i)array[2];
        var block = world.GetBlock(vector3i);
        var entityPlayerLocal = array[3] as EntityPlayerLocal;
        var tileEntityLootContainer = world.GetTileEntity(clrIdx, vector3i) as TileEntityLootContainer;
        if (tileEntityLootContainer == null)
            world.GetGameManager()
                .DropContentOfLootContainerServer(blockValue, vector3i, tileEntityLootContainer.entityId);

        // Pick up the item and put it inyor your inventory.
        var uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
        var itemStack = new ItemStack(block.ToItemValue(), 1);
        if (!uiforPlayer.xui.PlayerInventory.AddItem(itemStack, true))
            uiforPlayer.xui.PlayerInventory.DropItem(itemStack);
        world.SetBlockRPC(clrIdx, vector3i, BlockValue.Air);

        #endregion
    }

    // We want to give the user the ability to pick up the blocks too, but the loot containers don't support that directly.
    public void TakeItemWithTimer(int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        #region TakeItemWithTimer

        if (_blockValue.damage > 0)
        {
            GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("ttRepairBeforePickup"),
                "ui_denied");
            return;
        }

        var playerUI = (_player as EntityPlayerLocal).PlayerUI;
        playerUI.windowManager.Open("timer", true);
        var childByType = playerUI.xui.GetChildByType<XUiC_Timer>();
        var timerEventData = new TimerEventData();
        timerEventData.Data = new object[]
        {
            _cIdx,
            _blockValue,
            _blockPos,
            _player
        };
        timerEventData.Event += EventData_Event;
        childByType.SetTimer(TakeDelay, timerEventData);

        #endregion
    }


    // Play the music when its activated. We stop the sound broadcasting, in case they want to restart it again; otherwise we can get two sounds playing.
    public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos,
        BlockValue _blockValue, EntityAlive _player)
    {
        #region OnBlockActivated

        // If there's no transform, no sense on keeping going for this class.
        var _ebcd = _world.GetChunkFromWorldPos(_blockPos).GetBlockEntity(_blockPos);
        if (_ebcd == null || _ebcd.transform == null)
            return false;

        var myMusicBoxScript = _ebcd.transform.GetComponent<MusicBoxScript>();
        if (myMusicBoxScript == null)
            myMusicBoxScript = _ebcd.transform.gameObject.AddComponent<MusicBoxScript>();

        var bRuntimeSwitch = myMusicBoxScript.enabled;


        // Turn off the music box before we do anything with it.
        myMusicBoxScript.enabled = false;

        switch (_commandName)
        {
            case "light":
                break;
            case "take":
                TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
                return true;
            case "search":
                base.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
                return true;
        }


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
            var mySounds = new List<string>();
            var myVideos = new List<string>();

            var tileLootContainer = (TileEntityLootContainer)_world.GetTileEntity(_cIdx, _blockPos);

            if (tileLootContainer.items != null)
            {
                var array = tileLootContainer.items;
                for (var i = 0; i < array.Length; i++)
                {
                    if (array[i].IsEmpty())
                        continue;

                    if (array[i].itemValue.ItemClass.Properties.Values.ContainsKey("OnPlayBuff"))
                    {
                        var Buff = array[i].itemValue.ItemClass.Properties.Values["OnPlayBuff"];
                        _player.Buffs.AddBuff(Buff);
                    }

                    if (array[i].itemValue.ItemClass.Properties.Values.ContainsKey("OnPlayQuest"))
                    {
                        var Quest = array[i].itemValue.ItemClass.Properties.Values["OnPlayQuest"];
                        if (_player is EntityPlayerLocal)
                        {
                            var myQuest = QuestClass.CreateQuest(Quest);
                            if (myQuest != null)
                                (_player as EntityPlayerLocal).QuestJournal.AddQuest(myQuest);
                        }
                    }

                    // Check for a SoundDataNode for a potential sound clip.
                    if (array[i].itemValue.ItemClass.Properties.Values.ContainsKey("SoundDataNode"))
                    {
                        var strSound = array[i].itemValue.ItemClass.Properties.Values["SoundDataNode"];
                        if (!mySounds.Contains(strSound))
                            mySounds.Add(strSound);
                    }

                    // Check for a video Source for a video clip. If we find it, load the asset and add it to the music box script.
                    if (array[i].itemValue.ItemClass.Properties.Values.ContainsKey("VideoSource"))
                    {
                        // Check if the video source is an asset bundle, and if so, load it directly into the video clip on
                        var strVideo = array[i].itemValue.ItemClass.Properties.Values["VideoSource"];
                        if (strVideo.IndexOf('#') == 0 && strVideo.IndexOf('?') > 0)
                            if (!myVideos.Contains(strVideo))
                                myVideos.Add(strVideo);
                    }
                }
            }

            // Initialize the data with our defaults.
            myMusicBoxScript.strAudioSource = strAudioSource;
            myMusicBoxScript.strSoundSource = strSoundSource;
            myMusicBoxScript.strVideoSource = strVideoSource;
            myMusicBoxScript.myEntity = _player;

            // List of Videos and Sound clips.
            myMusicBoxScript.VideoGroups = myVideos;
            myMusicBoxScript.SoundGroups = mySounds;

            myMusicBoxScript.myVideoClip = null;
            myMusicBoxScript.enabled = bRuntimeSwitch;
        }


        return false;

        #endregion
    }
}