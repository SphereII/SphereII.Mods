using System;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
//[RequireComponent(typeof(MeshRenderer))]
public class SCoreVideo : MonoBehaviour {
    private VideoPlayer _videoPlayer;
    private MeshRenderer _meshRenderer;
    private void Awake() {
        _videoPlayer = GetComponent<VideoPlayer>();
        _videoPlayer.playOnAwake = false;
        _meshRenderer = transform.GetComponentInChildren<MeshRenderer>();
        if ( _meshRenderer == null)
            _meshRenderer = transform.gameObject.GetOrAddComponent<MeshRenderer>();
    }

    public void Configure(string url) {
        _videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
        _videoPlayer.targetMaterialRenderer = _meshRenderer;
        _videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        _videoPlayer.source = VideoSource.Url;
        _videoPlayer.url = url;
        _videoPlayer.isLooping = true;
        _videoPlayer.Play();
    }
    
    
    
    
}