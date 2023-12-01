using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayRandomSound : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] audioClipsStart;
    public AudioClip[] audioClips;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = audioClipsStart[Random.Range(0, audioClips.Length-1)];
        audioSource.Play();
    }

    public void PlaySoundRandom()
    {
        audioSource.clip = audioClips[Random.Range(0, audioClips.Length-1)];
        audioSource.Play();
    }
}
