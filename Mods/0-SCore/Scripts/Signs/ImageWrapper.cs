using OldMoatGames;
using UnityEngine;

public class ImageWrapper : MonoBehaviour {
    private AnimatedGifPlayer AnimatedGifPlayer;
    private SCoreVideo _sCoreVideo;

    public void Awake() {
        // Get the GIF player component
        AnimatedGifPlayer = GetComponent<AnimatedGifPlayer>();
        if (AnimatedGifPlayer == null)
            AnimatedGifPlayer = gameObject.AddComponent<AnimatedGifPlayer>();

        _sCoreVideo = gameObject.GetOrAddComponent<SCoreVideo>();

        // Set the file to use. File has to be in StreamingAssets folder or a remote url (For example: http://www.example.com/example.gif).
        AnimatedGifPlayer.FileName = "AnimatedGIFPlayerExampe 3.gif";

        // Disable autoplay
        AnimatedGifPlayer.AutoPlay = false;

        // Add ready event to start play when GIF is ready to play
        AnimatedGifPlayer.OnReady += OnGifLoaded;

        // Add ready event for when loading has failed
        AnimatedGifPlayer.OnLoadError += OnGifLoadError;
    }

    public void OnDisable() {
        AnimatedGifPlayer.OnReady -= Play;
    }

    public bool ValidURL(ref string url) {
        url = url.TrimEnd();
        if (url.EndsWith("gif") || url.EndsWith("gifv"))
        {
            url = url.Replace(".gifv", ".mp4");
            url = url.Replace(".gif", ".mp4");
            return true;
        }

        if (url.EndsWith("png") || url.EndsWith("jpg"))
            return true;

        if (url.EndsWith("bmp") || url.EndsWith("jpeg"))
            return true;

        if (url.EndsWith("mp4"))
        {
            //  url.Replace(".mp4", ".gifv");
            return true;
        }

        return false;
    }

    public bool IsNewURL(string url) {
        if (string.IsNullOrEmpty(url))
            return false;

        if (url == AnimatedGifPlayer.FileName)
            return false;
        return true;
    }

    public void Init(string url) {
        if (string.IsNullOrEmpty(url))
            return;


        // Init the GIF player
        if (url.EndsWith("mp4"))
        {
            _sCoreVideo.Configure(url);
            return;
        }

        AnimatedGifPlayer.FileName = url;
        AnimatedGifPlayer.Init();
    }

    private void OnGifLoaded() {
        Play();
    }

    private void OnGifLoadError() {
        Debug.Log("Error Loading GIF");
    }

    public void Play() {
        // Start playing the GIF
        AnimatedGifPlayer.Play();
    }

    public void Pause() {
        // Stop playing the GIF
        AnimatedGifPlayer.Pause();
    }
}