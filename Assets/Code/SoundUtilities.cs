using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class SoundUtilities {

    [ClientRpc(channel = 1)]
	public static void RpcPlaySound(AudioSource audioSource, AudioClip audioClip, float volume, float deltaPitch)
    {
        audioSource.pitch = Random.Range(1 - deltaPitch, 1 + deltaPitch);
        audioSource.volume = volume;
        audioSource.clip = audioClip;
        audioSource.Play();
    }
}
