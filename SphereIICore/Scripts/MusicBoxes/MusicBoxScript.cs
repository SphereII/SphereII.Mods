
using Audio;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Video;


class MusicBoxScript : MonoBehaviour
{
    // This is the entity that initialized the music box.
    public Entity myEntity;

    // strSoundSource is the current SoundDataNode being played. It gets updated from the list of SoundGroups.
    public string strSoundSource;

    // strVideoSource is the current video clip being played. It gets updated from the list of video clips.
    public string strVideoSource;

    // These are the optional list of sound and videos clips that can be part of this.
    public List<string> SoundGroups = new List<string>();
    public List<string> VideoGroups = new List<string>();

    // AudioSource is the SoundData's Node AudioSource attribute. It's the "speaker"
    public string strAudioSource;

    
    // References to our Animator and Video Player, if it exists.
    public Animator anim;
    public VideoPlayer videoPlayer;
    public VideoClip defaultClip;
    public Vector3i myBlockPos;

    // The video clip reference, which gets loaded from the asset bundle.
    public VideoClip myVideoClip;

    // We need a randomize element to pick the sound and video from their groups.
    public System.Random random = new System.Random();

    // This is the AudioSource that is attached to the VideoPlayer, so we can hear the sounds from the video.
    public AudioSource myAudioSource;

    // Set up a throttle to delay the Update(), since we don't really need it called each frame
    private float nextCheck = 0;
    public float CheckDelay = 1f;
    public bool IsVideoPaused = false;

    void Start()
    {
        if (videoPlayer && videoPlayer.clip != null)
            defaultClip = videoPlayer.clip;

       // myAudioSource = gameObject.AddComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (nextCheck < Time.time)
        {
            nextCheck = Time.time + CheckDelay;
            CheckAudioPlayer();
            CheckVideoPlayer();
        }

     

    }

    void CheckVideoPlayer()
    {
        if (!this.videoPlayer)
            return;

        if ( GameManager.Instance.IsPaused())
        {
            this.videoPlayer.Pause();
            IsVideoPaused = true;
            return;
        }

        if ( IsVideoPaused == true )
        {
            // If the video was paused, and we want to resume it, just re-play, and not change any clips.
            if (IsVideoPaused == true)
            {
                this.videoPlayer.Play();
                IsVideoPaused = false;
                return;
            }
        }

        if ( !this.videoPlayer.isPlaying)
        { 
            // If the video has a clip already attached, then check if there's another clip we could use instead.
            if (this.videoPlayer.clip)
            {
                // Otherwise, if we have another video clip specified, use that.
                if (myVideoClip)
                {
                    this.videoPlayer.clip = myVideoClip;
                }
            }

            // If there's still no video clip, try to parse the Video Source as a URL
            if (!this.videoPlayer.clip)
            {
                this.videoPlayer.url = this.strVideoSource;
            }

            // If the VideoGroups is populated, that means we can randomize based on all the videos groups in our container.
            if (VideoGroups.Count > 0)
            {
                int randomIndex = random.Next(0, VideoGroups.Count);
                this.strVideoSource = VideoGroups[randomIndex].ToString();
                if (strVideoSource.StartsWith("#"))
                {
                    this.videoPlayer.clip = DataLoader.LoadAsset<VideoClip>(strVideoSource);
                }
            }
            else
            {
                if (defaultClip)
                    this.videoPlayer.clip = defaultClip;
                else
                    Debug.Log("There are no Groups of video detected.");

            }
            this.videoPlayer.Play();
        }
    }

    void OnEnable()
    {    
        // Toggle the animation if it's available
        if (this.anim)
        {
            this.anim.enabled = true;
            this.anim.SetBool("IsOn", true);
        }
    }

    void CheckAudioPlayer()
    {
        // Check to see if we are still playing music or not, and if everything is initialized.
        if (!IsPlaying())
        {
            // Toggle the sound if we have sound references.
            if (myEntity && !string.IsNullOrEmpty(this.strSoundSource) && !string.IsNullOrEmpty(this.strAudioSource))
            {

                // If the SoundGroups is populated, that means we can randomize based on all the sound groups in our container.
                if (SoundGroups.Count > 0)
                {
                    int randomIndex = random.Next(0, SoundGroups.Count);
                    this.strSoundSource = SoundGroups[randomIndex].ToString();
                }

                if (myBlockPos != null)
                {
                    Manager.BroadcastPlay( this.myBlockPos.ToVector3(), this.strSoundSource, 0f);
                }
            }
        }

    }

    // Disable the script; turning off any sounds that may be playing, and shutting down the animation.
    void OnDisable()
    {
        if (this.videoPlayer)
            this.videoPlayer.Stop();

        if (this.anim)
            this.anim.enabled = false;
       
        // In order for BroadcastStop() to work, when you are using an external audio source, is that it must be set to Loop. This is the Unity AudioSource, 
        // not the sounds.xml AudioClip line.
        Manager.BroadcastStop(this.myBlockPos.ToVector3(), this.strSoundSource);
 
    }

    // Since the Manager.Audio doesn't give us access to our playing source, we have to poll the List<> of sources to see if ours is still playing.
    bool IsPlaying()
    {
        // If the audio source isn't set, don't blindly try to play, but tell the loop that something is playing to prevent it starting something.
        if (string.IsNullOrEmpty(this.strAudioSource))
            return true;

        // look for the audio sources 
        foreach (var each in Manager.playingAudioSources)
        {
            if (each.ToString().Contains(this.strAudioSource))
            {
                return true;
            }
        }

        return false;
    }
}
