using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vase : MonoBehaviour
{
    public AudioClip[] Clips;
        
    private AudioSource audioSource;
    private int lastIndex;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        Invoke("EnableAudio", 0.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        int index;
        do index = Random.Range(0, Clips.Length);
        while (index == lastIndex);

        lastIndex = index;
        audioSource.PlayOneShot(Clips[index]);
    }

    private void EnableAudio()
    {
        audioSource.volume = 0.5f;
    }
}