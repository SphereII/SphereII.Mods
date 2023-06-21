using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LocksetAudio : MonoBehaviour
{
    public AudioClip[] clips;
    private float _delay;
    private AudioSource audioSource;

    public void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayAudioClip(float volume = 0.5f)
    {
        if (clips == null)
            return;

        if (clips.Length > 0)
        {
            SelectRandomClip();
            audioSource.volume = volume;
            audioSource.Play();
        }
    }

    public void PlayOnce()
    {
        SelectRandomClip();
        if (audioSource.clip == null)
            return;

        audioSource.PlayOneShot(audioSource.clip);
    }

    public bool isAudioPlaying()
    {
        return audioSource.isPlaying;
    }

    public void DelayPlay(float delay)
    {
        _delay = delay;
        StartCoroutine("Delay");
    }

    public IEnumerator Delay()
    {
        yield return new WaitForSeconds(_delay);
        PlayOnce();
    }

    public void PlayLoop()
    {
        if (!audioSource.isPlaying)
        {
            SelectRandomClip();
            audioSource.Play();
        }
    }

    public void StopLoop()
    {
        audioSource.Pause();
    }

    public void SelectRandomClip()
    {
        if (clips == null)
            return;

        if (clips.Length > 0) audioSource.clip = clips[Random.Range(0, clips.Length)];
    }
}