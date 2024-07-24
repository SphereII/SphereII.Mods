using System;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
[RequireComponent(typeof(MeshRenderer))]
public class SCoreVideo : MonoBehaviour {
    private VideoPlayer _videoPlayer;
    private MeshRenderer _meshRenderer;
    private AudioSource _audioSource;
    private void Awake() {
        _videoPlayer = GetComponent<VideoPlayer>();
        _audioSource = GetComponent<AudioSource>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _videoPlayer.playOnAwake = false;
    }

    public void Configure(string url) {
        _videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
        var newMat = new Material(SCoreModEvents.GetStandardShader());
        _meshRenderer.material = newMat;
        _videoPlayer.targetMaterialRenderer = _meshRenderer;
        _videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        
        _videoPlayer.source = VideoSource.Url;
        _videoPlayer.url = url;
        _videoPlayer.isLooping = true;
        _videoPlayer.Play();
    }
    
    
    
    
}