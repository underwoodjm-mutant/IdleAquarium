using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Mixer Groups")]
    [Tooltip("Drag the Ambient mixer group here.")]
    [SerializeField] private AudioMixerGroup ambientMixerGroup;
    [Tooltip("Drag the SFX mixer group here.")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    private void Start()
    {
        InitializeAudioSources();
        //AssignAndPlayRandomTrack();
        if (bgmSource != null && bgmSource.clip != null)
        {
            bgmSource.Play();
        }
    }

    /// <summary>
    /// Ensures the AudioSources are routed to the correct Mixer Groups 
    /// so the SettingsManager sliders actually work.
    /// </summary>
    private void InitializeAudioSources()
    {
        if (bgmSource != null && ambientMixerGroup != null)
        {
            bgmSource.outputAudioMixerGroup = ambientMixerGroup;
            bgmSource.loop = true; // Ensure the background track loops endlessly
        }

        if (sfxSource != null && sfxMixerGroup != null)
        {
            sfxSource.outputAudioMixerGroup = sfxMixerGroup;
        }
    }


    /// <summary>
    /// Call this from any script to play a quick sound effect (e.g., UI clicks, collecting Lumins).
    /// </summary>
    /// <param name="clip">The sound effect to play.</param>
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            // PlayOneShot allows multiple SFX to overlap without cutting each other off
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource != null && clip != null)
        {
            bgmSource.clip = clip;
            bgmSource.Play();
        }
    }
}
