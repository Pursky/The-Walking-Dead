using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    private Rigidbody rigbod;
    private AudioSource audioSource;

    void Start()
    {
        rigbod = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (UI.Instance.Paused) return;

        if (rigbod.angularVelocity.magnitude > 0.1f)
        {
            audioSource.volume = rigbod.angularVelocity.magnitude;
            if (!audioSource.isPlaying) audioSource.Play();
        }
        else audioSource.Stop();
    }
}