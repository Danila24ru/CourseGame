using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SoundUtilities {

	public static void PlaySound(AudioSource audioSource, AudioClip audioClip, float volume, float deltaPitch)
    {
        audioSource.pitch = Random.Range(1 - deltaPitch, 1 + deltaPitch);
        audioSource.volume = volume;
        audioSource.clip = audioClip;
        audioSource.Play();
    }
}
