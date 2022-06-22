using System;
using UnityEngine;


public class AudioManager : MonoBehaviour {


    [SerializeField] AudioSource audioSourceMusic;
    [SerializeField] AudioSource audioSourceAmbience;
    [SerializeField] AudioSource audioSourceGlobalSounds;


    public void PlayMusic(AudioClip audioClip) {
        PlayLoopAudioClip(audioSourceMusic, audioClip);
    }

    public void PlayAmbience(AudioClip audioClip) {
        PlayLoopAudioClip(audioSourceAmbience, audioClip);
    }

    void PlayLoopAudioClip(AudioSource audioSource, AudioClip audioClip) {

        audioSource.clip = audioClip;

        if (!audioSource.isPlaying) {
            audioSource.Play();
        }
    }

    public void PlaySimpleSound(AudioClip audioClip) {

        if (audioClip == null) {
            throw new ArgumentException("Trying to play a null audio clip");
        }

        audioSourceGlobalSounds.PlayOneShot(audioClip);
    }

}
